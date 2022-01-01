using System;
using System.Collections.Generic;
using System.Data;
using Yuniql.Extensibility;
using Npgsql;
using System.Collections;
using System.IO;

namespace Yuniql.Redshift
{
    ///<inheritdoc/>
    public class RedshiftDataService : IDataService
    {
        private string _connectionString;
        private readonly ITraceService _traceService;

        ///<inheritdoc/>
        public RedshiftDataService(ITraceService traceService)
        {
            this._traceService = traceService;
        }

        ///<inheritdoc/>
        public bool IsTransactionalDdlSupported => true;

        ///<inheritdoc/>
        public bool IsMultiTenancySupported { get; } = true;

        ///<inheritdoc/>
        public bool IsSchemaSupported { get; } = true;

        ///<inheritdoc/>
        public bool IsBatchSqlSupported { get; } = false;

        ///<inheritdoc/>
        public bool IsUpsertSupported => false;

        ///<inheritdoc/>
        public string MetaTableName { get; set; } = "__yuniql_schema_version";

        ///<inheritdoc/>
        public string MetaSchemaName { get; set; } = "public";

        ///<inheritdoc/>
        public void Initialize(string connectionString)
        {
            this._connectionString = connectionString;
        }

        ///<inheritdoc/>
        public IDbConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        ///<inheritdoc/>
        public IDbConnection CreateMasterConnection()
        {
            var masterConnectionStringBuilder = new NpgsqlConnectionStringBuilder(_connectionString);
            masterConnectionStringBuilder.Database = "dev";

            return new NpgsqlConnection(masterConnectionStringBuilder.ConnectionString);
        }

        ///<inheritdoc/>
        public ConnectionInfo GetConnectionInfo()
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(_connectionString);
            return new ConnectionInfo { DataSource = connectionStringBuilder.Host, Database = connectionStringBuilder.Database };
        }

        ///<inheritdoc/>
        public List<string> BreakStatements(string sqlStatementRaw)
        {
            return new List<string> { sqlStatementRaw };
        }

        ///<inheritdoc/>
        public string GetSqlForCheckIfDatabaseExists()
            => @"
SELECT 1 from pg_database WHERE datname = '${YUNIQL_DB_NAME}';
            ";

        ///<inheritdoc/>
        public string GetSqlForCreateDatabase()
            => @"
CREATE DATABASE ""${YUNIQL_DB_NAME}"";
            ";

        //NOTE: 25001: DROP DATABASE cannot run inside a multiple commands statement
        ///<inheritdoc/>
        public List<string> GetSqlForDropDatabase()
            => new List<string> {
@"
--disallow new connections, set exclusive to current session
ALTER DATABASE ""${YUNIQL_DB_NAME}"" CONNECTION LIMIT 1;
",
@"
--terminate existing connections
SELECT pg_terminate_backend(procpid) FROM pg_stat_activity WHERE datname = '${YUNIQL_DB_NAME}';
",
@"
--drop database
DROP DATABASE ""${YUNIQL_DB_NAME}"";
" };

        ///<inheritdoc/>
        public string GetSqlForCheckIfSchemaExists()
            => @"
SELECT 1 FROM information_schema.schemata WHERE schema_name = '${YUNIQL_SCHEMA_NAME}';
            ";

        //https://docs.aws.amazon.com/redshift/latest/dg/r_CREATE_SCHEMA.html
        ///<inheritdoc/>
        public string GetSqlForCreateSchema()
            => @"
CREATE SCHEMA ""${YUNIQL_SCHEMA_NAME}"";
            ";

        ///<inheritdoc/>
        public string GetSqlForCheckIfDatabaseConfigured()
            => @"
SELECT 1 FROM pg_tables WHERE  tablename = '${YUNIQL_TABLE_NAME}' AND schemaname = '${YUNIQL_SCHEMA_NAME}';
            ";

        ///<inheritdoc/>
        public string GetSqlForCheckIfDatabaseConfiguredv10()
            => @"
SELECT 1 FROM pg_tables WHERE  tablename = '__yuniqldbversion';
            ";

        //https://docs.aws.amazon.com/redshift/latest/dg/c_Supported_data_types.html
        ///<inheritdoc/>
        public string GetSqlForConfigureDatabase()
            => @"
CREATE TABLE ${YUNIQL_SCHEMA_NAME}.${YUNIQL_TABLE_NAME}(
    sequence_id  INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    version VARCHAR(512) NOT NULL,
    applied_on_utc TIMESTAMP NOT NULL DEFAULT(GETDATE()),
    applied_by_user VARCHAR(128) NOT NULL DEFAULT(CURRENT_USER),
    applied_by_tool VARCHAR(32) NOT NULL,
    applied_by_tool_version VARCHAR(16) NOT NULL,
    status VARCHAR(32) NOT NULL,
    duration_ms INTEGER NOT NULL,
    checksum VARCHAR(64) NOT NULL,
    failed_script_path VARCHAR(4000) NULL,
    failed_script_error VARCHAR(4000) NULL,
    additional_artifacts VARCHAR(4000) NULL,
    CONSTRAINT ix_${YUNIQL_TABLE_NAME} UNIQUE(version)
);
            ";

        ///<inheritdoc/>
        public string GetSqlForGetCurrentVersion()
            => @"
SELECT version FROM ${YUNIQL_SCHEMA_NAME}.${YUNIQL_TABLE_NAME} WHERE status = 'Successful' ORDER BY sequence_id DESC LIMIT 1;
            ";

        ///<inheritdoc/>
        public string GetSqlForGetAllVersions()
            => @"
SELECT sequence_id, version, applied_on_utc, applied_by_user, applied_by_tool, applied_by_tool_version, status, duration_ms, checksum, failed_script_path, failed_script_error, additional_artifacts 
FROM ${YUNIQL_SCHEMA_NAME}.${YUNIQL_TABLE_NAME} ORDER BY version ASC;
            ";

        ///<inheritdoc/>
        public string GetSqlForInsertVersion()
            => @"
INSERT INTO ${YUNIQL_SCHEMA_NAME}.${YUNIQL_TABLE_NAME} (version, applied_by_tool, applied_by_tool_version, status, duration_ms, checksum, failed_script_path, failed_script_error, additional_artifacts) 
VALUES ('${YUNIQL_VERSION}', '${YUNIQL_APPLIED_BY_TOOL}', '${YUNIQL_APPLIED_BY_TOOL_VERSION}', '${YUNIQL_STATUS}', '${YUNIQL_DURATION_MS}', '${YUNIQL_CHECKSUM}', '${YUNIQL_FAILED_SCRIPT_PATH}', '${YUNIQL_FAILED_SCRIPT_ERROR}', '${YUNIQL_ADDITIONAL_ARTIFACTS}');
            ";

        ///<inheritdoc/>
        public string GetSqlForUpdateVersion()
            => @"
UPDATE ${YUNIQL_SCHEMA_NAME}.${YUNIQL_TABLE_NAME}
SET 
    applied_on_utc          =  GETDATE(),
    applied_by_user         =  CURRENT_USER,
    applied_by_tool         = '${YUNIQL_APPLIED_BY_TOOL}', 
    applied_by_tool_version = '${YUNIQL_APPLIED_BY_TOOL_VERSION}', 
    status                  = '${YUNIQL_STATUS}', 
    duration_ms             = '${YUNIQL_DURATION_MS}', 
    failed_script_path      = '${YUNIQL_FAILED_SCRIPT_PATH}', 
    failed_script_error     = '${YUNIQL_FAILED_SCRIPT_ERROR}', 
    additional_artifacts    = '${YUNIQL_ADDITIONAL_ARTIFACTS}'
WHERE
    version                 = '${YUNIQL_VERSION}';
            ";

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
            var assembly = typeof(RedshiftDataService).Assembly;
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
                if (exception is PostgresException sqlException)
                {
                    var dataList = new List<string>();
                    foreach (DictionaryEntry item in sqlException.Data)
                        dataList.Add($"{item.Key}: {item.Value}");

                    result = $"(0x{sqlException.ErrorCode:X}) Error {sqlException.Message}. Exception data: {string.Join(", ", dataList)}";
                    return true;
                }
            }
            catch (Exception) { return false; }
            return false;
        }
    }
}
