using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using Yuniql.Extensibility;

namespace Yuniql.SqlServer
{
    public class SqlServerDataService : IDataService
    {
        private int _commandTimeout = 30;
        private string _connectionString;
        private readonly ITraceService _traceService;

        public SqlServerDataService(ITraceService traceService)
        {
            this._traceService = traceService;
        }

        public bool IsAtomicDDLSupported => true;

        public void ExecuteNonQuery(string connectionString, string sqlStatement, int commandTimeout = 30)
        {
            _traceService.Debug($"Executing sql statement: {Environment.NewLine}{sqlStatement}");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlStatement;
                command.CommandTimeout = commandTimeout;
                command.ExecuteNonQuery();
            }
        }

        public bool QuerySingleBool(string connectionString, string sqlStatement, int commandTimeout = 30)
        {
            _traceService.Debug($"Executing sql statement: {Environment.NewLine}{sqlStatement}");

            bool result = false;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlStatement;
                command.CommandTimeout = commandTimeout;

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

        public string QuerySingleString(string connectionString, string sqlStatement, int commandTimeout = 30)
        {
            _traceService.Debug($"Executing sql statement: {Environment.NewLine}{sqlStatement}");

            string result = null;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlStatement;
                command.CommandTimeout = commandTimeout;

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

        public void ExecuteNonQuery(IDbConnection activeConnection, string sqlStatement, IDbTransaction transaction = null, int commandTimeout = 30)
        {
            _traceService.Debug($"Executing sql statement: {Environment.NewLine}{sqlStatement}");

            var command = activeConnection.CreateCommand();
            command.Transaction = transaction;
            command.CommandType = CommandType.Text;
            command.CommandText = sqlStatement;
            command.CommandTimeout = commandTimeout;
            command.ExecuteNonQuery();
        }

        public int ExecuteScalar(IDbConnection activeConnection, string sqlStatement, IDbTransaction transaction = null, int commandTimeout = 30)
        {
            _traceService.Debug($"Executing sql statement: {Environment.NewLine}{sqlStatement}");

            var result = 0;

            var command = activeConnection.CreateCommand();
            command.Transaction = transaction;
            command.CommandType = CommandType.Text;
            command.CommandText = sqlStatement;
            command.CommandTimeout = commandTimeout;
            result = command.ExecuteNonQuery();

            return result;
        }

        public bool QuerySingleBool(IDbConnection activeConnection, string sqlStatement, IDbTransaction transaction = null, int commandTimeout = 30)
        {
            _traceService.Debug($"Executing sql statement: {Environment.NewLine}{sqlStatement}");

            bool result = false;
            var command = activeConnection.CreateCommand();
            command.Transaction = transaction;
            command.CommandType = CommandType.Text;
            command.CommandText = sqlStatement;
            command.CommandTimeout = commandTimeout;

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    result = Convert.ToBoolean(reader.GetValue(0));
                }
            }

            return result;
        }

        public string QuerySingleString(IDbConnection activeConnection, string sqlStatement, IDbTransaction transaction = null, int commandTimeout = 30)
        {
            _traceService.Debug($"Executing sql statement: {Environment.NewLine}{sqlStatement}");

            string result = null;

            var command = activeConnection.CreateCommand();
            command.Transaction = transaction;
            command.CommandType = CommandType.Text;
            command.CommandText = sqlStatement;
            command.CommandTimeout = commandTimeout;

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    result = reader.GetString(0);
                }
            }
            return result;
        }

        public void Initialize(string connectionString, int commandTimeout = 30)
        {
            this._connectionString = connectionString;
            this._commandTimeout = commandTimeout;
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public bool IsTargetDatabaseExists()
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(_connectionString);
            var sqlStatement = $"SELECT ISNULL(database_id,0) FROM [sys].[databases] WHERE name = '{connectionStringBuilder.InitialCatalog}'";

            //check if database exists and auto-create when its not
            var masterConnectionStringBuilder = new SqlConnectionStringBuilder(_connectionString);
            masterConnectionStringBuilder.InitialCatalog = "master";

            var result = QuerySingleBool(masterConnectionStringBuilder.ConnectionString, sqlStatement, _commandTimeout);

            return result;
        }

        public void CreateDatabase()
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(_connectionString);
            var sqlStatement = $"CREATE DATABASE {connectionStringBuilder.InitialCatalog};";

            var masterConnectionStringBuilder = new SqlConnectionStringBuilder(_connectionString);
            masterConnectionStringBuilder.InitialCatalog = "master";

            ExecuteNonQuery(masterConnectionStringBuilder.ConnectionString, sqlStatement, _commandTimeout);
        }

        public bool IsTargetDatabaseConfigured()
        {
            var sqlStatement = $"SELECT ISNULL(object_id,0) FROM [sys].[tables] WHERE name = '__YuniqlDbVersion'";
            var result = QuerySingleBool(_connectionString, sqlStatement, _commandTimeout);

            return result;
        }

        public void ConfigureDatabase()
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(_connectionString);
            var sqlStatement = $@"
                    IF OBJECT_ID('[dbo].[__YuniqlDbVersion]') IS NULL 
                    BEGIN
	                    CREATE TABLE [dbo].[__YuniqlDbVersion](
		                    [Id] [SMALLINT] IDENTITY(1,1),
		                    [Version] [NVARCHAR](32) NOT NULL,
		                    [DateInsertedUtc] [DATETIME] NOT NULL,
		                    [LastUpdatedUtc] [DATETIME] NOT NULL,
		                    [LastUserId] [NVARCHAR](128) NOT NULL,
		                    [Artifact] [VARBINARY](MAX) NULL,
		                    CONSTRAINT [PK___YuniqlDbVersion] PRIMARY KEY CLUSTERED ([Id] ASC),
		                    CONSTRAINT [IX___YuniqlDbVersion] UNIQUE NONCLUSTERED ([Version] ASC)
	                    );

	                    ALTER TABLE [dbo].[__YuniqlDbVersion] ADD  CONSTRAINT [DF___YuniqlDbVersion_DateInsertedUtc]  DEFAULT (GETUTCDATE()) FOR [DateInsertedUtc];
	                    ALTER TABLE [dbo].[__YuniqlDbVersion] ADD  CONSTRAINT [DF___YuniqlDbVersion_LastUpdatedUtc]  DEFAULT (GETUTCDATE()) FOR [LastUpdatedUtc];
	                    ALTER TABLE [dbo].[__YuniqlDbVersion] ADD  CONSTRAINT [DF___YuniqlDbVersion_LastUserId]  DEFAULT (SUSER_SNAME()) FOR [LastUserId];
                    END                
            ";

            _traceService.Debug($"Executing sql statement: {Environment.NewLine}{sqlStatement}", _commandTimeout);

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlStatement;
                command.CommandTimeout = _commandTimeout;
                command.ExecuteNonQuery();
            }
        }

        public string GetCurrentVersion()
        {
            var sqlStatement = $"SELECT TOP 1 Version FROM [dbo].[__YuniqlDbVersion] ORDER BY Id DESC;";
            var result = QuerySingleString(_connectionString, sqlStatement, _commandTimeout);

            return result;
        }

        public List<DbVersion> GetAllVersions()
        {
            var result = new List<DbVersion>();

            var sqlStatement = $"SELECT Id, Version, DateInsertedUtc, LastUserId FROM [dbo].[__YuniqlDbVersion] ORDER BY Version ASC;";
            _traceService.Debug($"Executing sql statement: {Environment.NewLine}{sqlStatement}");

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlStatement;
                command.CommandTimeout = _commandTimeout;

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

        public void UpdateVersion(IDbConnection activeConnection, IDbTransaction transaction, string version, int commandTimeOut = 30)
        {
            var sqlStatement = $"INSERT INTO [dbo].[__YuniqlDbVersion] (Version) VALUES ('{version}');";
            _traceService.Debug($"Executing sql statement: {Environment.NewLine}{sqlStatement}");

            var command = activeConnection.CreateCommand();
            command.Transaction = transaction;
            command.CommandType = CommandType.Text;
            command.CommandText = sqlStatement;
            command.CommandTimeout = commandTimeOut;
            command.ExecuteNonQuery();
        }

        //https://stackoverflow.com/questions/25563876/executing-sql-batch-containing-go-statements-in-c-sharp/25564722#25564722
        public List<string> BreakStatements(string sqlStatementRaw)
        {
            return Regex.Split(sqlStatementRaw, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
        }

        public ConnectionInfo GetConnectionInfo()
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(_connectionString);
            return new ConnectionInfo {DataSource = connectionStringBuilder.DataSource, Database = connectionStringBuilder.InitialCatalog };
        }
    }
}
