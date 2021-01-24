using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Yuniql.Extensibility;
using Yuniql.Extensibility.SqlBatchParser;

namespace Yuniql.SqlServer
{
    ///<inheritdoc/>
    public class SqlServerDataService : IDataService
    {
        private string _connectionString;
        private readonly ITraceService _traceService;

        ///<inheritdoc/>
        public SqlServerDataService(ITraceService traceService)
        {
            this._traceService = traceService;
        }

        ///<inheritdoc/>
        public void Initialize(string connectionString)
        {
            this._connectionString = connectionString;
        }

        ///<inheritdoc/>
        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        ///<inheritdoc/>
        public IDbConnection CreateMasterConnection()
        {
            var masterConnectionStringBuilder = new SqlConnectionStringBuilder(_connectionString);
            masterConnectionStringBuilder.InitialCatalog = "master";

            return new SqlConnection(masterConnectionStringBuilder.ConnectionString);
        }

        ///<inheritdoc/>
        public ConnectionInfo GetConnectionInfo()
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(_connectionString);
            return new ConnectionInfo { DataSource = connectionStringBuilder.DataSource, Database = connectionStringBuilder.InitialCatalog };
        }

        ///<inheritdoc/>
        public bool IsTransactionalDdlSupported => true;

        ///<inheritdoc/>
        public bool IsSchemaSupported { get; } = true;

        ///<inheritdoc/>
        public bool IsBatchSqlSupported { get; } = true;

        ///<inheritdoc/>
        public bool IsUpsertSupported { get; } = false;

        ///<inheritdoc/>
        public string TableName { get; set; } = "__yuniqldbversion";

        ///<inheritdoc/>
        public string SchemaName { get; set; } = "dbo";

        ///<inheritdoc/>
        public List<string> BreakStatements(string sqlStatementRaw)
        {
            var sqlBatchParser = new SqlBatchParser(_traceService, new GoSqlBatchLineAnalyzer(), new CommentAnalyzer());
            return sqlBatchParser.Parse(sqlStatementRaw).Select(s => s.BatchText).ToList();
        }

        ///<inheritdoc/>
        public string GetSqlForCheckIfDatabaseExists()
            => @"
SELECT 1 FROM [sys].[databases] WHERE name = '${YUNIQL_DB_NAME}';
            ";

        ///<inheritdoc/>
        public string GetSqlForCreateDatabase()
            => @"
CREATE DATABASE [${YUNIQL_DB_NAME}];
            ";

        ///<inheritdoc/>
        public string GetSqlForCreateSchema()
            => @"
CREATE SCHEMA [${YUNIQL_SCHEMA_NAME}];
            ";

        ///<inheritdoc/>
        public string GetSqlForCheckIfDatabaseConfigured()
            => @"
SELECT ISNULL(OBJECT_ID('[${YUNIQL_SCHEMA_NAME}].[${YUNIQL_TABLE_NAME}]'), 0);
            ";

        ///<inheritdoc/>
        public string GetSqlForConfigureDatabase()
            => @"
CREATE TABLE [${YUNIQL_SCHEMA_NAME}].[${YUNIQL_TABLE_NAME}] (
	[sequence_id] [SMALLINT] IDENTITY(1,1) NOT NULL,
	[version] [NVARCHAR](512) NOT NULL,
	[applied_on_utc] [DATETIME] NOT NULL,
	[applied_by_user] [NVARCHAR](32) NOT NULL,
	[applied_by_tool] [NVARCHAR](32) NOT NULL,
	[applied_by_tool_version] [NVARCHAR](16) NOT NULL,
	[status] [NVARCHAR](32) NOT NULL,
	[duration_ms] [INT] NOT NULL,
	[failed_script_path] [NVARCHAR](4000) NULL,
	[failed_script_error] [NVARCHAR](4000) NULL,
	[additional_artifacts] [NVARCHAR](4000) NULL,
    CONSTRAINT [PK___YuniqlDbVersion] PRIMARY KEY CLUSTERED ([sequence_id] ASC),
    CONSTRAINT [IX___YuniqlDbVersion] UNIQUE NONCLUSTERED  ([version] ASC
));

ALTER TABLE [${YUNIQL_SCHEMA_NAME}].[${YUNIQL_TABLE_NAME}] ADD  CONSTRAINT [DF___YuniqlDbVersion_applied_on_utc]  DEFAULT (GETUTCDATE()) FOR [applied_on_utc];
ALTER TABLE [${YUNIQL_SCHEMA_NAME}].[${YUNIQL_TABLE_NAME}] ADD  CONSTRAINT [DF___YuniqlDbVersion_applied_by_user]  DEFAULT (SUSER_SNAME()) FOR [applied_by_user];
            ";

        ///<inheritdoc/>
        public string GetSqlForGetCurrentVersion()
            => @"
SELECT TOP 1 [version] FROM [${YUNIQL_SCHEMA_NAME}].[${YUNIQL_TABLE_NAME}] WHERE status = 'Successful' ORDER BY [sequence_id] DESC;
            ";

        ///<inheritdoc/>
        public string GetSqlForGetAllVersions()
            => @"
SELECT [sequence_id], [version], [applied_on_utc], [applied_by_user], [applied_by_tool], [applied_by_tool_version], [status], [duration_ms], [failed_script_path], [failed_script_error], [additional_artifacts]
FROM [${YUNIQL_SCHEMA_NAME}].[${YUNIQL_TABLE_NAME}] ORDER BY version ASC;
            ";

        ///<inheritdoc/>
        public string GetSqlForInsertVersion()
            => @"
INSERT INTO [${YUNIQL_SCHEMA_NAME}].[${YUNIQL_TABLE_NAME}] ([version], [applied_on_utc], [applied_by_user], [applied_by_tool], [applied_by_tool_version], [status], [duration_ms], [failed_script_path], [failed_script_error], [additional_artifacts]) 
VALUES ('${YUNIQL_VERSION}', GETUTCDATE(), SUSER_SNAME(), '${YUNIQL_APPLIED_BY_TOOL}', '${YUNIQL_APPLIED_BY_TOOL_VERSION}', '${YUNIQL_STATUS}', '${YUNIQL_DURATION_MS}', '${YUNIQL_FAILED_SCRIPT_PATH}', '${YUNIQL_FAILED_SCRIPT_ERROR}', '${YUNIQL_ADDITIONAL_ARTIFACTS}');
            ";

        ///<inheritdoc/>
        public string GetSqlForUpdateVersion()
            => @"
UPDATE [${YUNIQL_SCHEMA_NAME}].[${YUNIQL_TABLE_NAME}]
SET 	
	[applied_on_utc]          = GETUTCDATE(),
	[applied_by_user]         = SUSER_SNAME(),
	[applied_by_tool]         = '${YUNIQL_APPLIED_BY_TOOL}', 
	[applied_by_tool_version]  = '${YUNIQL_APPLIED_BY_TOOL_VERSION}',
	[status]                = '${YUNIQL_STATUS}',
	[duration_ms]            = '${YUNIQL_DURATION_MS}',
	[failed_script_path]      = '${YUNIQL_FAILED_SCRIPT_PATH}',
	[failed_script_error]     = '${YUNIQL_FAILED_SCRIPT_ERROR}',
	[additional_artifacts]   = '${YUNIQL_ADDITIONAL_ARTIFACTS}' 
WHERE
	[version]               = '${YUNIQL_VERSION}';
            ";

        ///<inheritdoc/>
        public string GetSqlForUpsertVersion()
            => @"
MERGE [${YUNIQL_SCHEMA_NAME}].[${YUNIQL_TABLE_NAME}] AS T
USING (SELECT 
	'${YUNIQL_VERSION}' [version], 
	GETUTCDATE() [applied_on_utc], 
	SUSER_SNAME() [applied_by_user], 
	'${YUNIQL_APPLIED_BY_TOOL}' [applied_by_tool], 
	'${YUNIQL_APPLIED_BY_TOOL_VERSION}' [applied_by_tool_version], 
	'${YUNIQL_STATUS}' [status], 
	'${YUNIQL_DURATION_MS}' [duration_ms], 
	'${YUNIQL_FAILED_SCRIPT_PATH}' [failed_script_path], 
	'${YUNIQL_FAILED_SCRIPT_ERROR}' [failed_script_error], 
	'${YUNIQL_ADDITIONAL_ARTIFACTS}' [additional_artifacts]) AS S 
ON T.[version] = S.[version]
WHEN MATCHED THEN
  UPDATE SET 	
	T.[applied_on_utc] = S.[applied_on_utc],
	T.[applied_by_user] = S.[applied_by_user],
	T.[applied_by_tool]= S.[applied_by_tool], 
	T.[applied_by_tool_version] = S.[applied_by_tool_version],
	T.[status] = S.[status],
	T.[duration_ms] = S.[duration_ms],
	T.[failed_script_path] = S.[failed_script_path],
	T.[failed_script_error] = S.[failed_script_error],
	T.[additional_artifacts] = S.[additional_artifacts]
WHEN NOT MATCHED THEN
  INSERT ([version], [applied_on_utc], [applied_by_user], [applied_by_tool], [applied_by_tool_version], [status], [duration_ms], [failed_script_path], [failed_script_error], [additional_artifacts]) 
  VALUES (S.[version], GETUTCDATE(), SUSER_SNAME(), S.[applied_by_tool], S.[applied_by_tool_version], S.[status], S.[duration_ms], S.[failed_script_path], S.[failed_script_error], S.[additional_artifacts]);
            ";

        ///<inheritdoc/>
        public bool UpdateDatabaseConfiguration(IDbConnection dbConnection, ITraceService traceService = null, string metaSchemaName = null, string metaTableName = null)
        {
            //no need to update tracking table as the structure has no been changed so far
            return false;
        }

        ///<inheritdoc/>
        public bool TryParseErrorFromException(Exception exception, out string result)
        {
            result = null;
            try
            {
                if (exception is SqlException sqlException)
                {
                    result = $"(0x{sqlException.ErrorCode:X}) Error {sqlException.Number}: {sqlException.Message}";
                    return true;
                }
            }
            catch (Exception) { return false; }
            return false;
        }
    }
}