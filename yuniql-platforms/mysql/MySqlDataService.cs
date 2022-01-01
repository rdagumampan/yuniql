using System.Collections.Generic;
using System.Data;
using Yuniql.Extensibility;
using MySql.Data.MySqlClient;
using System;
using System.IO;

namespace Yuniql.MySql
{
    ///<inheritdoc/>
    public class MySqlDataService : IDataService, IMixableTransaction
    {
        private string _connectionString;
        private readonly ITraceService _traceService;
        
        ///<inheritdoc/>
        public MySqlDataService(ITraceService traceService)
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
        public bool IsMultiTenancySupported { get; } = true;

        ///<inheritdoc/>
        public bool IsSchemaSupported { get; } = false;

        ///<inheritdoc/>
        public bool IsBatchSqlSupported { get; } = false;

        ///<inheritdoc/>
        public bool IsUpsertSupported => false;

        ///<inheritdoc/>
        public string MetaTableName { get; set; } = "__yuniql_schema_version";

        ///<inheritdoc/>
        public string MetaSchemaName { get; set; } = string.Empty;

        ///<inheritdoc/>
        public IDbConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        ///<inheritdoc/>
        public IDbConnection CreateMasterConnection()
        {
            var masterConnectionStringBuilder = new MySqlConnectionStringBuilder(_connectionString);
            masterConnectionStringBuilder.Database = "INFORMATION_SCHEMA";

            return new MySqlConnection(masterConnectionStringBuilder.ConnectionString);
        }

        ///<inheritdoc/>
        public ConnectionInfo GetConnectionInfo()
        {
            var connectionStringBuilder = new MySqlConnectionStringBuilder(_connectionString);
            return new ConnectionInfo { DataSource = connectionStringBuilder.Server, Database = connectionStringBuilder.Database };
        }

        ///<inheritdoc/>
        public List<string> BreakStatements(string sqlStatementRaw)
        {
            return new List<string> { sqlStatementRaw };
        }

        ///<inheritdoc/>
        public string GetSqlForCheckIfDatabaseExists()
            => @"
SELECT 1 FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '${YUNIQL_DB_NAME}';
            ";

        ///<inheritdoc/>
        public string GetSqlForCreateDatabase()
            => @"
CREATE DATABASE `${YUNIQL_DB_NAME}`;
            ";

        ///<inheritdoc/>
        public List<string> GetSqlForDropDatabase()
            => new List<string> { @"
DROP DATABASE `${YUNIQL_DB_NAME}`;
            " };

        ///<inheritdoc/>
        public string GetSqlForCheckIfSchemaExists()
            => throw new NotSupportedException("Custom schema is not supported in MySql.");

        ///<inheritdoc/>
        public string GetSqlForCreateSchema()
            => throw new NotSupportedException("Custom schema is not supported in MySql.");

        ///<inheritdoc/>
        public string GetSqlForCheckIfDatabaseConfigured()
            => @"
SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '${YUNIQL_DB_NAME}' AND TABLE_NAME = '${YUNIQL_TABLE_NAME}' LIMIT 1;
            ";

        ///<inheritdoc/>
        public string GetSqlForCheckIfDatabaseConfiguredv10()
            => @"
SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '${YUNIQL_DB_NAME}' AND TABLE_NAME = '__yuniqldbversion' LIMIT 1;
            ";

        ///<inheritdoc/>
        public string GetSqlForConfigureDatabase()
            => @"
CREATE TABLE ${YUNIQL_TABLE_NAME} (
	sequence_id INT AUTO_INCREMENT PRIMARY KEY NOT NULL,
	version VARCHAR(190) NOT NULL,
	applied_on_utc TIMESTAMP NOT NULL,
	applied_by_user VARCHAR(128) NOT NULL,
	applied_by_tool VARCHAR(32) NOT NULL,
	applied_by_tool_version VARCHAR(16) NOT NULL,
    status VARCHAR(32) NOT NULL,
    duration_ms INT NOT NULL,
    checksum VARCHAR(64) NOT NULL,
    failed_script_path VARCHAR(4000) NULL,
    failed_script_error VARCHAR(4000) NULL,
    additional_artifacts VARCHAR(4000) NULL,
	CONSTRAINT ix_${YUNIQL_TABLE_NAME} UNIQUE (version)
) ENGINE=InnoDB;
            ";

        ///<inheritdoc/>
        public string GetSqlForGetCurrentVersion()
            => @"
SELECT version FROM ${YUNIQL_TABLE_NAME} WHERE status = 'Successful' ORDER BY sequence_id DESC LIMIT 1;
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
VALUES ('${YUNIQL_VERSION}', UTC_TIMESTAMP(), CURRENT_USER(), '${YUNIQL_APPLIED_BY_TOOL}', '${YUNIQL_APPLIED_BY_TOOL_VERSION}','${YUNIQL_STATUS}', '${YUNIQL_DURATION_MS}', '${YUNIQL_CHECKSUM}', '${YUNIQL_FAILED_SCRIPT_PATH}', '${YUNIQL_FAILED_SCRIPT_ERROR}', '${YUNIQL_ADDITIONAL_ARTIFACTS}');
            ";

        ///<inheritdoc/>
        public string GetSqlForUpdateVersion()
            => @"
UPDATE ${YUNIQL_TABLE_NAME}
SET
    applied_on_utc          =  UTC_TIMESTAMP(),
    applied_by_user         =  CURRENT_USER(),
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
            => @"
INSERT INTO ${YUNIQL_TABLE_NAME} (version, applied_on_utc, applied_by_user, applied_by_tool, applied_by_tool_version, additional_artifacts, status, checksum, failed_script_path, failed_script_error) 
VALUES ('${YUNIQL_VERSION}', UTC_TIMESTAMP(), CURRENT_USER(), '${YUNIQL_APPLIED_BY_TOOL}', '${YUNIQL_APPLIED_BY_TOOL_VERSION}', '${YUNIQL_ADDITIONAL_ARTIFACTS}', '${YUNIQL_STATUS}', '${YUNIQL_CHECKSUM}', '${YUNIQL_FAILED_SCRIPT_PATH}', '${YUNIQL_FAILED_SCRIPT_ERROR}')
ON DUPLICATE KEY UPDATE
    applied_on_utc = VALUES(applied_on_utc),
    applied_by_user = VALUES(applied_by_user),
    applied_by_tool = VALUES(applied_by_tool),
    applied_by_tool_version = VALUES(applied_by_tool_version),
    additional_artifacts = VALUES(additional_artifacts),
    status = VALUES(status),
    failed_script_path = VALUES(failed_script_path),
    failed_script_error = VALUES(failed_script_error);
            ";

        ///<inheritdoc/>
        public string GetSqlForCheckRequireMetaSchemaUpgrade(string currentSchemaVersion)
        //when table __yuniqldbversion exists, we need to upgrade from yuniql v1.0 to v1.1 version
        => @"
SELECT 'v1.1' FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '${YUNIQL_DB_NAME}' AND TABLE_NAME = '__yuniqldbversion' LIMIT 1;
        ";

        ///<inheritdoc/>
        public string GetSqlForUpgradeMetaSchema(string requiredSchemaVersion)
        {
            var assembly = typeof(MySqlDataService).Assembly;
            var resource = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.SchemaUpgrade_{requiredSchemaVersion.Replace(".", "_")}.sql");
            using var reader = new StreamReader(resource);
            return reader.ReadToEnd();
        }

        ///<inheritdoc/>
        public bool TryParseErrorFromException(Exception exception, out string result)
        {
            result = null;
            if (exception is MySqlException sqlException)
            {
                result = $"(0x{sqlException.ErrorCode:X}) Error {sqlException.Number}: {sqlException.Message}";
                return true;
            }
            return false;
        }
    }
}