using System.Collections.Generic;
using System.Data;
using Yuniql.Extensibility;
using MySql.Data.MySqlClient;

namespace Yuniql.MySql
{
    public class MySqlDataService : IDataService
    {
        private string _connectionString;
        private readonly ITraceService _traceService;

        public MySqlDataService(ITraceService traceService)
        {
            this._traceService = traceService;
        }

        public void Initialize(string connectionString)
        {
            this._connectionString = connectionString;
        }

        public bool IsAtomicDDLSupported => false;

        public bool IsSchemaSupported { get; } = false;

        public IDbConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        public IDbConnection CreateMasterConnection()
        {
            var masterConnectionStringBuilder = new MySqlConnectionStringBuilder(_connectionString);
            masterConnectionStringBuilder.Database = "INFORMATION_SCHEMA";

            return new MySqlConnection(masterConnectionStringBuilder.ConnectionString);
        }

        public List<string> BreakStatements(string sqlStatementRaw)
        {
            return new List<string> { sqlStatementRaw };
        }

        public ConnectionInfo GetConnectionInfo()
        {
            var connectionStringBuilder = new MySqlConnectionStringBuilder(_connectionString);
            return new ConnectionInfo { DataSource = connectionStringBuilder.Server, Database = connectionStringBuilder.Database };
        }

        public string GetSqlForCheckIfDatabaseExists()
            => @"SELECT 1 FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{0}';";

        public string GetSqlForCreateDatabase()
            => @"CREATE DATABASE `{0}`;";

        public string GetSqlForCheckIfDatabaseConfigured()
            => @"SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '__yuniqldbversion' LIMIT 1;";

        public string GetSqlForConfigureDatabase()
            => @"
                CREATE TABLE __yuniqldbversion (
	                sequence_id INT AUTO_INCREMENT PRIMARY KEY NOT NULL,
	                version VARCHAR(512) NOT NULL,
	                applied_on_utc TIMESTAMP NOT NULL,
	                applied_by_user VARCHAR(32) NOT NULL,
	                applied_by_tool VARCHAR(32) NULL,
	                applied_by_tool_version VARCHAR(16) NULL,
	                additional_artifacts BLOB NULL,
	                CONSTRAINT ix___yuniqldbversion UNIQUE (version)
                ) ENGINE=InnoDB;
            ";

        public string GetSqlForGetCurrentVersion()
            => @"SELECT version FROM __yuniqldbversion ORDER BY sequence_id DESC LIMIT 1;";

        public string GetSqlForGetAllVersions()
            => @"SELECT sequence_id, version, applied_on_utc, applied_by_user, applied_by_tool, applied_by_tool_version FROM __yuniqldbversion ORDER BY version ASC;";

        public string GetSqlForInsertVersion()
            => @"INSERT INTO __yuniqldbversion (version, applied_on_utc, applied_by_user, applied_by_tool, applied_by_tool_version) VALUES ('{0}', UTC_TIMESTAMP(), CURRENT_USER(), '{1}', '{2}');";
    }
}