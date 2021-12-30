using Yuniql.Extensibility;
using System.IO;
using Npgsql;
using System;
using Yuniql.Core;
using Yuniql.PostgreSql;
using Yuniql.PlatformTests.Setup;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Linq;

namespace Yuniql.PlatformTests.Platforms.Redshift
{
    public class OracleTestDataService : TestDataServiceBase
    {
        public OracleTestDataService(IDataService dataService, ITokenReplacementService tokenReplacementService) : base(dataService, tokenReplacementService)
        {
        }

        public override string GetConnectionString(string databaseName)
        {
            var connectionString = EnvironmentHelper.GetEnvironmentVariable("YUNIQL_TEST_CONNECTION_STRING");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ApplicationException("Missing environment variable YUNIQL_TEST_CONNECTION_STRING. See WIKI for developer guides.");
            }

            var result = new NpgsqlConnectionStringBuilder(connectionString);
            result.Database = databaseName;

            return result.ConnectionString;
        }

        public override bool CheckIfDbExist(string connectionString)
        {
            throw new NotSupportedException($"Multitenancy statements is not supported in this platform. " +
                $"See {nameof(PostgreSqlDataService)}.{nameof(PostgreSqlDataService.IsBatchSqlSupported)}");
        }

        public override bool CheckIfDbObjectExist(string connectionString, string objectName)
        {
            var dbObject = GetObjectNameWithSchema(objectName);
            var dbObjectName = dbObject.Item2.IsDoubleQuoted() ?dbObject.Item2.UnQuote() : dbObject.Item2;

            var sqlStatement = $"SELECT 1 FROM SYS.ALL_TABLES WHERE TABLE_NAME = '{dbObjectName}'";
            var result = QuerySingleBool(connectionString, sqlStatement);

            return result;
        }

        public override string GetSqlForCreateDbSchema(string schemaName)
        {
            return $@"
CREATE SCHEMA {schemaName};
";
        }

        public override string GetSqlForCreateDbObject(string objectName)
        {
            var dbObject = GetObjectNameWithSchema(objectName);
            return $@"
CREATE TABLE {dbObject.Item2} (
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
";
        }

        public override string GetSqlForCreateDbObjectWithError(string objectName)
        {
            var dbObject = GetObjectNameWithSchema(objectName);
            return $@"
CREATE TABLE {dbObject.Item2} (
	TEST_DB_COLUMN_1 INT NOT NULL THIS_IS_AN_ERROR,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
";
        }

        public override string GetSqlForCreateDbObjectWithTokens(string objectName)
        {
            var dbObject = GetObjectNameWithSchema($@"{objectName}_${{Token1}}_${{Token2}}_${{Token3}}");
            return $@"
CREATE TABLE {dbObject.Item2} (
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
";
        }

        public override string GetSqlForCreateBulkTable(string objectName)
        {
            var dbObject = GetObjectNameWithSchema(objectName);
            return $@"
CREATE TABLE {dbObject.Item2} (
	FirstName VARCHAR(50) NOT NULL,
	LastName VARCHAR(50) NOT NULL,
	BirthDate VARCHAR(50) NULL
);
";
        }

        public override string GetSqlForGetBulkTestData(string objectName)
        {
            var dbObject = GetObjectNameWithSchema(objectName);
            return $"SELECT * FROM {dbObject.Item2}";
        }

        public override string GetSqlForSingleLine(string objectName)
        {
            var dbObject = GetObjectNameWithSchema(objectName);
            return $@"
CREATE TABLE {dbObject.Item2} (
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
";
        }

        public override string GetSqlForSingleLineWithoutTerminator(string objectName)
        {
            var dbObject = GetObjectNameWithSchema(objectName);
            return $@"
CREATE TABLE {dbObject.Item2} (
	TEST_DB_COLUMN_1 VARCHAR(50) NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
)
";
        }

        public override string GetSqlForMultilineWithoutTerminatorInLastLine(string objectName1, string objectName2, string objectName3)
        {
            return $@"
CREATE TABLE {GetObjectNameWithSchema(objectName1).Item2} (
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);

CREATE TABLE {GetObjectNameWithSchema(objectName2).Item2} (
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);

CREATE TABLE {GetObjectNameWithSchema(objectName3).Item2} (
	TEST_DB_COLUMN_1 VARCHAR(50) NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
)
";
        }

        public override string GetSqlForMultilineWithTerminatorInCommentBlock(string objectName1, string objectName2, string objectName3)
        {
            return $@"
--; inline comment
CREATE TABLE {GetObjectNameWithSchema(objectName1).Item2} (
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);

CREATE TABLE {GetObjectNameWithSchema(objectName2).Item2} (
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);

CREATE TABLE {GetObjectNameWithSchema(objectName3).Item2} (
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
)
";
        }

        public override string GetSqlForMultilineWithTerminatorInsideStatements(string objectName1, string objectName2, string objectName3)
        {
            return $@"
CREATE TABLE {GetObjectNameWithSchema(objectName1).Item2} (
    --; inline comment
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);

CREATE TABLE {GetObjectNameWithSchema(objectName2).Item2} (
    --; inline comment
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);

CREATE TABLE {GetObjectNameWithSchema(objectName3).Item2} (
    --; inline comment
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
)
";
        }

        public override string GetSqlForMultilineWithError(string objectName1, string objectName2)
        {
            return $@"
CREATE TABLE {GetObjectNameWithSchema(objectName1).Item2} (
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);

CREATE TABLE {GetObjectNameWithSchema(objectName2).Item2} (
	TEST_DB_COLUMN_1 INT NOT NULL THIS_IS_AN_ERROR,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
";
        }

        public override void CreateScriptFile(string sqlFilePath, string sqlStatement)
        {
            using var sw = File.CreateText(sqlFilePath);
            sw.WriteLine(sqlStatement);
        }   

        public override string GetSqlForEraseDbObjects()
        {
            var dbObject1 = GetObjectNameWithSchema(TEST_DBOBJECTS.DB_OBJECT_1);
            var dbObject2 = GetObjectNameWithSchema(TEST_DBOBJECTS.DB_OBJECT_2);
            var dbObject3 = GetObjectNameWithSchema(TEST_DBOBJECTS.DB_OBJECT_3);

            return $@"
DROP TABLE {dbObject1.Item2};
DROP TABLE {dbObject2.Item2};
DROP TABLE {dbObject3.Item2};
";
        }

        private Tuple<string, string> GetObjectNameWithSchema(string objectName)
        {
            //check if a non-default dbo schema is used
            var schemaName = string.Empty;
            var newObjectName = objectName;

            if (objectName.IndexOf('.') > 0)
            {
                schemaName = objectName.Split('.')[0];
                newObjectName = objectName.Split('.')[1];
            }

            //we do this because oracle always converts unquoted names into upper case
            schemaName = schemaName.HasLower() ? schemaName.DoubleQuote() : schemaName;
            newObjectName = newObjectName.HasLower() ? newObjectName.DoubleQuote() : newObjectName;

            return new Tuple<string, string>(schemaName, newObjectName);
        }

        //TODO: Refactor this into Erase!
        public override void CleanupDbObjects(string connectionString)
        {
            var connectionStringBuilder = new OracleConnectionStringBuilder(connectionString);
            var sqlStatements = BreakStatements(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Platforms", "Oracle", "Cleanup.sql")));
            sqlStatements.ForEach(s => base.ExecuteNonQuery(connectionStringBuilder.ConnectionString, s));
        }

        //TODO: Refactor this!
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
                            results.Add(sqlStatement.Substring(0, sqlStatement.Length - 1));
                            sqlStatement = string.Empty;
                        }
                    }
                    ++lineNo;
                }
            }

            return results;
        }
    }
}
