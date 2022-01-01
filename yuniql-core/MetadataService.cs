using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Yuniql.Extensibility;

namespace Yuniql.Core
{
    /// <summary>
    /// Service responsible for accessing target database configuration and executing sql statement batches.
    /// This facility is used by MigrationService and must be not be used directly. See <see cref="MigrationService"./>
    /// </summary>
    public class MetadataService : IMetadataService
    {
        private readonly IDataService _dataService;
        private readonly ITraceService _traceService;
        private readonly ITokenReplacementService _tokenReplacementService;

        ///<inheritdoc/>
        public MetadataService(
            IDataService dataService,
            ITraceService traceService,
            ITokenReplacementService tokenReplacementService)
        {
            this._dataService = dataService;
            this._traceService = traceService;
            _tokenReplacementService = tokenReplacementService;
        }

        private string GetPreparedSqlStatement(string sqlStatement, string metaSchemaName, string metaTableName)
        {
            var tokens = new List<KeyValuePair<string, string>> {
             new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_DB_NAME, _dataService.GetConnectionInfo().Database),
             new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_SCHEMA_NAME, metaSchemaName ?? _dataService.MetaSchemaName),
             new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_TABLE_NAME, metaTableName?? _dataService.MetaTableName)
            };

            return _tokenReplacementService.Replace(tokens, sqlStatement);
        }

        ///<inheritdoc/>
        public bool IsDatabaseExists(int? commandTimeout = null)
        {
            var tokens = new List<KeyValuePair<string, string>> {
             new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_DB_NAME, _dataService.GetConnectionInfo().Database),
            };
            var sqlStatement = _tokenReplacementService.Replace(tokens, _dataService.GetSqlForCheckIfDatabaseExists());
            using (var connection = _dataService.CreateMasterConnection())
            {
                return connection.QuerySingleRow(
                    commandText: sqlStatement,
                    commandTimeout: commandTimeout,
                    transaction: null,
                    traceService: _traceService);
            }
        }

        ///<inheritdoc/>
        public void CreateDatabase(int? commandTimeout = null)
        {
            var tokens = new List<KeyValuePair<string, string>> {
             new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_DB_NAME, _dataService.GetConnectionInfo().Database),
            };
            var sqlStatement = _tokenReplacementService.Replace(tokens, _dataService.GetSqlForCreateDatabase());
            using (var connection = _dataService.CreateMasterConnection())
            {
                connection.ExecuteNonQuery(
                    commandText: sqlStatement,
                    commandTimeout: commandTimeout,
                    transaction: null,
                    traceService: _traceService);
            }
        }

        ///<inheritdoc/>
        public bool IsSchemaExists(string metaSchemaName, int? commandTimeout = null)
        {
            var tokens = new List<KeyValuePair<string, string>> {
             new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_SCHEMA_NAME, metaSchemaName),
            };
            var sqlStatement = _tokenReplacementService.Replace(tokens, _dataService.GetSqlForCheckIfSchemaExists());
            using (var connection = _dataService.CreateConnection())
            {
                return connection.QuerySingleRow(
                    commandText: sqlStatement,
                    commandTimeout: commandTimeout,
                    transaction: null,
                    traceService: _traceService);
            }
        }

        ///<inheritdoc/>
        public void CreateSchema(string metaSchemaName, int? commandTimeout = null)
        {
            var tokens = new List<KeyValuePair<string, string>> {
             new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_SCHEMA_NAME, metaSchemaName),
            };
            var sqlStatement = _tokenReplacementService.Replace(tokens, _dataService.GetSqlForCreateSchema());
            using (var connection = _dataService.CreateConnection())
            {
                connection.ExecuteNonQuery(
                    commandText: sqlStatement,
                    commandTimeout: commandTimeout,
                    transaction: null,
                    traceService: _traceService);
            }
        }

        ///<inheritdoc/>
        public bool IsDatabaseConfigured(
            string metaSchemaName = null,
            string metaTableName = null,
            int? commandTimeout = null)
        {
            var result = false;

            //check existing of schema history table in version > v1.0
            var sqlStatement = GetPreparedSqlStatement(_dataService.GetSqlForCheckIfDatabaseConfigured(), metaSchemaName, metaTableName);
            using (var connection = _dataService.CreateConnection())
            {
                result = connection.QuerySingleBool(
                    commandText: sqlStatement,
                    commandTimeout: commandTimeout,
                    transaction: null,
                    traceService: _traceService);
            }

            if (!result)
            {
                //check existing of schema history table created by version v1.0
                var sqlStatementv10 = GetPreparedSqlStatement(_dataService.GetSqlForCheckIfDatabaseConfiguredv10(), metaSchemaName, metaTableName);
                using (var connection = _dataService.CreateConnection())
                {
                    result = connection.QuerySingleBool(
                        commandText: sqlStatementv10,
                        commandTimeout: commandTimeout,
                        transaction: null,
                        traceService: _traceService);
                }

                if(result)
                    _traceService.Warn($"Schema version history table {metaSchemaName ?? _dataService.MetaSchemaName}.__yuniqldbversion is deprecated in this release. " +
                        $"New table {metaSchemaName ?? _dataService.MetaSchemaName}.{metaTableName ?? _dataService.MetaTableName} will be created and existing data will be migrated automaticaly.");
            }

            return result;
        }

        ///<inheritdoc/>
        public void ConfigureDatabase(
            string metaSchemaName = null,
            string metaTableName = null,
            int? commandTimeout = null)
        {
            var sqlStatements = _dataService.BreakStatements(_dataService.GetSqlForConfigureDatabase());
            sqlStatements.ForEach(sqlStatementRaw => {
                var sqlStatement = GetPreparedSqlStatement(sqlStatementRaw, metaSchemaName, metaTableName);
                using (var connection = _dataService.CreateConnection())
                {
                    connection.ExecuteNonQuery(
                        commandText: sqlStatement,
                        commandTimeout: commandTimeout,
                        transaction: null,
                        traceService: _traceService);
                }
            });
        }

        ///<inheritdoc/>
        public bool UpdateDatabaseConfiguration(
            string metaSchemaName = null,
            string metaTableName = null,
            int? commandTimeout = null)
        {
            var version = typeof(IMigrationService).Assembly.GetName().Version.ToString();
            using (var connection = _dataService.CreateConnection())
            {
                try
                {
                    var sqlStatementRequireUpgrade = GetPreparedSqlStatement(_dataService.GetSqlForCheckRequireMetaSchemaUpgrade(version), metaSchemaName, metaTableName);
                    var requiredSchemaVersion = connection.QuerySingleString(
                    commandText: sqlStatementRequireUpgrade,
                    commandTimeout: commandTimeout,
                    transaction: null,
                    traceService: _traceService);

                    if (!string.IsNullOrEmpty(requiredSchemaVersion))
                    {
                        _traceService.Warn("Schema version history table will be upgraded as required in this release of yuniql.");
                        var sqlStatement = GetPreparedSqlStatement(_dataService.GetSqlForUpgradeMetaSchema(requiredSchemaVersion), metaSchemaName, metaTableName);
                        var result = connection.ExecuteNonQuery(
                            commandText: sqlStatement,
                            commandTimeout: commandTimeout,
                            transaction: null,
                            traceService: _traceService);

                        return result > 0;
                    }
                }
                catch (NotSupportedException)
                {
                    //swallow exception if target platform no longer supports schema migration
                }
            }
            return false;
        }

        ///<inheritdoc/>
        public string GetCurrentVersion(
            string metaSchemaName = null,
            string metaTableName = null,
            int? commandTimeout = null)
        {
            var sqlStatement = GetPreparedSqlStatement(_dataService.GetSqlForGetCurrentVersion(), metaSchemaName, metaTableName);
            using (var connection = _dataService.CreateConnection())
            {
                return connection.QuerySingleString(
                    commandText: sqlStatement,
                    commandTimeout: commandTimeout,
                    transaction: null,
                    traceService: _traceService);
            }
        }

        ///<inheritdoc/>
        public List<DbVersion> GetAllAppliedVersions(
            string metaSchemaName = null,
            string metaTableName = null,
            int? commandTimeout = null)
        {
            return this.GetAllVersions(metaSchemaName, metaTableName, commandTimeout)
                .Where(x => x.Status == Status.Successful).ToList();
        }

        ///<inheritdoc/>
        public List<DbVersion> GetAllVersions(
            string metaSchemaName = null,
            string metaTableName = null,
            int? commandTimeout = null)
        {
            var sqlStatement = GetPreparedSqlStatement(_dataService.GetSqlForGetAllVersions(), metaSchemaName, metaTableName);
            _traceService?.Debug($"Executing statement: {Environment.NewLine}{sqlStatement}");

            var result = new List<DbVersion>();
            using (var connection = _dataService.CreateConnection().KeepOpen())
            {
                var command = connection.CreateCommand(
                    commandText: sqlStatement,
                    commandTimeout: commandTimeout,
                    transaction: null);

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var dbVersion = new DbVersion
                    {
                        SequenceId = reader.GetInt16(0),
                        Version = reader.GetString(1),
                        AppliedOnUtc = reader.GetDateTime(2),
                        AppliedByUser = reader.GetString(3),
                        AppliedByTool = reader.GetString(4),
                        AppliedByToolVersion = reader.GetString(5),
                        Status = Enum.Parse<Status>(reader.GetString(6)),
                        DurationMs = reader.GetInt32(7),
                        Checksum = reader.GetString(8)
                    };

                    dbVersion.FailedScriptPath = !reader.IsDBNull(9) ? reader.GetString(9).Unescape() : string.Empty;

                    var failedScriptErrorBase64 = reader.GetValue(10) as string;
                    if (!string.IsNullOrEmpty(failedScriptErrorBase64))
                    {
                        dbVersion.FailedScriptError = Encoding.UTF8.GetString(Convert.FromBase64String(failedScriptErrorBase64));
                    }

                    var additionalArtifactsBase64 = reader.GetValue(11) as string;
                    if (!string.IsNullOrEmpty(additionalArtifactsBase64))
                    {
                        dbVersion.AdditionalArtifacts = Encoding.UTF8.GetString(Convert.FromBase64String(additionalArtifactsBase64));
                    }

                    result.Add(dbVersion);
                }
            }

            return result;
        }

        ///<inheritdoc/>
        public void InsertVersion(
            IDbConnection connection,
            IDbTransaction transaction,
            string version,
            TransactionContext transactionContext,
            string metaSchemaName = null,
            string metaTableName = null,
            int? commandTimeout = null,
            string appliedByTool = null,
            string appliedByToolVersion = null,
            string failedScriptPath = null,
            string failedScriptError = null,
            string additionalArtifacts = null,
            int durationMs = 0)
        {
            var sqlStatement = string.Empty;
            var command = connection
                .KeepOpen()
                .CreateCommand(
                    commandText: sqlStatement,
                    commandTimeout: commandTimeout,
                    transaction: transaction
                );

            var toolName = string.IsNullOrEmpty(appliedByTool) ? "yuniql-nuget" : appliedByTool;
            var toolVersion = string.IsNullOrEmpty(appliedByToolVersion) ? $"v{this.GetType().Assembly.GetName().Version.ToString()}" : $"v{appliedByToolVersion}";
            var statusString = string.IsNullOrEmpty(failedScriptPath) ? Status.Successful.ToString() : Status.Failed.ToString();
            var versionChecksum = "WIP"; //TODO: Implement directory checksum
            var failedScriptPathEscaped = string.IsNullOrEmpty(failedScriptPath) ? string.Empty : failedScriptPath.Escape();
            var failedScriptErrorBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(failedScriptError ?? string.Empty)); ;
            var additionalArtifactsBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(additionalArtifacts ?? string.Empty)); ;

            var tokens = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_DB_NAME, _dataService.GetConnectionInfo().Database),
                new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_SCHEMA_NAME, metaSchemaName ?? _dataService.MetaSchemaName),
                new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_TABLE_NAME, metaTableName?? _dataService.MetaTableName),

                new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_VERSION, version),
                new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_APPLIED_BY_TOOL, toolName),
                new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_APPLIED_BY_TOOL_VERSION, toolVersion),

                new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_DURATION_MS, durationMs.ToString()),
                new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_STATUS, statusString),
                new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_CHECKSUM, versionChecksum),

                new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_FAILED_SCRIPT_PATH, failedScriptPathEscaped),
                new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_FAILED_SCRIPT_ERROR, failedScriptErrorBase64),
                new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_ADDITIONAL_ARTIFACTS, additionalArtifactsBase64),
            };

            //override insert statement with upsert when targeting platforms not supporting non-transaction ddl
            sqlStatement = _tokenReplacementService.Replace(tokens, _dataService.GetSqlForInsertVersion());
            var existingSavedVersion = (null!= transactionContext) && !string.IsNullOrEmpty(transactionContext.LastKnownFailedVersion) 
                && string.Equals(transactionContext.LastKnownFailedVersion, version, StringComparison.InvariantCultureIgnoreCase);
            if (existingSavedVersion)
            {
                sqlStatement = _dataService.IsUpsertSupported ?
                    _tokenReplacementService.Replace(tokens, _dataService.GetSqlForUpsertVersion()) :
                    _tokenReplacementService.Replace(tokens, _dataService.GetSqlForUpdateVersion());
            }

            //upsert version information
            var statementCorrelationId = Guid.NewGuid().ToString().Fixed();
            _traceService?.Debug($"Executing statement {statementCorrelationId}: {Environment.NewLine}{sqlStatement}");
            command.CommandText = sqlStatement;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            command.ExecuteNonQuery();

            stopwatch.Stop();
            _traceService?.Debug($"Statement {statementCorrelationId} executed in {stopwatch.ElapsedMilliseconds} ms");
        }

        ///<inheritdoc/>
        public int ExecuteSql(
            IDbConnection connection,
            string commandText,
            int? commandTimeout = null,
            IDbTransaction transaction = null,
            ITraceService traceService = null)
        {
            return connection.ExecuteNonQuery(
                commandText: commandText,
                transaction: transaction,
                commandTimeout: commandTimeout,
                traceService: _traceService);

        }
    }
}
