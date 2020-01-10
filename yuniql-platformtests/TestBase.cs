using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System;

namespace Yuniql.PlatformTests
{
    [TestClass]
    public class TestBase
    {
        public string CreateEmptyWorkspace()
        {
            var workingPath = Path.Combine(Environment.CurrentDirectory, @$"yuniql_testdb_{Guid.NewGuid().ToString().Substring(0, 6)}"); ;
            if (!Directory.Exists(workingPath))
            {
                Directory.CreateDirectory(workingPath);
            }

            return workingPath;
        }

        public string CloneSampleWorkspace(string workingPath)
        {
            var source = EnvironmentHelper.GetEnvironmentVariable(EnvironmentVariableNames.YUNIQL_TEST_SAMPLEDB);
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

        public string GetTargetPlatform()
        {
            var targetPlatform = EnvironmentHelper.GetEnvironmentVariable(EnvironmentVariableNames.YUNIQL_TEST_TARGET_PLATFORM);
            if (string.IsNullOrEmpty(targetPlatform))
            {
                targetPlatform = "sqlserver";
            }
            return targetPlatform;
        }

        public TestConfiguration ConfigureWithEmptyWorkspace()
        {
            //create test run configuration from empty workspace
            var workspacePath = CreateEmptyWorkspace();
            var databaseName = new DirectoryInfo(workspacePath).Name;

            //prepare a unique connection string for each test case
            var connectionString = EnvironmentHelper.GetEnvironmentVariable(EnvironmentVariableNames.YUNIQL_TEST_CONNECTION_STRING);
            connectionString = connectionString.Replace("yuniqldb", databaseName);

            return new TestConfiguration
            {
                CliProcessFile = EnvironmentHelper.GetEnvironmentVariable(EnvironmentVariableNames.YUNIQL_TEST_CLI),
                TargetPlatform = EnvironmentHelper.GetEnvironmentVariable(EnvironmentVariableNames.YUNIQL_TEST_TARGET_PLATFORM),
                ConnectionString = connectionString,
                WorkspacePath = workspacePath,
                DatabaseName = databaseName
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
            var connectionString = EnvironmentHelper.GetEnvironmentVariable(EnvironmentVariableNames.YUNIQL_TEST_CONNECTION_STRING);
            connectionString = connectionString.Replace("yuniqldb", databaseName);

            //handle environment where tests are executed
            var isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
            var cliProcessFile = Path.Combine(EnvironmentHelper.GetEnvironmentVariable(EnvironmentVariableNames.YUNIQL_TEST_CLI), "yuniql.exe");
            if (!isWindows)
            {
                cliProcessFile = Path.Combine(EnvironmentHelper.GetEnvironmentVariable(EnvironmentVariableNames.YUNIQL_TEST_CLI), "yuniql");
            }

            return new TestConfiguration
            {
                CliProcessFile = cliProcessFile,
                TargetPlatform = EnvironmentHelper.GetEnvironmentVariable(EnvironmentVariableNames.YUNIQL_TEST_TARGET_PLATFORM),
                ConnectionString = connectionString,
                WorkspacePath = workspacePath,
                DatabaseName = databaseName
            };
        }

    }
}
