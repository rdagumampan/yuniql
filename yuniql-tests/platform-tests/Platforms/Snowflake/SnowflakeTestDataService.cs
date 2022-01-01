using Yuniql.Extensibility;
using System.IO;
using System;
using Yuniql.Core;
using Snowflake.Data.Client;
using System.Collections.Generic;
using Yuniql.PlatformTests.Setup;
using Yuniql.Extensibility.SqlBatchParser;
using System.Linq;

namespace Yuniql.PlatformTests.Platforms.Snowflake
{
    public class SnowflakeTestDataService : TestDataServiceBase
    {
        private readonly IDataService _dataService;
        private readonly ITokenReplacementService _tokenReplacementService;

        public SnowflakeTestDataService(
            IDataService dataService,
            ITokenReplacementService tokenReplacementService) : base(dataService, tokenReplacementService)
        {
            _dataService = dataService;
            _tokenReplacementService = tokenReplacementService;
        }

        public override string GetConnectionString(string databaseName)
        {
            var connectionString = EnvironmentHelper.GetEnvironmentVariable("YUNIQL_TEST_CONNECTION_STRING");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ApplicationException("Missing environment variable YUNIQL_TEST_CONNECTION_STRING. See WIKI for developer guides.");
            }

            //extract the default database name from connection string and replaced with test database name
            var connectionStringBuilder = new SnowflakeDbConnectionStringBuilder();
            connectionStringBuilder.ConnectionString = connectionString;
            connectionStringBuilder.Remove("db");
            connectionStringBuilder.Add("db", databaseName.DoubleQuote());
            return connectionStringBuilder.ConnectionString;
        }

        public override bool CheckIfDbExist(string connectionString)
        {
            //extract the test database name from connection string
            var connectionStringBuilder = new SnowflakeDbConnectionStringBuilder();
            connectionStringBuilder.ConnectionString = connectionString;
            connectionStringBuilder.TryGetValue("db", out object databaseName);

            //prepare the sql statement
            var tokens = new List<KeyValuePair<string, string>> {
                 new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_DB_NAME, databaseName.ToString()),
                 new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_SCHEMA_NAME, base.MetaSchemaName),
            };
            var sqlStatement = _tokenReplacementService.Replace(tokens, _dataService.GetSqlForCheckIfDatabaseExists());

            //prepare a connection to snowflake without any targetdb or schema, we need to remove these keys else it throws an error that db doesn't exists
            var masterConnectionStringBuilder = new SnowflakeDbConnectionStringBuilder();
            masterConnectionStringBuilder.ConnectionString = connectionString;
            masterConnectionStringBuilder.Remove("db");
            masterConnectionStringBuilder.Remove("schema");

            return QuerySingleRow(masterConnectionStringBuilder.ConnectionString, sqlStatement);
        }

        public override bool CheckIfDbObjectExist(string connectionString, string objectName)
        {
            var dbObject = objectName.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase);
            var dbSchemaName = dbObject.Item1.IsDoubleQuoted() ? dbObject.Item1.UnQuote() : dbObject.Item1;
            var dbObjectName = dbObject.Item2.IsDoubleQuoted() ? dbObject.Item2.UnQuote() : dbObject.Item2;

            var sqlStatement = $"SELECT 1 WHERE EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{dbSchemaName}' AND TABLE_NAME = '{dbObjectName}' AND TABLE_TYPE = 'BASE TABLE')";
            var result = base.QuerySingleBool(connectionString, sqlStatement);

            return result;
        }

        public override string GetSqlForCreateDbSchema(string schemaName)
        {
            return $@"
CREATE SCHEMA {schemaName.DoubleQuote()};
";
        }

        public override string GetSqlForCreateDbObject(string objectName)
        {
            var dbObject = objectName.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase);
            return $@"
CREATE TABLE {dbObject.Item1}.{dbObject.Item2}(
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
";
        }
          
        //https://stackoverflow.com/questions/42436932/transactions-not-working-for-my-mysql-db
        public override string GetSqlForCreateDbObjectWithError(string objectName)
        {
            var dbObject = objectName.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase);
            return $@"
CREATE TABLE {dbObject.Item1}.{dbObject.Item2}(
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
CREATE TABLE {dbObject.Item1}.{dbObject.Item2}(
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
CREATE TABLE {dbObject.Item1}.{dbObject.Item2}(
	{"FirstName".DoubleQuote()} VARCHAR(50) NOT NULL,
	{"LastName".DoubleQuote()} VARCHAR(50) NOT NULL,
	{"BirthDate".DoubleQuote()} VARCHAR(50) NULL
);
";
        }

        public override string GetSqlForGetBulkTestData(string objectName)
        {
            var dbObject = objectName.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase);
            return $"SELECT * FROM {dbObject.Item1}.{dbObject.Item2};";
        }

        public override string GetSqlForSingleLine(string objectName)
        {
            var dbObject = objectName.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase);
            return $@"
CREATE TABLE {dbObject.Item1}.{dbObject.Item2} (
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
GO
";
        }

        public override string GetSqlForSingleLineWithoutTerminator(string objectName)
        {
            var dbObject = objectName.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase);
            return $@"
CREATE TABLE {dbObject.Item1}.{dbObject.Item2} (
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
";
        }

        public override string GetSqlForMultilineWithoutTerminatorInLastLine(string objectName1, string objectName2, string objectName3)
        {
            var dbObject1 = objectName1.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase);
            var dbObject2 = objectName2.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase);
            var dbObject3 = objectName3.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase);

            return $@"
CREATE TABLE {dbObject1.Item1}.{dbObject1.Item2} (
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
GO

CREATE TABLE {dbObject2.Item1}.{dbObject2.Item2} (
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
GO

CREATE TABLE {dbObject3.Item1}.{dbObject3.Item2} (
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
";
        }

        public override string GetSqlForMultilineWithTerminatorInCommentBlock(string objectName1, string objectName2, string objectName3)
        {
            var dbObject1 = objectName1.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase);
            var dbObject2 = objectName2.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase);
            var dbObject3 = objectName3.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase);

            return $@"
--GO inline comment
CREATE TABLE {dbObject1.Item1}.{dbObject1.Item2} (
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
GO

CREATE TABLE {dbObject2.Item1}.{dbObject2.Item2} (
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
GO

CREATE TABLE {dbObject3.Item1}.{dbObject3.Item2} (
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
GO
";
        }

        public override string GetSqlForMultilineWithTerminatorInsideStatements(string objectName1, string objectName2, string objectName3)
        {
            var dbObject1 = objectName1.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase);
            var dbObject2 = objectName2.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase);
            var dbObject3 = objectName3.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase);

            return $@"
CREATE TABLE {dbObject1.Item1}.{dbObject1.Item2} (
    --GO inline comment
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
GO

CREATE TABLE {dbObject2.Item1}.{dbObject2.Item2} (
    --GO inline comment
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
GO

CREATE TABLE {dbObject3.Item1}.{dbObject3.Item2} (
    --GO inline comment
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
GO
";
        }

        public override string GetSqlForMultilineWithError(string objectName1, string objectName2)
        {
            var dbObject1 = objectName1.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase);
            var dbObject2 = objectName2.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyLowerCase);

            return $@"
CREATE TABLE {dbObject1.Item1}.{dbObject1.Item2} (
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
GO

CREATE TABLE {dbObject2.Item1}.{dbObject2.Item2} (
	TEST_DB_COLUMN_1 INT NOT NULL THIS_IS_AN_ERROR,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
GO
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
DROP TABLE IF EXISTS {dbObject1.Item1}.{dbObject1.Item2};
GO
DROP TABLE IF EXISTS {dbObject2.Item1}.{dbObject2.Item2};
GO
DROP TABLE IF EXISTS {dbObject3.Item1}.{dbObject3.Item2};
GO
";
        }

        public override void CleanupDbObjects(string connectionString)
        {
            var connectionStringBuilder = new SnowflakeDbConnectionStringBuilder();
            connectionStringBuilder.ConnectionString = connectionString;
            var sqlStatements = base.BreakStatements(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Platforms", "Snowflake", "Cleanup.sql")));
            sqlStatements.ForEach(sqlStatement => base.ExecuteNonQuery(connectionStringBuilder.ConnectionString, sqlStatement));

            ////extract the test database name from connection string
            //var connectionStringBuilder = new SnowflakeDbConnectionStringBuilder();
            //connectionStringBuilder.ConnectionString = connectionString;
            //connectionStringBuilder.TryGetValue("db", out object databaseName);

            //var sqlStatement = $"DROP DATABASE {databaseName.ToString().DoubleQuote()};";

            ////prepare a connection to snowflake without any targetdb or schema, we need to remove these keys else it throws an error that db doesn't exists
            //var masterConnectionStringBuilder = new SnowflakeDbConnectionStringBuilder();
            //masterConnectionStringBuilder.ConnectionString = connectionString;
            //masterConnectionStringBuilder.Remove("db");
            //masterConnectionStringBuilder.Remove("schema");

            //base.ExecuteNonQuery(masterConnectionStringBuilder.ConnectionString, sqlStatement);
        }
    }
}
