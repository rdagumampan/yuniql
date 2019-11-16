using Yuniql.Extensibility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace Yuniql.SqlServer.Tests
{
    public static class TestDbHelper
    {
        public static string GetWorkingPath()
        {
            return Path.Combine(Environment.CurrentDirectory, @$"yuniql_testdb_{Guid.NewGuid().ToString().Substring(0, 4)}");
        }

        public static void CleanUp(string workingPath)
        {
            if (Directory.Exists(workingPath))
            {
                Directory.Delete(workingPath, true);
            }
        }

        public static string GetConnectionString(string databaseName)
        {
            var connectionString = EnvironmentHelper.GetEnvironmentVariable("YUNIQL_CONNECTION_STRING");
            if (string.IsNullOrEmpty(connectionString))
            {
                //use this when running against local instance of sql server with integrated security
                //return $"Data Source=.;Integrated Security=SSPI;Initial Catalog={databaseName}";

                //use this when running against sql server container with published port 1400
                return $"Server=localhost,1400;Database={databaseName};User Id=SA;Password=P@ssw0rd!";
            }

            var result = new SqlConnectionStringBuilder(connectionString);
            result.InitialCatalog = databaseName;

            return result.ConnectionString;
        }

        public static bool CheckDbExist(string connectionString)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            var sqlStatement = $"SELECT ISNULL(database_id, 0) FROM [sys].[databases] WHERE name = '{connectionStringBuilder.InitialCatalog}'";

            //check if database exists and auto-create when its not
            var masterConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            masterConnectionStringBuilder.InitialCatalog = "master";

            var result = QuerySingleBool(masterConnectionStringBuilder.ConnectionString, sqlStatement);

            return result;
        }
        public static string GetCurrentVersion(string connectionString)
        {
            var sqlStatement = $"SELECT TOP 1 Version FROM dbo.__YuniqlDbVersion ORDER BY Id DESC";
            var result = TestDbHelper.QuerySingleString(connectionString, sqlStatement);

            return result;
        }

        public static List<DbVersion> GetAllDbVersions(string connectionString)
        {
            var result = new List<DbVersion>();

            var sqlStatement = $"SELECT Id, Version, DateInsertedUtc, LastUserId FROM [dbo].[__YuniqlDbVersion] ORDER BY Version ASC;";

            using (var connection = new SqlConnection(connectionString))
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

        public static bool QuerySingleBool(string connectionString, string sqlStatement)
        {
            bool result;
            using (var connection = new SqlConnection(connectionString))
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

        public static string QuerySingleString(string connectionString, string sqlStatement)
        {
            string result = null;
            using (var connection = new SqlConnection(connectionString))
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

        public static void CreateScriptFile(string sqlFilePath, string sqlStatement)
        {
            using var sw = File.CreateText(sqlFilePath);
            sw.WriteLine(sqlStatement);
        }

        public static string CreateDbObjectScript(string scriptName)
        {
            return $@"
CREATE PROC [dbo].[{scriptName}]
AS
    SELECT 1;
GO
                ";
        }

        public static string CreateTokenizedDbObjectScript(string objectName)
        {
            return $@"
CREATE PROC [dbo].[{objectName}_${{Token1}}_${{Token2}}_${{Token3}}]
AS
    SELECT '${{Token1}}.${{Token2}}.${{Token3}}' AS ReplacedStatement;
";
        }


        public static string CreateCheckDbObjectExistScript(string objectName)
        {
            return $"SELECT ISNULL(OBJECT_ID('[dbo].[{objectName}]'), 0) AS ObjectID";
        }

        public static string CreateBulkTableScript(string tableName)
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

        public static string CreateCleanupScript()
        {
            return @"
DROP PROCEDURE [dbo].[script1];
DROP PROCEDURE [dbo].[script2];
DROP PROCEDURE [dbo].[script3];
";
        }
    }
}
