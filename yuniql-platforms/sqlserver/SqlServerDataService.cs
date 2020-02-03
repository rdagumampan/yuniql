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
        private string _connectionString;
        private readonly ITraceService _traceService;

        public SqlServerDataService(ITraceService traceService)
        {
            this._traceService = traceService;
        }

        public void Initialize(string connectionString)
        {
            this._connectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public IDbConnection CreateMasterConnection()
        {
            var masterConnectionStringBuilder = new SqlConnectionStringBuilder(_connectionString);
            masterConnectionStringBuilder.InitialCatalog = "master";

            return new SqlConnection(masterConnectionStringBuilder.ConnectionString);
        }

        public ConnectionInfo GetConnectionInfo()
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(_connectionString);
            return new ConnectionInfo { DataSource = connectionStringBuilder.DataSource, Database = connectionStringBuilder.InitialCatalog };
        }

        public bool IsAtomicDDLSupported => true;

        public bool IsSchemaSupported { get; } = true;

        //https://stackoverflow.com/questions/25563876/executing-sql-batch-containing-go-statements-in-c-sharp/25564722#25564722
        public List<string> BreakStatements(string sqlStatementRaw)
        {
            return Regex.Split(sqlStatementRaw, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
        }

        public string GetSqlForCheckIfDatabaseExists()
            => @"SELECT ISNULL(database_id,0) FROM [sys].[databases] WHERE name = '{0}'";

        public string GetSqlForCreateDatabase()
            => @"CREATE DATABASE [{0}];";

        public string GetSqlForCheckIfDatabaseConfigured()
            => @"SELECT ISNULL(object_id,0) FROM [sys].[tables] WHERE name = '__YuniqlDbVersion'";

        public string GetSqlForConfigureDatabase()
            => @"
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

        public string GetSqlForGetCurrentVersion()
            => @"SELECT TOP 1 Version FROM [dbo].[__YuniqlDbVersion] ORDER BY Id DESC;";

        public string GetSqlForGetAllVersions()
            => @"SELECT Id, Version, DateInsertedUtc, LastUserId FROM [dbo].[__YuniqlDbVersion] ORDER BY Version ASC;";

        public string GetSqlForUpdateVersion()
            => @"INSERT INTO [dbo].[__YuniqlDbVersion] (Version) VALUES ('{0}');";
    }
}
