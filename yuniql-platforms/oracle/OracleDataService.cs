using System.Collections.Generic;
using System.Data;
using Yuniql.Extensibility;
using Oracle.ManagedDataAccess.Client;
using System;
using System.IO;
using System.Linq;

namespace Yuniql.Oracle
{
    ///<inheritdoc/>
    public class OracleDataService : IDataService, IMixableTransaction
    {
        private string _connectionString;
        private readonly ITraceService _traceService;

        ///<inheritdoc/>
        public OracleDataService(ITraceService traceService)
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
        public bool IsMultiTenancySupported { get; } = false;

        ///<inheritdoc/>
        public bool IsSchemaSupported { get; } = false;

        ///<inheritdoc/>
        public bool IsBatchSqlSupported { get; } = true;

        ///<inheritdoc/>
        public bool IsUpsertSupported => false;

        ///<inheritdoc/>
        public string MetaTableName { get; set; } = "__yuniql_schema_version";

        ///<inheritdoc/>
        public string MetaSchemaName { get; set; } = string.Empty;

        ///<inheritdoc/>
        public IDbConnection CreateConnection()
        {
            return new OracleConnection(_connectionString);
        }

        ///<inheritdoc/>
        public IDbConnection CreateMasterConnection()
        {
            //There's no concept of master database in Oracle <= v12
            //All metadata are stored in SYS schema
            return new OracleConnection(_connectionString);
        }

        ///<inheritdoc/>
        public ConnectionInfo GetConnectionInfo()
        {
            //Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=49161))(CONNECT_DATA=(SERVICE_NAME=xe)));User Id=myuser;Password=mypassword;
            var stringParts = _connectionString.Split('(');

            //HOST=localhost)
            var hostPair = stringParts.First(s => s.Contains("HOST",StringComparison.InvariantCultureIgnoreCase)).Split("=");
            var host = hostPair[1].Substring(0, hostPair[1].IndexOf(")"));

            //PORT=49161)
            var portPair = stringParts.First(s => s.Contains("PORT", StringComparison.InvariantCultureIgnoreCase)).Split("=");
            var port = Convert.ToInt32(portPair[1].Substring(0, portPair[1].IndexOf(")")).Trim());

            //SERVICE_NAME=xe)
            var serviceNamePair = stringParts.First(s => s.Contains("SERVICE_NAME", StringComparison.InvariantCultureIgnoreCase)).Split("=");
            var serviceName = serviceNamePair[1].Substring(0, serviceNamePair[1].IndexOf(")"));

            var connectionStringBuilder = new OracleConnectionStringBuilder(_connectionString);
            return new ConnectionInfo { DataSource = $"{host}", Port = port, Database = serviceName };
        }

        ///<inheritdoc/>
        public List<string> BreakStatements(string sqlStatementRaw)
        {
            //breaks statements into batches using semicolon (;) or forward slash (/) batch separator
            //any existence of / in the line means it batch separated by /
            var statementBatchTerminator = sqlStatementRaw.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                .Any(s => s.Equals("/"))
                ? "/" : ";";

            var results = new List<string>();
            var sqlStatement = string.Empty;
            var sqlStatementLine2 = string.Empty; byte lineNo = 0;
            using (var sr = new StringReader(sqlStatementRaw))
            {
                while ((sqlStatementLine2 = sr.ReadLine()) != null)
                {
                    if (sqlStatementLine2.Length > 0 && !sqlStatementLine2.StartsWith("--"))
                    {
                        sqlStatement += (sqlStatement.Length > 0 ? Environment.NewLine : string.Empty) + sqlStatementLine2;
                        if (sqlStatement.EndsWith(statementBatchTerminator))
                        {
                            //pickup the formed sql statement
                            results.Add(sqlStatement.Substring(0, sqlStatement.Length - 1));
                            sqlStatement = string.Empty;
                        }
                    }
                    ++lineNo;
                }

                //pickup the last formed sql statement
                if (!string.IsNullOrEmpty(sqlStatement.Trim()))
                {
                    results.Add(sqlStatement);
                }
            }

            return results;
        }

        //Only applies with oracle 12c
        //https://docs.oracle.com/en/database/oracle/oracle-database/19/riwin/about-pluggable-databases-in-oracle-rac.html
        //https://dba.stackexchange.com/questions/27725/how-to-see-list-of-databases-in-oracle
        ///<inheritdoc/>
        public string GetSqlForCheckIfDatabaseExists()
            => @"
SELECT 1 FROM DUAL
            ";

        //https://blog.devart.com/how-to-create-database-in-oracle.html
        ///<inheritdoc/>
        public string GetSqlForCreateDatabase()
            => throw new NotSupportedException("Not supported in the target platform.");

        ///<inheritdoc/>
        public List<string> GetSqlForDropDatabase()
            => throw new NotSupportedException("Not supported in the target platform.");

        ///<inheritdoc/>
        public string GetSqlForCheckIfSchemaExists()
            => throw new NotSupportedException("Not supported in the target platform.");

        //https://www.techonthenet.com/oracle/schemas/create_schema_statement.php
        //https://docs.oracle.com/cd/B19306_01/server.102/b14200/statements_6014.htm
        ///<inheritdoc/>
        public string GetSqlForCreateSchema()
            => throw new NotSupportedException("Not supported in the target platform.");

        ///<inheritdoc/>
        public string GetSqlForCheckIfDatabaseConfigured()
            => @"
SELECT 1 FROM SYS.ALL_TABLES WHERE TABLE_NAME = '${YUNIQL_TABLE_NAME}' AND ROWNUM = 1
            ";

        ///<inheritdoc/>
        public string GetSqlForCheckIfDatabaseConfiguredv10()
            => @"
SELECT 1 FROM SYS.ALL_TABLES WHERE TABLE_NAME = '__yuniqldbversion' AND ROWNUM = 1
            ";

        ///<inheritdoc/>
        public string GetSqlForConfigureDatabase()
            => @"
CREATE TABLE ""${YUNIQL_TABLE_NAME}"" (
    ""sequence_id"" NUMBER NOT NULL,
    ""version"" VARCHAR2(190) NOT NULL,
    ""applied_on_utc"" TIMESTAMP NOT NULL,
    ""applied_by_user"" VARCHAR2(32) NOT NULL,
    ""applied_by_tool"" VARCHAR2(32) NOT NULL,
    ""applied_by_tool_version"" VARCHAR2(16) NOT NULL,
    ""status"" VARCHAR2(32) NOT NULL,
    ""duration_ms"" NUMBER NOT NULL,
    ""checksum"" VARCHAR2(64) NOT NULL,
    ""failed_script_path"" VARCHAR2(4000) NULL,
    ""failed_script_error"" VARCHAR2(4000) NULL,
    ""additional_artifacts"" VARCHAR2(4000) NULL,
    CONSTRAINT ""pk___${YUNIQL_TABLE_NAME}"" PRIMARY KEY(""sequence_id""),
    CONSTRAINT ""ix___${YUNIQL_TABLE_NAME}"" UNIQUE(""version"")
);

CREATE SEQUENCE ""${YUNIQL_TABLE_NAME}_SEQ""
  MINVALUE 1
  START WITH 1
  INCREMENT BY 1
  CACHE 20;
            ";

        ///<inheritdoc/>
        public string GetSqlForGetCurrentVersion()
            => @"
SELECT ""version"" FROM ""${YUNIQL_TABLE_NAME}"" WHERE ""status"" = 'Successful' AND ROWNUM = 1 ORDER BY ""sequence_id"" DESC
            ";

        ///<inheritdoc/>
        public string GetSqlForGetAllVersions()
            => @"
SELECT ""sequence_id"", ""version"", ""applied_on_utc"", ""applied_by_user"", ""applied_by_tool"", ""applied_by_tool_version"", ""status"", ""duration_ms"", ""checksum"", ""failed_script_path"", ""failed_script_error"", ""additional_artifacts""
FROM ""${YUNIQL_TABLE_NAME}"" ORDER BY ""version"" ASC
            ";

        ///<inheritdoc/>
        public string GetSqlForInsertVersion()
            => @"
INSERT INTO ""${YUNIQL_TABLE_NAME}"" (""sequence_id"", ""version"", ""applied_on_utc"", ""applied_by_user"", ""applied_by_tool"", ""applied_by_tool_version"", ""status"", ""duration_ms"", ""checksum"", ""failed_script_path"", ""failed_script_error"", ""additional_artifacts"") 
VALUES (""${YUNIQL_TABLE_NAME}_SEQ"".NEXTVAL, '${YUNIQL_VERSION}', SYS_EXTRACT_UTC(SYSTIMESTAMP), USER, '${YUNIQL_APPLIED_BY_TOOL}', '${YUNIQL_APPLIED_BY_TOOL_VERSION}','${YUNIQL_STATUS}', '${YUNIQL_DURATION_MS}', '${YUNIQL_CHECKSUM}', '${YUNIQL_FAILED_SCRIPT_PATH}', '${YUNIQL_FAILED_SCRIPT_ERROR}', '${YUNIQL_ADDITIONAL_ARTIFACTS}')
            ";

        ///<inheritdoc/>
        public string GetSqlForUpdateVersion()
            => @"
UPDATE ""${YUNIQL_TABLE_NAME}""
SET
    ""applied_on_utc""          =  SYS_EXTRACT_UTC(SYSTIMESTAMP),
    ""applied_by_user""         =  USER,
    ""applied_by_tool""         = '${YUNIQL_APPLIED_BY_TOOL}', 
    ""applied_by_tool_version"" = '${YUNIQL_APPLIED_BY_TOOL_VERSION}', 
    ""status""                  = '${YUNIQL_STATUS}', 
    ""duration_ms""             = '${YUNIQL_DURATION_MS}', 
    ""failed_script_path""      = '${YUNIQL_FAILED_SCRIPT_PATH}', 
    ""failed_script_error""     = '${YUNIQL_FAILED_SCRIPT_ERROR}', 
    ""additional_artifacts""    = '${YUNIQL_ADDITIONAL_ARTIFACTS}'
WHERE
    ""version""                 = '${YUNIQL_VERSION}'
            ";

        ///<inheritdoc/>
        public string GetSqlForUpsertVersion()
            => throw new NotSupportedException("Not supported in the target platform.");

        ///<inheritdoc/>
        public string GetSqlForCheckRequireMetaSchemaUpgrade(string currentSchemaVersion)
            //when table __yuniqldbversion exists, we need to upgrade from yuniql v1.0 to v1.1 version
            => throw new NotSupportedException("Not supported in the target platform.");

        ///<inheritdoc/>
        public string GetSqlForUpgradeMetaSchema(string requiredSchemaVersion)
        {
            var assembly = typeof(OracleDataService).Assembly;
            var resource = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.SchemaUpgrade_{requiredSchemaVersion.Replace(".", "_")}.sql");
            using var reader = new StreamReader(resource);
            return reader.ReadToEnd();
        }

        ///<inheritdoc/>
        public bool TryParseErrorFromException(Exception exception, out string result)
        {
            result = null;
            if (exception is OracleException sqlException)
            {
                result = $"(0x{sqlException.ErrorCode:X}) Error {sqlException.Number}: {sqlException.Message}";
                return true;
            }
            return false;
        }
    }
}