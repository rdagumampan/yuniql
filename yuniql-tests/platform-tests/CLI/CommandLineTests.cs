using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Diagnostics;
using System.IO;
using Yuniql.PlatformTests.Interfaces;
using Yuniql.PlatformTests.Setup;

namespace Yuniql.PlatformTests.CLI
{
    [TestClass]
    public class CommandLineTests : TestClassBase
    {
        private TestConfiguration _testConfiguration;
        private CommandLineExecutionService _executionService;
        private ITestDataService _testDataService;

        public void SetupWithWorkspace()
        {
            //ensures connection string values with double quote are preserved in when passed along the CLI
            _testConfiguration = ConfigureWithEmptyWorkspace();
            _executionService = new CommandLineExecutionService(_testConfiguration.CliProcessPath);

            //create test data service provider
            var testDataServiceFactory = new TestDataServiceFactory();
            _testDataService = testDataServiceFactory.Create(_testConfiguration.Platform);
        }

        public void SetupWorkspaceWithSampleDb()
        {
            //ensures connection string values with double quote are preserved in when passed along the CLI
            _testConfiguration = ConfigureWorkspaceWithSampleDb();
            _executionService = new CommandLineExecutionService(_testConfiguration.CliProcessPath);

            //create test data service provider
            var testDataServiceFactory = new TestDataServiceFactory();
            _testDataService = testDataServiceFactory.Create(_testConfiguration.Platform);
        }

        [TestCleanup]
        public void Cleanup()
        {
            //drop the test directory
            try
            {
                if (Directory.Exists(_testConfiguration.WorkspacePath))
                    Directory.Delete(_testConfiguration.WorkspacePath, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            //drop test database
            try
            {
                _testDataService.CleanupDbObjects(_testConfiguration.ConnectionString);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
        [DataTestMethod]
        [DataRow("check", "")]
        [DataRow("check", "-d")]
        //[DataRow("check", "-d --trace-sensitive-data")]
        [DataRow("check", "-d --trace-to-file")]
        [DataRow("check", "-d --trace-to-directory c:\\temp\\not-existing")]
        public void Test_yuniql_check(string command, string arguments)
        {
            //arrange
            SetupWorkspaceWithSampleDb();

            //setup database to ping
            var result = _executionService.Run("run", _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, "-a -d");
            result.Contains($"Failed to execute run").ShouldBeFalse();

            //act & assert
            result = _executionService.Run(command, _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }



        [DataTestMethod]
        [DataRow("init", "")]
        [DataRow("init", "-d")]
        //[DataRow("init", "-d --trace-sensitive-data")]
        [DataRow("init", "-d --trace-to-file")]
        [DataRow("init", "-d --trace-to-directory c:\\temp\\not-existing")]
        public void Test_yuniql_init(string command, string arguments)
        {
            //arrange
            SetupWithWorkspace();

            //act & assert
            var result = _executionService.Run(command, _testConfiguration.WorkspacePath, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        [DataTestMethod]
        [DataRow("vnext", "")]
        [DataRow("vnext", "-d")]
        //[DataRow("vnext", "-d --trace-sensitive-data")]
        [DataRow("vnext", "-d --trace-to-file")]
        [DataRow("vnext", "-d --trace-to-directory c:\\temp\\not-existing")]
        [DataRow("vnext", "-d -m")]
        [DataRow("vnext", "-d -m -f test-vminor-script.sql")]
        [DataRow("vnext", "-d --minor -f test-vminor-script.sql")]
        [DataRow("vnext", "-d --minor --file test-vminor-script.sql")]
        [DataRow("vnext", "-d -M")]
        [DataRow("vnext", "-d --Major")]
        [DataRow("vnext", "-d --Major -file test-vmajor-script.sql")]
        public void Test_yuniql_vnext(string command, string arguments)
        {
            //arrange
            SetupWorkspaceWithSampleDb();

            //act & assert
            var result = _executionService.Run(command, _testConfiguration.WorkspacePath, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        [DataTestMethod]
        [DataRow("run", "-a")]
        [DataRow("run", "-a -d")]
        //[DataRow("run", "-a -d --trace-sensitive-data")]
        [DataRow("run", "-a -d --trace-to-file")]
        [DataRow("run", "-a -d --trace-to-directory c:\\temp\\not-existing")]
        [DataRow("run", "--autocreate-db -d")]
        [DataRow("run", "-a -d -t v0.01")]
        [DataRow("run", "-a -d --target-version v0.01")]
        [DataRow("run", "-a -d --bulk-separator")]
        [DataRow("run", "-a -d --bulk-batch-size 50")]
        [DataRow("run", "-a -d --command-timeout 10")]
        [DataRow("run", "-a -d --environment DEV")]
        [DataRow("run", "-a -d --meta-table \"my_versions\" ")]
        [DataRow("run", "-a -d -k \"VwColumnPrefix1=Vw1,VwColumnPrefix2=Vw2,VwColumnPrefix3=Vw3,VwColumnPrefix4=Vw4\"")]
        [DataRow("run", "-a -d -k \"VwColumnPrefix1=Vw1\" -k \"VwColumnPrefix2=Vw2\" -k \"VwColumnPrefix3=Vw3\" -k \"VwColumnPrefix4=Vw4\"")]
        [DataRow("run", "-a -d --transaction-mode session")]
        [DataRow("run", "-a -d --transaction-mode version")]
        [DataRow("run", "-a -d --transaction-mode statement")]
        [DataRow("apply", "-a")]
        [DataRow("apply", "-a -d")]
        //[DataRow("apply", "-a -d --trace-sensitive-data")]
        [DataRow("apply", "-a -d --trace-to-file")]
        [DataRow("apply", "-a -d --trace-to-directory c:\\temp\\not-existing")]
        [DataRow("apply", "--autocreate-db -d")]
        [DataRow("apply", "-a -d -t v0.01")]
        [DataRow("apply", "-a -d --target-version v0.01")]
        [DataRow("apply", "-a -d --bulk-separator")]
        [DataRow("apply", "-a -d --bulk-batch-size 50")]
        [DataRow("apply", "-a -d --command-timeout 10")]
        [DataRow("apply", "-a -d --environment DEV")]
        [DataRow("apply", "-a -d --meta-table \"my_versions\" ")]
        [DataRow("apply", "-a -d -k \"VwColumnPrefix1=Vw1,VwColumnPrefix2=Vw2,VwColumnPrefix3=Vw3,VwColumnPrefix4=Vw4\"")]
        [DataRow("apply", "-a -d -k \"VwColumnPrefix1=Vw1\" -k \"VwColumnPrefix2=Vw2\" -k \"VwColumnPrefix3=Vw3\" -k \"VwColumnPrefix4=Vw4\"")]
        [DataRow("apply", "-a -d --transaction-mode session")]
        [DataRow("apply", "-a -d --transaction-mode version")]
        [DataRow("apply", "-a -d --transaction-mode statement")]
        public void Test_yuniql_run(string command, string arguments)
        {
            //arrange
            SetupWorkspaceWithSampleDb();

            //act & assert
            var result = _executionService.Run(command, _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        [DataTestMethodEx(Requires = "IsSchemaSupported")]
        [DataRow("run", "-a -d --meta-schema \"my_schema\"")]
        [DataRow("run", "-a -d --meta-schema \"my_schema\" --meta-table \"my_versions\" ")]
        [DataRow("apply", "-a -d --meta-schema \"my_schema\"")]
        [DataRow("apply", "-a -d --meta-schema \"my_schema\" --meta-table \"my_versions\" ")]
        public void Test_yuniql_run_With_Custom_Schema(string command, string arguments)
        {
            //arrange
            SetupWorkspaceWithSampleDb();

            //act & assert
            var result = _executionService.Run(command, _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        [DataTestMethod]
        [DataRow("verify", "")]
        [DataRow("verify", "-d")]
        //[DataRow("verify", "-d --trace-sensitive-data")]
        [DataRow("verify", "-d --trace-to-file")]
        [DataRow("verify", "-d --trace-to-directory c:\\temp\\not-existing")]
        [DataRow("verify", "-d -t v0.01")]
        [DataRow("verify", "-d --target-version v0.01")]
        [DataRow("verify", "-d --bulk-separator ,")]
        [DataRow("verify", "-d --bulk-batch-size 50")]
        [DataRow("verify", "-d --command-timeout 10")]
        [DataRow("verify", "-d --environment DEV")]
        [DataRow("verify", "-d --meta-schema \"my_schema\"")]
        [DataRow("verify", "-d --meta-table \"my_versions\" ")]
        [DataRow("verify", "-d --meta-schema \"my_schema\" --meta-table \"my_versions\" ")]
        [DataRow("verify", "-d -k \"VwColumnPrefix1=Vw1,VwColumnPrefix2=Vw2,VwColumnPrefix3=Vw3,VwColumnPrefix4=Vw4\"")]
        [DataRow("verify", "-d -k \"VwColumnPrefix1=Vw1\" -k \"VwColumnPrefix2=Vw2\" -k \"VwColumnPrefix3=Vw3\" -k \"VwColumnPrefix4=Vw4\"")]
        public void Test_yuniql_verify(string command, string arguments)
        {
            //arrange
            SetupWorkspaceWithSampleDb();

            //act & assert
            var result = _executionService.Run("run", _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, "-a -t v0.00");
            result.Contains($"Failed to execute run").ShouldBeFalse();

            //act & assert
            result = _executionService.Run(command, _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        [DataTestMethodEx(Requires = "IsSchemaSupported")]
        [DataRow("verify", "-d -t v0.01")]
        [DataRow("verify", "-d --target-version v0.01")]
        [DataRow("verify", "-d --bulk-separator ,")]
        [DataRow("verify", "-d --bulk-batch-size 50")]
        [DataRow("verify", "-d --command-timeout 10")]
        [DataRow("verify", "-d --environment DEV")]
        [DataRow("verify", "-d -k \"VwColumnPrefix1=Vw1,VwColumnPrefix2=Vw2,VwColumnPrefix3=Vw3,VwColumnPrefix4=Vw4\"")]
        public void Test_yuniql_verify_With_Custom_Schema(string command, string arguments)
        {
            //arrange
            SetupWorkspaceWithSampleDb();

            //act & assert
            var result = _executionService.Run("run", _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, "-a -t v0.00 --meta-schema \"my_schema\" --meta-table \"my_versions\" ");
            result.Contains($"Failed to execute run").ShouldBeFalse();

            //act & assert
            result = _executionService.Run(command, _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, "--meta-schema \"my_schema\" --meta-table \"my_versions\" " + arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        [DataTestMethod]
        [DataRow("list", "")]
        [DataRow("list", "-d")]
        //[DataRow("list", "-d --trace-sensitive-data")]
        [DataRow("list", "-d --trace-to-file")]
        [DataRow("list", "-d --trace-to-directory c:\\temp\\not-existing")]
        [DataRow("list", "-d --command-timeout 10")]
        public void Test_yuniql_list(string command, string arguments)
        {
            //arrange
            SetupWorkspaceWithSampleDb();

            //act & assert
            var result = _executionService.Run("run", _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, "-a -d");
            result.Contains($"Failed to execute run").ShouldBeFalse();

            //act & assert
            result = _executionService.Run(command, _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        [DataTestMethodEx(Requires = "IsSchemaSupported")]
        [DataRow("list", "")]
        [DataRow("list", "-d")]
        [DataRow("list", "-d --command-timeout 10")]
        public void Test_yuniql_list_With_Custom_Schema(string command, string arguments)
        {
            //arrange
            SetupWorkspaceWithSampleDb();

            //act & assert
            var result = _executionService.Run("run", _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, "-a --meta-schema \"my_schema\" --meta-table \"my_versions\" -d");
            result.Contains($"Failed to execute run").ShouldBeFalse();

            //act & assert
            result = _executionService.Run(command, _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, "--meta-schema \"my_schema\" --meta-table \"my_versions\" " + arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        [DataTestMethod]
        [DataRow("erase", "")]
        [DataRow("erase", "-d")]
        //[DataRow("erase", "-d --force --trace-sensitive-data")]
        [DataRow("erase", "-d --force --trace-to-file")]
        [DataRow("erase", "-d --force --trace-to-directory c:\\temp\\not-existing")]
        [DataRow("erase", "-d --force")]
        [DataRow("erase", "-d --force --environment DEV")]
        [DataRow("erase", "-d --force --command-timeout 10")]
        public void Test_yuniql_erase(string command, string arguments)
        {
            //arrange
            SetupWorkspaceWithSampleDb();

            //act & assert
            var result = _executionService.Run("run", _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, "-a -d");
            result.Contains($"Failed to execute run").ShouldBeFalse();

            //act & assert
            result = _executionService.Run(command, _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        [DataTestMethodEx(Requires = "IsMultiTenancySupported")]
        [DataRow("destroy", "")]
        [DataRow("destroy", "-d")]
        //[DataRow("destroy", "-d --force --trace-sensitive-data")]
        [DataRow("destroy", "-d --force --trace-to-file")]
        [DataRow("destroy", "-d --force --trace-to-directory c:\\temp\\not-existing")]
        [DataRow("destroy", "-d --force")]
        public void Test_yuniql_destroy(string command, string arguments)
        {
            //arrange
            SetupWorkspaceWithSampleDb();

            //act & assert
            var result = _executionService.Run("run", _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, "-a -d");
            result.Contains($"Failed to execute run").ShouldBeFalse();

            //act & assert
            result = _executionService.Run(command, _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        [DataTestMethod]
        [DataRow("platforms", "")]
        [DataRow("platforms", "-d")]
        //[DataRow("platforms", "-d --trace-sensitive-data")]
        [DataRow("platforms", "-d --trace-to-file")]
        [DataRow("platforms", "-d --trace-to-directory c:\\temp\\not-existing")]
        public void Test_yuniql_platforms(string command, string arguments)
        {
            //arrange
            SetupWorkspaceWithSampleDb();

            //act & assert
            var result = _executionService.Run("run", _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, "-a -d");
            result.Contains($"Failed to execute run").ShouldBeFalse();

            //act & assert
            result = _executionService.Run(command, _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        [DataTestMethod]
        [DataRow("baseline", "")]
        [DataRow("baseline", "-d")]
        //[DataRow("baseline", "-d --trace-sensitive-data")]
        public void Test_yuniql_baseline(string command, string arguments)
        {
            //arrange
            SetupWorkspaceWithSampleDb();

            //act & assert
            var result = _executionService.Run("run", _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, "-a -d");
            result.Contains($"Failed to execute run").ShouldBeFalse();

            //act & assert
            result = _executionService.Run(command, _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeTrue();
            result.Contains($"Not yet implemented, stay tune!");
        }

        [DataTestMethod]
        [DataRow("rebase", "")]
        [DataRow("rebase", "-d")]
        public void Test_yuniql_rebase(string command, string arguments)
        {
            //arrange
            SetupWorkspaceWithSampleDb();

            //act & assert
            var result = _executionService.Run("run", _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, "-a -d");
            result.Contains($"Failed to execute run").ShouldBeFalse();

            //act & assert
            result = _executionService.Run(command, _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeTrue();
            result.Contains($"Not yet implemented, stay tune!");
        }
    }
}
