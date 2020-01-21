using Yuniql.Extensibility;
using System.Collections.Generic;
using System.IO;
using Npgsql;
using System;
using System.Data;
using Yuniql.Core;

namespace Yuniql.PlatformTests
{
    public class PostgreSqlTestDataService : ITestDataService
    {
        private readonly IDataService _dataService;

        public PostgreSqlTestDataService(IDataService dataService)
        {
            this._dataService = dataService;
        }

        public bool IsAtomicDDLSupported => _dataService.IsAtomicDDLSupported;

        public bool IsSchemaSupported => _dataService.IsSchemaSupported;

        public string GetConnectionString(string databaseName)
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

        public bool QuerySingleBool(string connectionString, string sqlStatement)
        {
            _dataService.Initialize(connectionString);
            using (var connection = _dataService.CreateConnection().KeepOpen())
            {
                return connection.QuerySingleBool(sqlStatement);
            }
        }

        public string QuerySingleString(string connectionString, string sqlStatement)
        {
            _dataService.Initialize(connectionString);
            using (var connection = _dataService.CreateConnection().KeepOpen())
            {
                return connection.QuerySingleString(sqlStatement);
            }
        }

        public string GetCurrentDbVersion(string connectionString)
        {
            _dataService.Initialize(connectionString);
            var sqlStatement = _dataService.GetGetCurrentVersionSql();
            using (var connection = _dataService.CreateConnection().KeepOpen())
            {
                return connection.QuerySingleString(commandText: sqlStatement);
            }
        }

        public List<DbVersion> GetAllDbVersions(string connectionString)
        {

            _dataService.Initialize(connectionString);
            var sqlStatement = _dataService.GetGetAllVersionsSql();

            var result = new List<DbVersion>();
            using (var connection = _dataService.CreateConnection().KeepOpen())
            {
                var command = connection.CreateCommand(commandText: sqlStatement);

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var dbVersion = new DbVersion
                    {
                        Id = reader.GetInt16(0),
                        Version = reader.GetString(1),
                        DateInsertedUtc = reader.GetDateTime(2),
                        LastUserId = reader.GetString(3)
                    };
                    result.Add(dbVersion);
                }
            }

            return result;
        }

        public bool CheckIfDbExist(string connectionString)
        {
            //use the target user database to migrate, this is part of orig connection string
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
            var sqlStatement = $"SELECT 1 from pg_database WHERE datname ='{connectionStringBuilder.Database}';";

            //switch database into master/system database where db catalogs are maintained
            connectionStringBuilder.Database = "postgres";
            return QuerySingleBool(connectionStringBuilder.ConnectionString, sqlStatement);
        }

        public bool CheckIfDbObjectExist(string connectionString, string objectName)
        {
            var dbObject = GetObjectNameWithSchema(objectName);

            //check from procedures, im just lazy to figure out join in pgsql :)
            var sqlStatement = $"SELECT 1 FROM pg_proc WHERE  proname = '{objectName.ToLower()}'";
            bool result = QuerySingleBool(connectionString, sqlStatement);

            //check from tables, im just lazy to figure out join in pgsql :)
            if (!result)
            {
                sqlStatement = $"SELECT 1 FROM pg_class WHERE  relname = '{objectName.ToLower()}'";
                result = QuerySingleBool(connectionString, sqlStatement);
            }

            if (!result) {
                sqlStatement = $"SELECT 1 FROM information_schema.tables WHERE TABLE_SCHEMA = '{dbObject.Item1}'  AND TABLE_NAME = '{dbObject.Item2}'";
                result = QuerySingleBool(connectionString, sqlStatement);
            }

            return result;
        }

        public string CreateDbSchemaScript(string schemaName)
        {
            return $@"
CREATE SCHEMA {schemaName};
";
        }

        public string CreateDbObjectScript(string objectName)
        {
            return $@"
CREATE TABLE public.{objectName} (
	VisitorID SERIAL NOT NULL,
	FirstName VARCHAR(255) NULL,
	LastName VARCHAR(255) NULL,
	Address VARCHAR(255) NULL,
	Email VARCHAR(255) NULL
);
";
        }

        public string CreateDbObjectScriptWithError(string objectName)
        {
            return $@"
CREATE TABLE public.{objectName} (
	VisitorID SERIAL NOT NULL,
	FirstName VARCHAR(255) NULL,
	LastName VARCHAR(255) NULL,
	Address VARCHAR(255) NULL,
	Email [VARCHAR](255) NULL
);
";
        }

        public string CreateTokenizedDbObjectScript(string objectName)
        {
            return $@"
CREATE TABLE public.{objectName}_${{Token1}}_${{Token2}}_${{Token3}} (
	VisitorID SERIAL NOT NULL,
	FirstName VARCHAR(255) NULL,
	LastName VARCHAR(255) NULL,
	Address VARCHAR(255) NULL,
	Email VARCHAR(255) NULL
);
";
        }

        public string CreateBulkTableScript(string tableName)
        {
            return $@"
CREATE TABLE {tableName}(
	FirstName VARCHAR(50) NOT NULL,
	LastName VARCHAR(50) NOT NULL,
	BirthDate TIMESTAMP NULL
);
";
        }

        public string CreateSingleLineScript(string objectName)
        {
            return $@"
CREATE TABLE public.{objectName} (
	VisitorID SERIAL NOT NULL,
	FirstName VARCHAR(255) NULL,
	LastName VARCHAR(255) NULL,
	Address VARCHAR(255) NULL,
	Email VARCHAR(255) NULL
);
";
        }

        public string CreateSingleLineScriptWithoutTerminator(string objectName)
        {
            return $@"
CREATE TABLE public.{objectName} (
	VisitorID SERIAL NOT NULL,
	FirstName VARCHAR(255) NULL,
	LastName VARCHAR(255) NULL,
	Address VARCHAR(255) NULL,
	Email VARCHAR(255) NULL
)
";
        }

        public string CreateMultilineScriptWithoutTerminatorInLastLine(string objectName1, string objectName2, string objectName3)
        {
            return $@"
CREATE TABLE public.{objectName1} (
	VisitorID SERIAL NOT NULL,
	FirstName VARCHAR(255) NULL,
	LastName VARCHAR(255) NULL,
	Address VARCHAR(255) NULL,
	Email VARCHAR(255) NULL
);

CREATE VIEW public.{objectName2} AS
SELECT VisitorId, FirstName, LastName, Address, Email
FROM  public.{objectName1};

CREATE OR REPLACE FUNCTION public.{objectName3} ()
RETURNS integer AS ${objectName3}$
declare
	total integer;
BEGIN
   SELECT count(*) into total FROM public.{objectName1};
   RETURN total;
END;
${objectName3}$ LANGUAGE plpgsql
";
        }

        public string CreateMultilineScriptWithTerminatorInsideStatements(string objectName1, string objectName2, string objectName3)
        {
            return $@"
CREATE TABLE public.{objectName1} (
	VisitorID SERIAL NOT NULL,
	FirstName VARCHAR(255) NULL,
	LastName VARCHAR(255) NULL,
	Address VARCHAR(255) NULL,
	Email VARCHAR(255) NULL
);

CREATE VIEW public.{objectName2} AS
SELECT VisitorId, FirstName, LastName, Address, Email
FROM  public.{objectName1};

CREATE OR REPLACE FUNCTION public.{objectName3} ()
RETURNS integer AS ${objectName3}$
declare
	total integer;
BEGIN
    --this is a comment with terminator ; as part of the sentence;
    --;this is a comment with terminator ; as part of the sentence
   SELECT count(*) into total FROM public.{objectName1};
   RETURN total;
END;
${objectName3}$ LANGUAGE plpgsql
";
        }

        public string CreateMultilineScriptWithError(string objectName1, string objectName2)
        {
            return $@"
CREATE TABLE public.{objectName1} (
	VisitorID SERIAL NOT NULL,
	FirstName VARCHAR(255) NULL,
	LastName VARCHAR(255) NULL,
	Address VARCHAR(255) NULL,
	Email VARCHAR(255) NULL
);

CREATE VIEW public.{objectName2} AS
SELECT VisitorId, FirstName, LastName, Address, Email
FROM  public.{objectName1};

SELECT 1/0;
";
        }

        public void CreateScriptFile(string sqlFilePath, string sqlStatement)
        {
            using var sw = File.CreateText(sqlFilePath);
            sw.WriteLine(sqlStatement);
        }

        public string CreateCleanupScript()
        {
            return @"
DROP TABLE script1;
DROP TABLE script2;
DROP TABLE script3;
";
        }

        public List<BulkTestDataRow> GetBulkTestData(string connectionString, string tableName)
        {
            var results = new List<BulkTestDataRow>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                var sqlStatement = $"SELECT * FROM {tableName};";
                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlStatement;
                command.CommandTimeout = 0;

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new BulkTestDataRow
                        {
                            FirstName = !reader.IsDBNull(0) ? reader.GetString(0) : null,
                            LastName = !reader.IsDBNull(1) ? reader.GetString(1) : null,
                            BirthDate = !reader.IsDBNull(2) ? reader.GetDateTime(2) : new DateTime?()
                        });
                    }
                }
            }
            return results;
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

            return new Tuple<string, string>(schemaName.ToLower(), newObjectName.ToLower());
        }

    }
}
