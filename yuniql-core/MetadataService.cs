using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Yuniql.Extensibility;

namespace Yuniql.Core
{
    /// <summary>
    /// Service responsible for accessing target database configuration and executing sql statement batches.
    /// This facility is used by MigrationService and must be not be used directly. See <see cref="MigrationServiceTransactional"./>
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
             new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_SCHEMA_NAME, metaSchemaName ?? _dataService.SchemaName),
             new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_TABLE_NAME, metaTableName?? _dataService.TableName)
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
            var sqlStatement = GetPreparedSqlStatement(_dataService.GetSqlForCheckIfDatabaseConfigured(), metaSchemaName, metaTableName);
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
            string metaSchemaName = null,
            string metaTableName = null,
            int? commandTimeout = null)
        {
            var sqlStatement = GetPreparedSqlStatement(_dataService.GetSqlForConfigureDatabase(), metaSchemaName, metaTableName);
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
            string metaSchemaName = null,
            string metaTableName = null,
            int? commandTimeout = null)
        {
            using (var connection = _dataService.CreateConnection())
            {
                connection.KeepOpen();
                return _dataService.UpdateDatabaseConfiguration(connection, _traceService, metaSchemaName, metaTableName);
            }
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
                        AppliedByToolVersion = reader.GetString(5)
                    };

                    //capture additional artifacts when present present
                    if (!reader.IsDBNull(6))
                    {
                        var additionalArtifactsByteStream = reader.GetValue(6) as byte[];
                        dbVersion.AdditionalArtifacts = Encoding.UTF8.GetString(additionalArtifactsByteStream);
                    }

                    //fill up with information only available for platforms not supporting transactional ddl
                    if (!_dataService.IsTransactionalDdlSupported)
                    {
                        dbVersion.Status = Enum.Parse<Status>(reader.GetString(7));
                        dbVersion.FailedScriptPath = reader.GetValue(8) as string;      //as string handles null values
                        dbVersion.FailedScriptError = reader.GetValue(9) as string;     //as string handles null values

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
            string metaSchemaName = null,
            string metaTableName = null,
            int? commandTimeout = null,
            string appliedByTool = null,
            string appliedByToolVersion = null,
            string additionalArtifacts = null,
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
            var additionalArtifactsByteStream = Encoding.UTF8.GetBytes(additionalArtifacts ?? string.Empty);

            command.Parameters.Add(CreateDbParameter("version", version));
            command.Parameters.Add(CreateDbParameter("toolName", toolName));
            command.Parameters.Add(CreateDbParameter("toolVersion", toolVersion));
            command.Parameters.Add(CreateDbParameter("additionalArtifacts", additionalArtifactsByteStream));

            //in case database supports non-transactional flow
            if (_dataService is IMixableTransaction nonTransactionalDataService)
            {
                //override insert statement with upsert when targeting platforms not supporting non-transaction ddl
                sqlStatement = GetPreparedSqlStatement(nonTransactionalDataService.GetSqlForUpsertVersion(), metaSchemaName, metaTableName);
                var status = string.IsNullOrEmpty(failedScriptPath) ? Status.Successful.ToString() : Status.Failed.ToString();
                command.Parameters.Add(CreateDbParameter("status", status));
                command.Parameters.Add(CreateDbParameter("failedScriptPath", failedScriptPath));
                command.Parameters.Add(CreateDbParameter("failedScriptError", failedScriptError));
            }
            else
            {
                sqlStatement = GetPreparedSqlStatement(_dataService.GetSqlForInsertVersion(), metaSchemaName, metaTableName);
            }

            //upsert version information
            command.CommandText = sqlStatement;
            command.ExecuteNonQuery();

            //local function
            IDbDataParameter CreateDbParameter(string name, object value)
            {
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
