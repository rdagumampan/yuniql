using Yuniql.Extensibility;
using System.IO;
using Npgsql;
using System;
using Yuniql.Core;
using Yuniql.PostgreSql;
using Yuniql.PlatformTests.Setup;

namespace Yuniql.PlatformTests.Platforms.PostgreSql
{
    public class PostgreSqlTestDataService : TestDataServiceBase
    {
        public PostgreSqlTestDataService(IDataService dataService, ITokenReplacementService tokenReplacementService) : base(dataService, tokenReplacementService)
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
            //use the target user database to migrate, this is part of orig connection string
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
            var sqlStatement = $"SELECT 1 from pg_database WHERE datname ='{connectionStringBuilder.Database}';";

            //switch database into master/system database where db catalogs are maintained
            connectionStringBuilder.Database = "postgres";
            return QuerySingleBool(connectionStringBuilder.ConnectionString, sqlStatement);
        }

        public override bool CheckIfDbObjectExist(string connectionString, string objectName)
        {
            var dbObject = objectName.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyUpperCase);
            var dbSchemaName = dbObject.Item1.IsDoubleQuoted() ? dbObject.Item1.UnQuote() : dbObject.Item1;
            var dbObjectName = dbObject.Item2.IsDoubleQuoted() ? dbObject.Item2.UnQuote() : dbObject.Item2;

            //check from procedures, im just lazy to figure out join in pgsql :)
            var sqlStatement = $"SELECT 1 FROM information_schema.tables WHERE TABLE_SCHEMA = '{dbSchemaName}'  AND TABLE_NAME = '{dbObjectName}'";
            var result = QuerySingleBool(connectionString, sqlStatement);

            return result;
        }

        public override string GetSqlForCreateDbSchema(string schemaName)
        {
            schemaName = schemaName.HasUpper() ? schemaName.DoubleQuote() : schemaName;
            return $@"
CREATE SCHEMA {schemaName};
";
        }

        public override string GetSqlForCreateDbObject(string objectName)
        {
            var dbObject = objectName.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyUpperCase);
            return $@"
CREATE TABLE {dbObject.Item1}.{dbObject.Item2} (
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
";
        }

        public override string GetSqlForCreateDbObjectWithError(string objectName)
        {
            var dbObject = objectName.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyUpperCase);
            return $@"
CREATE TABLE {dbObject.Item1}.{dbObject.Item2} (
	TEST_DB_COLUMN_1 INT NOT NULL THIS_IS_AN_ERROR,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
";
        }

        public override string GetSqlForCreateDbObjectWithTokens(string objectName)
        {
            var dbObject = $@"{objectName}_${{Token1}}_${{Token2}}_${{Token3}}".SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyUpperCase);
            return $@"
CREATE TABLE {dbObject.Item1}.{dbObject.Item2} (
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
";
        }

        public override string GetSqlForCreateBulkTable(string objectName)
        {
            var dbObject = objectName.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyUpperCase);
            return $@"
CREATE TABLE {dbObject.Item1}.{dbObject.Item2}(
	FirstName VARCHAR(50) NOT NULL,
	LastName VARCHAR(50) NOT NULL,
	BirthDate VARCHAR(50) NULL
);
";
        }

        public override string GetSqlForGetBulkTestData(string objectName)
        {
            var dbObject = objectName.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyUpperCase);
            return $"SELECT * FROM {dbObject.Item1}.{dbObject.Item2}";
        }

        public override string GetSqlForSingleLine(string objectName)
        {
            throw new NotSupportedException($"Batching statements is not supported in this platform. " +
                $"See {nameof(PostgreSqlDataService)}.{nameof(PostgreSqlDataService.IsBatchSqlSupported)}");
        }

        public override string GetSqlForSingleLineWithoutTerminator(string objectName)
        {
            throw new NotSupportedException($"Batching statements is not supported in this platform. " +
                $"See {nameof(PostgreSqlDataService)}.{nameof(PostgreSqlDataService.IsBatchSqlSupported)}");
        }

        public override string GetSqlForMultilineWithoutTerminatorInLastLine(string objectName1, string objectName2, string objectName3)
        {
            throw new NotSupportedException($"Batching statements is not supported in this platform. " +
                $"See {nameof(PostgreSqlDataService)}.{nameof(PostgreSqlDataService.IsBatchSqlSupported)}");
        }

        public override string GetSqlForMultilineWithTerminatorInCommentBlock(string objectName1, string objectName2, string objectName3)
        {
            throw new NotSupportedException($"Batching statements is not supported in this platform. " +
                $"See {nameof(PostgreSqlDataService)}.{nameof(PostgreSqlDataService.IsBatchSqlSupported)}");
        }

        public override string GetSqlForMultilineWithTerminatorInsideStatements(string objectName1, string objectName2, string objectName3)
        {
            throw new NotSupportedException($"Batching statements is not supported in this platform. " +
                $"See {nameof(PostgreSqlDataService)}.{nameof(PostgreSqlDataService.IsBatchSqlSupported)}");
        }

        public override string GetSqlForMultilineWithError(string objectName1, string objectName2)
        {
            throw new NotSupportedException($"Batching statements is not supported in this platform. " +
                $"See {nameof(PostgreSqlDataService)}.{nameof(PostgreSqlDataService.IsBatchSqlSupported)}");
        }

        public override void CreateScriptFile(string sqlFilePath, string sqlStatement)
        {
            using var sw = File.CreateText(sqlFilePath);
            sw.WriteLine(sqlStatement);
        }

        public override string GetSqlForEraseDbObjects()
        {
            var dbObject1 = TEST_DBOBJECTS.DB_OBJECT_1.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyUpperCase);
            var dbObject2 = TEST_DBOBJECTS.DB_OBJECT_2.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyUpperCase);
            var dbObject3 = TEST_DBOBJECTS.DB_OBJECT_3.SplitSchema(base.MetaSchemaName, CaseSenstiveOption.QuouteWhenAnyUpperCase);

            return $@"
DROP TABLE IF EXISTS {dbObject1.Item1}.{dbObject1.Item2};
DROP TABLE IF EXISTS {dbObject2.Item1}.{dbObject2.Item2};
DROP TABLE IF EXISTS {dbObject3.Item1}.{dbObject3.Item2};
";
        }

        //https://dba.stackexchange.com/questions/11893/force-drop-db-while-others-may-be-connected
        public override void CleanupDbObjects(string connectionString)
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
            var sqlStatements = base.BreakStatements(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Platforms", "PostgreSql", "Cleanup.sql")));
            sqlStatements.ForEach(sqlStatement => base.ExecuteNonQuery(connectionStringBuilder.ConnectionString, sqlStatement));

            //            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);

            //            //not needed need since test cases are executed against disposable database containers
            //            //we could simply docker rm the running test container after tests completed

            //            //use the target user database to migrate, this is part of orig connection string
            //            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
            //            var databaseName = connectionStringBuilder.Database;

            //            var sqlStatement = $@"
            //--making sure the database exists
            //SELECT * from pg_database where datname = '{databaseName}';

            //--disallow new connections
            //UPDATE pg_database SET datallowconn = 'false' WHERE datname = '{databaseName}';
            //ALTER DATABASE {databaseName} CONNECTION LIMIT 1;

            //--terminate existing connections
            //SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '{databaseName}';

            //--drop database
            //DROP DATABASE {databaseName};
            //";

            //            //switch database into master/system database where db catalogs are maintained
            //            connectionStringBuilder.Database = "postgres";
            //            ExecuteNonQuery(connectionStringBuilder.ConnectionString, sqlStatement);
        }
    }
}
