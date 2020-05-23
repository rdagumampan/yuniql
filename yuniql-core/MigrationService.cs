using Yuniql.Extensibility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;

namespace Yuniql.Core
{
    /// <inheritdoc />
    public class MigrationService : IMigrationService
    {
        private readonly ILocalVersionService _localVersionService;
        private readonly IDataService _dataService;
        private readonly IBulkImportService _bulkImportService;
        private readonly ITokenReplacementService _tokenReplacementService;
        private readonly IDirectoryService _directoryService;
        private readonly IFileService _fileService;
        private readonly ITraceService _traceService;
        private readonly IConfigurationDataService _configurationDataService;

        /// <inheritdoc />
        public MigrationService(
            ILocalVersionService localVersionService,
            IDataService dataService,
            IBulkImportService bulkImportService,
            IConfigurationDataService configurationDataService,
            ITokenReplacementService tokenReplacementService,
            IDirectoryService directoryService,
            IFileService fileService,
            ITraceService traceService)
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
        public void Initialize(
            string connectionString,
            int? commandTimeout = null)
        {
            //initialize dependencies
            _dataService.Initialize(connectionString);
            _bulkImportService.Initialize(connectionString);
        }

        /// <inheritdoc />
        public string GetCurrentVersion(string schemaName = null, string tableName = null)
        {
            return _configurationDataService.GetCurrentVersion(schemaName, tableName);
        }

        /// <inheritdoc />
        public List<DbVersion> GetAllVersions(string schemaName = null, string tableName = null)
        {
            return _configurationDataService.GetAllVersions(schemaName, tableName);
        }

        /// <inheritdoc />
        public void Run(
            string workingPath,
            string targetVersion = null,
            bool? autoCreateDatabase = false,
            List<KeyValuePair<string, string>> tokenKeyPairs = null,
            bool? verifyOnly = false,
            string bulkSeparator = null,
            string schemaName = null,
            string tableName = null,
            int? commandTimeout = null,
            int? bulkBatchSize = null,
            string appliedByTool = null,
            string appliedByToolVersion = null,
            string environmentCode = null,
            NonTransactionalResolvingOption? resumeFromFailure = null
         )
        {
            //check the workspace structure if required directories are present
            _localVersionService.Validate(workingPath);

            //when uncomitted run is not supported, fail migration, throw exceptions and return error exit code
            if (verifyOnly.HasValue && verifyOnly == true && !_dataService.IsAtomicDDLSupported)
            {
                throw new NotSupportedException("Yuniql.Verify is not supported in the target platform. " +
                    "The feature requires support for atomic DDL operations. " +
                    "An atomic DDL operations ensures creation of tables, views and other objects and data are rolledback in case of error. " +
                    "For more information see https://yuniql.io/docs/.");
            }

            //when no target version specified, we use the latest local version available
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
                //we only check if the db exists when --auto-create-db is true
                var targetDatabaseExists = _configurationDataService.IsDatabaseExists();
                if (!targetDatabaseExists)
                {
                    _traceService.Info($"Target database does not exist. Creating database {targetDatabaseName} on {targetDatabaseServer}.");
                    _configurationDataService.CreateDatabase();
                    _traceService.Info($"Created database {targetDatabaseName} on {targetDatabaseServer}.");
                }
            }

            //check if database has been pre-configured to support migration and setup when its not
            var targetDatabaseConfigured = _configurationDataService.IsDatabaseConfigured(schemaName, tableName);
            if (!targetDatabaseConfigured)
            {
                //create custom schema when user supplied and only if platform supports it
                if (_dataService.IsSchemaSupported && null != schemaName && !_dataService.SchemaName.Equals(schemaName))
                {
                    _traceService.Info($"Target schema does not exist. Creating schema {schemaName} on {targetDatabaseName} on {targetDatabaseServer}.");
                    _configurationDataService.CreateSchema(schemaName);
                    _traceService.Info($"Created schema {schemaName} on {targetDatabaseName} on {targetDatabaseServer}.");
                }

                //create empty versions tracking table
                _traceService.Info($"Target database {targetDatabaseName} on {targetDatabaseServer} not yet configured for migration.");
                _configurationDataService.ConfigureDatabase(schemaName, tableName);
                _traceService.Info($"Configured database migration support for {targetDatabaseName} on {targetDatabaseServer}.");
            }

            var allVersions = _configurationDataService.GetAllVersions(schemaName, tableName)
                .Select(dv => dv.Version)
                .OrderBy(v => v)
                .ToList();

            //check if target database already runs the latest version and skips work if it already is
            var targeDatabaseLatest = IsTargetDatabaseLatest(targetVersion, schemaName, tableName);
            if (!targeDatabaseLatest)
            {
                //enclose all executions in a single transaction, in the event of failure we roll back everything
                using (var connection = _dataService.CreateConnection())
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            //run all migrations present in all directories
                            RunAllInternal(connection, transaction);

                            //when true, the execution is an uncommitted transaction 
                            //and only for purpose of testing if all can go well when it run to the target environment
                            if (verifyOnly.HasValue && verifyOnly == true)
                                transaction.Rollback();
                            else
                                transaction.Commit();
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
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
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            //run all scripts present in the _pre, _draft and _post directories
                            RunDraftInternal(connection, transaction);

                            //when true, the execution is an uncommitted transaction 
                            //and only for purpose of testing if all can go well when it run to the target environment
                            if (verifyOnly.HasValue && verifyOnly == true)
                                transaction.Rollback();
                            else
                                transaction.Commit();
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
                _traceService.Info($"Target database runs the latest version already. Scripts in _pre, _draft and _post are executed.");
            }

            //local method
            void RunAllInternal(IDbConnection connection, IDbTransaction transaction)
            {
                //check if database has been pre-configured and execute init scripts
                if (!targetDatabaseConfigured)
                {
                    //runs all scripts in the _init folder
                    RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_init"), tokenKeyPairs, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environmentCode: environmentCode);
                    _traceService.Info($"Executed script files on {Path.Combine(workingPath, "_init")}");
                }

                //checks if target database already runs the latest version and skips work if it already is
                //runs all scripts in the _pre folder and subfolders
                RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_pre"), tokenKeyPairs, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environmentCode: environmentCode);
                _traceService.Info($"Executed script files on {Path.Combine(workingPath, "_pre")}");

                //runs all scripts int the vxx.xx folders and subfolders
                RunVersionScripts(connection, transaction, allVersions, workingPath, targetVersion, tokenKeyPairs, bulkSeparator: bulkSeparator, schemaName: schemaName, tableName: tableName, commandTimeout: commandTimeout, bulkBatchSize: bulkBatchSize, appliedByTool: appliedByTool, appliedByToolVersion: appliedByToolVersion, environmentCode: environmentCode);

                //runs all scripts in the _draft folder and subfolders
                RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_draft"), tokenKeyPairs, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environmentCode: environmentCode);
                _traceService.Info($"Executed script files on {Path.Combine(workingPath, "_draft")}");

                //runs all scripts in the _post folder and subfolders
                RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_post"), tokenKeyPairs, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environmentCode: environmentCode);
                _traceService.Info($"Executed script files on {Path.Combine(workingPath, "_post")}");
            }

            //local method
            void RunDraftInternal(IDbConnection connection, IDbTransaction transaction)
            {
                //runs all scripts in the _pre folder and subfolders
                RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_pre"), tokenKeyPairs, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environmentCode: environmentCode);
                _traceService.Info($"Executed script files on {Path.Combine(workingPath, "_pre")}");

                //runs all scripts in the _draft folder and subfolders
                RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_draft"), tokenKeyPairs, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environmentCode: environmentCode);
                _traceService.Info($"Executed script files on {Path.Combine(workingPath, "_draft")}");

                //runs all scripts in the _post folder and subfolders
                RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_post"), tokenKeyPairs, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environmentCode: environmentCode);
                _traceService.Info($"Executed script files on {Path.Combine(workingPath, "_post")}");
            }
        }

        private bool IsTargetDatabaseLatest(string targetVersion, string schemaName = null, string tableName = null)
        {
            //get the current version stored in database
            var remoteCurrentVersion = _configurationDataService.GetCurrentVersion(schemaName, tableName);
            if (string.IsNullOrEmpty(remoteCurrentVersion)) return false;

            //compare version applied in db vs versions available locally
            var localCurrentVersion = new LocalVersion(remoteCurrentVersion);
            var localTargetVersion = new LocalVersion(targetVersion);
            return string.Compare(localCurrentVersion.SemVersion, localTargetVersion.SemVersion) == 1 || //db has more updated than local version
                string.Compare(localCurrentVersion.SemVersion, localTargetVersion.SemVersion) == 0;      //db has the same version as local version
        }

        private void RunNonVersionScripts(
            IDbConnection connection,
            IDbTransaction transaction,
            string workingPath,
            List<KeyValuePair<string, string>> tokenKeyPairs = null,
            string bulkSeparator = null,
            int? commandTimeout = null,
            string environmentCode = null
        )
        {
            //extract and filter out scripts when environment code is used
            var sqlScriptFiles = _directoryService.GetAllFiles(workingPath, "*.sql").ToList();
            sqlScriptFiles = _directoryService.FilterFiles(workingPath, environmentCode, sqlScriptFiles).ToList();
            _traceService.Info($"Found the {sqlScriptFiles.Count} script files on {workingPath}");
            _traceService.Info($"{string.Join(@"\r\n\t", sqlScriptFiles.Select(s => new FileInfo(s).Name))}");

            //execute all script files in the target folder
            sqlScriptFiles.Sort();
            sqlScriptFiles.ForEach(scriptFile =>
            {
                var sqlStatementRaw = _fileService.ReadAllText(scriptFile);
                var sqlStatements = _dataService.BreakStatements(sqlStatementRaw)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();

                sqlStatements.ForEach(sqlStatement =>
                {
                    //replace tokens with values from the cli
                    sqlStatement = _tokenReplacementService.Replace(tokenKeyPairs, sqlStatement);

                    _traceService.Debug($"Executing sql statement as part of : {scriptFile}{Environment.NewLine}{sqlStatement}");
                    _configurationDataService.ExecuteSql(
                        connection: connection,
                        commandText: sqlStatement,
                        transaction: transaction,
                        commandTimeout: commandTimeout,
                        traceService: _traceService);
                });

                _traceService.Info($"Executed script file {scriptFile}.");
            });
        }

        private void RunVersionScripts(
            IDbConnection connection,
            IDbTransaction transaction,
            List<string> dbVersions,
            string workingPath,
            string targetVersion,
            List<KeyValuePair<string, string>> tokenKeyPairs = null,
            string bulkSeparator = null,
            string schemaName = null,
            string tableName = null,
            int? commandTimeout = null,
            int? bulkBatchSize = null,
            string appliedByTool = null,
            string appliedByToolVersion = null,
            string environmentCode = null
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
                    try
                    {
                        //run scripts in all sub-directories
                        var scriptSubDirectories = _directoryService.GetAllDirectories(versionDirectory, "*").ToList();
                        scriptSubDirectories.Sort();
                        scriptSubDirectories.ForEach(scriptSubDirectory =>
                        {
                            //run all scripts in the current version folder
                            RunSqlScripts(connection, transaction, workingPath, scriptSubDirectory, schemaName, tableName, tokenKeyPairs, commandTimeout, environmentCode);

                            //import csv files into tables of the the same filename as the csv
                            RunBulkImport(connection, transaction, workingPath, scriptSubDirectory, bulkSeparator, bulkBatchSize, commandTimeout, environmentCode);
                        });

                        //run all scripts in the current version folder
                        RunSqlScripts(connection, transaction, workingPath, versionDirectory, schemaName, tableName, tokenKeyPairs, commandTimeout, environmentCode);

                        //import csv files into tables of the the same filename as the csv
                        RunBulkImport(connection, transaction, workingPath, versionDirectory, bulkSeparator, bulkBatchSize, commandTimeout, environmentCode);

                        //update db version
                        var versionName = new DirectoryInfo(versionDirectory).Name;
                        _configurationDataService.InsertVersion(connection, transaction, versionName,
                            schemaName: schemaName,
                            tableName: tableName,
                            commandTimeout: commandTimeout,
                            appliedByTool: appliedByTool,
                            appliedByToolVersion: appliedByToolVersion);

                        _traceService.Info($"Completed migration to version {versionDirectory}");
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                });
            }
            else
            {
                var connectionInfo = _dataService.GetConnectionInfo();
                _traceService.Info($"Target database is updated. No migration step executed at {connectionInfo.Database} on {connectionInfo.DataSource}.");
            }
        }

        private void RunBulkImport(
            IDbConnection connection,
            IDbTransaction transaction,
            string workingPath,
            string scriptDirectory,
            string bulkSeparator = null,
            int? bulkBatchSize = null,
            int? commandTimeout = null,
            string environmentCode = null
        )
        {
            //extract and filter out scripts when environment code is used
            var bulkFiles = _directoryService.GetFiles(scriptDirectory, "*.csv").ToList();
            bulkFiles = _directoryService.FilterFiles(workingPath, environmentCode, bulkFiles).ToList();
            _traceService.Info($"Found the {bulkFiles.Count} bulk files on {scriptDirectory}");
            _traceService.Info($"{string.Join(@"\r\n\t", bulkFiles.Select(s => new FileInfo(s).Name))}");

            bulkFiles.Sort();
            bulkFiles.ForEach(csvFile =>
            {
                _bulkImportService.Run(connection, transaction, csvFile, bulkSeparator, bulkBatchSize: bulkBatchSize, commandTimeout: commandTimeout);
                _traceService.Info($"Imported bulk file {csvFile}.");
            });
        }

        private void RunSqlScripts(
            IDbConnection connection,
            IDbTransaction transaction,
            string workingPath,
            string scriptDirectory,
            string schemaName,
            string tableName,
            List<KeyValuePair<string, string>> tokenKeyPairs = null,
            int? commandTimeout = null,
            string environmentCode = null,
            string appliedByTool = null,
            string appliedByToolVersion = null
        )
        {
            //extract and filter out scripts when environment code is used
            var sqlScriptFiles = _directoryService.GetFiles(scriptDirectory, "*.sql").ToList();
            sqlScriptFiles = _directoryService.FilterFiles(workingPath, environmentCode, sqlScriptFiles).ToList();
            _traceService.Info($"Found the {sqlScriptFiles.Count} script files on {scriptDirectory}");
            _traceService.Info($"{string.Join(@"\r\n\t", sqlScriptFiles.Select(s => new FileInfo(s).Name))}");

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
                        sqlStatement = _tokenReplacementService.Replace(tokenKeyPairs, sqlStatement);

                        _traceService.Debug($"Executing sql statement as part of : {scriptFile}{Environment.NewLine}{sqlStatement}");
                        _configurationDataService.ExecuteSql(
                            connection: connection,
                            commandText: sqlStatement,
                            transaction: transaction,
                            commandTimeout: commandTimeout,
                            traceService: _traceService);
                    });

                    _traceService.Info($"Executed script file {scriptFile}.");
                });
        }

        /// <inheritdoc />
        public void Erase(
            string workingPath,
            List<KeyValuePair<string, string>> tokenKeyPairs = null,
            int? commandTimeout = null,
            string environmentCode = null
        )
        {
            //create a shared open connection to entire migration run
            using (var connection = _dataService.CreateConnection())
            {
                connection.KeepOpen();

                //enclose all executions in a single transaction in case platform supports it
                _traceService.Info(@$"Target platform fully supports transactions. Migration will run in single transaction.");
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        //runs all scripts in the _erase folder
                        RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_erase"), tokenKeyPairs: tokenKeyPairs, bulkSeparator: DEFAULT_CONSTANTS.BULK_SEPARATOR, commandTimeout: commandTimeout, environmentCode: environmentCode);
                        _traceService.Info($"Executed script files on {Path.Combine(workingPath, "_erase")}");

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
