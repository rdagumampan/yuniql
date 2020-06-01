using System;
using System.Collections.Generic;
using System.Data;
using Yuniql.Extensibility;
using Npgsql;

namespace Yuniql.PostgreSql
{
    ///<inheritdoc/>
    public class PostgreSqlDataService : IDataService
    {
        private string _connectionString;
        private readonly ITraceService _traceService;

        ///<inheritdoc/>
        public PostgreSqlDataService(ITraceService traceService)
        {
            this._traceService = traceService;
        }

        ///<inheritdoc/>
        public bool IsAtomicDDLSupported => true;

        ///<inheritdoc/>
        public bool IsSchemaSupported { get; } = true;

        ///<inheritdoc/>
        public bool IsBatchSqlSupported { get; } = false;

        ///<inheritdoc/>
        public string TableName { get; set; } = "__yuniqldbversion";

        ///<inheritdoc/>
        public string SchemaName { get; set; } = "public";

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
            masterConnectionStringBuilder.Database = "postgres";

            return new NpgsqlConnection(masterConnectionStringBuilder.ConnectionString);
        }

        ///<inheritdoc/>
        public List<string> BreakStatements(string sqlStatementRaw)
        {
            return new List<string> { sqlStatementRaw };
        }

        ///<inheritdoc/>
        public ConnectionInfo GetConnectionInfo()
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(_connectionString);
            return new ConnectionInfo { DataSource = connectionStringBuilder.Host, Database = connectionStringBuilder.Database };
        }

        ///<inheritdoc/>
        public string GetSqlForCheckIfDatabaseExists()
            => @"SELECT 1 from pg_database WHERE datname = '${YUNIQL_DB_NAME}';";

        ///<inheritdoc/>
        public string GetSqlForCreateDatabase()
            => "CREATE DATABASE \"${YUNIQL_DB_NAME}\";";

        ///<inheritdoc/>
        public string GetSqlForCreateSchema()
            => "CREATE SCHEMA \"${YUNIQL_SCHEMA_NAME}\";";

        ///<inheritdoc/>
        public string GetSqlForCheckIfDatabaseConfigured()
            => @"SELECT 1 FROM pg_tables WHERE  tablename = '${YUNIQL_TABLE_NAME}'";

        ///<inheritdoc/>
        public string GetSqlForConfigureDatabase()
            => @"CREATE TABLE ${YUNIQL_SCHEMA_NAME}.${YUNIQL_TABLE_NAME}(
                    sequence_id  SMALLSERIAL PRIMARY KEY NOT NULL,
                    version VARCHAR(512) NOT NULL,
                    applied_on_utc TIMESTAMP NOT NULL DEFAULT(current_timestamp AT TIME ZONE 'UTC'),
                    applied_by_user VARCHAR(32) NOT NULL DEFAULT(user),
                    applied_by_tool VARCHAR(32) NULL,
                    applied_by_tool_version VARCHAR(16) NULL,
                    additional_artifacts BYTEA NULL,
                    CONSTRAINT ix___yuniqldbversion UNIQUE(version)
	            );";

        ///<inheritdoc/>
        public string GetSqlForGetCurrentVersion()
            => @"SELECT version FROM ${YUNIQL_SCHEMA_NAME}.${YUNIQL_TABLE_NAME} ORDER BY sequence_id DESC LIMIT 1;";

        ///<inheritdoc/>
        public string GetSqlForGetAllVersions()
            => @"SELECT sequence_id, version, applied_on_utc, applied_by_user, applied_by_tool, applied_by_tool_version, additional_artifacts FROM ${YUNIQL_SCHEMA_NAME}.${YUNIQL_TABLE_NAME} ORDER BY version ASC;";

        public string GetSqlForGetAllVersionAsJson()
            => @"SELECT json_agg(t)::jsonb FROM
            (SELECT sequence_id, version, applied_on_utc, applied_by_user, applied_by_tool, applied_by_tool_version FROM ${YUNIQL_SCHEMA_NAME}.${YUNIQL_TABLE_NAME} ORDER BY version ASC) t;";
            
        public string GetSqlForInsertVersionWithArtifact()
            => @"INSERT INTO ${YUNIQL_SCHEMA_NAME}.${YUNIQL_TABLE_NAME} (version, applied_by_tool, applied_by_tool_version, additional_artifacts) VALUES ('{0}', '{1}', '{2}', '{3}');";

        public string GetSqlForClearAllVersions()
            => @"TRUNCATE ${YUNIQL_SCHEMA_NAME}.${YUNIQL_TABLE_NAME}  CASCADE;";
            
        ///<inheritdoc/>
        public string GetSqlForInsertVersion()
            => @"INSERT INTO ${YUNIQL_SCHEMA_NAME}.${YUNIQL_TABLE_NAME} (version, applied_by_tool, applied_by_tool_version, additional_artifacts) VALUES (@version, @toolName, @toolVersion, @additionalArtifacts);";

        ///<inheritdoc/>
        public bool UpdateDatabaseConfiguration(IDbConnection dbConnection, ITraceService traceService = null, string schemaName = null, string tableName = null)
        {
            //no need to update tracking table as the structure has no been changed so far
            return false;
        }

        ///<inheritdoc/>
        public bool TryParseErrorFromException(Exception exc, out string result)
        {
            result = null;
            return false;
        }
    }
}
