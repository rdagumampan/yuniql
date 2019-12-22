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

        public bool IsAtomicDDLSupported => true;

        public void Initialize(string connectionString)
        {
            this._connectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        public void ExecuteNonQuery(string connectionString, string sqlStatement)
        {
            _traceService.Debug($"Executing sql statement: {Environment.NewLine}{sqlStatement}");

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlStatement;
                command.CommandTimeout = 0;
                command.ExecuteNonQuery();
            }
        }

        public bool QuerySingleBool(string connectionString, string sqlStatement)
        {
            _traceService.Debug($"Executing sql statement: {Environment.NewLine}{sqlStatement}");

            bool result = false;
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlStatement;
                command.CommandTimeout = 0;

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result = Convert.ToBoolean(reader.GetValue(0));
                    }
                }
            }

            return result;
        }

        public string QuerySingleString(string connectionString, string sqlStatement)
        {
            _traceService.Debug($"Executing sql statement: {Environment.NewLine}{sqlStatement}");

            string result = null;
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlStatement;
                command.CommandTimeout = 0;

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result = reader.GetString(0);
                    }
                }
            }

            return result;
        }

        public void ExecuteNonQuery(IDbConnection activeConnection, string sqlStatement, IDbTransaction transaction = null)
        {
            _traceService.Debug($"Executing sql statement: {Environment.NewLine}{sqlStatement}");

            var command = activeConnection.CreateCommand();
            command.Transaction = transaction;
            command.CommandType = CommandType.Text;
            command.CommandText = sqlStatement;
            command.CommandTimeout = 0;
            command.ExecuteNonQuery();
        }

        public int ExecuteScalar(IDbConnection activeConnection, string sqlStatement, IDbTransaction transaction = null)
        {
            _traceService.Debug($"Executing sql statement: {Environment.NewLine}{sqlStatement}");

            var result = 0;

            var command = activeConnection.CreateCommand();
            command.Transaction = transaction;
            command.CommandType = CommandType.Text;
            command.CommandText = sqlStatement;
            command.CommandTimeout = 0;
            result = command.ExecuteNonQuery();

            return result;
        }

        public bool QuerySingleBool(IDbConnection activeConnection, string sqlStatement, IDbTransaction transaction = null)
        {
            _traceService.Debug($"Executing sql statement: {Environment.NewLine}{sqlStatement}");

            bool result = false;
            var command = activeConnection.CreateCommand();
            command.Transaction = transaction;
            command.CommandType = CommandType.Text;
            command.CommandText = sqlStatement;
            command.CommandTimeout = 0;

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    result = Convert.ToBoolean(reader.GetValue(0));
                }
            }

            return result;
        }

        public string QuerySingleString(IDbConnection activeConnection, string sqlStatement, IDbTransaction transaction = null)
        {
            _traceService.Debug($"Executing sql statement: {Environment.NewLine}{sqlStatement}");

            string result = null;

            var command = activeConnection.CreateCommand();
            command.Transaction = transaction;
            command.CommandType = CommandType.Text;
            command.CommandText = sqlStatement;
            command.CommandTimeout = 0;

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    result = reader.GetString(0);
                }
            }
            return result;
        }

        public bool IsTargetDatabaseExists()
        {
            //use the target user database to migrate, this is part of orig connection string
            var connectionStringBuilder = new MySqlConnectionStringBuilder(_connectionString);
            var sqlStatement = $"SELECT 1 FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{connectionStringBuilder.Database}';";

            //switch database into master/system database where db catalogs are maintained
            connectionStringBuilder.Database = "INFORMATION_SCHEMA";
            return QuerySingleBool(connectionStringBuilder.ConnectionString, sqlStatement);
        }

        public void CreateDatabase()
        {
            //use the target user database to migrate, this is part of orig connection string
            var connectionStringBuilder = new MySqlConnectionStringBuilder(_connectionString);
            var sqlStatement = $"CREATE DATABASE {connectionStringBuilder.Database};";

            //switch database into master/system database where db catalogs are maintained
            connectionStringBuilder.Database = "INFORMATION_SCHEMA";
            ExecuteNonQuery(connectionStringBuilder.ConnectionString, sqlStatement);
        }

        public bool IsTargetDatabaseConfigured()
        {
            var connectionStringBuilder = new MySqlConnectionStringBuilder(_connectionString);
            var sqlStatement = $"SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{connectionStringBuilder.Database}' AND TABLE_NAME = '__YuniqlDbVersion' LIMIT 1;";
            var result = QuerySingleBool(_connectionString, sqlStatement);

            return result;
        }

        public void ConfigureDatabase()
        {
            var connectionStringBuilder = new MySqlConnectionStringBuilder(_connectionString);
            var sqlStatement = $@"
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

            _traceService.Debug($"Executing sql statement: {Environment.NewLine}{sqlStatement}");

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlStatement;
                command.CommandTimeout = 0;
                command.ExecuteNonQuery();
            }
        }

        public string GetCurrentVersion()
        {
            var sqlStatement = $"SELECT Version FROM __YuniqlDbVersion ORDER BY Id DESC LIMIT 1;";
            return QuerySingleString(_connectionString, sqlStatement);
        }

        public List<DbVersion> GetAllVersions()
        {
            var result = new List<DbVersion>();

            var sqlStatement = $"SELECT Id, Version, DateInsertedUtc, LastUserId FROM __YuniqlDbVersion ORDER BY Version ASC;";
            _traceService.Debug($"Executing sql statement: {Environment.NewLine}{sqlStatement}");

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlStatement;
                command.CommandTimeout = 0;

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var dbVersion = new DbVersion
                    {
                        Id = reader.GetInt16(0),
                        Version = reader.GetString(1),
                        DateInsertedUtc = reader.GetDateTime(2),
                        LastUserId = reader.GetString(3)
                    };
                    result.Add(dbVersion);
                }
            }

            return result;
        }

        public void UpdateVersion(IDbConnection activeConnection, IDbTransaction transaction, string version)
        {
            var incrementVersionSqlStatement = $"INSERT INTO __YuniqlDbVersion (Version, DateInsertedUtc, LastUpdatedUtc, LastUserId) VALUES ('{version}', NOW(), NOW(), USER());";
            _traceService.Debug($"Executing sql statement: {Environment.NewLine}{incrementVersionSqlStatement}");

            var command = activeConnection.CreateCommand();
            command.Transaction = transaction;
            command.CommandType = CommandType.Text;
            command.CommandText = incrementVersionSqlStatement;
            command.CommandTimeout = 0;
            command.ExecuteNonQuery();
        }

        public List<string> BreakStatements(string sqlStatementRaw)
        {
            return new List<string> { sqlStatementRaw };
        }

        public ConnectionInfo GetConnectionInfo()
        {
            var connectionStringBuilder = new MySqlConnectionStringBuilder(_connectionString);
            return new ConnectionInfo {DataSource = connectionStringBuilder.Server, Database = connectionStringBuilder.Database };
        }
    }
}
