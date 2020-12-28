using Yuniql.Extensibility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;

namespace Yuniql.Core
{
    ///<inheritdoc/>
    public abstract class MigrationServiceBase : IMigrationService
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
        public MigrationServiceBase(
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
        public virtual void Initialize(
            string connectionString,
            int? commandTimeout = null)
        {
            //initialize dependencies
            _dataService.Initialize(connectionString);
            _bulkImportService.Initialize(connectionString);
        }

        /// <inheritdoc />
        public virtual string GetCurrentVersion(string metaSchemaName = null, string metaTableName = null)
        {
            return _configurationDataService.GetCurrentVersion(metaSchemaName, metaTableName);
        }

        /// <inheritdoc />
        //TODO: Move this to MigrationServiceBase
        public virtual List<DbVersion> GetAllVersions(string metaSchemaName = null, string metaTableName = null)
        {
            return _configurationDataService.GetAllAppliedVersions(metaSchemaName, metaTableName);
        }

        public abstract void Run(Configuration configuration);

        /// <inheritdoc />
        public abstract void Run(
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
            bool requiredClearedDraftFolder = false
         );

        /// <inheritdoc />
        public virtual bool IsTargetDatabaseLatest(string targetVersion, string metaSchemaName = null, string metaTableName = null)
        {
            //get the current version stored in database
            var remoteCurrentVersion = _configurationDataService.GetCurrentVersion(metaSchemaName, metaTableName);
            if (string.IsNullOrEmpty(remoteCurrentVersion)) return false;

            //compare version applied in db vs versions available locally
            var localCurrentVersion = new LocalVersion(remoteCurrentVersion);
            var localTargetVersion = new LocalVersion(targetVersion);
            return string.Compare(localCurrentVersion.SemVersion, localTargetVersion.SemVersion) == 1 || //db has more updated than local version
                string.Compare(localCurrentVersion.SemVersion, localTargetVersion.SemVersion) == 0;      //db has the same version as local version
        }

        /// <inheritdoc />
        public virtual void RunNonVersionScripts(
            IDbConnection connection,
            IDbTransaction transaction,
            string workingPath,
            List<KeyValuePair<string, string>> tokenKeyPairs = null,
            string bulkSeparator = null,
            int? commandTimeout = null,
            string environmentCode = null,
            string transactionMode = null,
            bool requiredClearedDraft = false
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

                            RunNonVersionScriptsInternal(internalConnection, internalTransaction);
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
                RunNonVersionScriptsInternal(connection, transaction);
            }

            void RunNonVersionScriptsInternal(IDbConnection connection, IDbTransaction transaction)
            {
                //extract and filter out scripts when environment code is used
                var sqlScriptFiles = _directoryService.GetAllFiles(workingPath, "*.sql").ToList();

                // Throw exception when --require-cleared-draft is set to TRUE 
                if (sqlScriptFiles.Any() && requiredClearedDraft && workingPath.Contains("_draft"))
                {
                    throw new YuniqlMigrationException($"Special _draft directory is not cleared. Found files in _draft directory while the migration option --require-cleared-draft is set to TRUE." +
                        $"Move the script files to a version directory and re-execute the migration. Or remove --require-cleared-draft in parameter.");
                }

                sqlScriptFiles = _directoryService.FilterFiles(workingPath, environmentCode, sqlScriptFiles).ToList();
                _traceService.Info($"Found {sqlScriptFiles.Count} script files on {workingPath}" + (sqlScriptFiles.Count > 0 ? Environment.NewLine : string.Empty) +
                       $"{string.Join(Environment.NewLine, sqlScriptFiles.Select(s => "  + " + new FileInfo(s).Name))}");

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
                        try
                        {
                            sqlStatement = _tokenReplacementService.Replace(tokenKeyPairs, sqlStatement);
                            _traceService.Debug($"Executing sql statement as part of : {scriptFile}");

                            _configurationDataService.ExecuteSql(
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

        /// <inheritdoc />
        public abstract void RunVersionScripts(
            IDbConnection connection,
            IDbTransaction transaction,
            List<string> dbVersions,
            string workingPath,
            string targetVersion,
            NonTransactionalContext nonTransactionalContext,
            List<KeyValuePair<string, string>> tokenKeyPairs = null,
            string bulkSeparator = null,
            string metaschemaName = null,
            string metaTableName = null,
            int? commandTimeout = null,
            int? bulkBatchSize = null,
            string appliedByTool = null,
            string appliedByToolVersion = null,
            string environmentCode = null,
            string transactionMode = null
        );

        /// <inheritdoc />
        public virtual void RunBulkImport(
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
            _traceService.Info($"Found {bulkFiles.Count} script files on {scriptDirectory}" + (bulkFiles.Count > 0 ? Environment.NewLine : string.Empty) +
                   $"{string.Join(Environment.NewLine, bulkFiles.Select(s => "  + " + new FileInfo(s).Name))}");
            bulkFiles.Sort();
            bulkFiles.ForEach(csvFile =>
            {
                _bulkImportService.Run(connection, transaction, csvFile, bulkSeparator, bulkBatchSize: bulkBatchSize, commandTimeout: commandTimeout);
                _traceService.Info($"Imported bulk file {csvFile}.");
            });
        }

        /// <inheritdoc />
        public abstract void RunSqlScripts(
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
        );

        /// <inheritdoc />
        public virtual void Erase(
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
