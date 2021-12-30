using Yuniql.Extensibility;
using System;
using System.Data.SqlClient;
using System.IO;
using Yuniql.Core;
using Yuniql.PlatformTests.Setup;

namespace Yuniql.PlatformTests.Platforms.SqlServer
{
    public class SqlServerTestDataService : TestDataServiceBase
    {
        public SqlServerTestDataService(IDataService dataService, ITokenReplacementService tokenReplacementService) : base(dataService, tokenReplacementService)
        {
        }

        public override string GetConnectionString(string databaseName)
        {
            var connectionString = EnvironmentHelper.GetEnvironmentVariable("YUNIQL_TEST_CONNECTION_STRING");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ApplicationException("Missing environment variable YUNIQL_TEST_CONNECTION_STRING. See WIKI for developer guides.");
            }

            var result = new SqlConnectionStringBuilder(connectionString);
            result.InitialCatalog = databaseName;

            return result.ConnectionString;
        }

        public override bool CheckIfDbExist(string connectionString)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            var sqlStatement = $"SELECT 1 FROM [sys].[databases] WHERE name = '{connectionStringBuilder.InitialCatalog}'";

            //check if database exists and auto-create when its not
            var masterConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            masterConnectionStringBuilder.InitialCatalog = "master";

            return base.QuerySingleBool(masterConnectionStringBuilder.ConnectionString, sqlStatement);
        }

        public override bool CheckIfDbObjectExist(string connectionString, string objectName)
        {
            var sqlStatement = $"SELECT ISNULL(OBJECT_ID('{objectName}'), 0) AS ObjectID";
            return base.QuerySingleBool(connectionString, sqlStatement);
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
CREATE TABLE {dbObject.Item1}.{dbObject.Item2}(
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
CREATE TABLE {dbObject.Item1}.{dbObject.Item2}(
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
CREATE TABLE {dbObject.Item1}.{dbObject.Item2}(
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
	[FirstName] [nvarchar](50) NOT NULL,
	[LastName] [nvarchar](50) NOT NULL,
	[BirthDate] [nvarchar](50) NULL
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
            var dbObject = GetObjectNameWithSchema(objectName);
            return $@"
CREATE TABLE {dbObject.Item1}.{dbObject.Item2}(
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
GO
";
        }

        public override string GetSqlForSingleLineWithoutTerminator(string objectName)
        {
            var dbObject = GetObjectNameWithSchema(objectName);
            return $@"
CREATE TABLE {dbObject.Item1}.{dbObject.Item2}(
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
";
        }

        public override string GetSqlForMultilineWithoutTerminatorInLastLine(string objectName1, string objectName2, string objectName3)
        {
            var dbObject1 = GetObjectNameWithSchema(objectName1);
            var dbObject2 = GetObjectNameWithSchema(objectName2);
            var dbObject3 = GetObjectNameWithSchema(objectName3);

            return $@"
CREATE TABLE {dbObject1.Item1}.{dbObject1.Item2}(
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
GO

CREATE TABLE {dbObject2.Item1}.{dbObject2.Item2}(
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
GO

CREATE TABLE {dbObject3.Item1}.{dbObject3.Item2}(
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
";
        }

        public override string GetSqlForMultilineWithTerminatorInCommentBlock(string objectName1, string objectName2, string objectName3)
        {
            var dbObject1 = GetObjectNameWithSchema(objectName1);
            var dbObject2 = GetObjectNameWithSchema(objectName2);
            var dbObject3 = GetObjectNameWithSchema(objectName3);

            return $@"
--GO inline comment
CREATE TABLE {dbObject1.Item1}.{dbObject1.Item2}(
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
GO

/*
GO in inline comment block
*/

CREATE TABLE {dbObject2.Item1}.{dbObject2.Item2}(
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
GO

/* multiline comment block
GO
*/

CREATE TABLE {dbObject3.Item1}.{dbObject3.Item2}(
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
GO
";
        }

        public override string GetSqlForMultilineWithTerminatorInsideStatements(string objectName1, string objectName2, string objectName3)
        {
            var dbObject1 = GetObjectNameWithSchema(objectName1);
            var dbObject2 = GetObjectNameWithSchema(objectName2);
            var dbObject3 = GetObjectNameWithSchema(objectName3);

            return $@"
CREATE TABLE {dbObject1.Item1}.{dbObject1.Item2}(
    --this is a comment with GO as part of the sentence (ALL CAPS)
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
GO

CREATE TABLE {dbObject2.Item1}.{dbObject2.Item2}(
    --this is a comment with go as part of the sentence (small caps)
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
GO
CREATE TABLE {dbObject3.Item1}.{dbObject3.Item2}(
    --this is a comment with Go as part of the sentence (Pascal)
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
GO
";
        }

        public override string GetSqlForMultilineWithError(string objectName1, string objectName2)
        {
            var dbObject1 = GetObjectNameWithSchema(objectName1);
            var dbObject2 = GetObjectNameWithSchema(objectName2);

            return $@"
CREATE TABLE {dbObject1.Item1}.{dbObject1.Item2}(
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
GO

CREATE TABLE {dbObject2.Item2}.{dbObject2.Item2}(
	TEST_DB_COLUMN_1 INT NOT NULL,
	TEST_DB_COLUMN_2 VARCHAR(255) NULL,
	TEST_DB_COLUMN_3 VARCHAR(255) NULL
);
GO

--throws divide by zero error
INSERT INTO {dbObject1.Item1}.{dbObject1.Item2} (TEST_DB_COLUMN_1) VALUES (3/0);
GO

INSERT INTO {dbObject1.Item1}.{dbObject1.Item2} (TEST_DB_COLUMN_1) VALUES (1);
INSERT INTO {dbObject1.Item1}.{dbObject1.Item2} (TEST_DB_COLUMN_1) VALUES (2);
GO
";
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
            var schemaName = base.SchemaName;
            var newObjectName = objectName;

            if (objectName.IndexOf('.') > 0)
            {
                schemaName = objectName.Split('.')[0];
                newObjectName = objectName.Split('.')[1];
            }

            //we keep the original value as sql server is not case sensitive
            return new Tuple<string, string>(schemaName, newObjectName);
        }

        //TODO: Refactor this into Erase!
        public override void DropDatabase(string connectionString)
        {
            var sqlStatements = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Platforms", "SqlServer", "Erase.sql"));
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            base.ExecuteNonQuery(connectionStringBuilder.ConnectionString, sqlStatements);

            ////capture the test database from connection string
            //var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            //var sqlStatement = @$"
            //ALTER DATABASE [{connectionStringBuilder.InitialCatalog}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
            //DROP DATABASE [{connectionStringBuilder.InitialCatalog}];
            //";

            ////switch connection string to use master database
            //var masterConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            //masterConnectionStringBuilder.InitialCatalog = "master";

            //base.ExecuteNonQuery(masterConnectionStringBuilder.ConnectionString, sqlStatement);
        }
    }
}
