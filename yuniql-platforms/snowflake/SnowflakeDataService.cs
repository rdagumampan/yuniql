using Snowflake.Data.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Yuniql.Extensibility;
using Yuniql.Extensibility.SqlBatchParser;

namespace Yuniql.Snowflake
{
    public class SnowflakeDataService : IDataService
    {
        private string _connectionString;
        private readonly ITraceService _traceService;

        public SnowflakeDataService(ITraceService traceService)
        {
            this._traceService = traceService;
        }

        public void Initialize(string connectionString)
        {
            this._connectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            var connection = new SnowflakeDbConnection();
            connection.ConnectionString = _connectionString;

            //replace original database name with quoted name for case-sensitivity
            //by default snowflake converts all object identifies into upper case unless it is enclosed in double quote
            //do not rebuild the connection string because it will add single quote to the value
            //https://docs.snowflake.com/en/sql-reference/identifiers-syntax.html
            var connectionStringBuilder = new SnowflakeDbConnectionStringBuilder();
            connectionStringBuilder.ConnectionString = _connectionString;
            connectionStringBuilder.TryGetValue("db", out object result);

            //db name is empty when checking if the database exists
            if (null != result)
            {
                var databaseName = result.ToString();
                if (!databaseName.ToString().IsDoubleQuoted())
                {
                    var modifiedConnectionString = _connectionString.Replace(databaseName, databaseName.DoubleQuote());
                    connection.ConnectionString = modifiedConnectionString;
                }
            }

            return connection;
        }

        public IDbConnection CreateMasterConnection()
        {
            var connectionStringBuilder = new SnowflakeDbConnectionStringBuilder();
            connectionStringBuilder.ConnectionString = _connectionString;

            //remove existing db & schema from connection string parameters
            //this is necessary to avoid connection errors as it will attempt to connect to non-existing database
            connectionStringBuilder.Remove("db");
            connectionStringBuilder.Remove("schema");

            var connection = new SnowflakeDbConnection();
            connection.ConnectionString = connectionStringBuilder.ConnectionString;
            return connection;
        }

        public ConnectionInfo GetConnectionInfo()
        {
            var connectionStringBuilder = new SnowflakeDbConnectionStringBuilder();
            connectionStringBuilder.ConnectionString = _connectionString;

            //extract the server information
            connectionStringBuilder.TryGetValue("host", out object dataSource);

            //extract the database name
            connectionStringBuilder.TryGetValue("db", out object database);

            return new ConnectionInfo { DataSource = dataSource?.ToString(), Database = database?.ToString() };
        }

        public bool IsTransactionalDdlSupported => false;

        public bool IsSchemaSupported { get; } = true;

        public bool IsBatchSqlSupported => false;

        public string TableName { get; set; } = "__YUNIQLDBVERSIONS";

        public string SchemaName { get; set; } = "PUBLIC";

        public List<string> BreakStatements(string sqlStatementRaw)
        {
            var sqlBatchParser = new SqlBatchParser(_traceService, new GoSqlBatchLineAnalyzer(), new CommentAnalyzer());
            return sqlBatchParser.Parse(sqlStatementRaw).Select(s => s.BatchText).ToList();
        }

        public string GetSqlForCheckIfDatabaseExists()
            => "SHOW DATABASES LIKE '${YUNIQL_DB_NAME}';";

        public string GetSqlForCreateDatabase()
            => "CREATE DATABASE \"${YUNIQL_DB_NAME}\";";

        public string GetSqlForCreateSchema()
            => "CREATE SCHEMA \"${YUNIQL_DB_NAME}\".\"${YUNIQL_SCHEMA_NAME}\";";

        public string GetSqlForCheckIfDatabaseConfigured()
            => "SELECT 1 WHERE EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '${YUNIQL_SCHEMA_NAME}' AND TABLE_NAME = '${YUNIQL_TABLE_NAME}' AND TABLE_TYPE = 'BASE TABLE')";

        public string GetSqlForConfigureDatabase()
            => "CREATE TABLE \"${YUNIQL_DB_NAME}\".\"${YUNIQL_SCHEMA_NAME}\".\"${YUNIQL_TABLE_NAME}\"(" +
            "\"SequenceId\" NUMBER NOT NULL IDENTITY START 1 INCREMENT 1," +
            "\"Version\" VARCHAR(512) NOT NULL," +
            "\"AppliedOnUtc\" TIMESTAMP_NTZ(9) NOT NULL DEFAULT CURRENT_TIMESTAMP()," +
            "\"AppliedByUser\" VARCHAR(32) NOT NULL DEFAULT CURRENT_USER()," +
            "\"AppliedByTool\" VARCHAR(32) NULL," +
            "\"AppliedByToolVersion\" VARCHAR(16) NULL," +
            "\"AdditionalArtifacts\" VARBINARY NULL," +
            "PRIMARY KEY (\"SequenceId\")" +
            ");";

        public string GetSqlForGetCurrentVersion()
            => "SELECT TOP 1 \"Version\" FROM \"${YUNIQL_DB_NAME}\".\"${YUNIQL_SCHEMA_NAME}\".\"${YUNIQL_TABLE_NAME}\" ORDER BY \"SequenceId\" DESC;";

        public string GetSqlForGetAllVersions()
            => "SELECT \"SequenceId\", \"Version\", \"AppliedOnUtc\", \"AppliedByUser\", \"AppliedByTool\", \"AppliedByToolVersion\", \"AdditionalArtifacts\" FROM \"${YUNIQL_DB_NAME}\".\"${YUNIQL_SCHEMA_NAME}\".\"${YUNIQL_TABLE_NAME}\" ORDER BY \"Version\" ASC;";

        public string GetSqlForInsertVersion()
            => "INSERT INTO \"${YUNIQL_DB_NAME}\".\"${YUNIQL_SCHEMA_NAME}\".\"${YUNIQL_TABLE_NAME}\" (\"Version\", \"AppliedByTool\", \"AppliedByToolVersion\") VALUES ('${YUNIQL_VERSION}', '${YUNIQL_APPLIED_BY_TOOL}', '${YUNIQL_APPLIED_BY_TOOL_VERSION}');";

        public bool UpdateDatabaseConfiguration(IDbConnection dbConnection, ITraceService traceService = null, string metaSchemaName = null, string metaTableName = null)
        {
            //no need to update tracking table as the structure has no been changed so far
            return false;
        }

        public bool TryParseErrorFromException(Exception exc, out string result)
        {
            result = null;
            return false;
        }
    }
}
