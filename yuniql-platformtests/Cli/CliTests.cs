using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.Diagnostics;
using System.IO;

namespace Yuniql.PlatformTests
{
    [TestClass]
    public class CliTests : TestBase
    {
        private TestConfiguration _testConfiguration;
        private CliExecutionService _executionService;

        public void SetupWithEmptyWorkspace()
        {
            _testConfiguration = base.ConfigureWithEmptyWorkspace();
            _executionService = new CliExecutionService(_testConfiguration.CliProcessPath);
        }

        public void SetupWorkspaceWithSampleDb()
        {
            _testConfiguration = base.ConfigureWorkspaceWithSampleDb();
            _executionService = new CliExecutionService(_testConfiguration.CliProcessPath);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_testConfiguration.WorkspacePath))
                Directory.Delete(_testConfiguration.WorkspacePath, true);
        }

        [DataTestMethod]
        [DataRow("init", "")]
        [DataRow("init", "-d")]
        public void Test_Cli_init(string command, string arguments)
        {
            //arrange
            SetupWithEmptyWorkspace();

            //act & assert
            var result = _executionService.Run(command, _testConfiguration.WorkspacePath, arguments);
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
            var result = _executionService.Run(command, _testConfiguration.WorkspacePath, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        [DataTestMethod]
        [DataRow("run", "-a")]
        [DataRow("run", "-a --command-timeout 10")]
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
            var result = _executionService.Run(command, _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }
        
        [DataTestMethod]
        [DataRow("verify", "-t v1.00")]
        [DataRow("verify", "--target-version v1.00")]
        [DataRow("verify", "--delimiter \",\"")]
        [DataRow("verify", "--command-timeout 10")]
        [DataRow("verify", "-k \"VwColumnPrefix1=Vw1,VwColumnPrefix2=Vw2,VwColumnPrefix3=Vw3,VwColumnPrefix4=Vw4\"")]
        public void Test_Cli_verify(string command, string arguments)
        {
            //arrange
            SetupWorkspaceWithSampleDb();

            //act & assert
            var result = _executionService.Run("run", _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, "-a -t v0.00 -k \"VwColumnPrefix1=Vw1,VwColumnPrefix2=Vw2,VwColumnPrefix3=Vw3,VwColumnPrefix4=Vw4\"");
            result.Contains($"Failed to execute run").ShouldBeFalse();

            //act & assert
            result = _executionService.Run(command, _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        [DataTestMethod]
        [DataRow("info", "")]
        [DataRow("info", "--command-timeout 10")]
        [DataRow("info", "-d")]
        public void Test_Cli_info(string command, string arguments)
        {
            //arrange
            SetupWorkspaceWithSampleDb();

            //act & assert
            var result = _executionService.Run("run", _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, "-a -k \"VwColumnPrefix1=Vw1,VwColumnPrefix2=Vw2,VwColumnPrefix3=Vw3,VwColumnPrefix4=Vw4\"");
            result.Contains($"Failed to execute run").ShouldBeFalse();

            //act & assert
            result = _executionService.Run(command, _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        [DataTestMethod]
        [DataRow("erase", "")]
        [DataRow("erase", "--command-timeout 10")]
        [DataRow("erase", "-d")]
        public void Test_Cli_erase(string command, string arguments)
        {
            //arrange
            SetupWorkspaceWithSampleDb();

            //act & assert
            var result = _executionService.Run("run", _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, "-a -k \"VwColumnPrefix1=Vw1,VwColumnPrefix2=Vw2,VwColumnPrefix3=Vw3,VwColumnPrefix4=Vw4\"");
            result.Contains($"Failed to execute run").ShouldBeFalse();

            //act & assert
            result = _executionService.Run(command, _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }
    }
}
