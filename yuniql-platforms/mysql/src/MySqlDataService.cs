using System;
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

        public string GetCheckIfDatabaseExistsSql()
            => "SELECT 1 FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{0}';";

        public string GetCreateDatabaseSql()
            => "CREATE DATABASE {0};";

        public string GetCheckIfDatabaseConfiguredSql()
            => "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '__YuniqlDbVersion' LIMIT 1;";

        public string GetConfigureDatabaseSql()
            => @"
                CREATE TABLE __YuniqlDbVersion (
	                Id INT AUTO_INCREMENT PRIMARY KEY NOT NULL,
	                Version VARCHAR(32) NOT NULL,
	                DateInsertedUtc DATETIME NOT NULL,
	                LastUpdatedUtc DATETIME NOT NULL,
	                LastUserId VARCHAR(128) NOT NULL,
	                Artifact BLOB NULL,
	                CONSTRAINT IX___YuniqlDbVersion UNIQUE (Version)
                ) ENGINE=InnoDB;
            ";

        public string GetGetCurrentVersionSql()
            => "SELECT Version FROM __YuniqlDbVersion ORDER BY Id DESC LIMIT 1;";

        public string GetGetAllVersionsSql()
            => "SELECT Id, Version, DateInsertedUtc, LastUserId FROM __YuniqlDbVersion ORDER BY Version ASC;";

        public string GetUpdateVersionSql()
            => "INSERT INTO __YuniqlDbVersion (Version, DateInsertedUtc, LastUpdatedUtc, LastUserId) VALUES ('{0}', NOW(), NOW(), USER());";
    }
}