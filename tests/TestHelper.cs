using System;
using System.IO;

namespace Yuniql.Tests
{
    public static class TestHelper
    {
        public static string GetConnectionString(string databaseName)
        {
            //use this when running against local instance of sql server with integrated security
            //return $"Data Source=.;Integrated Security=SSPI;Initial Catalog={databaseName}";

            //use this when running against sql server container with published port 1401
            return $"Server=localhost,1401;Database={databaseName};User Id=SA;Password=P@ssw0rd!";
        }

        public static string GetWorkingPath()
        {
            return Path.Combine(Environment.CurrentDirectory, @$"yuniqltests_{Guid.NewGuid().ToString().Substring(0, 4)}");
        }

        public static void CleanUp(string workingPath)
        {
            if (Directory.Exists(workingPath))
            {
                Directory.Delete(workingPath, true);
            }
        }
        public static string CreateScript(string scriptName)
        {
            return $@"
                CREATE PROC [dbo].[{scriptName}]
                AS
                    SELECT 1;
                GO
                ";
        }

        public static string CreateAssetScript(string scriptName)
        {
            return $"SELECT ISNULL(OBJECT_ID('[dbo].[{scriptName}]'), 0) AS ObjectID";
        }

        public static void CreateScriptFile(string sqlFilePath, string sqlStatement)
        {
            using (var sw = File.CreateText(sqlFilePath))
            {
                sw.WriteLine(sqlStatement);
            }
        }

    }
}
