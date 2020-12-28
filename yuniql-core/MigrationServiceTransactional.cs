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
        private readonly ILocalVersionService _localVersionService;
        private readonly IDataService _dataService;
        private readonly IBulkImportService _bulkImportService;
        private readonly ITokenReplacementService _tokenReplacementService;
        private readonly IDirectoryService _directoryService;
        private readonly IFileService _fileService;
        private readonly ITraceService _traceService;
        private readonly IMetadataService _metadataService;

        /// <inheritdoc />
        public MigrationServiceTransactional(
            ILocalVersionService localVersionService,
            IDataService dataService,
            IBulkImportService bulkImportService,
            IMetadataService metadataService,
            ITokenReplacementService tokenReplacementService,
            IDirectoryService directoryService,
            IFileService fileService,
            ITraceService traceService)
            : base(
                localVersionService,
                dataService,
                bulkImportService,
                metadataService,
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
            this._metadataService = metadataService;
        }

        /// <inheritdoc />
        public override void Run(Configuration configuration)
        {
            Run(
               workingPath: configuration.WorkspacePath,
               targetVersion: configuration.TargetVersion,
               autoCreateDatabase: configuration.AutoCreateDatabase,
               tokenKeyPairs: configuration.Tokens,
               verifyOnly: configuration.VerifyOnly,
               bulkSeparator: configuration.BulkSeparator,
               metaSchemaName: configuration.MetaSchemaName,
               metaTableName: configuration.MetaTableName,
               commandTimeout: configuration.CommandTimeout,
               bulkBatchSize: configuration.BulkBatchSize,
               appliedByTool: configuration.AppliedByTool,
               appliedByToolVersion: configuration.AppliedByToolVersion,
               environmentCode: configuration.Environment,
               continueAfterFailure: configuration.ContinueAfterFailure,
               transactionMode: configuration.TransactionMode,
               requiredClearedDraft: configuration.RequiredClearedDraft
            );
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
            bool? continueAfterFailure = null,
            string transactionMode = null,
            bool requiredClearedDraft = false
         )
        {
            //print run configuration information
            var configuration = new
            {
                workingPath,
                targetVersion,
                autoCreateDatabase,
                tokenKeyPairs,
                verifyOnly,
                bulkSeparator,
                bulkBatchSize,
                metaSchemaName,
                metaTableName,
                commandTimeout,
                appliedByTool,
                appliedByToolVersion,
                environmentCode,
                continueAfterFailure,
                transactionMode,
                requiredClearedDraft
            };
            var serializedConfiguration = JsonSerializer.Serialize(configuration, new JsonSerializerOptions { WriteIndented = true });
            _traceService.Info($"Run configuration: {Environment.NewLine}{serializedConfiguration}");

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

                            RunAllInternal(connection, transaction, requiredClearedDraft);

                            //when true, the execution is an uncommitted transaction 
                            //and only for purpose of testing if all can go well when it run to the target environment
                            if (verifyOnly.HasValue && verifyOnly == true)
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

                            RunDraftInternal(connection, transaction, requiredClearedDraft);

                            //when true, the execution is an uncommitted transaction 
                            //and only for purpose of testing if all can go well when it run to the target environment
                            if (verifyOnly.HasValue && verifyOnly == true)
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
                _traceService.Info($"Target database runs the latest version already. Scripts in _pre, _draft and _post are executed.");
            }

            //local method
            void RunAllInternal(IDbConnection connection, IDbTransaction transaction, bool requiredClearedDraft)
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
                RunVersionScripts(connection, transaction, allVersions, workingPath, targetVersion, null, tokenKeyPairs, bulkSeparator: bulkSeparator, metaSchemaName: metaSchemaName, metaTableName: metaTableName, commandTimeout: commandTimeout, bulkBatchSize: bulkBatchSize, appliedByTool: appliedByTool, appliedByToolVersion: appliedByToolVersion, environmentCode: environmentCode, transactionMode: transactionMode);

                //runs all scripts in the _draft folder and subfolders
                RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_draft"), tokenKeyPairs, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environmentCode: environmentCode, transactionMode: transactionMode, requiredClearedDraft: requiredClearedDraft);
                _traceService.Info($"Executed script files on {Path.Combine(workingPath, "_draft")}");

                //runs all scripts in the _post folder and subfolders
                RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_post"), tokenKeyPairs, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environmentCode: environmentCode, transactionMode: transactionMode);
                _traceService.Info($"Executed script files on {Path.Combine(workingPath, "_post")}");
            }

            //local method
            void RunDraftInternal(IDbConnection connection, IDbTransaction transaction, bool requiredClearedDraft)
            {
                //runs all scripts in the _pre folder and subfolders
                RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_pre"), tokenKeyPairs, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environmentCode: environmentCode, transactionMode: transactionMode);
                _traceService.Info($"Executed script files on {Path.Combine(workingPath, "_pre")}");

                //runs all scripts in the _draft folder and subfolders
                RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_draft"), tokenKeyPairs, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environmentCode: environmentCode, transactionMode: transactionMode, requiredClearedDraft: requiredClearedDraft);
                _traceService.Info($"Executed script files on {Path.Combine(workingPath, "_draft")}");

                //runs all scripts in the _post folder and subfolders
                RunNonVersionScripts(connection, transaction, Path.Combine(workingPath, "_post"), tokenKeyPairs, bulkSeparator: bulkSeparator, commandTimeout: commandTimeout, environmentCode: environmentCode, transactionMode: transactionMode);
                _traceService.Info($"Executed script files on {Path.Combine(workingPath, "_post")}");
            }
        }

        ///<inheritdoc/>
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
                    RunSqlScripts(connection, transaction, nonTransactionalContext, versionName, workingPath, scriptSubDirectory, metaSchemaName, metaTableName, tokenKeyPairs, commandTimeout, environmentCode);

                    //import csv files into tables of the the same filename as the csv
                    RunBulkImport(connection, transaction, workingPath, scriptSubDirectory, bulkSeparator, bulkBatchSize, commandTimeout, environmentCode);
                });

                //run all scripts in the current version folder
                RunSqlScripts(connection, transaction, nonTransactionalContext, versionName, workingPath, versionDirectory, metaSchemaName, metaTableName, tokenKeyPairs, commandTimeout, environmentCode);

                //import csv files into tables of the the same filename as the csv
                RunBulkImport(connection, transaction, workingPath, versionDirectory, bulkSeparator, bulkBatchSize, commandTimeout, environmentCode);

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
            //extract and filter out scripts when environment code is used
            var sqlScriptFiles = _directoryService.GetFiles(scriptDirectory, "*.sql").ToList();
            sqlScriptFiles = _directoryService.FilterFiles(workingPath, environmentCode, sqlScriptFiles).ToList();
            _traceService.Info($"Found {sqlScriptFiles.Count} script files on {workingPath}" + (sqlScriptFiles.Count > 0 ? Environment.NewLine : string.Empty) +
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
                            sqlStatement = _tokenReplacementService.Replace(tokenKeyPairs, sqlStatement);
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
