using Yuniql.Extensibility;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Npgsql;

namespace Yuniql.PostgreSql
{
    public class PostgreSqlTestDataService : ITestDataService
    {
        public string GetConnectionString(string databaseName)
        {
            var connectionString = EnvironmentHelper.GetEnvironmentVariable("YUNIQL_TEST_CONNECTION_STRING");
            if (string.IsNullOrEmpty(connectionString))
            {
                //use this when running against docker container with published port 5432
                return $"Host=localhost;Port=5432;Username=app;Password=app;Database={databaseName}";
            }

            var result = new NpgsqlConnectionStringBuilder(connectionString);
            result.Database = databaseName;

            return result.ConnectionString;
        }

        public string GetCurrentDbVersion(string connectionString)
        {
            var sqlStatement = $"SELECT Version FROM __YuniqlDbVersion ORDER BY Id DESC LIMIT 1;";
            return QuerySingleString(connectionString, sqlStatement);
        }

        public List<DbVersion> GetAllDbVersions(string connectionString)
        {
            var result = new List<DbVersion>();

            var sqlStatement = $"SELECT Id, Version, DateInsertedUtc, LastUserId FROM __YuniqlDbVersion ORDER BY Version ASC;";
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlStatement;
                command.CommandTimeout = 0;

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
            var sqlStatement = $"SELECT 1 FROM pg_class WHERE  relname = '{objectName}'";
            return QuerySingleBool(connectionString, sqlStatement);
        }

        public bool QuerySingleBool(string connectionString, string sqlStatement)
        {
            bool result;
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlStatement;
                command.CommandTimeout = 0;

                var reader = command.ExecuteReader();
                reader.Read();
                result = Convert.ToBoolean(reader.GetValue(0));
            }

            return result;
        }

        public string QuerySingleString(string connectionString, string sqlStatement)
        {
            string result = null;
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlStatement;
                command.CommandTimeout = 0;

                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    result = reader.GetString(0);
                }
            }

            return result;
        }

        public string CreateDbObjectScript(string scriptName)
        {
            return $@"
CREATE PROC [dbo].[{scriptName}]
AS
    SELECT 1;
GO
                ";
        }

        public string CreateTokenizedDbObjectScript(string objectName)
        {
            return $@"
CREATE PROC [dbo].[{objectName}_${{Token1}}_${{Token2}}_${{Token3}}]
AS
    SELECT '${{Token1}}.${{Token2}}.${{Token3}}' AS ReplacedStatement;
";
        }

        public string CreateBulkTableScript(string tableName)
        {
            return $@"
IF (NOT EXISTS(SELECT 1 FROM [sys].[objects] WHERE type = 'U' AND name = '{tableName}'))
BEGIN
    CREATE TABLE [dbo].[{tableName}](
	    [FirstName] [nvarchar](50) NULL,
	    [LastName] [nvarchar](50) NULL,
	    [BirthDate] [datetime] NULL
    );
END
            ";
        }

        public string CreateSingleLineScript(string objectName)
        {
            return $@"
CREATE PROC [dbo].[{objectName}]
AS
    SELECT 1;
GO
";
        }

        public string CreateSingleLineScriptWithoutTerminator(string objectName)
        {
            return $@"
CREATE PROC [dbo].[{objectName}]
AS
    SELECT 1;
";
        }

        public string CreateMultilineScriptWithoutTerminatorInLastLine(string objectName1, string objectName2, string objectName3)
        {
            return $@"
CREATE PROC [dbo].[{objectName1}]
AS
    SELECT 1;
GO

CREATE PROC [dbo].[{objectName2}]
AS
    SELECT 1;
GO

CREATE PROC [dbo].[{objectName3}]
AS
    SELECT 1;
";
        }

        public string CreateMultilineScriptWithTerminatorInsideStatements(string objectName1, string objectName2, string objectName3)
        {
            return $@"
CREATE PROC [dbo].[{objectName1}]
AS
    --this is a comment with GO as part of the sentence (ALL CAPS)
    SELECT 1;
GO

CREATE PROC [dbo].[{objectName2}]
AS
    --this is a comment with go as part of the sentence (small caps)
    SELECT 1;
GO

CREATE PROC [dbo].[{objectName3}]
AS
    --this is a comment with Go as part of the sentence (Pascal)
    SELECT 1;
";
        }

        public string CreateMultilineScriptWithError(string objectName1, string objectName2)
        {
            return $@"
CREATE TABLE [dbo].[{objectName1}](        
    [TestId][INT] IDENTITY(1, 1) NOT NULL,        
    [TestColumn] [DECIMAL] NOT NULL
)
GO

CREATE PROC [dbo].[{objectName2}]
AS
    SELECT 1;
GO

--throws divide by zero error
INSERT INTO [dbo].[{objectName1}] (TestColumn) VALUES (3/0);
GO

INSERT INTO [dbo].[{objectName1}] (TestColumn) VALUES (1);
INSERT INTO [dbo].[{objectName1}] (TestColumn) VALUES (2);
GO
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
DROP PROCEDURE [dbo].[script1];
DROP PROCEDURE [dbo].[script2];
DROP PROCEDURE [dbo].[script3];
";
        }
    }
}
