using Yuniql.Extensibility;
using System.IO;
using MySql.Data.MySqlClient;
using System;
using Yuniql.Core;
using Yuniql.MySql;
using Yuniql.PlatformTests.Setup;

namespace Yuniql.PlatformTests.Platforms.MySql
{
    public class MySqlTestDataService : TestDataServiceBase
    {
        public MySqlTestDataService(IDataService dataService, ITokenReplacementService tokenReplacementService) : base(dataService, tokenReplacementService)
        {
        }

        public override string GetConnectionString(string databaseName)
        {
            var connectionString = EnvironmentHelper.GetEnvironmentVariable("YUNIQL_TEST_CONNECTION_STRING");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ApplicationException("Missing environment variable YUNIQL_TEST_CONNECTION_STRING. See WIKI for developer guides.");
            }

            var result = new MySqlConnectionStringBuilder(connectionString);
            result.Database = databaseName;

            return result.ConnectionString;
        }

        public override bool CheckIfDbExist(string connectionString)
        {
            //use the target user database to migrate, this is part of orig connection string
            var connectionStringBuilder = new MySqlConnectionStringBuilder(connectionString);
            var sqlStatement = $"SELECT 1 FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{connectionStringBuilder.Database}';";

            //switch database into master/system database where db catalogs are maintained
            connectionStringBuilder.Database = "INFORMATION_SCHEMA";
            return QuerySingleBool(connectionStringBuilder.ConnectionString, sqlStatement);
        }

        public override bool CheckIfDbObjectExist(string connectionString, string objectName)
        {
            //check from tables, im just lazy to figure out join :)
            var connectionStringBuilder = new MySqlConnectionStringBuilder(connectionString);
            var sqlStatement = $"SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{connectionStringBuilder.Database}' AND TABLE_NAME = '{objectName}' LIMIT 1;";
            bool result = QuerySingleBool(connectionString, sqlStatement);

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
            return $@"
CREATE TABLE {objectName} (
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
) ENGINE=InnoDB;
";
        }

        //https://stackoverflow.com/questions/42436932/transactions-not-working-for-my-mysql-db
        public override string GetSqlForCreateDbObjectWithError(string objectName)
        {
            return $@"
CREATE TABLE {objectName} (
	TEST_DB_COLUMN_1 INT NOT NULL THIS_IS_AN_ERROR,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
) ENGINE=InnoDB;
";
        }

        public override string GetSqlForCreateDbObjectWithTokens(string objectName)
        {
            return $@"
CREATE TABLE {objectName}_${{Token1}}_${{Token2}}_${{Token3}} (
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
) ENGINE=InnoDB;
";
        }

        public override string GetSqlForCreateBulkTable(string tableName)
        {
            return $@"
CREATE TABLE {tableName}(
	FirstName VARCHAR(50) NOT NULL,
	LastName VARCHAR(50) NOT NULL,
	BirthDate VARCHAR(50) NULL
) ENGINE=InnoDB;
";
        }

        public override string GetSqlForGetBulkTestData(string tableName)
        {
            return $"SELECT * FROM {tableName};";
        }

        public override string GetSqlForSingleLine(string objectName)
        {
            throw new NotSupportedException($"Batching statements is not supported in this platform. " +
                $"See {nameof(MySqlDataService)}.{nameof(MySqlDataService.IsBatchSqlSupported)}");
        }

        public override string GetSqlForSingleLineWithoutTerminator(string objectName)
        {
            throw new NotSupportedException($"Batching statements is not supported in this platform. " +
                $"See {nameof(MySqlDataService)}.{nameof(MySqlDataService.IsBatchSqlSupported)}");
        }

        public override string GetSqlForMultilineWithoutTerminatorInLastLine(string objectName1, string objectName2, string objectName3)
        {
            throw new NotSupportedException($"Batching statements is not supported in this platform. " +
                $"See {nameof(MySqlDataService)}.{nameof(MySqlDataService.IsBatchSqlSupported)}");
        }

        public override string GetSqlForMultilineWithTerminatorInCommentBlock(string objectName1, string objectName2, string objectName3)
        {
            throw new NotSupportedException($"Batching statements is not supported in this platform. " +
                $"See {nameof(MySqlDataService)}.{nameof(MySqlDataService.IsBatchSqlSupported)}");
        }

        public override string GetSqlForMultilineWithTerminatorInsideStatements(string objectName1, string objectName2, string objectName3)
        {
            throw new NotSupportedException($"Batching statements is not supported in this platform. " +
                $"See {nameof(MySqlDataService)}.{nameof(MySqlDataService.IsBatchSqlSupported)}");
        }

        public override string GetSqlForMultilineWithError(string objectName1, string objectName2)
        {
            throw new NotSupportedException($"Batching statements is not supported in this platform. " +
                $"See {nameof(MySqlDataService)}.{nameof(MySqlDataService.IsBatchSqlSupported)}");
        }

        public override void CreateScriptFile(string sqlFilePath, string sqlStatement)
        {
            using var sw = File.CreateText(sqlFilePath);
            sw.WriteLine(sqlStatement);
        }

        public override string GetSqlForEraseDbObjects()
        {
            return $@"
DROP TABLE IF EXISTS {TEST_DBOBJECTS.DB_OBJECT_1};
DROP TABLE IF EXISTS {TEST_DBOBJECTS.DB_OBJECT_2};
DROP TABLE IF EXISTS {TEST_DBOBJECTS.DB_OBJECT_3};
";
        }

        public override void CleanupDbObjects(string connectionString)
        {
            var connectionStringBuilder = new MySqlConnectionStringBuilder(connectionString);
            var sqlStatements = base.BreakStatements(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Platforms", "MySql", "Cleanup.sql")));
            sqlStatements.ForEach(sqlStatement => base.ExecuteNonQuery(connectionStringBuilder.ConnectionString, sqlStatement));
        }
    }
}
