using Yuniql.Extensibility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;
using System.Text.Json;

namespace Yuniql.Core
{
    /// <inheritdoc />
    public class MigrationServiceTransactional : MigrationServiceBase
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
        public MigrationServiceTransactional(
            IWorkspaceService workspaceService,
            IDataService dataService,
            IBulkImportService bulkImportService,
            IMetadataService metadataService,
            ITokenReplacementService tokenReplacementService,
            IDirectoryService directoryService,
            IFileService fileService,
            ITraceService traceService,
            IConfigurationService configurationService)
            : base(
                workspaceService,
                dataService,
                bulkImportService,
                metadataService,
                tokenReplacementService,
                directoryService,
                fileService,
                traceService,
                configurationService
            )
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
        public override void Run()
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
        public override void Run(
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
                //we only check if the db exists when --auto-create-db is true
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
                if (_dataService.IsSchemaSupported && null != metaSchemaName && !_dataService.SchemaName.Equals(metaSchemaName))
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

            var allVersions = _metadataService.GetAllVersions(metaSchemaName, metaTableName)
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
                            //run all migrations present in all directories
                            if (null != transaction)
                                _traceService.Info("Transaction created for current session. This migration run will be executed in a shared connection and transaction context.");

                            RunAllInternal(connection, transaction, isRequiredClearedDraft);

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
            }
            else
            {
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

                            RunDraftInternal(connection, transaction, isRequiredClearedDraft);

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
                    RunNonVersionScripts(connection, transaction, Path.Combine(workspace, RESERVED_DIRECTORY_NAME.INIT), tokens, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environment: environment, transactionMode: transactionMode);
                    _traceService.Info($"Executed script files on {Path.Combine(workspace, RESERVED_DIRECTORY_NAME.INIT)}");
                }

                //checks if target database already runs the latest version and skips work if it already is
                //runs all scripts in the _pre folder and subfolders
                RunNonVersionScripts(connection, transaction, Path.Combine(workspace, RESERVED_DIRECTORY_NAME.PRE), tokens, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environment: environment, transactionMode: transactionMode);
                _traceService.Info($"Executed script files on {Path.Combine(workspace, RESERVED_DIRECTORY_NAME.PRE)}");

                //runs all scripts int the vxx.xx folders and subfolders
                RunVersionScripts(connection, transaction, allVersions, workspace, targetVersion, null, tokens, bulkSeparator: bulkSeparator, metaSchemaName: metaSchemaName, metaTableName: metaTableName, commandTimeout: commandTimeout, bulkBatchSize: bulkBatchSize, appliedByTool: appliedByTool, appliedByToolVersion: appliedByToolVersion, environment: environment, transactionMode: transactionMode);

                //runs all scripts in the _draft folder and subfolders
                RunNonVersionScripts(connection, transaction, Path.Combine(workspace, RESERVED_DIRECTORY_NAME.DRAFT), tokens, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environment: environment, transactionMode: transactionMode, isRequiredClearedDraft: isRequiredClearedDraft);
                _traceService.Info($"Executed script files on {Path.Combine(workspace, RESERVED_DIRECTORY_NAME.DRAFT)}");

                //runs all scripts in the _post folder and subfolders
                RunNonVersionScripts(connection, transaction, Path.Combine(workspace, RESERVED_DIRECTORY_NAME.POST), tokens, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environment: environment, transactionMode: transactionMode);
                _traceService.Info($"Executed script files on {Path.Combine(workspace, RESERVED_DIRECTORY_NAME.POST)}");
            }

            //local method
            void RunDraftInternal(IDbConnection connection, IDbTransaction transaction, bool requiredClearedDraft)
            {
                //runs all scripts in the _pre folder and subfolders
                RunNonVersionScripts(connection, transaction, Path.Combine(workspace, RESERVED_DIRECTORY_NAME.PRE), tokens, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environment: environment, transactionMode: transactionMode);
                _traceService.Info($"Executed script files on {Path.Combine(workspace, RESERVED_DIRECTORY_NAME.PRE)}");

                //runs all scripts in the _draft folder and subfolders
                RunNonVersionScripts(connection, transaction, Path.Combine(workspace, RESERVED_DIRECTORY_NAME.DRAFT), tokens, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environment: environment, transactionMode: transactionMode, isRequiredClearedDraft: requiredClearedDraft);
                _traceService.Info($"Executed script files on {Path.Combine(workspace, RESERVED_DIRECTORY_NAME.DRAFT)}");

                //runs all scripts in the _post folder and subfolders
                RunNonVersionScripts(connection, transaction, Path.Combine(workspace, RESERVED_DIRECTORY_NAME.POST), tokens, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environment: environment, transactionMode: transactionMode);
                _traceService.Info($"Executed script files on {Path.Combine(workspace, RESERVED_DIRECTORY_NAME.POST)}");
            }
        }

        ///<inheritdoc/>
        public override void RunVersionScripts(
            IDbConnection connection,
            IDbTransaction transaction,
            List<string> versions,
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
            var versionDirectories = _directoryService.GetDirectories(workspace, "v*.*")
                .Where(v => !versions.Contains(new DirectoryInfo(v).Name))
                .ToList();

            //exclude all versions greater than the target version
            if (!string.IsNullOrEmpty(targetVersion))
            {
                versionDirectories.RemoveAll(v =>
                {
                    var cv = new LocalVersion(new DirectoryInfo(v).Name);
                    var tv = new LocalVersion(targetVersion);

                    return string.Compare(cv.SemVersion, tv.SemVersion) == 1;
                });
            }

            //execute all sql scripts in the version folders
            if (versionDirectories.Any())
            {
                versionDirectories.Sort();
                versionDirectories.ForEach(versionDirectory =>
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

                                    RunVersionScriptsInternal(internalConnection, internalTransaction, versionDirectory);
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
                        if (null == transaction)
                            _traceService.Info("Transaction is disabled for current session. This version migration run will be executed without explicit transaction context.");

                        RunVersionScriptsInternal(connection, transaction, versionDirectory);
                    }
                });
            }
            else
            {
                var connectionInfo = _dataService.GetConnectionInfo();
                _traceService.Info($"Target database is updated. No migration step executed at {connectionInfo.Database} on {connectionInfo.DataSource}.");
            }

            void RunVersionScriptsInternal(IDbConnection connection, IDbTransaction transaction, string versionDirectory)
            {
                var versionName = new DirectoryInfo(versionDirectory).Name;

                //run scripts in all sub-directories
                var scriptSubDirectories = _directoryService.GetAllDirectories(versionDirectory, "*").ToList();
                scriptSubDirectories.Sort();
                scriptSubDirectories.ForEach(scriptSubDirectory =>
                {
                    //run all scripts in the current version folder
                    RunSqlScripts(connection, transaction, transactionContext, versionName, workspace, scriptSubDirectory, metaSchemaName, metaTableName, tokens, commandTimeout, environment);

                    //import csv files into tables of the the same filename as the csv
                    RunBulkImport(connection, transaction, workspace, scriptSubDirectory, bulkSeparator, bulkBatchSize, commandTimeout, environment);
                });

                //run all scripts in the current version folder
                RunSqlScripts(connection, transaction, transactionContext, versionName, workspace, versionDirectory, metaSchemaName, metaTableName, tokens, commandTimeout, environment);

                //import csv files into tables of the the same filename as the csv
                RunBulkImport(connection, transaction, workspace, versionDirectory, bulkSeparator, bulkBatchSize, commandTimeout, environment);

                //update db version
                _metadataService.InsertVersion(connection, transaction, versionName,
                    metaSchemaName: metaSchemaName,
                    metaTableName: metaTableName,
                    commandTimeout: commandTimeout,
                    appliedByTool: appliedByTool,
                    appliedByToolVersion: appliedByToolVersion);

                _traceService.Info($"Completed migration to version {versionDirectory}");

            }
        }

        ///<inheritdoc/>
        public override void RunSqlScripts(
            IDbConnection connection,
            IDbTransaction transaction,
            TransactionContext transactionContext,
            string version,
            string workspace,
            string scriptDirectory,
            string metaSchemaName,
            string metaTableName,
            List<KeyValuePair<string, string>> tokens = null,
            int? commandTimeout = null,
            string environment = null,
            string appliedByTool = null,
            string appliedByToolVersion = null
        )
        {
            //extract and filter out scripts when environment code is used
            var sqlScriptFiles = _directoryService.GetFiles(scriptDirectory, "*.sql").ToList();
            sqlScriptFiles = _directoryService.FilterFiles(workspace, environment, sqlScriptFiles).ToList();
            _traceService.Info($"Found {sqlScriptFiles.Count} script files on {workspace}" + (sqlScriptFiles.Count > 0 ? Environment.NewLine : string.Empty) +
                   $"{string.Join(Environment.NewLine, sqlScriptFiles.Select(s => "  + " + new FileInfo(s).Name))}");

            //execute all script files in the version folder
            sqlScriptFiles.Sort();
            sqlScriptFiles
                .ForEach(scriptFile =>
                {
                    var sqlStatementRaw = _fileService.ReadAllText(scriptFile);
                    var sqlStatements = _dataService.BreakStatements(sqlStatementRaw)
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToList();
                    ;
                    sqlStatements.ForEach(sqlStatement =>
                    {
                        try
                        {
                            sqlStatement = _tokenReplacementService.Replace(tokens, sqlStatement);
                            _traceService.Debug($"Executing sql statement as part of : {scriptFile}");
                            _metadataService.ExecuteSql(
                                connection: connection,
                                commandText: sqlStatement,
                                transaction: transaction,
                                commandTimeout: commandTimeout,
                                traceService: _traceService);
                        }
                        catch (Exception)
                        {
                            _traceService.Error($"Failed to execute sql statements in script file {scriptFile}.{Environment.NewLine}" +
                                $"The failing statement starts here --------------------------{Environment.NewLine}" +
                                $"{sqlStatement} {Environment.NewLine}" +
                                $"The failing statement ends here --------------------------");
                            throw;
                        }
                    });

                    _traceService.Info($"Executed script file {scriptFile}.");
                });
        }
    }
}
