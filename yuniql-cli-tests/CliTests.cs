using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.Diagnostics;
using System.IO;

namespace CliTests
{
    [TestClass]
    public class CliTests : TestBase
    {
        private TestConfiguration _testConfiguration;

        public void SetupWithEmptyWorkspace()
        {
            //create test run configuration from empty workspace
            var workspacePath = CreateEmptyWorkspace();
            var databaseName = new DirectoryInfo(workspacePath).Name;

            var connectionString = EnvironmentHelper.GetEnvironmentVariable("YUNIQL_TEST_CONNECTION_STRING");

            _testConfiguration = new TestConfiguration
            {
                ProcessFile = EnvironmentHelper.GetEnvironmentVariable("YUNIQL_TEST_CLI"),
                TargetPlatform = EnvironmentHelper.GetEnvironmentVariable("YUNIQL_TEST_TARGET_PLATFORM"),
                ConnectionString = connectionString,
                WorkspacePath = workspacePath,
                DatabaseName = databaseName
            };
        }

        public void SetupWithSampleWorkspace()
        {
            //create test run configuration from sample workspace
            var workspacePath = CreateEmptyWorkspace();
            CloneSampleWorkspace(workspacePath);

            var databaseName = new DirectoryInfo(workspacePath).Name;
            _testConfiguration = new TestConfiguration
            {
                ProcessFile = @"C:\play\yuniql\yuniql-cli\bin\release\netcoreapp3.0\win-x64\publish\yuniql.exe",
                TargetPlatform = "sqlserver",
                WorkspacePath = workspacePath,
                DatabaseName = databaseName,
                ConnectionString = @$"Server=.\;Database={databaseName};Trusted_Connection=True;"
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            //drop test database
            //_testDataService.DropTestDatabase(_testConfiguration.ConnectionString, _testConfiguration.DatabaseName);

            //drop test directories
            if (Directory.Exists(_testConfiguration.WorkspacePath))
            {
                Directory.Delete(_testConfiguration.WorkspacePath, true);
            }
        }

        [DataTestMethod]
        [DataRow("init", "")]
        public void Test_Cli_init(string command, string arguments)
        {
            //arrange
            SetupWithEmptyWorkspace();

            //act & assert
            var result = ExecuteCli(command, _testConfiguration.WorkspacePath, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        [DataTestMethod]
        [DataRow("vnext", "")]
        [DataRow("vnext", "-m")]
        [DataRow("vnext", "-m -f test-vminor-script.sql")]
        [DataRow("vnext", "--minor -f test-vminor-script.sql")]
        [DataRow("vnext", "--minor --file test-vminor-script.sql")]
        [DataRow("vnext", "-M")]
        [DataRow("vnext", "--Major")]
        [DataRow("vnext", "--Major -file test-vmajor-script.sql")]
        public void Test_Cli_vnext(string command, string arguments)
        {
            //arrange
            SetupWithSampleWorkspace();

            //act & assert
            var result = ExecuteCli(command, _testConfiguration.WorkspacePath, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        [DataTestMethod]
        [DataRow("run", "-a -k \"VwColumnPrefix1=Vw1,VwColumnPrefix2=Vw2,VwColumnPrefix3=Vw3,VwColumnPrefix4=Vw4\"")]
        public void Tes_Cli_run(string command, string arguments)
        {
            //arrange
            SetupWithSampleWorkspace();

            //act & assert
            var result = ExecuteCli(command, _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        [DataTestMethod]
        [DataRow("info", "")]
        public void Tes_Cli_info(string command, string arguments)
        {
            //arrange
            SetupWithSampleWorkspace();

            //act & assert
            var result = ExecuteCli("run", _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, "-a -k \"VwColumnPrefix1=Vw1,VwColumnPrefix2=Vw2,VwColumnPrefix3=Vw3,VwColumnPrefix4=Vw4\"");
            result.Contains($"Failed to execute run").ShouldBeFalse();

            //act & assert
            result = ExecuteCli(command, _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        [DataTestMethod]
        [DataRow("verify", "-a -k \"VwColumnPrefix1=Vw1,VwColumnPrefix2=Vw2,VwColumnPrefix3=Vw3,VwColumnPrefix4=Vw4\"")]
        public void Tes_Cli_verify(string command, string arguments)
        {
            //arrange
            SetupWithSampleWorkspace();

            //act & assert
            var result = ExecuteCli("run", _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, "-a -k \"VwColumnPrefix1=Vw1,VwColumnPrefix2=Vw2,VwColumnPrefix3=Vw3,VwColumnPrefix4=Vw4\"");
            result.Contains($"Failed to execute run").ShouldBeFalse();

            //act & assert
            result = ExecuteCli(command, _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        [DataTestMethod]
        [DataRow("erase", "")]
        public void Tes_Cli_erase(string command, string arguments)
        {
            //arrange
            SetupWithSampleWorkspace();

            //act & assert
            var result = ExecuteCli("run", _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, "-a -k \"VwColumnPrefix1=Vw1,VwColumnPrefix2=Vw2,VwColumnPrefix3=Vw3,VwColumnPrefix4=Vw4\"");
            result.Contains($"Failed to execute run").ShouldBeFalse();

            //act & assert
            result = ExecuteCli(command, _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        private string ExecuteCli(string command, string workspace, string connectionString, string arguments)
        {
            string processArguments = @$"{command} -p {workspace} -c {connectionString} {arguments}";
            return ExecuteCli(processArguments);
        }

        private string ExecuteCli(string command, string workspace, string arguments)
        {
            string processArguments = @$"{command} -p {workspace} {arguments}";
            return ExecuteCli(processArguments);
        }

        private string ExecuteCli(string arguments)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _testConfiguration.ProcessFile,
                    Arguments = arguments,
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
