using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System;

namespace Yuniql.SqlServer.Tests
{
    [TestClass]
    public class TestBase
    {
        public static string GetConnectionString(string databaseName)
        {
            var connectionString = EnvironmentHelper.GetEnvironmentVariable("YUNIQL_TEST_CONNECTION_STRING");
            if (string.IsNullOrEmpty(connectionString))
            {
                //use this when running against local instance of sql server with integrated security
                //return $"Data Source=.;Integrated Security=SSPI;Initial Catalog={databaseName}";

                //use this when running against sql server container with published port 1400
                return $"Server=localhost,1400;Database={databaseName};User Id=SA;Password=P@ssw0rd!";
            }

            var result = new NpgsqlConnectionStringBuilder(connectionString);
            result.Database = databaseName;

            return result.ConnectionString;
        }

        public string GetOrCreateWorkingPath()
        {
            var workingPath = Path.Combine(Environment.CurrentDirectory, @$"yuniql_testdb_{Guid.NewGuid().ToString().Substring(0, 4)}"); ;
            if (!Directory.Exists(workingPath))
            {
                Directory.CreateDirectory(workingPath);
            }

            return workingPath;
        }

        public void CleanUp(string workingPath)
        {
            if (Directory.Exists(workingPath))
            {
                Directory.Delete(workingPath, true);
            }
        }
    }
}
