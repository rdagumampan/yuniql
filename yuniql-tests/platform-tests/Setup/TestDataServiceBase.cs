using Yuniql.Extensibility;
using System;
using System.Collections.Generic;
using Yuniql.Core;
using System.Data;
using System.Text;
using Yuniql.PlatformTests.Interfaces;

namespace Yuniql.PlatformTests.Setup
{
    public abstract class TestDataServiceBase : ITestDataService
    {
        private readonly IDataService _dataService;
        private readonly ITokenReplacementService _tokenReplacementService;

        public TestDataServiceBase(
            IDataService dataService,
            ITokenReplacementService tokenReplacementService)
        {
            _dataService = dataService;
            _tokenReplacementService = tokenReplacementService;
        }

        public virtual bool IsTransactionalDdlSupported => _dataService.IsTransactionalDdlSupported;

        public virtual bool IsSchemaSupported => _dataService.IsSchemaSupported;

        public virtual bool IsBatchSqlSupported => _dataService.IsBatchSqlSupported;

        public virtual string TableName => _dataService.TableName;

        public virtual string SchemaName => _dataService.SchemaName;

        public virtual void ExecuteNonQuery(string connectionString, string sqlStatement)
        {
            _dataService.Initialize(connectionString);
            using (var connection = _dataService.CreateConnection().KeepOpen())
            {
                connection.ExecuteNonQuery(sqlStatement);
            }
        }

        public virtual bool QuerySingleBool(string connectionString, string sqlStatement)
        {
            _dataService.Initialize(connectionString);
            using (var connection = _dataService.CreateConnection().KeepOpen())
            {
                return connection.QuerySingleBool(sqlStatement);
            }
        }

        public virtual string QuerySingleString(string connectionString, string sqlStatement)
        {
            _dataService.Initialize(connectionString);
            using (var connection = _dataService.CreateConnection().KeepOpen())
            {
                return connection.QuerySingleString(sqlStatement);
            }
        }

        public virtual bool QuerySingleRow(string connectionString, string sqlStatement)
        {
            _dataService.Initialize(connectionString);
            using (var connection = _dataService.CreateConnection().KeepOpen())
            {
                return connection.QuerySingleRow(sqlStatement);
            }
        }

        public virtual string GetCurrentDbVersion(string connectionString)
        {
            _dataService.Initialize(connectionString);
            var sqlStatement = GetPreparedSqlStatement(_dataService.GetSqlForGetCurrentVersion(), _dataService.SchemaName, _dataService.TableName);
            using (var connection = _dataService.CreateConnection().KeepOpen())
            {
                return connection.QuerySingleString(commandText: sqlStatement);
            }
        }

        public virtual List<DbVersion> GetAllDbVersions(string connectionString)
        {
            _dataService.Initialize(connectionString);
            var sqlStatement = GetPreparedSqlStatement(_dataService.GetSqlForGetAllVersions(), _dataService.SchemaName, _dataService.TableName);

            var result = new List<DbVersion>();
            using (var connection = _dataService.CreateConnection().KeepOpen())
            {
                var command = connection.CreateCommand(commandText: sqlStatement);

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
                        Checksum = reader.GetString(8),
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

        public virtual List<BulkTestDataRow> GetBulkTestData(string connectionString, string tableName)
        {
            var results = new List<BulkTestDataRow>();
            using (var connection = _dataService.CreateConnection())
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = GetSqlForGetBulkTestData(tableName);
                command.CommandTimeout = 0;

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new BulkTestDataRow
                        {
                            FirstName = !reader.IsDBNull(0) ? reader.GetString(0) : null,
                            LastName = !reader.IsDBNull(1) ? reader.GetString(1) : null,
                            BirthDate = !reader.IsDBNull(2) ? reader.GetDateTime(2) : new DateTime?()
                        });
                    }
                }
            }
            return results;
        }

        public abstract string GetConnectionString(string databaseName);

        public abstract bool CheckIfDbExist(string connectionString);

        public abstract bool CheckIfDbObjectExist(string connectionString, string objectName);

        public abstract void CreateScriptFile(string sqlFilePath, string sqlStatement);

        public abstract string GetSqlForCleanup();

        public abstract string GetSqlForCreateBulkTable(string tableName);

        public abstract string GetSqlForCreateDbObject(string scriptName);

        public abstract string GetSqlForCreateDbObjectWithError(string objectName);

        public abstract string GetSqlForCreateDbObjectWithTokens(string objectName);

        public abstract string GetSqlForCreateDbSchema(string schemaName);

        public abstract string GetSqlForMultilineWithError(string objectName1, string objectName2);

        public abstract string GetSqlForMultilineWithoutTerminatorInLastLine(string objectName1, string objectName2, string objectName3);

        public abstract string GetSqlForMultilineWithTerminatorInsideStatements(string objectName1, string objectName2, string objectName3);

        public abstract string GetSqlForSingleLine(string objectName);

        public abstract string GetSqlForSingleLineWithoutTerminator(string objectName);

        public abstract string GetSqlForMultilineWithTerminatorInCommentBlock(string objectName1, string objectName2, string objectName3);

        public abstract void DropDatabase(string connectionString);

        public virtual string GetSqlForGetBulkTestData(string tableName)
        {
            return $"SELECT * FROM {tableName};";
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

    }
}
