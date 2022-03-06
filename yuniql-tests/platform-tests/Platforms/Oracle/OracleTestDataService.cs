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
            var dbObject = objectName.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase);
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
            var dbObject = objectName.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase);
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
            var dbObject = objectName.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase);
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
            var dbObject = $@"{objectName}_${{Token1}}_${{Token2}}_${{Token3}}".SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase);
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
            var dbObject = objectName.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase);
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
            var dbObject = objectName.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase);
            return $"SELECT * FROM {dbObject.Item2}";
        }

        public override string GetSqlForSingleLine(string objectName)
        {
            var dbObject = objectName.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase);
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
            var dbObject = objectName.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase);
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
CREATE TABLE {objectName1.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase).Item2} (
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);

CREATE TABLE {objectName2.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase).Item2} (
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);

CREATE TABLE {objectName3.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase).Item2} (
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
CREATE TABLE {objectName1.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase).Item2} (
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);

CREATE TABLE {objectName2.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase).Item2} (
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);

CREATE TABLE {objectName3.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase).Item2} (
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
)
";
        }

        public override string GetSqlForMultilineWithTerminatorInsideStatements(string objectName1, string objectName2, string objectName3)
        {
            return $@"
CREATE TABLE {objectName1.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase).Item2} (
    --; inline comment
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);

CREATE TABLE {objectName2.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase).Item2} (
    --; inline comment
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);

CREATE TABLE {objectName3.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase).Item2} (
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
CREATE TABLE {objectName1.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase).Item2} (
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);

CREATE TABLE {objectName2.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase).Item2} (
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
            var dbObject1 = TEST_DBOBJECTS.DB_OBJECT_1.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase);
            var dbObject2 = TEST_DBOBJECTS.DB_OBJECT_2.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase);
            var dbObject3 = TEST_DBOBJECTS.DB_OBJECT_3.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase);

            return $@"
BEGIN
   EXECUTE IMMEDIATE 'DROP TABLE ""{TEST_DBOBJECTS.DB_OBJECT_1}""';
EXCEPTION
   WHEN OTHERS THEN
      IF SQLCODE != -942 THEN
         RAISE;
            END IF;
            END;
/
BEGIN
   EXECUTE IMMEDIATE 'DROP TABLE ""{TEST_DBOBJECTS.DB_OBJECT_2}""';
EXCEPTION
   WHEN OTHERS THEN
      IF SQLCODE != -942 THEN
         RAISE;
            END IF;
            END;
/
BEGIN
   EXECUTE IMMEDIATE 'DROP TABLE ""{TEST_DBOBJECTS.DB_OBJECT_3}""';
EXCEPTION
   WHEN OTHERS THEN
      IF SQLCODE != -942 THEN
         RAISE;
            END IF;
            END;
/
";
        }

        public override void CleanupDbObjects(string connectionString)
        {
            var connectionStringBuilder = new OracleConnectionStringBuilder(connectionString);
            var sqlStatements = base.BreakStatements(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Platforms", "Oracle", "Cleanup.sql")));
            sqlStatements.ForEach(sqlStatement => base.ExecuteNonQuery(connectionStringBuilder.ConnectionString, sqlStatement));
        }
    }
}
