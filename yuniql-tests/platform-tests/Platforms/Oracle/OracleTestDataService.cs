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

            var sqlStatement = $"SELECT 1 FROM SYS.ALL_TABLES WHERE TABLE_NAME = TABLE_NAME = '{dbObject.Item2}'";
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
            return $@"
CREATE TABLE {objectName} (
	VisitorID INT NOT NULL,
	FirstName VARCHAR(255) NULL,
	LastName VARCHAR(255) NULL,
	Address VARCHAR(255) NULL,
	Email VARCHAR(255) NULL
);
";
        }

        public override string GetSqlForCreateDbObjectWithError(string objectName)
        {
            return $@"
CREATE TABLE {objectName} (
	VisitorID INT NOT NULL,
	FirstName VARCHAR(255) NULL,
	LastName VARCHAR(255) NULL,
	Address VARCHAR(255) NULL,
	Email [VARCHAR](255) NULL
);
";
        }

        public override string GetSqlForCreateDbObjectWithTokens(string objectName)
        {
            return $@"
CREATE TABLE public.{objectName}_${{Token1}}_${{Token2}}_${{Token3}} (
	VisitorID INT NOT NULL,
	FirstName VARCHAR(255) NULL,
	LastName VARCHAR(255) NULL,
	Address VARCHAR(255) NULL,
	Email VARCHAR(255) NULL
);
";
        }

        public override string GetSqlForCreateBulkTable(string objectName)
        {
            return $@"
CREATE TABLE {objectName}(
	FirstName VARCHAR(50) NOT NULL,
	LastName VARCHAR(50) NOT NULL,
	BirthDate TIMESTAMP NULL
);
";
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
            return @"
DROP TABLE script1;
DROP TABLE script2;
DROP TABLE script3;
";
        }

        private Tuple<string, string> GetObjectNameWithSchema(string objectName)
        {
            //check if a non-default dbo schema is used
            var schemaName = "";
            var newObjectName = objectName;

            if (objectName.IndexOf('.') > 0)
            {
                schemaName = objectName.Split('.')[0];
                newObjectName = objectName.Split('.')[1];
            }

            return new Tuple<string, string>(schemaName.ToLower(), newObjectName.ToLower());
        }

        //SELECT '"'+datname+'",' FROM pg_database where datname like '%yuniql_test%'
        //https://dba.stackexchange.com/questions/11893/force-drop-db-while-others-may-be-connected
        public override void DropDatabase(string connectionString)
        {
            //not needed need since test cases are executed against disposable database containers
            //we could simply docker rm the running test container after tests completed

            //use the target user database to migrate, this is part of orig connection string
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
            var databaseName = connectionStringBuilder.Database;

            var sqlStatements = new List<string> {
$@"
--disallow new connections
ALTER DATABASE {databaseName.DoubleQuote()} CONNECTION LIMIT 1;
",
$@"
--terminate existing connections
SELECT pg_terminate_backend(procpid) FROM pg_stat_activity WHERE datname = '{databaseName}';
",
$@"
--drop database
DROP DATABASE {databaseName.DoubleQuote()};
",
            };

            //switch database into master/system database where db catalogs are maintained
            connectionStringBuilder.Database = "dev";
            sqlStatements.ForEach(sqlStatement =>
            {
                ExecuteNonQuery(connectionStringBuilder.ConnectionString, sqlStatement);
            });
        }
    }
}
