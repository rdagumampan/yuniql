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
        ///<inheritdoc/>
        public void RunVersionSqlScripts(
            IDbConnection connection,
            IDbTransaction transaction,
            TransactionContext transactionContext,
            Stopwatch stopwatch,
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
            string currentScriptFile = null;
            try
            {
                //filter out scripts when environment code is used
                var sqlScriptFiles = _directoryService.GetFiles(scriptDirectory, "*.sql").ToList();
                sqlScriptFiles = _directoryService.FilterFiles(workspace, environment, sqlScriptFiles).ToList();
                _traceService.Info($"Found {sqlScriptFiles.Count} script files on {workspace}" + (sqlScriptFiles.Count > 0 ? Environment.NewLine : string.Empty) +
                       $"{string.Join(Environment.NewLine, sqlScriptFiles.Select(s => "  + " + new FileInfo(s).Name))}");

                //execute all script files in the version folder, we also make sure its sorted by file name
                sqlScriptFiles.Sort();
                sqlScriptFiles.ForEach(scriptFile =>
                {
                    currentScriptFile = scriptFile;

                    //in case the non-transactional failure is resolved, skip scripts
                    if (null != transactionContext
                        && transactionContext.ContinueAfterFailure.HasValue
                        && transactionContext.ContinueAfterFailure.Value
                        && !transactionContext.IsFailedScriptPathMatched)
                    {
                        //set failed script file as matched
                        if (string.Equals(scriptFile, transactionContext.LastKnownFailedScriptPath, StringComparison.InvariantCultureIgnoreCase))
                        {
                            transactionContext.SetFailedScriptPathMatch();
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
                            sqlStatement = _tokenReplacementService.Replace(tokens, sqlStatement);
                            _traceService.Debug($"Executing sql statement as part of : {scriptFile}");

                            _metadataService.ExecuteSql(
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
                //try parse the known sql error and if not sucesfull, use the whole exception
                if (!_dataService.TryParseErrorFromException(exception, out string parsedExceptionMessage))
                    parsedExceptionMessage = exception.Message;

                //in case scripts are not executed within transaction, mark version as failed in database
                var configuration = _configurationService.GetConfiguration();
                if (configuration.TransactionMode == TRANSACTION_MODE.NONE)
                {
                    stopwatch.Stop();
                    _metadataService.InsertVersion(connection, transaction, version, transactionContext,
                        metaSchemaName: metaSchemaName,
                        metaTableName: metaTableName,
                        commandTimeout: commandTimeout,
                        appliedByTool: configuration.AppliedByTool,
                        appliedByToolVersion: configuration.AppliedByToolVersion,
                        failedScriptPath: currentScriptFile,
                        failedScriptError: parsedExceptionMessage,
                        durationMs: Convert.ToInt32(stopwatch.ElapsedMilliseconds));
                }

                var transactionModeText = configuration.TransactionMode == TRANSACTION_MODE.NONE ? "not running in transaction" : "running in transaction";
                var suggestionText = configuration.TransactionMode == TRANSACTION_MODE.NONE ? MESSAGES.ManualResolvingAfterFailureMessage : MESSAGES.TransactionalAfterFailureMessage;
                var exceptionMessage = @$"Migration of version {version} was {transactionModeText} and has failed while attempting to execute script file {currentScriptFile} due to error ""{parsedExceptionMessage}"". {suggestionText}";
                throw new YuniqlMigrationException(exceptionMessage, exception);
            }
        }

        /// <inheritdoc />
        public void RunNonVersionSqlScripts(
            IDbConnection connection, 
            IDbTransaction transaction,
            string workspace,
            List<KeyValuePair<string, string>> tokens = null,
            string environment = null,
            int? commandTimeout = null,
            bool isRequiredClearedDraft = false
            )
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

        /// <inheritdoc />
        public void RunBulkImportScripts(
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
    }
}
