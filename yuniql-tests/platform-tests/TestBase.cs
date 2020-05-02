using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System;
using Yuniql.Core;

namespace Yuniql.PlatformTests
{
    [TestClass]
    public class TestBase
    {
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

        public TestConfiguration ConfigureWithEmptyWorkspace()
        {
            //create test run configuration from empty workspace
            var workspacePath = CreateEmptyWorkspace();
            var databaseName = new DirectoryInfo(workspacePath).Name;

            //prepare a unique connection string for each test case
            var connectionString = EnvironmentHelper.GetEnvironmentVariable(ENVIRONMENT_TEST_VARIABLE.YUNIQL_TEST_CONNECTION_STRING);
            connectionString = connectionString.Replace("yuniqldb", databaseName);

            return new TestConfiguration
            {
                Platform = EnvironmentHelper.GetEnvironmentVariable(ENVIRONMENT_TEST_VARIABLE.YUNIQL_TEST_TARGET_PLATFORM),
                ConnectionString = connectionString,
                DatabaseName = databaseName,
                WorkspacePath = workspacePath,
                CliProcessPath = GetCliProcessFile(),
                PluginsPath = EnvironmentHelper.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_PLUGINS),
                TestAgentHost = EnvironmentHelper.GetEnvironmentVariable(ENVIRONMENT_TEST_VARIABLE.YUNIQL_TEST_HOST)
            };
        }

        public TestConfiguration ConfigureWorkspaceWithSampleDb()
        {
            //create test run configuration from sample workspace
            var workspacePath = CreateEmptyWorkspace();
            var databaseName = new DirectoryInfo(workspacePath).Name;

            //copy sample db project
            CloneSampleWorkspace(workspacePath);

            //prepare a unique connection string for each test case
            var connectionString = EnvironmentHelper.GetEnvironmentVariable(ENVIRONMENT_TEST_VARIABLE.YUNIQL_TEST_CONNECTION_STRING);
            connectionString = connectionString.Replace("yuniqldb", databaseName);

            return new TestConfiguration
            {
                Platform = EnvironmentHelper.GetEnvironmentVariable(ENVIRONMENT_TEST_VARIABLE.YUNIQL_TEST_TARGET_PLATFORM),
                ConnectionString = connectionString,
                DatabaseName = databaseName,
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
