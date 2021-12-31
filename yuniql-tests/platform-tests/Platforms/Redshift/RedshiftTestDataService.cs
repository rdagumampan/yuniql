using Yuniql.Extensibility;
using System.IO;
using Npgsql;
using System;
using Yuniql.Core;
using Yuniql.PostgreSql;
using Yuniql.PlatformTests.Setup;
using System.Collections.Generic;

namespace Yuniql.PlatformTests.Platforms.Redshift
{
    public class RedshiftTestDataService : TestDataServiceBase
    {
        public RedshiftTestDataService(IDataService dataService, ITokenReplacementService tokenReplacementService) : base(dataService, tokenReplacementService)
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
            connectionStringBuilder.Database = "dev";
            return QuerySingleBool(connectionStringBuilder.ConnectionString, sqlStatement);
        }

        public override bool CheckIfDbObjectExist(string connectionString, string objectName)
        {
            var dbObject = GetObjectNameWithSchema(objectName);
            var dbSchemaName = dbObject.Item1.IsDoubleQuoted() ? dbObject.Item1.UnQuote() : dbObject.Item1;
            var dbObjectName = dbObject.Item2.IsDoubleQuoted() ? dbObject.Item2.UnQuote() : dbObject.Item2;

            //check from procedures, im just lazy to figure out join in pgsql :)
            var sqlStatement = $"SELECT 1 FROM information_schema.tables WHERE TABLE_SCHEMA = '{dbSchemaName}'  AND TABLE_NAME = '{dbObjectName}'";
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
CREATE TABLE {dbObject.Item1}.{dbObject.Item2} (
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
CREATE TABLE {dbObject.Item1}.{dbObject.Item2} (
	TEST_DB_COLUMN_1 INT NOT NULL THIS_IS_AN_ERROR,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
";
        }

        public override string GetSqlForCreateDbObjectWithTokens(string objectName)
        {
            var dbObject = GetObjectNameWithSchema(objectName);
            return $@"
CREATE TABLE {dbObject.Item1}.{dbObject.Item2}_${{Token1}}_${{Token2}}_${{Token3}} (
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

        public override string GetSqlForEraseDbObjects()
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

        //TODO: Move this into Extensibility namespace
        private Tuple<string, string> GetObjectNameWithSchema(string objectName)
        {
            //check if a non-default dbo schema is used
            var schemaName = base.SchemaName;
            var newObjectName = objectName;

            if (objectName.IndexOf('.') > 0)
            {
                schemaName = objectName.Split('.')[0];
                newObjectName = objectName.Split('.')[1];
            }

            //we do this because postgres always converts all names into small case
            //this is regardless if the names is double qouted names, it still ends up as lower case
            schemaName = schemaName.HasUpper() ? schemaName.ToLower() : schemaName;
            newObjectName = newObjectName.HasUpper() ? newObjectName.ToLower() : newObjectName;

            return new Tuple<string, string>(schemaName, newObjectName);
        }

        //SELECT '"'+datname+'",' FROM pg_database where datname like '%yuniql_test%'
        //https://dba.stackexchange.com/questions/11893/force-drop-db-while-others-may-be-connected
        public override void CleanupDbObjects(string connectionString)
        {
            var sqlStatements = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Platforms", "Redshift", "Cleanup.sql"));
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
            base.ExecuteNonQuery(connectionStringBuilder.ConnectionString, sqlStatements);

            //            //not needed need since test cases are executed against disposable database containers
            //            //we could simply docker rm the running test container after tests completed

            //            //use the target user database to migrate, this is part of orig connection string
            //            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
            //            var databaseName = connectionStringBuilder.Database;

            //            var sqlStatements = new List<string> {
            //$@"
            //--disallow new connections
            //ALTER DATABASE {databaseName.DoubleQuote()} CONNECTION LIMIT 1;
            //",
            //$@"
            //--terminate existing connections
            //SELECT pg_terminate_backend(procpid) FROM pg_stat_activity WHERE datname = '{databaseName}';
            //",
            //$@"
            //--drop database
            //DROP DATABASE {databaseName.DoubleQuote()};
            //",
            //            };

            //            //switch database into master/system database where db catalogs are maintained
            //            connectionStringBuilder.Database = "dev";
            //            sqlStatements.ForEach(sqlStatement =>
            //            {
            //                ExecuteNonQuery(connectionStringBuilder.ConnectionString, sqlStatement);
            //            });
        }
    }
}
