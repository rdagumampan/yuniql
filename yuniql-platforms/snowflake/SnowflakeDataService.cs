using Snowflake.Data.Client;
using Snowflake.Data.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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

            //configure snowflake loggers to follow yuniql debug settings
            var logger = SFLoggerFactory.GetLogger<SnowflakeDataService>();
            logger.SetDebugMode(_traceService.IsDebugEnabled);
        }

        ///<inheritdoc/>
        public bool IsTransactionalDdlSupported => false;

        ///<inheritdoc/>
        public bool IsMultiTenancySupported { get; } = true;

        ///<inheritdoc/>
        public bool IsSchemaSupported { get; } = true;

        ///<inheritdoc/>
        public bool IsBatchSqlSupported => true;

        ///<inheritdoc/>
        public bool IsUpsertSupported => false;

        ///<inheritdoc/>
        public string MetaTableName { get; set; } = "__YUNIQL_SCHEMA_VERSION";

        ///<inheritdoc/>
        public string MetaSchemaName { get; set; } = "PUBLIC";

        ///<inheritdoc/>
        public IDbConnection CreateConnection()
        {
            var connection = new SnowflakeDbConnection();
            connection.ConnectionString = _connectionString;

            //NOTE: replace original database name with quoted name for case-sensitivity
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

            //NOTE: remove existing db & schema from connection string parameters
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
        public List<string> GetSqlForDropDatabase()
            => new List<string> { @"
DROP DATABASE ""${YUNIQL_DB_NAME}"";
            " };

        ///<inheritdoc/>
        public string GetSqlForCheckIfSchemaExists()
            => @"
SHOW SCHEMAS LIKE '${YUNIQL_SCHEMA_NAME}';
            ";

        //https://docs.snowflake.com/en/sql-reference/sql/create-schema.html
        ///<inheritdoc/>
        public string GetSqlForCreateSchema()
            => @"
CREATE SCHEMA ""${YUNIQL_SCHEMA_NAME}"";
            ";

        ///<inheritdoc/>
        public string GetSqlForCheckIfDatabaseConfigured()
            => @"
SELECT 1 WHERE EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '${YUNIQL_SCHEMA_NAME}' AND TABLE_NAME = '${YUNIQL_TABLE_NAME}' AND TABLE_TYPE = 'BASE TABLE');
            ";

        ///<inheritdoc/>
        public string GetSqlForCheckIfDatabaseConfiguredv10()
            => @"
SELECT 1 WHERE EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '__yuniqldbversion' AND TABLE_NAME = '${YUNIQL_TABLE_NAME}' AND TABLE_TYPE = 'BASE TABLE');
            ";

        ///<inheritdoc/>
        public string GetSqlForConfigureDatabase()
            => @"
CREATE TABLE ""${YUNIQL_DB_NAME}"".""${YUNIQL_SCHEMA_NAME}"".""${YUNIQL_TABLE_NAME}"" (
    ""sequence_id"" NUMBER NOT NULL IDENTITY START 1 INCREMENT 1,
    ""version"" VARCHAR(512) NOT NULL,
    ""applied_on_utc"" TIMESTAMP_NTZ(9) NOT NULL DEFAULT CURRENT_TIMESTAMP(),
    ""applied_by_user"" VARCHAR(128) NOT NULL DEFAULT CURRENT_USER(),
    ""applied_by_tool"" VARCHAR(32) NOT NULL,
    ""applied_by_tool_version"" VARCHAR(16) NOT NULL,
    ""status""  VARCHAR(32) NOT NULL,
    ""duration_ms"" NUMBER NOT NULL,
    ""checksum"" VARCHAR(64) NOT NULL,
    ""failed_script_path""  VARCHAR(4000) NULL,
    ""failed_script_error""  VARCHAR(4000) NULL,
    ""additional_artifacts""  VARCHAR(4000) NULL,
    PRIMARY KEY (""sequence_id"")
);
             ";

        ///<inheritdoc/>
        public string GetSqlForGetCurrentVersion()
            => @"
SELECT TOP 1 ""version"" FROM ""${YUNIQL_DB_NAME}"".""${YUNIQL_SCHEMA_NAME}"".""${YUNIQL_TABLE_NAME}"" WHERE ""status"" = 'Successful' ORDER BY ""sequence_id"" DESC;
            ";

        ///<inheritdoc/>
        public string GetSqlForGetAllVersions()
            => @"
SELECT ""sequence_id"", ""version"", ""applied_on_utc"", ""applied_by_user"", ""applied_by_tool"", ""applied_by_tool_version"", ""status"", ""duration_ms"", ""checksum"", ""failed_script_path"", ""failed_script_error"", ""additional_artifacts""
FROM ""${YUNIQL_DB_NAME}"".""${YUNIQL_SCHEMA_NAME}"".""${YUNIQL_TABLE_NAME}"" ORDER BY ""version"" ASC;
            ";

        ///<inheritdoc/>
        public string GetSqlForInsertVersion()
            => @"
INSERT INTO ""${YUNIQL_DB_NAME}"".""${YUNIQL_SCHEMA_NAME}"".""${YUNIQL_TABLE_NAME}"" (""version"", ""applied_by_tool"", ""applied_by_tool_version"", ""status"", ""duration_ms"", ""checksum"",  ""failed_script_path"", ""failed_script_error"", ""additional_artifacts"")
VALUES ('${YUNIQL_VERSION}', '${YUNIQL_APPLIED_BY_TOOL}', '${YUNIQL_APPLIED_BY_TOOL_VERSION}', '${YUNIQL_STATUS}', '${YUNIQL_DURATION_MS}', '${YUNIQL_CHECKSUM}', '${YUNIQL_FAILED_SCRIPT_PATH}', '${YUNIQL_FAILED_SCRIPT_ERROR}', '${YUNIQL_ADDITIONAL_ARTIFACTS}');
            ";

        ///<inheritdoc/>
        public string GetSqlForUpdateVersion()
            => @"
UPDATE ""${YUNIQL_SCHEMA_NAME}"".""${YUNIQL_TABLE_NAME}""
SET 	
	""applied_on_utc""         =  CURRENT_TIMESTAMP(),
	""applied_by_user""        =  CURRENT_USER(),
	""applied_by_tool""        = '${YUNIQL_APPLIED_BY_TOOL}', 
	""applied_by_tool_version"" = '${YUNIQL_APPLIED_BY_TOOL_VERSION}',
	""status""                 = '${YUNIQL_STATUS}',
	""duration_ms""            = '${YUNIQL_DURATION_MS}',
	""failed_script_path""     = '${YUNIQL_FAILED_SCRIPT_PATH}',
	""failed_script_error""    = '${YUNIQL_FAILED_SCRIPT_ERROR}',
	""additional_artifacts""   = '${YUNIQL_ADDITIONAL_ARTIFACTS}' 
WHERE
	""version""                = '${YUNIQL_VERSION}';
            ";

        //https://docs.snowflake.com/en/sql-reference/sql/merge.html
        ///<inheritdoc/>
        public string GetSqlForUpsertVersion()
            => throw new NotSupportedException("Not supported for the target platform");

        ///<inheritdoc/>
        public string GetSqlForCheckRequireMetaSchemaUpgrade(string currentSchemaVersion)
            => @"
SELECT NULL;
            ";

        ///<inheritdoc/>
        public string GetSqlForUpgradeMetaSchema(string requiredSchemaVersion)
        {
            var assembly = typeof(SnowflakeDataService).Assembly;
            var resource = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.SchemaUpgrade_{requiredSchemaVersion.Replace(".", "_")}.sql");
            using var reader = new StreamReader(resource);
            return reader.ReadToEnd();
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
