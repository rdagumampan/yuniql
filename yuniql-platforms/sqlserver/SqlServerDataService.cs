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
	                        [SequenceId] [SMALLINT] IDENTITY(1,1) NOT NULL,
	                        [Version] [NVARCHAR](512) NOT NULL,
	                        [AppliedOnUtc] [DATETIME] NOT NULL,
	                        [AppliedByUser] [NVARCHAR](32) NOT NULL,
	                        [AppliedByTool] [NVARCHAR](32) NULL,
	                        [AppliedByToolVersion] [NVARCHAR](16) NULL,
	                        [AdditionalArtifacts] [VARBINARY](MAX) NULL,
                         CONSTRAINT [PK___YuniqlDbVersion] PRIMARY KEY CLUSTERED ([SequenceId] ASC),
                         CONSTRAINT [IX___YuniqlDbVersion] UNIQUE NONCLUSTERED  ([Version] ASC
                        ));

                        ALTER TABLE [dbo].[__YuniqlDbVersion] ADD  CONSTRAINT [DF___YuniqlDbVersion_AppliedOnUtc]  DEFAULT (GETUTCDATE()) FOR [AppliedOnUtc];
                        ALTER TABLE [dbo].[__YuniqlDbVersion] ADD  CONSTRAINT [DF___YuniqlDbVersion_AppliedByUser]  DEFAULT (SUSER_SNAME()) FOR [AppliedByUser];
                    END                
            ";

        public string GetSqlForGetCurrentVersion()
            => @"SELECT TOP 1 Version FROM [dbo].[__YuniqlDbVersion] ORDER BY SequenceId DESC;";

        public string GetSqlForGetAllVersions()
            => @"SELECT SequenceId, Version, AppliedOnUtc, AppliedByUser, AppliedByTool, AppliedByToolVersion FROM [dbo].[__YuniqlDbVersion] ORDER BY Version ASC;";

        public string GetSqlForInsertVersion()
            => @"INSERT INTO [dbo].[__YuniqlDbVersion] (Version, AppliedByTool, AppliedByToolVersion) VALUES ('{0}','{1}','{2}');";
    }
}
