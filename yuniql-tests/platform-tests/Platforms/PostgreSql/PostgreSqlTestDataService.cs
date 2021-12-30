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
            var dbObject = GetObjectNameWithSchema(objectName);
            var dbSchemaName = dbObject.Item1.IsDoubleQuoted() ? dbObject.Item1.UnQuote() : dbObject.Item1;
            var dbObjectName = dbObject.Item2.IsDoubleQuoted() ? dbObject.Item2.UnQuote() : dbObject.Item2;

            //check from procedures, im just lazy to figure out join in pgsql :)
            var sqlStatement = $"SELECT 1 FROM pg_proc WHERE  proname = '{dbObjectName}'";
            bool result = QuerySingleBool(connectionString, sqlStatement);

            //check from tables, im just lazy to figure out join in pgsql :)
            if (!result)
            {
                sqlStatement = $"SELECT 1 FROM pg_class WHERE  relname = '{dbObjectName}'";
                result = QuerySingleBool(connectionString, sqlStatement);
            }

            if (!result)
            {
                sqlStatement = $"SELECT 1 FROM information_schema.tables WHERE TABLE_SCHEMA = '{dbSchemaName}'  AND TABLE_NAME = '{dbObjectName}'";
                result = QuerySingleBool(connectionString, sqlStatement);
            }

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
            var dbObject = GetObjectNameWithSchema(objectName);
            return $@"
CREATE TABLE {dbObject.Item1}.{dbObject.Item2} (
	VisitorID SERIAL NOT NULL,
	FirstName VARCHAR(255) NULL,
	LastName VARCHAR(255) NULL,
	Address VARCHAR(255) NULL,
	Email VARCHAR(255) NULL
);
";
        }

        public override string GetSqlForCreateDbObjectWithError(string objectName)
        {
            var dbObject = GetObjectNameWithSchema(objectName);
            return $@"
CREATE TABLE {dbObject.Item1}.{dbObject.Item2} (
	VisitorID SERIAL NOT NULL THIS_IS_AN_ERROR,
	FirstName VARCHAR(255) NULL,
	LastName VARCHAR(255) NULL,
	Address VARCHAR(255) NULL,
	Email [VARCHAR](255) NULL
);
";
        }

        public override string GetSqlForCreateDbObjectWithTokens(string objectName)
        {
            var dbObject = GetObjectNameWithSchema($@"{objectName}_${{Token1}}_${{Token2}}_${{Token3}}");
            return $@"
CREATE TABLE {dbObject.Item1}.{dbObject.Item2} (
	VisitorID SERIAL NOT NULL,
	FirstName VARCHAR(255) NULL,
	LastName VARCHAR(255) NULL,
	Address VARCHAR(255) NULL,
	Email VARCHAR(255) NULL
);
";
        }

        public override string GetSqlForCreateBulkTable(string objectName)
        {
            var dbObject = GetObjectNameWithSchema(objectName);
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
            var dbObject = GetObjectNameWithSchema(objectName);
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

        public override string GetSqlForCleanup()
        {
            var dbObject1 = GetObjectNameWithSchema(TEST_DBOBJECTS.DB_OBJECT_1);
            var dbObject2 = GetObjectNameWithSchema(TEST_DBOBJECTS.DB_OBJECT_2);
            var dbObject3 = GetObjectNameWithSchema(TEST_DBOBJECTS.DB_OBJECT_3);

            return $@"
DROP TABLE IF EXISTS {dbObject1.Item1}.{dbObject1.Item2};
DROP TABLE IF EXISTS {dbObject2.Item1}.{dbObject2.Item2};
DROP TABLE IF EXISTS {dbObject3.Item1}.{dbObject3.Item2};
";
        }

        private Tuple<string, string> GetObjectNameWithSchema(string objectName)
        {
            //check if a non-default dbo schema is used
            var schemaName = "public";
            var newObjectName = objectName;

            if (objectName.IndexOf('.') > 0)
            {
                schemaName = objectName.Split('.')[0];
                newObjectName = objectName.Split('.')[1];
            }

            //we do this because postgres always converts unquoted names into small case
            schemaName = schemaName.HasUpper() ? schemaName.DoubleQuote() : schemaName;
            newObjectName = newObjectName.HasUpper() ? newObjectName.DoubleQuote() : newObjectName;

            return new Tuple<string, string>(schemaName, newObjectName);
        }

        //https://dba.stackexchange.com/questions/11893/force-drop-db-while-others-may-be-connected
        public override void DropDatabase(string connectionString)
        {
            var sqlStatements = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Platforms", "PostgreSql", "Erase.sql"));
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
            base.ExecuteNonQuery(connectionStringBuilder.ConnectionString, sqlStatements);

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
