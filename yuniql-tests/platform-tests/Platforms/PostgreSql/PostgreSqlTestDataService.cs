using Yuniql.Extensibility;
using System.IO;
using Npgsql;
using System;

namespace Yuniql.PlatformTests
{
    public class PostgreSqlTestDataService : TestDataServiceBase
    {
        public PostgreSqlTestDataService(IDataService dataService): base(dataService)
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

        public override string GetSqlForCreateDbSchema(string schemaName)
        {
            return $@"
CREATE SCHEMA {schemaName};
";
        }

        public override string GetSqlForCreateDbObject(string objectName)
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

        public override string GetSqlForCreateDbObjectWithError(string objectName)
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

        public override string GetSqlForCreateDbObjectWithTokens(string objectName)
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

        public override string GetSqlForCreateBulkTable(string tableName)
        {
            return $@"
CREATE TABLE {tableName}(
	FirstName VARCHAR(50) NOT NULL,
	LastName VARCHAR(50) NOT NULL,
	BirthDate TIMESTAMP NULL
);
";
        }

        public override string GetSqlForSingleLine(string objectName)
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

        public override string GetSqlForSingleLineWithoutTerminator(string objectName)
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

        public override string GetSqlForMultilineWithoutTerminatorInLastLine(string objectName1, string objectName2, string objectName3)
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

        public override string GetSqlForMultilineWithTerminatorInsideStatements(string objectName1, string objectName2, string objectName3)
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

        public override string GetSqlForMultilineWithError(string objectName1, string objectName2)
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
