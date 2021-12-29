using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System;
using Yuniql.Core;
using Yuniql.Extensibility;

namespace Yuniql.PlatformTests.Setup
{

    [TestClass]
    public class TestClassBase
    {
        private const string YUNIQL_TEST_DB = "yuniqldb";
        public string CreateEmptyWorkspace()
        {
            var workingPath = Path.Combine(Environment.CurrentDirectory, @$"yuniql_testdb_{Guid.NewGuid().ToString().Substring(0, 8)}"); ;
            if (!Directory.Exists(workingPath))
            {
                Directory.CreateDirectory(workingPath);
            }

            return workingPath;
        }

        public string CloneSampleWorkspace(string workingPath)
        {
            var source = EnvironmentHelper.GetEnvironmentVariable(ENVIRONMENT_TEST_VARIABLE.YUNIQL_TEST_SAMPLEDB);
            var destination = workingPath;

            foreach (string dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(source, destination));
            }

            foreach (string newPath in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(source, destination), true);
            }

            return workingPath;
        }

        //used in all tests except the cli tests where sql files are prepared for each test case
        public TestConfiguration ConfigureWithEmptyWorkspace()
        {
            //create test run configuration from empty workspace
            var workspacePath = CreateEmptyWorkspace();
            //var databaseName = new DirectoryInfo(workspacePath).Name;

            //prepare a unique connection string for each test case
            var connectionString = EnvironmentHelper.GetEnvironmentVariable(ENVIRONMENT_TEST_VARIABLE.YUNIQL_TEST_CONNECTION_STRING);
            //var platform = EnvironmentHelper.GetEnvironmentVariable(ENVIRONMENT_TEST_VARIABLE.YUNIQL_TEST_PLATFORM);            
            //if (!connectionString.Contains("=yuniqldb") && !platform.Equals(SUPPORTED_DATABASES.ORACLE))
            //{
            //    throw new Exception("Your default database in your test connection string must be \"yuniqldb\". This is replaced during test execution with test database per test case.");
            //}
            //connectionString = connectionString.Replace("yuniqldb", databaseName);
            return new TestConfiguration
            {
                Platform = EnvironmentHelper.GetEnvironmentVariable(ENVIRONMENT_TEST_VARIABLE.YUNIQL_TEST_PLATFORM),
                ConnectionString = connectionString,
                DatabaseName = YUNIQL_TEST_DB,
                WorkspacePath = workspacePath,
                CliProcessPath = GetCliProcessFile(),
                PluginsPath = EnvironmentHelper.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_PLUGINS),
                TestAgentHost = EnvironmentHelper.GetEnvironmentVariable(ENVIRONMENT_TEST_VARIABLE.YUNIQL_TEST_HOST)
            };
        }

        //used in the cli tests so we have ready to run sample repo per platform
        public TestConfiguration ConfigureWorkspaceWithSampleDb()
        {
            //create test run configuration from sample workspace
            var workspacePath = CreateEmptyWorkspace();
            //var databaseName = new DirectoryInfo(workspacePath).Name;

            //copy sample db project
            CloneSampleWorkspace(workspacePath);

            //prepare a unique connection string for each test case
            var connectionString = EnvironmentHelper.GetEnvironmentVariable(ENVIRONMENT_TEST_VARIABLE.YUNIQL_TEST_CONNECTION_STRING);
            //connectionString = connectionString.Replace("yuniqldb", databaseName);

            return new TestConfiguration
            {
                Platform = EnvironmentHelper.GetEnvironmentVariable(ENVIRONMENT_TEST_VARIABLE.YUNIQL_TEST_PLATFORM),
                ConnectionString = connectionString,
                DatabaseName = YUNIQL_TEST_DB,
                WorkspacePath = workspacePath,
                CliProcessPath = GetCliProcessFile(),
                PluginsPath = EnvironmentHelper.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_PLUGINS),
                TestAgentHost = EnvironmentHelper.GetEnvironmentVariable(ENVIRONMENT_TEST_VARIABLE.YUNIQL_TEST_HOST)
            };
        }

        private string GetCliProcessFile()
        {
            //handle environment where tests are executed
            var isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
            var cliProcessFile = Path.Combine(EnvironmentHelper.GetEnvironmentVariable(ENVIRONMENT_TEST_VARIABLE.YUNIQL_TEST_CLI), "yuniql.exe");
            if (!isWindows)
            {
                cliProcessFile = Path.Combine(EnvironmentHelper.GetEnvironmentVariable(ENVIRONMENT_TEST_VARIABLE.YUNIQL_TEST_CLI), "yuniql");
            }

            return cliProcessFile;
        }
    }
}
