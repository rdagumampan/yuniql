using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Yuniql.CliTests;

namespace CliTests
{

    [TestClass]
    public class CliTests : TestBase
    {
        private TestConfiguration _testConfiguration;

        public void SetupWorkspace()
        {
            //create test run configuration from empty workspace
            var workspacePath = CreateEmptyWorkspace();
            var databaseName = new DirectoryInfo(workspacePath).Name;

            //prepare a unique connection string for each test case
            var connectionString = EnvironmentHelper.GetEnvironmentVariable("YUNIQL_TEST_CONNECTION_STRING");
            connectionString = connectionString.Replace("yuniqldb", databaseName);

            _testConfiguration = new TestConfiguration
            {
                ProcessFile = EnvironmentHelper.GetEnvironmentVariable("YUNIQL_TEST_CLI"),
                TargetPlatform = EnvironmentHelper.GetEnvironmentVariable("YUNIQL_TEST_TARGET_PLATFORM"),
                ConnectionString = connectionString,
                WorkspacePath = workspacePath,
                DatabaseName = databaseName
            };
        }

        public void SetupWorkspaceWithSampleDb()
        {
            //create test run configuration from sample workspace
            var workspacePath = CreateEmptyWorkspace();
            var databaseName = new DirectoryInfo(workspacePath).Name;

            //copy sample db project
            CloneSampleWorkspace(workspacePath);

            //prepare a unique connection string for each test case
            var connectionString = EnvironmentHelper.GetEnvironmentVariable("YUNIQL_TEST_CONNECTION_STRING");
            connectionString = connectionString.Replace("yuniqldb", databaseName);

            _testConfiguration = new TestConfiguration
            {
                ProcessFile = EnvironmentHelper.GetEnvironmentVariable("YUNIQL_TEST_CLI"),
                TargetPlatform = EnvironmentHelper.GetEnvironmentVariable("YUNIQL_TEST_TARGET_PLATFORM"),
                ConnectionString = connectionString,
                WorkspacePath = workspacePath,
                DatabaseName = databaseName
            };
        }

        [ClassInitialize]
        public static void SetupInfrastrucuture(TestContext testContext)
        {
            var containerFactory = new ContainerFactory();
            var container = containerFactory.Create("sqlserver");

            var dockerService = new DockerService();
            var foundContainer = dockerService.FindByName(container.Name).FirstOrDefault();
            if (null != foundContainer)
            {
                dockerService.Remove(foundContainer);
            }

            dockerService.Run(container);

            Thread.Sleep(1000 * 10);
        }

        [ClassCleanup]
        public static void TearDownInfrastructure()
        {
            var dockerService = new DockerService();
            var container = dockerService.FindByName("sqlserver-testserver-for-cli-tests").First(); ;
            dockerService.Remove(container);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            //drop test directories
            if (Directory.Exists(_testConfiguration.WorkspacePath))
            {
                Directory.Delete(_testConfiguration.WorkspacePath, true);
            }
        }

        [DataTestMethod]
        [DataRow("init", "")]
        [DataRow("init", "-d")]
        public void Test_Cli_init(string command, string arguments)
        {
            //arrange
            SetupWorkspace();

            //act & assert
            var result = ExecuteCli(command, _testConfiguration.WorkspacePath, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        [DataTestMethod]
        [DataRow("vnext", "")]
        [DataRow("vnext", "-d")]
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
            SetupWorkspaceWithSampleDb();

            //act & assert
            var result = ExecuteCli(command, _testConfiguration.WorkspacePath, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        [DataTestMethod]
        [DataRow("run", "-a")]
        [DataRow("run", "-a -d")]
        [DataRow("run", "--autocreate-db")]
        [DataRow("run", "-a -t v1.00")]
        [DataRow("run", "-a --target-version v1.00")]
        [DataRow("run", "-a -k \"VwColumnPrefix1=Vw1,VwColumnPrefix2=Vw2,VwColumnPrefix3=Vw3,VwColumnPrefix4=Vw4\"")]
        [DataRow("run", "-a -k \"VwColumnPrefix1=Vw1\" -k \"VwColumnPrefix2=Vw2\" -k \"VwColumnPrefix3=Vw3\" -k \"VwColumnPrefix4=Vw4\"")]
        [DataRow("run", "-a --delimiter \",\"")]
        public void Test_Cli_run(string command, string arguments)
        {
            //arrange
            SetupWorkspaceWithSampleDb();

            //act & assert
            var result = ExecuteCli(command, _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        [DataTestMethod]
        [DataRow("info", "")]
        [DataRow("info", "-d")]
        public void Test_Cli_info(string command, string arguments)
        {
            //arrange
            SetupWorkspaceWithSampleDb();

            //act & assert
            var result = ExecuteCli("run", _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, "-a -k \"VwColumnPrefix1=Vw1,VwColumnPrefix2=Vw2,VwColumnPrefix3=Vw3,VwColumnPrefix4=Vw4\"");
            result.Contains($"Failed to execute run").ShouldBeFalse();

            //act & assert
            result = ExecuteCli(command, _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        [DataTestMethod]
        [DataRow("verify", "-a -k \"VwColumnPrefix1=Vw1,VwColumnPrefix2=Vw2,VwColumnPrefix3=Vw3,VwColumnPrefix4=Vw4\"")]
        public void Test_Cli_verify(string command, string arguments)
        {
            //arrange
            SetupWorkspaceWithSampleDb();

            //act & assert
            var result = ExecuteCli("run", _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, "-a -k \"VwColumnPrefix1=Vw1,VwColumnPrefix2=Vw2,VwColumnPrefix3=Vw3,VwColumnPrefix4=Vw4\"");
            result.Contains($"Failed to execute run").ShouldBeFalse();

            //act & assert
            result = ExecuteCli(command, _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        [DataTestMethod]
        [DataRow("erase", "")]
        [DataRow("erase", "-d")]
        public void Test_Cli_erase(string command, string arguments)
        {
            //arrange
            SetupWorkspaceWithSampleDb();

            //act & assert
            var result = ExecuteCli("run", _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, "-a -k \"VwColumnPrefix1=Vw1,VwColumnPrefix2=Vw2,VwColumnPrefix3=Vw3,VwColumnPrefix4=Vw4\"");
            result.Contains($"Failed to execute run").ShouldBeFalse();

            //act & assert
            result = ExecuteCli(command, _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        private string ExecuteCli(string command, string workspace, string connectionString, string arguments)
        {
            string processArguments = $"{command} -p \"{workspace}\" -c \"{connectionString}\" {arguments}";
            return ExecuteCli(processArguments);
        }

        private string ExecuteCli(string command, string workspace, string arguments)
        {
            string processArguments = $"{command} -p \"{workspace}\" {arguments}";
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
