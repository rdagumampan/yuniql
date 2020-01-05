using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Diagnostics;
using System.IO;

namespace CliTests
{
    [TestClass]
    public class TestBase
    {
        public string GetOrCreateWorkingPath()
        {
            //prepare test directory
            var workingPath = Path.Combine(Environment.CurrentDirectory, @$"yuniql_cli_test_{Guid.NewGuid().ToString().Substring(0, 6)}"); ;
            if (!Directory.Exists(workingPath))
            {
                Directory.CreateDirectory(workingPath);
            }

            //copy sample project from root
            var source = @"C:\play\yuniql\sqlserver-samples\visitph-db";
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
    }

    public class TestConfiguration
    {
        public string WorkspacePath { get; set; }
        public string TargetPlatform { get; set; }
        public string DatabaseName { get; set; }
        public string ConnectionString { get; set; }
    }

    [TestClass]
    public class CliTests : TestBase
    {
        private TestConfiguration _testConfiguration;

        [TestInitialize]
        public void Setup()
        {
            //create test run configuration
            var workspacePath = GetOrCreateWorkingPath();
            var databaseName = new DirectoryInfo(workspacePath).Name;
            _testConfiguration = new TestConfiguration
            {
                TargetPlatform = "sqlserver",
                WorkspacePath = workspacePath,
                DatabaseName = databaseName,
                ConnectionString = @$"Server=.\;Database={databaseName};Trusted_Connection=True;"
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
        }

        [DataTestMethod]
        [DataRow("run", "-a -k \"VwColumnPrefix1=Vw1,VwColumnPrefix2=Vw2,VwColumnPrefix3=Vw3,VwColumnPrefix4=Vw4\"")]
        public void Test_SqlServer_Cli(string command, string arguments)
        {
            //act
            var result = Run(command, _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, arguments);

            //assert
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        private string Run(string command, string workspace, string connectionString, string arguments)
        {
            string processFileName = @"C:\play\yuniql\yuniql-cli\bin\release\netcoreapp3.0\win-x64\publish\yuniql.exe";
            string processArguments = @$"{command} -p {workspace} -c {connectionString} {arguments}";

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = processFileName,
                    Arguments = processArguments,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();

            var reader = process.StandardOutput;
            string output = reader.ReadToEnd();
            process.WaitForExit();

            return output;
        }
    }
}
