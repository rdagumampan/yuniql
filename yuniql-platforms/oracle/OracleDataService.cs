using System.Collections.Generic;
using System.Data;
using Yuniql.Extensibility;
using Oracle.ManagedDataAccess.Client;
using System;
using System.IO;
using Yuniql.Extensibility.SqlBatchParser;
using System.Linq;

namespace Yuniql.Oracle
{
    ///<inheritdoc/>
    public class OracleDataService : IDataService, IMixableTransaction
    {
        private string _connectionString;
        private readonly ITraceService _traceService;

        ///<inheritdoc/>
        public OracleDataService(ITraceService traceService)
        {
            this._traceService = traceService;
        }

        ///<inheritdoc/>
        public void Initialize(string connectionString)
        {
            this._connectionString = connectionString;
        }

        ///<inheritdoc/>
        public bool IsTransactionalDdlSupported => false;

        ///<inheritdoc/>
        public bool IsSchemaSupported { get; } = true;

        ///<inheritdoc/>
        public bool IsBatchSqlSupported { get; } = true;

        ///<inheritdoc/>
        public bool IsUpsertSupported => false;

        ///<inheritdoc/>
        public string TableName { get; set; } = "__yuniql_schema_version";

        ///<inheritdoc/>
        public string SchemaName { get; set; }

        ///<inheritdoc/>
        public IDbConnection CreateConnection()
        {
            return new OracleConnection(_connectionString);
        }

        ///<inheritdoc/>
        public IDbConnection CreateMasterConnection()
        {
            var masterConnectionStringBuilder = new OracleConnectionStringBuilder(_connectionString);
            masterConnectionStringBuilder.DataSource = "INFORMATION_SCHEMA";

            return new OracleConnection(masterConnectionStringBuilder.ConnectionString);
        }

        ///<inheritdoc/>
        public List<string> BreakStatements(string sqlStatementRaw)
        {
            var sqlBatchParser = new SqlBatchParser(_traceService, new GoSqlBatchLineAnalyzer(), new CommentAnalyzer());
            return sqlBatchParser.Parse(sqlStatementRaw).Select(s => s.BatchText).ToList();
        }

        ///<inheritdoc/>
        public ConnectionInfo GetConnectionInfo()
        {
            var connectionStringBuilder = new OracleConnectionStringBuilder(_connectionString);
            return new ConnectionInfo { DataSource = connectionStringBuilder.DataSource, Database = connectionStringBuilder.DataSource };
        }

        ///<inheritdoc/>
        public string GetSqlForCheckIfDatabaseExists()
            => @"
SELECT 1 FROM SYS.ALL_TABLES WHERE OWNER = '${YUNIQL_DB_NAME}';
            ";

        ///<inheritdoc/>
        public string GetSqlForCreateDatabase()
            => throw new NotSupportedException("Create database is not supported in Oracle.");

        ///<inheritdoc/>
        public string GetSqlForCreateSchema()
            => throw new NotSupportedException("Custom schema is not supported in Oracle.");

        ///<inheritdoc/>
        public string GetSqlForCheckIfDatabaseConfigured()
            => @"
SELECT 1 FROM SYS.ALL_TABLES WHERE OWNER = '${YUNIQL_DB_NAME}' AND TABLE_NAME = '${YUNIQL_TABLE_NAME}' AND ROWNUM = 1;
            ";

        ///<inheritdoc/>
        public string GetSqlForCheckIfDatabaseConfiguredv10()
            => @"
SELECT 1 FROM SYS.ALL_TABLES WHERE OWNER = '${YUNIQL_DB_NAME}' AND TABLE_NAME = '__yuniqldbversion' AND ROWNUM = 1;
            ";

        ///<inheritdoc/>
        public string GetSqlForConfigureDatabase()
            => @"
CREATE TABLE ${YUNIQL_TABLE_NAME} (
	sequence_id NUMBER NOT NULL,
	version VARCHAR2(190) NOT NULL,
	applied_on_utc TIMESTAMP NOT NULL,
	applied_by_user VARCHAR2(32) NOT NULL,
	applied_by_tool VARCHAR2(32) NOT NULL,
	applied_by_tool_version VARCHAR2(16) NOT NULL,
    status VARCHAR2(32) NOT NULL,
    duration_ms NUMBER NOT NULL,
    checksum VARCHAR2(64) NOT NULL,
    failed_script_path VARCHAR2(4000) NULL,
    failed_script_error VARCHAR2(4000) NULL,
    additional_artifacts VARCHAR2(4000) NULL,
    CONSTRAINT pk_${YUNIQL_TABLE_NAME} PRIMARY KEY (sequence_id),
	CONSTRAINT ix_${YUNIQL_TABLE_NAME} UNIQUE (version)
);
            ";

        ///<inheritdoc/>
        public string GetSqlForGetCurrentVersion()
            => @"
SELECT version FROM ${YUNIQL_TABLE_NAME} WHERE status = 'Successful' AND ROWNUM = 1 ORDER BY sequence_id DESC;
            ";

        ///<inheritdoc/>
        public string GetSqlForGetAllVersions()
            => @"
SELECT sequence_id, version, applied_on_utc, applied_by_user, applied_by_tool, applied_by_tool_version, status, duration_ms, checksum, failed_script_path, failed_script_error, additional_artifacts
FROM ${YUNIQL_TABLE_NAME} ORDER BY version ASC;
            ";

        ///<inheritdoc/>
        public string GetSqlForInsertVersion()
            => @"
INSERT INTO ${YUNIQL_TABLE_NAME} (version, applied_on_utc, applied_by_user, applied_by_tool, applied_by_tool_version, status, duration_ms, checksum, failed_script_path, failed_script_error, additional_artifacts) 
VALUES ('${YUNIQL_VERSION}', SYSDATE, USER, '${YUNIQL_APPLIED_BY_TOOL}', '${YUNIQL_APPLIED_BY_TOOL_VERSION}','${YUNIQL_STATUS}', '${YUNIQL_DURATION_MS}', '${YUNIQL_CHECKSUM}', '${YUNIQL_FAILED_SCRIPT_PATH}', '${YUNIQL_FAILED_SCRIPT_ERROR}', '${YUNIQL_ADDITIONAL_ARTIFACTS}');
            ";

        ///<inheritdoc/>
        public string GetSqlForUpdateVersion()
            => @"
UPDATE ${YUNIQL_TABLE_NAME}
SET
    applied_on_utc          =  SYSDATE,
    applied_by_user         =  USER,
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
        //when table __yuniqldbversion exists, we need to upgrade from yuniql v1.0 to v1.1 version
        => @"
SELECT 'v1.1' FROM SYS.ALL_TABLES WHERE OWNER = '${YUNIQL_DB_NAME}' AND TABLE_NAME = '__yuniqldbversion' AND ROWNUM = 1;
        ";

        ///<inheritdoc/>
        public string GetSqlForUpgradeMetaSchema(string requiredSchemaVersion)
        {
            var assembly = typeof(OracleDataService).Assembly;
            var resource = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.SchemaUpgrade_{requiredSchemaVersion.Replace(".", "_")}.sql");
            using var reader = new StreamReader(resource);
            return reader.ReadToEnd();
        }

        ///<inheritdoc/>
        public bool TryParseErrorFromException(Exception exception, out string result)
        {
            result = null;
            if (exception is OracleException sqlException)
            {
                result = $"(0x{sqlException.ErrorCode:X}) Error {sqlException.Number}: {sqlException.Message}";
                return true;
            }
            return false;
        }
    }
}