using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Shouldly;
using Yuniql.Core;
using Yuniql.Extensibility;

namespace Yuniql.PlatformTests
{
    [TestClass]
    public class EnvironmentAwareMigrationTests : TestBase
    {
        private ITestDataService _testDataService;
        private IMigrationServiceFactory _migrationServiceFactory;
        private ITraceService _traceService;
        private TestConfiguration _testConfiguration;

        [TestInitialize]
        public void Setup()
        {
            _testConfiguration = base.ConfigureWithEmptyWorkspace();

            //create test data service provider
            var testDataServiceFactory = new TestDataServiceFactory();
            _testDataService = testDataServiceFactory.Create(_testConfiguration.Platform);

            //create data service factory for migration proper
            _traceService = new FileTraceService { IsDebugEnabled = true };
            _migrationServiceFactory = new MigrationServiceFactory(_traceService);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_testConfiguration.WorkspacePath))
                Directory.Delete(_testConfiguration.WorkspacePath, true);
        }

        [TestMethod]
        public void Test_Run_Environment_Aware_Scripts_All_Directories()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);

            //creare environment-aware directories
            var init_dev = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "_init", "_dev")).FullName;
            var init_test = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "_init", "_test")).FullName;
            var init_prod = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "_init", "_prod")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(init_dev, $"init_dev.sql"), _testDataService.GetSqlForCreateDbObject($"init_dev"));
            _testDataService.CreateScriptFile(Path.Combine(init_test, $"init_test.sql"), _testDataService.GetSqlForCreateDbObject($"init_test"));
            _testDataService.CreateScriptFile(Path.Combine(init_prod, $"init_prod.sql"), _testDataService.GetSqlForCreateDbObject($"init_prod"));

            var pre_dev = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "_pre", "_dev")).FullName;
            var pre_test = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "_pre", "_test")).FullName;
            var pre_prod = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "_pre", "_prod")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(pre_dev, $"pre_dev.sql"), _testDataService.GetSqlForCreateDbObject($"pre_dev"));
            _testDataService.CreateScriptFile(Path.Combine(pre_test, $"pre_test.sql"), _testDataService.GetSqlForCreateDbObject($"pre_test"));
            _testDataService.CreateScriptFile(Path.Combine(pre_prod, $"pre_prod.sql"), _testDataService.GetSqlForCreateDbObject($"pre_prod"));

            var v00_dev = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "_dev")).FullName;
            var v00_test = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "_test")).FullName;
            var v00_prod = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "_prod")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(v00_dev, $"v00_dev.sql"), _testDataService.GetSqlForCreateDbObject($"v00_dev"));
            _testDataService.CreateScriptFile(Path.Combine(v00_test, $"v00_test.sql"), _testDataService.GetSqlForCreateDbObject($"v00_test"));
            _testDataService.CreateScriptFile(Path.Combine(v00_prod, $"v00_prod.sql"), _testDataService.GetSqlForCreateDbObject($"v00_prod"));

            var draft_dev = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "_draft", "_dev")).FullName;
            var draft_test = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "_draft", "_test")).FullName;
            var draft_prod = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "_draft", "_prod")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(draft_dev, $"draft_dev.sql"), _testDataService.GetSqlForCreateDbObject($"draft_dev"));
            _testDataService.CreateScriptFile(Path.Combine(draft_test, $"draft_test.sql"), _testDataService.GetSqlForCreateDbObject($"draft_test"));
            _testDataService.CreateScriptFile(Path.Combine(draft_prod, $"draft_prod.sql"), _testDataService.GetSqlForCreateDbObject($"draft_prod"));

            var post_dev = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "_post", "_dev")).FullName;
            var post_test = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "_post", "_test")).FullName;
            var post_prod = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "_post", "_prod")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(post_dev, $"post_dev.sql"), _testDataService.GetSqlForCreateDbObject($"post_dev"));
            _testDataService.CreateScriptFile(Path.Combine(post_test, $"post_test.sql"), _testDataService.GetSqlForCreateDbObject($"post_test"));
            _testDataService.CreateScriptFile(Path.Combine(post_prod, $"post_prod.sql"), _testDataService.GetSqlForCreateDbObject($"post_prod"));

            //act
            var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
            migrationService.Initialize(_testConfiguration.ConnectionString);
            migrationService.Run(_testConfiguration.WorkspacePath, "v0.00", autoCreateDatabase: true, environmentCode: "test");

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "init_dev").ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "init_test").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "init_prod").ShouldBeFalse();

            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "pre_dev").ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "pre_test").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "pre_prod").ShouldBeFalse();

            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "v00_dev").ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "v00_test").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "v00_prod").ShouldBeFalse();

            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "draft_dev").ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "draft_test").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "draft_prod").ShouldBeFalse();

            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "post_dev").ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "post_test").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "post_prod").ShouldBeFalse();
        }

        [TestMethod]
        public void Test_Run_Environment_Aware_Scripts_Placed_In_Sub_Directory()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);

            //creare environment-aware directories
            var v00_dev = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "tables", "_dev")).FullName;
            var v00_test = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "tables", "_test")).FullName;
            var v00_prod = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "tables", "_prod")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(v00_dev, $"v00_dev.sql"), _testDataService.GetSqlForCreateDbObject($"v00_dev"));
            _testDataService.CreateScriptFile(Path.Combine(v00_test, $"v00_test_01.sql"), _testDataService.GetSqlForCreateDbObject($"v00_test_01"));
            _testDataService.CreateScriptFile(Path.Combine(v00_test, $"v00_test_02.sql"), _testDataService.GetSqlForCreateDbObject($"v00_test_02"));
            _testDataService.CreateScriptFile(Path.Combine(v00_test, $"v00_test_03.sql"), _testDataService.GetSqlForCreateDbObject($"v00_test_03"));
            _testDataService.CreateScriptFile(Path.Combine(v00_prod, $"v00_prod.sql"), _testDataService.GetSqlForCreateDbObject($"v00_prod"));

            //act
            var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
            migrationService.Initialize(_testConfiguration.ConnectionString);
            migrationService.Run(_testConfiguration.WorkspacePath, "v0.00", autoCreateDatabase: true, environmentCode: "test");

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "v00_dev").ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "v00_test_01").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "v00_test_02").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "v00_test_03").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "v00_prod").ShouldBeFalse();
        }


        [TestMethod]
        public void Test_Run_Environment_Aware_Scripts_Placed_In_Sub_Directory_With_Scripts_Outside()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);

            //creare environment-aware directories
            var v00 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00")).FullName;
            var v00_dev = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "tables", "_dev")).FullName;
            var v00_test = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "tables", "_test")).FullName;
            var v00_prod = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "tables", "_prod")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(v00, $"v00_01.sql"), _testDataService.GetSqlForCreateDbObject($"v00_01"));
            _testDataService.CreateScriptFile(Path.Combine(v00, $"v00_02.sql"), _testDataService.GetSqlForCreateDbObject($"v00_02"));
            _testDataService.CreateScriptFile(Path.Combine(v00, $"v00_03.sql"), _testDataService.GetSqlForCreateDbObject($"v00_03"));

            _testDataService.CreateScriptFile(Path.Combine(v00_dev, $"v00_dev.sql"), _testDataService.GetSqlForCreateDbObject($"v00_dev"));
            _testDataService.CreateScriptFile(Path.Combine(v00_test, $"v00_test_01.sql"), _testDataService.GetSqlForCreateDbObject($"v00_test_01"));
            _testDataService.CreateScriptFile(Path.Combine(v00_test, $"v00_test_02.sql"), _testDataService.GetSqlForCreateDbObject($"v00_test_02"));
            _testDataService.CreateScriptFile(Path.Combine(v00_test, $"v00_test_03.sql"), _testDataService.GetSqlForCreateDbObject($"v00_test_03"));
            _testDataService.CreateScriptFile(Path.Combine(v00_prod, $"v00_prod.sql"), _testDataService.GetSqlForCreateDbObject($"v00_prod"));

            //act
            var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
            migrationService.Initialize(_testConfiguration.ConnectionString);
            migrationService.Run(_testConfiguration.WorkspacePath, "v0.00", autoCreateDatabase: true, environmentCode: "test");

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "v00_01").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "v00_02").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "v00_03").ShouldBeTrue();

            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "v00_dev").ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "v00_test_01").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "v00_test_02").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "v00_test_03").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "v00_prod").ShouldBeFalse();
        }
    }
}
