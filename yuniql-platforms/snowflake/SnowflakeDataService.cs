using Snowflake.Data.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Yuniql.Extensibility;
using Yuniql.Extensibility.SqlBatchParser;

namespace Yuniql.Snowflake
{
    ///<inheritdoc/>
    public class SnowflakeDataService : IDataService
    {
        private string _connectionString;
        private readonly ITraceService _traceService;

        ///<inheritdoc/>
        public SnowflakeDataService(ITraceService traceService)
        {
            this._traceService = traceService;
        }

        ///<inheritdoc/>
        public void Initialize(string connectionString)
        {
            this._connectionString = connectionString;
        }

        ///<inheritdoc/>
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

        ///<inheritdoc/>
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

        ///<inheritdoc/>
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

        ///<inheritdoc/>
        public bool IsTransactionalDdlSupported => false;

        ///<inheritdoc/>
        public bool IsSchemaSupported { get; } = true;

        ///<inheritdoc/>
        public bool IsBatchSqlSupported => true;

        ///<inheritdoc/>
        public bool IsUpsertSupported => false;

        ///<inheritdoc/>
        public string TableName { get; set; } = "__YUNIQLDBVERSIONS";

        ///<inheritdoc/>
        public string SchemaName { get; set; } = "PUBLIC";

        ///<inheritdoc/>
        public List<string> BreakStatements(string sqlStatementRaw)
        {
            var sqlBatchParser = new SqlBatchParser(_traceService, new GoSqlBatchLineAnalyzer(), new CommentAnalyzer());
            return sqlBatchParser.Parse(sqlStatementRaw).Select(s => s.BatchText).ToList();
        }

        ///<inheritdoc/>
        public string GetSqlForCheckIfDatabaseExists()
            => @"
SHOW DATABASES LIKE '${YUNIQL_DB_NAME}';
            ";

        ///<inheritdoc/>
        public string GetSqlForCreateDatabase()
            => @"
CREATE DATABASE ""${YUNIQL_DB_NAME}"";
            ";

        ///<inheritdoc/>
        public string GetSqlForCreateSchema()
            => @"
CREATE SCHEMA ""${YUNIQL_DB_NAME}"".""${YUNIQL_SCHEMA_NAME}"";
            ";

        ///<inheritdoc/>
        public string GetSqlForCheckIfDatabaseConfigured()
            => @"
SELECT 1 WHERE EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '${YUNIQL_SCHEMA_NAME}' AND TABLE_NAME = '${YUNIQL_TABLE_NAME}' AND TABLE_TYPE = 'BASE TABLE');
            ";

        ///<inheritdoc/>
        public string GetSqlForConfigureDatabase()
            => @"
CREATE TABLE ""${YUNIQL_DB_NAME}"".""${YUNIQL_SCHEMA_NAME}"".""${YUNIQL_TABLE_NAME}"" (
    ""SequenceId"" NUMBER NOT NULL IDENTITY START 1 INCREMENT 1,
    ""Version"" VARCHAR(512) NOT NULL,
    ""AppliedOnUtc"" TIMESTAMP_NTZ(9) NOT NULL DEFAULT CURRENT_TIMESTAMP(),
    ""AppliedByUser"" VARCHAR(32) NOT NULL DEFAULT CURRENT_USER(),
    ""AppliedByTool"" VARCHAR(32) NOT NULL,
    ""AppliedByToolVersion"" VARCHAR(16) NOT NULL,
    ""Status""  VARCHAR(32) NOT NULL,
    ""DurationMs"" NUMBER NOT NULL,
    ""FailedScriptPath""  VARCHAR(4000) NULL,
    ""FailedScriptError""  VARCHAR(4000) NULL,
    ""AdditionalArtifacts""  VARCHAR(4000) NULL,
    PRIMARY KEY (""SequenceId"")
);
             ";

        ///<inheritdoc/>
        public string GetSqlForGetCurrentVersion()
            => @"
SELECT TOP 1 ""Version"" FROM ""${YUNIQL_DB_NAME}"".""${YUNIQL_SCHEMA_NAME}"".""${YUNIQL_TABLE_NAME}"" WHERE ""Status"" = 'Successful' ORDER BY ""SequenceId"" DESC;
            ";

        ///<inheritdoc/>
        public string GetSqlForGetAllVersions()
            => @"
SELECT ""SequenceId"", ""Version"", ""AppliedOnUtc"", ""AppliedByUser"", ""AppliedByTool"", ""AppliedByToolVersion"", ""Status"", ""DurationMs"", ""FailedScriptPath"", ""FailedScriptError"", ""AdditionalArtifacts""
FROM ""${YUNIQL_DB_NAME}"".""${YUNIQL_SCHEMA_NAME}"".""${YUNIQL_TABLE_NAME}"" ORDER BY ""Version"" ASC;
            ";

        ///<inheritdoc/>
        public string GetSqlForInsertVersion()
            => @"
INSERT INTO ""${YUNIQL_DB_NAME}"".""${YUNIQL_SCHEMA_NAME}"".""${YUNIQL_TABLE_NAME}"" (""Version"", ""AppliedByTool"", ""AppliedByToolVersion"", ""Status"", ""DurationMs"", ""FailedScriptPath"", ""FailedScriptError"", ""AdditionalArtifacts"")
VALUES ('${YUNIQL_VERSION}', '${YUNIQL_APPLIED_BY_TOOL}', '${YUNIQL_APPLIED_BY_TOOL_VERSION}', '${YUNIQL_STATUS}', '${YUNIQL_DURATION_MS}', '${YUNIQL_FAILED_SCRIPT_PATH}', '${YUNIQL_FAILED_SCRIPT_ERROR}', '${YUNIQL_ADDITIONAL_ARTIFACTS}');
            ";

        ///<inheritdoc/>
        public string GetSqlForUpdateVersion()
            => @"
UPDATE ""${YUNIQL_SCHEMA_NAME}"".""${YUNIQL_TABLE_NAME}""
SET 	
	""AppliedOnUtc""         = CURRENT_TIMESTAMP(),
	""AppliedByUser""        = CURRENT_USER(),
	""AppliedByTool""        = '${YUNIQL_APPLIED_BY_TOOL}', 
	""AppliedByToolVersion"" = '${YUNIQL_APPLIED_BY_TOOL_VERSION}',
	""Status""               = '${YUNIQL_STATUS}',
	""DurationMs""           = '${YUNIQL_DURATION_MS}',
	""FailedScriptPath""     = '${YUNIQL_FAILED_SCRIPT_PATH}',
	""FailedScriptError""    = '${YUNIQL_FAILED_SCRIPT_ERROR}',
	""AdditionalArtifacts""  = '${YUNIQL_ADDITIONAL_ARTIFACTS}' 
WHERE
	""Version""              = '${YUNIQL_VERSION}';
            ";

        //https://docs.snowflake.com/en/sql-reference/sql/merge.html
        ///<inheritdoc/>
        public string GetSqlForUpsertVersion()
            => throw new NotSupportedException("Not supported for the target platform");

        ///<inheritdoc/>
        public bool UpdateDatabaseConfiguration(IDbConnection dbConnection, ITraceService traceService = null, string metaSchemaName = null, string metaTableName = null)
        {
            //no need to update tracking table as the structure has no been changed so far
            return false;
        }

        ///<inheritdoc/>
        public bool TryParseErrorFromException(Exception exception, out string result)
        {
            result = null;
            try
            {
                if (exception is SnowflakeDbException sqlException)
                {
                    var exceptionData = exception.ToString().Replace("\n", string.Empty).Replace("\r", string.Empty);
                    var exceptionMessage = sqlException.Message.Replace("\n", string.Empty).Replace("\r", string.Empty);
                    result = $"(0x{sqlException.ErrorCode:X}) Error {exceptionMessage} Exception data: {exceptionData}";
                    return true;
                }
            }
            catch (Exception) { return false; }
            return false;
        }
    }
}
