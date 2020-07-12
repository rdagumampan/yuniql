using Yuniql.Extensibility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;

namespace Yuniql.Core
{
    ///<inheritdoc/>
    public class NonTransactionalMigrationService : MigrationServiceBase
    {
        private readonly ILocalVersionService _localVersionService;
        private readonly IDataService _dataService;
        private readonly IBulkImportService _bulkImportService;
        private readonly ITokenReplacementService _tokenReplacementService;
        private readonly IDirectoryService _directoryService;
        private readonly IFileService _fileService;
        private readonly ITraceService _traceService;
        private readonly IConfigurationDataService _configurationDataService;

        ///<inheritdoc/>
        public NonTransactionalMigrationService(
            ILocalVersionService localVersionService,
            IDataService dataService,
            IBulkImportService bulkImportService,
            IConfigurationDataService configurationDataService,
            ITokenReplacementService tokenReplacementService,
            IDirectoryService directoryService,
            IFileService fileService,
            ITraceService traceService)
            : base (
                localVersionService, 
                dataService, 
                bulkImportService, 
                configurationDataService, 
                tokenReplacementService, 
                directoryService, 
                fileService, 
                traceService
            )
        {
            this._localVersionService = localVersionService;
            this._dataService = dataService;
            this._bulkImportService = bulkImportService;
            this._tokenReplacementService = tokenReplacementService;
            this._directoryService = directoryService;
            this._fileService = fileService;
            this._traceService = traceService;
            this._configurationDataService = configurationDataService;
        } 

        /// <inheritdoc />
        public override void Run(
            string workingPath,
            string targetVersion = null,
            bool? autoCreateDatabase = false,
            List<KeyValuePair<string, string>> tokenKeyPairs = null,
            bool? verifyOnly = false,
            string bulkSeparator = null,
            string metaSchemaName = null,
            string metaTableName = null,
            int? commandTimeout = null,
            int? bulkBatchSize = null,
            string appliedByTool = null,
            string appliedByToolVersion = null,
            string environmentCode = null,
            NonTransactionalResolvingOption? nonTransactionalResolvingOption = null,
            string transactionMode = null
         )
        {
            if (_dataService.IsTransactionalDdlSupported && nonTransactionalResolvingOption != null)
            {
                throw new NotSupportedException(@$"The non-transactional failure resolving option ""{nonTransactionalResolvingOption}"" is not available for this platform.");
            }

            //check the workspace structure if required directories are present
            _localVersionService.Validate(workingPath);

            //when uncomitted run is not supported, fail migration, throw exceptions and return error exit code
            if (verifyOnly.HasValue && verifyOnly == true && !_dataService.IsTransactionalDdlSupported)
            {
                throw new NotSupportedException("Yuniql.Verify is not supported in the target platform. " +
                    "The feature requires support for atomic DDL operations. " +
                    "An atomic DDL operations ensures creation of tables, views and other objects and data are rolledback in case of error. " +
                    "For more information see https://yuniql.io/docs/.");
            }

            //when no target version specified, we use the latest local version 
            if (string.IsNullOrEmpty(targetVersion))
            {
                targetVersion = _localVersionService.GetLatestVersion(workingPath);
                _traceService.Info($"No explicit target version requested. We'll use latest available locally {targetVersion} on {workingPath}.");
            }

            var connectionInfo = _dataService.GetConnectionInfo();
            var targetDatabaseName = connectionInfo.Database;
            var targetDatabaseServer = connectionInfo.DataSource;

            //we try to auto-create the database, we need this to be outside of the transaction scope
            //in an event of failure, users have to manually drop the auto-created database!
            //we only check if the db exists when --auto-create-db is true
            if (autoCreateDatabase.HasValue && autoCreateDatabase == true)
            {
                var targetDatabaseExists = _configurationDataService.IsDatabaseExists();
                if (!targetDatabaseExists)
                {
                    _traceService.Info($"Target database does not exist. Creating database {targetDatabaseName} on {targetDatabaseServer}.");
                    _configurationDataService.CreateDatabase();
                    _traceService.Info($"Created database {targetDatabaseName} on {targetDatabaseServer}.");
                }
            }

            //check if database has been pre-configured to support migration and setup when its not
            var targetDatabaseConfigured = _configurationDataService.IsDatabaseConfigured(metaSchemaName, metaTableName);
            if (!targetDatabaseConfigured)
            {
                //create custom schema when user supplied and only if platform supports it
                if (_dataService.IsSchemaSupported && null != metaSchemaName && !_dataService.SchemaName.Equals(metaSchemaName))
                {
                    _traceService.Info($"Target schema does not exist. Creating schema {metaSchemaName} on {targetDatabaseName} on {targetDatabaseServer}.");
                    _configurationDataService.CreateSchema(metaSchemaName);
                    _traceService.Info($"Created schema {metaSchemaName} on {targetDatabaseName} on {targetDatabaseServer}.");
                }

                //create empty versions tracking table
                _traceService.Info($"Target database {targetDatabaseName} on {targetDatabaseServer} not yet configured for migration.");
                _configurationDataService.ConfigureDatabase(metaSchemaName, metaTableName);
                _traceService.Info($"Configured database migration support for {targetDatabaseName} on {targetDatabaseServer}.");
            }

            var targetDatabaseUpdated = _configurationDataService.UpdateDatabaseConfiguration(metaSchemaName, metaTableName);
            if (targetDatabaseUpdated)
                _traceService.Info($"The configuration of migration has been updated for {targetDatabaseName} on {targetDatabaseServer}.");
            else
                _traceService.Debug($"The configuration of migration is up to date for {targetDatabaseName} on {targetDatabaseServer}.");

            NonTransactionalContext nonTransactionalContext = null;

            //check for presence of failed no-transactional versions from previous runs
            var allVersions = _configurationDataService.GetAllVersions(metaSchemaName, metaTableName);
            var failedVersion = allVersions.Where(x => x.Status == Status.Failed).FirstOrDefault();
            if (failedVersion != null)
            {
                //check if user had issue resolving option such as continue on failure
                if (nonTransactionalResolvingOption == null)
                {
                    //program should exit with non zero exit code
                    var message = @$"Previous migration of ""{failedVersion.Version}"" version was not running in transaction and has failed when executing of script ""{failedVersion.FailedScriptPath}"" with following error: {failedVersion.FailedScriptError} {MESSAGES.ManualResolvingAfterFailureMessage}";
                    _traceService.Error(message);
                    throw new InvalidOperationException(message);
                }

                _traceService.Info($@"The non-transactional failure resolving option ""{nonTransactionalResolvingOption}"" was used. Version scripts already applied by previous migration run will be skipped.");
                nonTransactionalContext = new NonTransactionalContext(failedVersion, nonTransactionalResolvingOption.Value);
            }
            else
            {
                if (nonTransactionalResolvingOption != null)
                {
                    //program should exit with non zero exit code
                    _traceService.Error(@$"The non-transactional failure resolving option ""{nonTransactionalResolvingOption}"" is available only if previous migration has failed.");
                    throw new InvalidOperationException();
                }
            }

            var appliedVersions = _configurationDataService.GetAllAppliedVersions(metaSchemaName, metaTableName)
                .Select(dv => dv.Version)
                .OrderBy(v => v)
                .ToList();

            //checks if target database already runs the latest version and skips work if it already is
            var targeDatabaseLatest = IsTargetDatabaseLatest(targetVersion, metaSchemaName, metaTableName);
            if (!targeDatabaseLatest)
            {
                //create a shared open connection to entire migration run
                using (var connection = _dataService.CreateConnection())
                {
                    connection.Open();
                    RunAllInternal(connection, null);
                }
            }
            else
            {
                //runs all scripts files inside _draft on every yuniql run regardless of state of target database
                using (var connection = _dataService.CreateConnection())
                {
                    connection.Open();
                    RunDraftInternal(connection, null);
                }
            }

            //local method
            void RunAllInternal(IDbConnection connection, IDbTransaction transaction)
            {
                //check if database has been pre-configured and execute init scripts
                if (!targetDatabaseConfigured)
                {
                    //runs all scripts in the _init folder
                    RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_init"), tokenKeyPairs, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environmentCode: environmentCode, transactionMode: transactionMode);
                    _traceService.Info($"Executed script files on {Path.Combine(workingPath, "_init")}");
                }

                //checks if target database already runs the latest version and skips work if it already is
                //runs all scripts in the _pre folder and subfolders
                RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_pre"), tokenKeyPairs, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environmentCode: environmentCode, transactionMode: transactionMode);
                _traceService.Info($"Executed script files on {Path.Combine(workingPath, "_pre")}");

                //runs all scripts int the vxx.xx folders and subfolders
                RunVersionScripts(connection, transaction, appliedVersions, workingPath, targetVersion, nonTransactionalContext, tokenKeyPairs, bulkSeparator: bulkSeparator, metaSchemaName: metaSchemaName, metaTableName: metaTableName, commandTimeout: commandTimeout, bulkBatchSize: bulkBatchSize, appliedByTool: appliedByTool, appliedByToolVersion: appliedByToolVersion, environmentCode: environmentCode, transactionMode: transactionMode);

                //runs all scripts in the _draft folder and subfolders
                RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_draft"), tokenKeyPairs, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environmentCode: environmentCode, transactionMode: transactionMode);
                _traceService.Info($"Executed script files on {Path.Combine(workingPath, "_draft")}");

                //runs all scripts in the _post folder and subfolders
                RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_post"), tokenKeyPairs, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environmentCode: environmentCode, transactionMode: transactionMode);
                _traceService.Info($"Executed script files on {Path.Combine(workingPath, "_post")}");
            }

            //local method
            void RunDraftInternal(IDbConnection connection, IDbTransaction transaction)
            {
                //runs all scripts in the _pre folder and subfolders
                RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_pre"), tokenKeyPairs, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environmentCode: environmentCode, transactionMode: transactionMode);
                _traceService.Info($"Executed script files on {Path.Combine(workingPath, "_pre")}");

                //runs all scripts in the _draft folder and subfolders
                RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_draft"), tokenKeyPairs, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environmentCode: environmentCode, transactionMode: transactionMode);
                _traceService.Info($"Executed script files on {Path.Combine(workingPath, "_draft")}");

                //runs all scripts in the _post folder and subfolders
                RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_post"), tokenKeyPairs, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environmentCode: environmentCode, transactionMode: transactionMode);
                _traceService.Info($"Executed script files on {Path.Combine(workingPath, "_post")}");
            }
        }

        /// <inheritdoc />
        public override void RunVersionScripts(
            IDbConnection connection,
            IDbTransaction transaction,
            List<string> dbVersions,
            string workingPath,
            string targetVersion,
            NonTransactionalContext nonTransactionalContext,
            List<KeyValuePair<string, string>> tokenKeyPairs = null,
            string bulkSeparator = null,
            string metaSchemaName = null,
            string metaTableName = null,
            int? commandTimeout = null,
            int? bulkBatchSize = null,
            string appliedByTool = null,
            string appliedByToolVersion = null,
            string environmentCode = null,
            string transactionMode = null
        )
        {
            //excludes all versions already executed
            var versionDirectories = _directoryService.GetDirectories(workingPath, "v*.*")
                .Where(v => !dbVersions.Contains(new DirectoryInfo(v).Name))
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
                    //run scripts in all sub-directories
                    var scriptSubDirectories = _directoryService.GetAllDirectories(versionDirectory, "*").ToList(); ;

                    //check for transaction directory
                    var isExplicitTransactionDefined = false;
                    var transactionDirectory = Path.Combine(versionDirectory, "_transaction");
                    if (_directoryService.Exists(transactionDirectory))
                    {
                        //version directory with _transaction directory only applies to platforms supporting transactional ddl
                        if (_dataService.IsTransactionalDdlSupported)
                        {
                            throw new YuniqlMigrationException(@$"The version directory ""{versionDirectory}"" can't contain ""_transaction"" subdirectory for selected target platform, because the whole migration is already running in single transaction.");
                        }

                        //version directory must only contain _transaction directory
                        if (_directoryService.GetDirectories(versionDirectory, "*").Count() > 1)
                        {
                            throw new YuniqlMigrationException(@$"The version directory ""{versionDirectory}"" containing ""_transaction"" subdirectory can't contain other subdirectories");
                        }

                        //version directory must only contain _transaction directory, files are also not allowed
                        //users need to place the script files and subdirectories inside _transaction directory
                        if (_directoryService.GetFiles(versionDirectory, "*.*").Count() > 0)
                        {
                            throw new YuniqlMigrationException(@$"The version directory ""{versionDirectory}"" containing ""_transaction"" subdirectory can't contain files");
                        }

                        isExplicitTransactionDefined = true;
                        scriptSubDirectories = _directoryService.GetAllDirectories(transactionDirectory, "*").ToList();
                    }

                    if (isExplicitTransactionDefined)
                    {
                        //run scripts within a single transaction
                        string versionName = new DirectoryInfo(versionDirectory).Name;
                        _traceService.Info(@$"The ""_transaction"" directory has been detected and therefore ""{versionName}"" version scripts will run in single transaction. The rollback will not be reliable in case the version scripts contain commands causing implicit commit (e.g. DDL)!");

                        using (var transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                RunVersionScriptsInternal(transaction, scriptSubDirectories, transactionDirectory, versionDirectory, metaSchemaName, metaTableName);
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
                        //run scripts without transaction
                        RunVersionScriptsInternal(transaction, scriptSubDirectories, versionDirectory, versionDirectory, metaSchemaName, metaTableName);
                    }
                });
            }
            else
            {
                var connectionInfo = _dataService.GetConnectionInfo();
                _traceService.Info($"Target database is updated. No migration step executed at {connectionInfo.Database} on {connectionInfo.DataSource}.");
            }

            //local method
            void RunVersionScriptsInternal(IDbTransaction transaction, List<string> scriptSubDirectories, string scriptDirectory, string versionDirectory, string schemaName, string tableName)
            {
                try
                {
                    var versionName = new DirectoryInfo(versionDirectory).Name;
                    scriptSubDirectories.Sort();
                    scriptSubDirectories.ForEach(scriptSubDirectory =>
                    {
                        //run all scripts in the current version folder
                        RunSqlScripts(connection, transaction, nonTransactionalContext, versionName, workingPath, scriptSubDirectory, schemaName, tableName, tokenKeyPairs, commandTimeout, environmentCode, appliedByTool, appliedByToolVersion);

                        //import csv files into tables of the the same filename as the csv
                        RunBulkImport(connection, transaction, workingPath, scriptSubDirectory, bulkSeparator, bulkBatchSize, commandTimeout, environmentCode);
                    });

                    //run all scripts in the current version folder
                    RunSqlScripts(connection, transaction, nonTransactionalContext, versionName, workingPath, scriptDirectory, schemaName, tableName, tokenKeyPairs, commandTimeout, environmentCode, appliedByTool, appliedByToolVersion);

                    //import csv files into tables of the the same filename as the csv
                    RunBulkImport(connection, transaction, workingPath, scriptDirectory, bulkSeparator, bulkBatchSize, commandTimeout, environmentCode);

                    //update db version
                    _configurationDataService.InsertVersion(connection, transaction, versionName,
                        metaSchemaName: schemaName,
                        metaTableName: tableName,
                        commandTimeout: commandTimeout,
                        appliedByTool: appliedByTool,
                        appliedByToolVersion: appliedByToolVersion);

                    _traceService.Info($"Completed migration to version {versionDirectory}");
                }
                finally
                {
                    //clear nontransactional context to ensure it is not applied on next version
                    nonTransactionalContext = null;
                }
            }
        }

        /// <inheritdoc />
        public override void RunSqlScripts(
            IDbConnection connection,
            IDbTransaction transaction,
            NonTransactionalContext nonTransactionalContext,
            string version,
            string workingPath,
            string scriptDirectory,
            string metaSchemaName,
            string metaTableName,
            List<KeyValuePair<string, string>> tokenKeyPairs = null,
            int? commandTimeout = null,
            string environmentCode = null,
            string appliedByTool = null,
            string appliedByToolVersion = null
        )
        {
            string currentScriptFile = null;

            try
            {
                //filter out scripts when environment code is used
                var sqlScriptFiles = _directoryService.GetFiles(scriptDirectory, "*.sql").ToList();
                sqlScriptFiles = _directoryService.FilterFiles(workingPath, environmentCode, sqlScriptFiles).ToList();
                _traceService.Info($"Found {sqlScriptFiles.Count} script files on {workingPath}" + (sqlScriptFiles.Count > 0 ? Environment.NewLine : string.Empty) +
                       $"{string.Join(Environment.NewLine, sqlScriptFiles.Select(s => "  + " + new FileInfo(s).Name))}");

                //execute all script files in the version folder, we also make sure its sorted by file name
                sqlScriptFiles.Sort();
                sqlScriptFiles.ForEach(scriptFile =>
                {
                    currentScriptFile = scriptFile;

                    //in case the non-transactional failure is resolved, skip scripts
                    if (nonTransactionalContext?.ResolvingOption == NonTransactionalResolvingOption.ContinueAfterFailure
                        && !nonTransactionalContext.IsFailedScriptPathMatched)
                    {
                        //set failed script file as matched
                        if (string.Equals(scriptFile, nonTransactionalContext.FailedScriptPath, StringComparison.InvariantCultureIgnoreCase))
                        {
                            nonTransactionalContext.SetFailedScriptPathMatch();
                        }
                        _traceService.Info($"Skipping script file {scriptFile} ...");
                    }
                    else //otherwise execute them
                    {
                        var sqlStatementRaw = _fileService.ReadAllText(scriptFile);
                        var sqlStatements = _dataService.BreakStatements(sqlStatementRaw)
                            .Where(s => !string.IsNullOrWhiteSpace(s))
                            .ToList();

                        sqlStatements.ForEach(sqlStatement =>
                        {
                            sqlStatement = _tokenReplacementService.Replace(tokenKeyPairs, sqlStatement);
                            _traceService.Debug($"Executing sql statement as part of : {scriptFile}");

                            _configurationDataService.ExecuteSql(
                                connection: connection,
                                commandText: sqlStatement,
                                transaction: transaction,
                                commandTimeout: commandTimeout,
                                traceService: _traceService);
                        });

                        _traceService.Info($"Executed script file {scriptFile}.");
                    }
                });
            }
            catch (Exception exception)
            {
                //try parse the known sql error
                if (!_dataService.TryParseErrorFromException(exception, out string sqlError))
                {
                    //if not sucesfull, use the whole exception
                    sqlError = exception.ToString();
                }

                //in case scripts are not executed within transaction, mark version as failed in database
                if (transaction == null)
                {
                    _configurationDataService.InsertVersion(connection, transaction, version,
                        metaSchemaName: metaSchemaName,
                        metaTableName: metaTableName,
                        commandTimeout: commandTimeout,
                        appliedByTool: appliedByTool,
                        appliedByToolVersion: appliedByToolVersion,                        
                        failedScriptPath: currentScriptFile,
                        failedScriptError: sqlError);

                    _traceService.Error(@$"Migration of ""{version}"" version was not running in transaction and has failed when executing of script file ""{currentScriptFile}"" with following error: {sqlError} {MESSAGES.ManualResolvingAfterFailureMessage}");
                }
                else
                {
                    _traceService.Error(@$"Migration of ""{version}"" version was running in transaction and has failed when executing of script file ""{currentScriptFile}"" with following error: {sqlError}");
                }

                throw;
            }
        }
    }
}
