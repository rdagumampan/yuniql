using Yuniql.Extensibility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace Yuniql.Core
{
    /// <inheritdoc />
    public partial class MigrationService : IMigrationService
    {
        private readonly IWorkspaceService _workspaceService;
        private readonly IDataService _dataService;
        private readonly IBulkImportService _bulkImportService;
        private readonly ITokenReplacementService _tokenReplacementService;
        private readonly IDirectoryService _directoryService;
        private readonly IFileService _fileService;
        private readonly ITraceService _traceService;
        private readonly IConfigurationService _configurationService;
        private readonly IMetadataService _metadataService;

        /// <inheritdoc /> 
        public MigrationService(
            IWorkspaceService workspaceService,
            IDataService dataService,
            IBulkImportService bulkImportService,
            IMetadataService metadataService,
            ITokenReplacementService tokenReplacementService,
            IDirectoryService directoryService,
            IFileService fileService,
            ITraceService traceService,
            IConfigurationService configurationService)
        {
            this._workspaceService = workspaceService;
            this._dataService = dataService;
            this._bulkImportService = bulkImportService;
            this._tokenReplacementService = tokenReplacementService;
            this._directoryService = directoryService;
            this._fileService = fileService;
            this._traceService = traceService;
            this._configurationService = configurationService;
            this._metadataService = metadataService;
        }

        /// <inheritdoc />
        public void Run()
        {
            var configuration = _configurationService.GetConfiguration();
            if (!configuration.IsInitialized)
                Initialize();

            Run(
               workspace: configuration.Workspace,
               targetVersion: configuration.TargetVersion,
               isAutoCreateDatabase: configuration.IsAutoCreateDatabase,
               tokens: configuration.Tokens,
               isVerifyOnly: configuration.IsVerifyOnly,
               bulkSeparator: configuration.BulkSeparator,
               metaSchemaName: configuration.MetaSchemaName,
               metaTableName: configuration.MetaTableName,
               commandTimeout: configuration.CommandTimeout,
               bulkBatchSize: configuration.BulkBatchSize,
               appliedByTool: configuration.AppliedByTool,
               appliedByToolVersion: configuration.AppliedByToolVersion,
               environment: configuration.Environment,
               isContinueAfterFailure: configuration.IsContinueAfterFailure,
               transactionMode: configuration.TransactionMode,
               isRequiredClearedDraft: configuration.IsRequiredClearedDraft
            );
        }

        /// <inheritdoc />
        public void Run(
            string workspace,
            string targetVersion = null,
            bool? isAutoCreateDatabase = false,
            List<KeyValuePair<string, string>> tokens = null,
            bool? isVerifyOnly = false,
            string bulkSeparator = null,
            string metaSchemaName = null,
            string metaTableName = null,
            int? commandTimeout = null,
            int? bulkBatchSize = null,
            string appliedByTool = null,
            string appliedByToolVersion = null,
            string environment = null,
            bool? isContinueAfterFailure = null,
            string transactionMode = null,
            bool isRequiredClearedDraft = false
         )
        {
            //print run configuration information            
            _traceService.Info($"Run configuration: {Environment.NewLine}{_configurationService.PrintAsJson()}");

            //check the workspace structure if required directories are present
            _workspaceService.Validate(workspace);

            //when uncomitted run is not supported, fail migration, throw exceptions and return error exit code
            if (isVerifyOnly.HasValue && isVerifyOnly == true && !_dataService.IsTransactionalDdlSupported)
            {
                throw new NotSupportedException("Yuniql.Verify is not supported in the target platform. " +
                    "The feature requires support for atomic DDL operations. " +
                    "An atomic DDL operations ensures creation of tables, views and other objects and data are rolledback in case of error. " +
                    "For more information see https://yuniql.io/docs/.");
            }

            //when no target version specified, we use the latest local version available
            if (string.IsNullOrEmpty(targetVersion))
            {
                targetVersion = _workspaceService.GetLatestVersion(workspace);
                _traceService.Info($"No explicit target version requested. We'll use latest available locally {targetVersion} on {workspace}.");
            }

            var connectionInfo = _dataService.GetConnectionInfo();
            var targetDatabaseName = connectionInfo.Database;
            var targetDatabaseServer = connectionInfo.DataSource;

            //we try to auto-create the database, we need this to be outside of the transaction scope
            //in an event of failure, users have to manually drop the auto-created database!
            //we only check if the db exists when --auto-create-db is true
            if (isAutoCreateDatabase.HasValue && isAutoCreateDatabase == true)
            {
                var targetDatabaseExists = _metadataService.IsDatabaseExists();
                if (!targetDatabaseExists)
                {
                    _traceService.Info($"Target database does not exist. Creating database {targetDatabaseName} on {targetDatabaseServer}.");
                    _metadataService.CreateDatabase();
                    _traceService.Info($"Created database {targetDatabaseName} on {targetDatabaseServer}.");
                }
            }

            //check if database has been pre-configured to support migration and setup when its not
            var targetDatabaseConfigured = _metadataService.IsDatabaseConfigured(metaSchemaName, metaTableName);
            if (!targetDatabaseConfigured)
            {
                //create custom schema when user supplied and only if platform supports it
                if (null != metaSchemaName 
                    && _dataService.IsSchemaSupported
                    && !_dataService.MetaSchemaName.Equals(metaSchemaName)
                    && !_metadataService.IsSchemaExists(metaSchemaName))
                {
                    _traceService.Info($"Target schema does not exist. Creating schema {metaSchemaName} on {targetDatabaseName} on {targetDatabaseServer}.");
                    _metadataService.CreateSchema(metaSchemaName);
                    _traceService.Info($"Created schema {metaSchemaName} on {targetDatabaseName} on {targetDatabaseServer}.");
                }

                //create empty versions tracking table
                _traceService.Info($"Target database {targetDatabaseName} on {targetDatabaseServer} not yet configured for migration.");
                _metadataService.ConfigureDatabase(metaSchemaName, metaTableName);
                _traceService.Info($"Configured database migration support for {targetDatabaseName} on {targetDatabaseServer}.");
            }

            //we may have to upgrade the version tracking table for yuniql to work in this release
            var targetDatabaseUpdated = _metadataService.UpdateDatabaseConfiguration(metaSchemaName, metaTableName);
            var databaseUpgradedMessage = targetDatabaseUpdated 
                ? $"The schema version tracking table has been upgraded for {targetDatabaseName} on {targetDatabaseServer}." 
                : $"The schema version tracking table is up to date for {targetDatabaseName} on {targetDatabaseServer}.";
            _traceService.Info(databaseUpgradedMessage);

            TransactionContext transactionContext = null;

            //check for presence of failed no-transactional versions from previous runs
            var allVersions = _metadataService.GetAllVersions(metaSchemaName, metaTableName);
            var failedVersion = allVersions.Where(x => x.Status == Status.Failed).FirstOrDefault();
            if (failedVersion != null)
            {
                //check if user had issue resolving option such as continue on failure
                if (isContinueAfterFailure == null)
                {
                    //program should exit with non zero exit code
                    var message = @$"Previous migration of ""{failedVersion.Version}"" version was not running in transaction and has failed when executing of script ""{failedVersion.FailedScriptPath}"" with following error: {failedVersion.FailedScriptError}. {MESSAGES.ManualResolvingAfterFailureMessage}";
                    _traceService.Error(message);
                    throw new InvalidOperationException(message);
                }

                _traceService.Warn($@"The non-transactional failure resolving option ""{isContinueAfterFailure}"" was used. Version scripts already applied by previous migration run will be skipped.");
                transactionContext = new TransactionContext(failedVersion, isContinueAfterFailure.Value);
            }
            else
            {
                //check if the non-txn option is passed even if there was no previous failed runs
                if (isContinueAfterFailure != null && isContinueAfterFailure.Value == true)
                {
                    //program should exit with non zero exit code
                    _traceService.Warn(@$"The transaction handling parameter --continue-after-failure received but no previous failed migrations recorded.");
                }
            }

            var appliedVersions = _metadataService.GetAllAppliedVersions(metaSchemaName, metaTableName)
                .Select(dv => dv.Version)
                .OrderBy(v => v)
                .ToList();

            //check if target database already runs the latest version and skips work if it already is
            var targeDatabaseLatest = IsTargetDatabaseLatest(targetVersion, metaSchemaName, metaTableName);
            if (!targeDatabaseLatest)
            {
                //enclose all executions in a single transaction, in the event of failure we roll back everything
                using (var connection = _dataService.CreateConnection())
                {
                    connection.Open();
                    using (var transaction = (!string.IsNullOrEmpty(transactionMode) && transactionMode.Equals(TRANSACTION_MODE.SESSION)) ? connection.BeginTransaction() : null)
                    {
                        try
                        {
                            if (null != transaction)
                                _traceService.Info("Transaction created for current session. This migration run will be executed in a shared connection and transaction context.");

                            //run all migrations present in all directories
                            RunAllInternal(connection, transaction, isRequiredClearedDraft);

                            //when true, the execution is an uncommitted transaction 
                            //and only for purpose of testing if all can go well when it run to the target environment
                            if (isVerifyOnly.HasValue && isVerifyOnly == true)
                                transaction?.Rollback();
                            else
                            {
                                if (transaction?.Connection == null)
                                    _traceService.Warn("Transaction has been committed before the end of the session. " +
                                        "Please verify if all schema migrations has been successfully applied. " +
                                        "If there was fault in the process, the database changes during migration process will be rolled back.");
                                else
                                    transaction?.Commit();
                            }
                        }
                        catch (Exception)
                        {
                            transaction?.Rollback();
                            throw;
                        }
                    }
                }
            }
            else
            {
                //when target database already runs the latest version, we at least execute scripts in draft folder
                //enclose all executions in a single transaction
                using (var connection = _dataService.CreateConnection())
                {
                    connection.Open();
                    using (var transaction = (!string.IsNullOrEmpty(transactionMode) && transactionMode.Equals(TRANSACTION_MODE.SESSION)) ? connection.BeginTransaction() : null)
                    {
                        try
                        {
                            //run all scripts present in the _pre, _draft and _post directories
                            if (null != transaction)
                                _traceService.Info("Transaction created for current session. This migration run will be executed in a shared connection and transaction context.");

                            RunPreDraftPostInternal(connection, transaction, isRequiredClearedDraft);

                            //when true, the execution is an uncommitted transaction 
                            //and only for purpose of testing if all can go well when it run to the target environment
                            if (isVerifyOnly.HasValue && isVerifyOnly == true)
                                transaction?.Rollback();
                            else
                                transaction?.Commit();
                        }
                        catch (Exception)
                        {
                            transaction?.Rollback();
                            throw;
                        }
                    }
                }
                _traceService.Info($"Target database runs the latest version already. Scripts in {RESERVED_DIRECTORY_NAME.PRE}, {RESERVED_DIRECTORY_NAME.DRAFT} and {RESERVED_DIRECTORY_NAME.POST} are executed.");
            }

            //local method
            void RunAllInternal(IDbConnection connection, IDbTransaction transaction, bool isRequiredClearedDraft)
            {
                //check if database has been pre-configured and execute init scripts
                if (!targetDatabaseConfigured)
                {
                    //runs all scripts in the _init folder
                    RunNonVersionDirectories(connection, transaction, workspace, Path.Combine(workspace, RESERVED_DIRECTORY_NAME.INIT), tokens: tokens, bulkBatchSize: bulkBatchSize, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environment: environment, transactionMode: transactionMode);
                    _traceService.Info($"Executed script files on {Path.Combine(workspace, RESERVED_DIRECTORY_NAME.INIT)}");
                }

                //checks if target database already runs the latest version and skips work if it already is
                //runs all scripts in the _pre folder and subfolders
                RunNonVersionDirectories(connection, transaction, workspace, Path.Combine(workspace, RESERVED_DIRECTORY_NAME.PRE), tokens: tokens, bulkBatchSize: bulkBatchSize, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environment: environment, transactionMode: transactionMode);
                _traceService.Info($"Executed script files on {Path.Combine(workspace, RESERVED_DIRECTORY_NAME.PRE)}");

                //runs all scripts int the vxx.xx folders and subfolders
                RunVersionDirectories(connection, transaction, appliedVersions, workspace, targetVersion, transactionContext, tokens, bulkSeparator: bulkSeparator, metaSchemaName: metaSchemaName, metaTableName: metaTableName, commandTimeout: commandTimeout, bulkBatchSize: bulkBatchSize, appliedByTool: appliedByTool, appliedByToolVersion: appliedByToolVersion, environment: environment, transactionMode: transactionMode);

                //runs all scripts in the _draft folder and subfolders
                RunNonVersionDirectories(connection, transaction, workspace, Path.Combine(workspace, RESERVED_DIRECTORY_NAME.DRAFT), tokens: tokens, bulkBatchSize: bulkBatchSize, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environment: environment, transactionMode: transactionMode, isRequiredClearedDraft: isRequiredClearedDraft);
                _traceService.Info($"Executed script files on {Path.Combine(workspace, RESERVED_DIRECTORY_NAME.DRAFT)}");

                //runs all scripts in the _post folder and subfolders
                RunNonVersionDirectories(connection, transaction, workspace, Path.Combine(workspace, RESERVED_DIRECTORY_NAME.POST), tokens: tokens, bulkBatchSize: bulkBatchSize, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environment: environment, transactionMode: transactionMode);
                _traceService.Info($"Executed script files on {Path.Combine(workspace, RESERVED_DIRECTORY_NAME.POST)}");
            }

            //local method
            void RunPreDraftPostInternal(IDbConnection connection, IDbTransaction transaction, bool requiredClearedDraft)
            {
                //runs all scripts in the _pre folder and subfolders
                RunNonVersionDirectories(connection, transaction, workspace, Path.Combine(workspace, RESERVED_DIRECTORY_NAME.PRE), tokens: tokens, bulkBatchSize: bulkBatchSize, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environment: environment, transactionMode: transactionMode);
                _traceService.Info($"Executed script files on {Path.Combine(workspace, RESERVED_DIRECTORY_NAME.PRE)}");

                //runs all scripts in the _draft folder and subfolders
                RunNonVersionDirectories(connection, transaction, workspace, Path.Combine(workspace, RESERVED_DIRECTORY_NAME.DRAFT), tokens: tokens, bulkBatchSize: bulkBatchSize, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environment: environment, transactionMode: transactionMode, isRequiredClearedDraft: requiredClearedDraft);
                _traceService.Info($"Executed script files on {Path.Combine(workspace, RESERVED_DIRECTORY_NAME.DRAFT)}");

                //runs all scripts in the _post folder and subfolders
                RunNonVersionDirectories(connection, transaction, workspace, Path.Combine(workspace, RESERVED_DIRECTORY_NAME.POST), tokens: tokens, bulkBatchSize: bulkBatchSize, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environment: environment, transactionMode: transactionMode);
                _traceService.Info($"Executed script files on {Path.Combine(workspace, RESERVED_DIRECTORY_NAME.POST)}");
            }
        }

        /// <inheritdoc />
        public void RunVersionDirectories(
            IDbConnection connection,
            IDbTransaction transaction,
            List<string> appliedVersions,
            string workspace,
            string targetVersion,
            TransactionContext transactionContext,
            List<KeyValuePair<string, string>> tokens = null,
            string bulkSeparator = null,
            string metaSchemaName = null,
            string metaTableName = null,
            int? commandTimeout = null,
            int? bulkBatchSize = null,
            string appliedByTool = null,
            string appliedByToolVersion = null,
            string environment = null,
            string transactionMode = null
        )
        {
            //excludes all versions already executed
            var localVersionDirectories = _directoryService.GetDirectories(workspace, "v*.*")
                .Where(v => !appliedVersions.Contains(new DirectoryInfo(v).Name))
                .Select(v => new LocalVersion(new DirectoryInfo(v).Name, v));

            var sortedLocalVersions = localVersionDirectories
                    .OrderBy(s => s.Major)
                    .ThenBy(s => s.Minor)
                    .ThenBy(s => s.Revision)
                    .ThenBy(s => s.Label)
                    .Select((v, idx) => new { Version = v, Index = idx })
                    .ToList();

            //exclude all versions greater than the target version
            if (!string.IsNullOrEmpty(targetVersion))
            {
                var targetVersionPath = Path.Combine(workspace, targetVersion);
                var targetVersionInList = sortedLocalVersions.FirstOrDefault(v => string.Equals(v.Version.Path, targetVersionPath, StringComparison.InvariantCultureIgnoreCase));
                if (null == targetVersionInList)
                {
                    throw new Exception("Target version does not exist in the workspace directory. Check if you entered the correct version name. Directories can be case sensitive depend on the Operation System yuniql runs.");
                }

                //remove later versions form list to be processed
                sortedLocalVersions.RemoveAll(v => v.Index > targetVersionInList.Index);
            }

            //execute all sql scripts in the version folders
            if (sortedLocalVersions.Any())
            {
                sortedLocalVersions.ForEach(version =>
                {
                    //initialize stop watch to measure duration of execution per version
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    if (!string.IsNullOrEmpty(transactionMode) && transactionMode.Equals(TRANSACTION_MODE.VERSION))
                    {
                        using (var internalConnection = _dataService.CreateConnection())
                        {
                            internalConnection.Open();
                            using (var internalTransaction = internalConnection.BeginTransaction())
                            {
                                try
                                {
                                    if (null != internalTransaction)
                                        _traceService.Info("Transaction created for current version. This version migration run will be executed in this dedicated connection and transaction context.");

                                    //run scripts in all sub-directories in the version
                                    var scriptSubDirectories = _directoryService.GetAllDirectories(version.Version.Path, "*").ToList(); ;
                                    RunVersionDirectoriesInternal(internalConnection, internalTransaction, scriptSubDirectories, version.Version.Path, version.Version.Path, stopwatch);

                                    internalTransaction.Commit();
                                }
                                catch (Exception)
                                {
                                    internalTransaction.Rollback();
                                    throw;
                                }
                            }
                        }
                    }
                    else
                    {
                        //collect all child directions in current version vxx.xx
                        var scriptSubDirectories = _directoryService.GetAllDirectories(version.Version.Path, "*").ToList(); ;

                        //check for special _transaction directory in the version vxx.xx directory
                        var transactionDirectory = Path.Combine(version.Version.Path, RESERVED_DIRECTORY_NAME.TRANSACTION);
                        var transactionExplicit = _directoryService.Exists(transactionDirectory);
                        if (transactionExplicit)
                        {
                            //check version directory with _transaction directory only applies to platforms NOT supporting transactional ddl
                            if (_dataService.IsTransactionalDdlSupported)
                            {
                                throw new YuniqlMigrationException(@$"The version directory ""{version.Version.Path}"" can't contain ""{RESERVED_DIRECTORY_NAME.TRANSACTION}"" subdirectory for selected target platform, because the whole migration is already running in single transaction.");
                            }

                            //check version directory must only contain _transaction directory and nothing else
                            if (_directoryService.GetDirectories(version.Version.Path, "*").Count() > 1)
                            {
                                throw new YuniqlMigrationException(@$"The version directory ""{version.Version.Path}"" containing ""{RESERVED_DIRECTORY_NAME.TRANSACTION}"" subdirectory can't contain other subdirectories.");
                            }

                            //check version directory must only contain _transaction directory, files are also not allowed
                            //check users need to place the script files and subdirectories inside _transaction directory
                            if (_directoryService.GetFiles(version.Version.Path, "*.*").Count() > 0)
                            {
                                throw new YuniqlMigrationException(@$"The version directory ""{version.Version.Path}"" containing ""{RESERVED_DIRECTORY_NAME.TRANSACTION}"" subdirectory can't contain files.");
                            }

                            //override the list of subdirectories to process by the directory list container in _transaction directory
                            scriptSubDirectories = _directoryService.GetAllDirectories(transactionDirectory, "*").ToList();
                        }


                        if (transactionExplicit)
                        {
                            //run scripts within a single transaction for all scripts inside _transaction directory and scripts in the child directories
                            string versionName = new DirectoryInfo(version.Version.Path).Name;
                            _traceService.Warn(@$"The ""{RESERVED_DIRECTORY_NAME.TRANSACTION}"" directory has been detected and therefore ""{versionName}"" version scripts will run in single transaction. The rollback will not be reliable in case the version scripts contain commands causing implicit commit (e.g. DDL)!");

                            using (var transaction = connection.BeginTransaction())
                            {
                                try
                                {
                                    //scriptSubDirectories is the child directories under _transaction directory c:\temp\vxx.xx\_transaction\list_of_directories
                                    //transactionDirectory the path of _transaction directory c:\temp\vxx.xx\_transaction
                                    //versionDirectory path of version c:\temp\vxx.xx
                                    RunVersionDirectoriesInternal(connection, transaction, scriptSubDirectories, transactionDirectory, version.Version.Path, stopwatch);
                                    transaction.Commit();

                                    _traceService.Info(@$"Target database has been commited after running ""{versionName}"" version scripts.");
                                }
                                catch (Exception)
                                {
                                    _traceService.Error(@$"Target database will be rolled back to the state before running ""{versionName}"" version scripts.");
                                    transaction.Rollback();
                                    throw;
                                }
                            }
                        }
                        else
                        {
                            if (null == transaction)
                                _traceService.Warn("Transaction is disabled for current session. This version migration run will be executed without explicit transaction context.");

                            //run scripts without transaction
                            //scriptSubDirectories is the child directories under _transaction directory c:\temp\vxx.xx\list_of_directories
                            //versionDirectory path of version c:\temp\vxx.xx
                            RunVersionDirectoriesInternal(connection, transaction, scriptSubDirectories, version.Version.Path, version.Version.Path, stopwatch);
                        }

                    }

                    //reset duration timer
                    stopwatch.Restart();
                });
            }
            else
            {
                var connectionInfo = _dataService.GetConnectionInfo();
                _traceService.Info($"Target database is updated. No migration step executed at {connectionInfo.Database} on {connectionInfo.DataSource}.");
            }

            void RunVersionDirectoriesInternal(IDbConnection connection, IDbTransaction transaction, List<string> scriptSubDirectories, string scriptDirectory, string versionDirectory, Stopwatch stopwatch)
            {
                try
                {
                    var versionName = new DirectoryInfo(versionDirectory).Name;
                    scriptSubDirectories.Sort();
                    scriptSubDirectories.ForEach(scriptSubDirectory =>
                    {
                        //run all scripts in the current version folder
                        RunVersionSqlScripts(connection, transaction, transactionContext, stopwatch, versionName, workspace, scriptSubDirectory, metaSchemaName, metaTableName, tokens, commandTimeout, environment, appliedByTool, appliedByToolVersion);

                        //import csv files into tables of the the same filename as the csv
                        RunBulkImportScripts(connection, transaction, workspace, scriptSubDirectory, bulkSeparator, bulkBatchSize, commandTimeout, environment, tokens);
                    });

                    //run all scripts in the current version folder
                    RunVersionSqlScripts(connection, transaction, transactionContext, stopwatch, versionName, workspace, scriptDirectory, metaSchemaName, metaTableName, tokens, commandTimeout, environment);

                    //import csv files into tables of the the same filename as the csv
                    RunBulkImportScripts(connection, transaction, workspace, scriptDirectory, bulkSeparator, bulkBatchSize, commandTimeout, environment, tokens);

                    //update db version
                    stopwatch.Stop();
                    _metadataService.InsertVersion(connection, transaction, versionName, transactionContext,
                        metaSchemaName: metaSchemaName,
                        metaTableName: metaTableName,
                        commandTimeout: commandTimeout,
                        appliedByTool: appliedByTool,
                        appliedByToolVersion: appliedByToolVersion,
                        durationMs: Convert.ToInt32(stopwatch.ElapsedMilliseconds));

                    _traceService.Info($"Completed migration to version {versionDirectory} in {Convert.ToInt32(stopwatch.ElapsedMilliseconds)} ms");
                }
                finally
                {
                    //clear nontransactional context to ensure it is not applied on next version
                    transactionContext = null;
                }
            }
        }

        /// <inheritdoc />
        public void RunNonVersionDirectories(
            IDbConnection connection,
            IDbTransaction transaction,
            string workspace,
            string scriptDirectory,
            List<KeyValuePair<string, string>> tokens = null,
            int? bulkBatchSize = null,
            string bulkSeparator = null,
            int? commandTimeout = null,
            string environment = null,
            string transactionMode = null,
            bool isRequiredClearedDraft = false
        )
        {
            if (!string.IsNullOrEmpty(transactionMode) && transactionMode.Equals(TRANSACTION_MODE.VERSION))
            {
                using (var internalConnection = _dataService.CreateConnection())
                {
                    internalConnection.Open();
                    using (var internalTransaction = internalConnection.BeginTransaction())
                    {
                        try
                        {
                            if (null != internalTransaction)
                                _traceService.Info("Transaction created for current version. This version migration run will be executed in this dedicated connection and transaction context.");

                            //run all scripts in the current non-version folder
                            RunNonVersionSqlScripts(internalConnection, internalTransaction, scriptDirectory, tokens, environment, commandTimeout, isRequiredClearedDraft);

                            //import csv files into tables of the the same filename as the csv
                            RunBulkImportScripts(internalConnection, internalTransaction, workspace, scriptDirectory, bulkSeparator, bulkBatchSize, commandTimeout, environment, tokens);

                            internalTransaction.Commit();
                        }
                        catch (Exception)
                        {
                            internalTransaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            else
            {
                //run all scripts in the current non-version folder
                RunNonVersionSqlScripts(connection, transaction, scriptDirectory, tokens, environment, commandTimeout, isRequiredClearedDraft);

                //import csv files into tables of the the same filename as the csv
                RunBulkImportScripts(connection, transaction, workspace, scriptDirectory, bulkSeparator, bulkBatchSize, commandTimeout, environment, tokens);

            }
        }
    }
}
