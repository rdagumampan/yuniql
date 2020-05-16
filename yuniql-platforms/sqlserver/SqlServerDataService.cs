using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using Yuniql.Extensibility;
using Yuniql.Extensibility.SqlBatchParser;

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

        public bool IsBatchSqlSupported { get; } = true;

        public string TableName { get; set; } = "__yuniqldbversion";

        public string SchemaName { get; set; } = "dbo";

        public List<string> BreakStatements(string sqlStatementRaw)
        {
            var sqlBatchParser = new SqlBatchParser(_traceService, new GoSqlBatchLineAnalyzer(), new CStyleCommentAnalyzer());
            return sqlBatchParser.Parse(sqlStatementRaw).Select(s => s.BatchText).ToList();
        }

        public string GetSqlForCheckIfDatabaseExists()
            => @"SELECT ISNULL(database_id, 0) FROM [sys].[databases] WHERE name = '${YUNIQL_DB_NAME}'";

        public string GetSqlForCreateDatabase()
            => @"CREATE DATABASE [${YUNIQL_DB_NAME}];";

        public string GetSqlForCreateSchema()
            => @"CREATE SCHEMA [${YUNIQL_SCHEMA_NAME}];";

        public string GetSqlForCheckIfDatabaseConfigured()
            => @"SELECT ISNULL(OBJECT_ID('[${YUNIQL_SCHEMA_NAME}].[${YUNIQL_TABLE_NAME}]'), 0)";

        public string GetSqlForConfigureDatabase()
            => @"
                    IF OBJECT_ID('[${YUNIQL_SCHEMA_NAME}].[${YUNIQL_TABLE_NAME}]') IS NULL 
                    BEGIN
                        CREATE TABLE [${YUNIQL_SCHEMA_NAME}].[${YUNIQL_TABLE_NAME}] (
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

                        ALTER TABLE [${YUNIQL_SCHEMA_NAME}].[${YUNIQL_TABLE_NAME}] ADD  CONSTRAINT [DF___YuniqlDbVersion_AppliedOnUtc]  DEFAULT (GETUTCDATE()) FOR [AppliedOnUtc];
                        ALTER TABLE [${YUNIQL_SCHEMA_NAME}].[${YUNIQL_TABLE_NAME}] ADD  CONSTRAINT [DF___YuniqlDbVersion_AppliedByUser]  DEFAULT (SUSER_SNAME()) FOR [AppliedByUser];
                    END                
            ";

        public string GetSqlForGetCurrentVersion()
            => @"SELECT TOP 1 Version FROM [${YUNIQL_SCHEMA_NAME}].[${YUNIQL_TABLE_NAME}] ORDER BY SequenceId DESC;";

        public string GetSqlForGetAllVersions()
            => @"SELECT SequenceId, Version, AppliedOnUtc, AppliedByUser, AppliedByTool, AppliedByToolVersion FROM [${YUNIQL_SCHEMA_NAME}].[${YUNIQL_TABLE_NAME}] ORDER BY Version ASC;";

        public string GetSqlForInsertVersion()
            => @"INSERT INTO [${YUNIQL_SCHEMA_NAME}].[${YUNIQL_TABLE_NAME}] (Version, AppliedByTool, AppliedByToolVersion) VALUES ('{0}','{1}','{2}');";
    }
}
