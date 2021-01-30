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

        public override string GetSqlForCreateDbObject(string scriptName)
        {
            return $@"
CREATE PROC {scriptName}
AS
    SELECT 1;
GO
";
        }

        public override string GetSqlForCreateDbObjectWithError(string scriptName)
        {
            return $@"
CREATE PROC [NONEXISTINGDB].[dbo].[{scriptName}]
AS
    SELECT 1/0;
GO
                ";
        }
        public override string GetSqlForCreateDbObjectWithTokens(string objectName)
        {
            return $@"
CREATE PROC {objectName}_${{Token1}}_${{Token2}}_${{Token3}}
AS
    SELECT '${{Token1}}.${{Token2}}.${{Token3}}' AS ReplacedStatement;
";
        }

        public override string GetSqlForCreateBulkTable(string tableName)
        {
            return $@"
IF (NOT EXISTS(SELECT 1 FROM [sys].[objects] WHERE type = 'U' AND name = '{tableName}'))
BEGIN
    CREATE TABLE {tableName}(
	    [FirstName] [nvarchar](50) NOT NULL,
	    [LastName] [nvarchar](50) NOT NULL,
	    [BirthDate] [datetime] NULL
    );
END
";
        }

        public override string GetSqlForSingleLine(string objectName)
        {
            return $@"
CREATE PROC {objectName}
AS
    SELECT 1;
GO
";
        }

        public override string GetSqlForSingleLineWithoutTerminator(string objectName)
        {
            return $@"
CREATE PROC {objectName}
AS
    SELECT 1;
";
        }

        public override string GetSqlForMultilineWithoutTerminatorInLastLine(string objectName1, string objectName2, string objectName3)
        {
            return $@"
CREATE PROC {objectName1}
AS
    SELECT 1;
GO

CREATE PROC {objectName2}
AS
    SELECT 1;
GO

CREATE PROC {objectName3}
AS
    SELECT 1;
";
        }

        public override string GetSqlForMultilineWithTerminatorInCommentBlock(string objectName1, string objectName2, string objectName3)
        {
            return $@"
--GO inline comment
CREATE PROC {objectName1}
AS
    SELECT 1;
GO

/*
GO in inline comment block
*/

CREATE PROC {objectName2}
AS
    SELECT 1;
GO

/* multiline comment block
GO
*/

CREATE PROC {objectName3}
AS
    SELECT 1;
";
        }

        public override string GetSqlForMultilineWithTerminatorInsideStatements(string objectName1, string objectName2, string objectName3)
        {
            return $@"
CREATE PROC {objectName1}
AS
    --this is a comment with GO as part of the sentence (ALL CAPS)
    SELECT 1;
GO

CREATE PROC {objectName2}
AS
    --this is a comment with go as part of the sentence (small caps)
    SELECT 1;
GO

CREATE PROC {objectName3}
AS
    --this is a comment with Go as part of the sentence (Pascal)
    SELECT 1;
";
        }

        public override string GetSqlForMultilineWithError(string objectName1, string objectName2)
        {
            return $@"
CREATE TABLE {objectName1}(        
    [TestId][INT] IDENTITY(1, 1) NOT NULL,        
    [TestColumn] [DECIMAL] NOT NULL
)
GO

CREATE PROC {objectName2}
AS
    SELECT 1;
GO

--throws divide by zero error
INSERT INTO {objectName1} (TestColumn) VALUES (3/0);
GO

INSERT INTO {objectName1} (TestColumn) VALUES (1);
INSERT INTO {objectName1} (TestColumn) VALUES (2);
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
            return @"
DROP PROCEDURE script1;
DROP PROCEDURE script2;
DROP PROCEDURE script3;
";
        }

        public override void DropDatabase(string connectionString)
        {
            //capture the test database from connection string
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            var sqlStatement = @$"
ALTER DATABASE [{connectionStringBuilder.InitialCatalog}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE [{connectionStringBuilder.InitialCatalog}];
";

            //switch connection string to use master database
            var masterConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            masterConnectionStringBuilder.InitialCatalog = "master";

            base.ExecuteNonQuery(masterConnectionStringBuilder.ConnectionString, sqlStatement);
        }
    }
}
