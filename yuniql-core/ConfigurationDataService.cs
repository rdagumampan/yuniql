using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Yuniql.Extensibility;

namespace Yuniql.Core
{
    /// <summary>
    /// Service responsible for accessing target database configuration and executing sql statement batches.
    /// This facility is used by MigrationService and must be not be used directly. See <see cref="MigrationService"./>
    /// </summary>
    public class ConfigurationDataService : IConfigurationDataService
    {
        private readonly IDataService _dataService;
        private readonly ITraceService _traceService;
        private readonly ITokenReplacementService _tokenReplacementService;

        ///<inheritdoc/>
        public ConfigurationDataService(
            IDataService dataService,
            ITraceService traceService,
            ITokenReplacementService tokenReplacementService)
        {
            this._dataService = dataService;
            this._traceService = traceService;
            _tokenReplacementService = tokenReplacementService;
        }

        private string GetPreparedSqlStatement(string sqlStatement, string schemaName, string tableName)
        {
            var tokens = new List<KeyValuePair<string, string>> {
             new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_DB_NAME, _dataService.GetConnectionInfo().Database),
             new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_SCHEMA_NAME, schemaName ?? _dataService.SchemaName),
             new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_TABLE_NAME, tableName?? _dataService.TableName)
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
                return connection.QuerySingleBool(
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
        public void CreateSchema(string schemaName, int? commandTimeout = null)
        {
            var tokens = new List<KeyValuePair<string, string>> {
             new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_SCHEMA_NAME, schemaName),
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
            string schemaName = null,
            string tableName = null,
            int? commandTimeout = null)
        {
            var sqlStatement = GetPreparedSqlStatement(_dataService.GetSqlForCheckIfDatabaseConfigured(), schemaName, tableName);
            using (var connection = _dataService.CreateConnection())
            {
                return connection.QuerySingleBool(
                    commandText: sqlStatement,
                    commandTimeout: commandTimeout,
                    transaction: null,
                    traceService: _traceService);
            }
        }

        ///<inheritdoc/>
        public void ConfigureDatabase(
            string schemaName = null,
            string tableName = null,
            int? commandTimeout = null)
        {
            var sqlStatement = GetPreparedSqlStatement(_dataService.GetSqlForConfigureDatabase(), schemaName, tableName);
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
        public bool UpdateDatabaseConfiguration(
            string schemaName = null,
            string tableName = null,
            int? commandTimeout = null)
        {
            using (var connection = _dataService.CreateConnection())
            {
                connection.KeepOpen();
                return _dataService.UpdateDatabaseConfiguration(connection, _traceService, schemaName, tableName);
            }
        }

        ///<inheritdoc/>
        public string GetCurrentVersion(
            string schemaName = null,
            string tableName = null,
            int? commandTimeout = null)
        {
            var sqlStatement = GetPreparedSqlStatement(_dataService.GetSqlForGetCurrentVersion(), schemaName, tableName);
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
            string schemaName = null,
            string tableName = null,
            int? commandTimeout = null)
        {
            return this.GetAllVersions(schemaName, tableName, commandTimeout)
                .Where(x => x.StatusId == StatusId.Succeeded).ToList();
        }

        ///<inheritdoc/>
        public List<DbVersion> GetAllVersions(
            string schemaName = null,
            string tableName = null,
            int? commandTimeout = null)
        {
            var sqlStatement = GetPreparedSqlStatement(_dataService.GetSqlForGetAllVersions(), schemaName, tableName);
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
                        AppliedByToolVersion = reader.GetString(5)
                    };

                    //fill up with information only available for platforms not supporting transactional ddl
                    if (!_dataService.IsAtomicDDLSupported)
                    {
                        dbVersion.StatusId = (StatusId)reader.GetInt32(6);
                        dbVersion.FailedScriptPath = reader.GetValue(7) as string;      //as string handles null values
                        dbVersion.FailedScriptError = reader.GetValue(8) as string;     //as string handles null values
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
            string schemaName = null,
            string tableName = null,
            int? commandTimeout = null,
            string appliedByTool = null,
            string appliedByToolVersion = null,
            string failedScriptPath = null,
            string failedScriptError = null
            )
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
            var statusId = string.IsNullOrEmpty(failedScriptPath) ? (int)StatusId.Succeeded : (int)StatusId.Failed;
            command.Parameters.Add(CreateDbParameter("version", version));
            command.Parameters.Add(CreateDbParameter("toolName", toolName));
            command.Parameters.Add(CreateDbParameter("toolVersion", toolVersion));

            //in case database supports non-transactional flow
            if (_dataService is INonTransactionalFlow nonTransactionalDataService)
            {
                //override insert statement with upsert when targeting platforms not supporting non-transaction ddl
                sqlStatement = GetPreparedSqlStatement(nonTransactionalDataService.GetSqlForUpsertVersion(), schemaName, tableName);
                command.Parameters.Add(CreateDbParameter("statusId", statusId));
                command.Parameters.Add(CreateDbParameter("failedScriptPath", failedScriptPath));
                command.Parameters.Add(CreateDbParameter("failedScriptError", failedScriptError));
            }
            else
            {
                sqlStatement = GetPreparedSqlStatement(_dataService.GetSqlForInsertVersion(), schemaName, tableName);
            }

            //upsert version information
            command.CommandText = sqlStatement;
            command.ExecuteNonQuery();

            //local function
            IDbDataParameter CreateDbParameter(string name, object value) {
                var parameter = command.CreateParameter();
                parameter.ParameterName = name;
                parameter.Value = value;
                parameter.Direction = ParameterDirection.Input;
                return parameter;
            }
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
