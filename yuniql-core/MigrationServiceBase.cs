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
        private readonly IWorkspaceService _workspaceService;
        private readonly IDataService _dataService;
        private readonly IBulkImportService _bulkImportService;
        private readonly ITokenReplacementService _tokenReplacementService;
        private readonly IDirectoryService _directoryService;
        private readonly IFileService _fileService;
        private readonly ITraceService _traceService;
        private readonly IConfigurationService _configurationService;
        private readonly IMetadataService _metadataService;

        ///<inheritdoc/>
        public MigrationServiceBase(
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
        public virtual void Initialize()
        {
            _configurationService.Initialize();
            _configurationService.Validate();

            var configuration = _configurationService.GetConfiguration();
            _dataService.Initialize(configuration.ConnectionString);
            _bulkImportService.Initialize(configuration.ConnectionString);
        }
        
        /// <inheritdoc />
        public virtual string GetCurrentVersion(string metaSchemaName = null, string metaTableName = null)
        {
            var configuration = _configurationService.GetConfiguration();
            if (!configuration.IsInitialized)
                Initialize();

            return _metadataService.GetCurrentVersion(metaSchemaName, metaTableName);
        }

        /// <inheritdoc />
        public virtual List<DbVersion> GetAllVersions(string metaSchemaName = null, string metaTableName = null)
        {
            var configuration = _configurationService.GetConfiguration();
            if (!configuration.IsInitialized)
                Initialize();

            return _metadataService.GetAllVersions(metaSchemaName, metaTableName);
        }

        /// <inheritdoc />
        public abstract void Run();

        /// <inheritdoc />
        public abstract void Run(
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
         );

        /// <inheritdoc />
        public virtual bool IsTargetDatabaseLatest(string targetVersion, string metaSchemaName = null, string metaTableName = null)
        {
            //get the current version stored in database
            var remoteCurrentVersion = _metadataService.GetCurrentVersion(metaSchemaName, metaTableName);
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
            string workspace,
            List<KeyValuePair<string, string>> tokens = null,
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
                var sqlScriptFiles = _directoryService.GetAllFiles(workspace, "*.sql").ToList();

                // Throw exception when --require-cleared-draft is set to TRUE 
                if (sqlScriptFiles.Any() && isRequiredClearedDraft && workspace.Contains(RESERVED_DIRECTORY_NAME.DRAFT))
                {
                    throw new YuniqlMigrationException($"Special {RESERVED_DIRECTORY_NAME.DRAFT} directory is not cleared. Found files in _draft directory while the migration option --require-cleared-draft is set to TRUE." +
                        $"Move the script files to a version directory and re-execute the migration. Or remove --require-cleared-draft in parameter.");
                }

                sqlScriptFiles = _directoryService.FilterFiles(workspace, environment, sqlScriptFiles).ToList();
                _traceService.Info($"Found {sqlScriptFiles.Count} script files on {workspace}" + (sqlScriptFiles.Count > 0 ? Environment.NewLine : string.Empty) +
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

        /// <inheritdoc />
        public abstract void RunVersionScripts(
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
        );

        /// <inheritdoc />
        public virtual void RunBulkImport(
            IDbConnection connection,
            IDbTransaction transaction,
            string workspace,
            string scriptDirectory,
            string bulkSeparator = null,
            int? bulkBatchSize = null,
            int? commandTimeout = null,
            string environment = null
        )
        {
            //extract and filter out scripts when environment code is used
            var bulkFiles = _directoryService.GetFiles(scriptDirectory, "*.csv").ToList();
            bulkFiles = _directoryService.FilterFiles(workspace, environment, bulkFiles).ToList();
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
        );

        /// <inheritdoc />
        public virtual void Erase()
        {
            Initialize();

            //create a shared open connection to entire migration run
            var configuration = _configurationService.GetConfiguration();
            using (var connection = _dataService.CreateConnection())
            {
                connection.KeepOpen();

                //enclose all executions in a single transaction in case platform supports it
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        //runs all scripts in the _erase folder
                        RunNonVersionScripts(connection, transaction, Path.Combine(configuration.Workspace, RESERVED_DIRECTORY_NAME.ERASE), tokens: configuration.Tokens, bulkSeparator: DEFAULT_CONSTANTS.BULK_SEPARATOR, commandTimeout: configuration.CommandTimeout, environment: configuration.Environment);
                        _traceService.Info($"Executed script files on {Path.Combine(configuration.Workspace, RESERVED_DIRECTORY_NAME.ERASE)}");

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
