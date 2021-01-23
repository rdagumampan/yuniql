using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Shouldly;
using Yuniql.Core;
using Yuniql.Extensibility;
using System;
using Yuniql.PlatformTests.Interfaces;
using Yuniql.PlatformTests.Setup;
using IMigrationServiceFactory = Yuniql.PlatformTests.Interfaces.IMigrationServiceFactory;
using MigrationServiceFactory = Yuniql.PlatformTests.Setup.MigrationServiceFactory;
using System.Diagnostics;

namespace Yuniql.PlatformTests.Core
{
    [TestClass]
    public class EnvironmentAwareMigrationTests : TestClassBase
    {
        private ITestDataService _testDataService;
        private IMigrationServiceFactory _migrationServiceFactory;
        private ITraceService _traceService;
        private TestConfiguration _testConfiguration;

        [TestInitialize]
        public void Setup()
        {
            _testConfiguration = ConfigureWithEmptyWorkspace();

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
                _testDataService.DropDatabase(_testConfiguration.ConnectionString);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        [TestMethod]
        public void Test_Run_Environment_Aware_Scripts_All_Directories()
        {
            //arrange
            var directoryService = new DirectoryService();
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            //creare environment-aware directories
            var init_dev = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.INIT, "_dev")).FullName;
            var init_test = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.INIT, "_test")).FullName;
            var init_prod = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.INIT, "_prod")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(init_dev, $"init_dev.sql"), _testDataService.GetSqlForCreateDbObject($"init_dev"));
            _testDataService.CreateScriptFile(Path.Combine(init_test, $"init_test.sql"), _testDataService.GetSqlForCreateDbObject($"init_test"));
            _testDataService.CreateScriptFile(Path.Combine(init_prod, $"init_prod.sql"), _testDataService.GetSqlForCreateDbObject($"init_prod"));

            var pre_dev = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.PRE, "_dev")).FullName;
            var pre_test = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.PRE, "_test")).FullName;
            var pre_prod = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.PRE, "_prod")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(pre_dev, $"pre_dev.sql"), _testDataService.GetSqlForCreateDbObject($"pre_dev"));
            _testDataService.CreateScriptFile(Path.Combine(pre_test, $"pre_test.sql"), _testDataService.GetSqlForCreateDbObject($"pre_test"));
            _testDataService.CreateScriptFile(Path.Combine(pre_prod, $"pre_prod.sql"), _testDataService.GetSqlForCreateDbObject($"pre_prod"));

            var v00_dev = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "_dev")).FullName;
            var v00_test = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "_test")).FullName;
            var v00_prod = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "_prod")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(v00_dev, $"v00_dev.sql"), _testDataService.GetSqlForCreateDbObject($"v00_dev"));
            _testDataService.CreateScriptFile(Path.Combine(v00_test, $"v00_test.sql"), _testDataService.GetSqlForCreateDbObject($"v00_test"));
            _testDataService.CreateScriptFile(Path.Combine(v00_prod, $"v00_prod.sql"), _testDataService.GetSqlForCreateDbObject($"v00_prod"));

            var draft_dev = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.DRAFT, "_dev")).FullName;
            var draft_test = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.DRAFT, "_test")).FullName;
            var draft_prod = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.DRAFT, "_prod")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(draft_dev, $"draft_dev.sql"), _testDataService.GetSqlForCreateDbObject($"draft_dev"));
            _testDataService.CreateScriptFile(Path.Combine(draft_test, $"draft_test.sql"), _testDataService.GetSqlForCreateDbObject($"draft_test"));
            _testDataService.CreateScriptFile(Path.Combine(draft_prod, $"draft_prod.sql"), _testDataService.GetSqlForCreateDbObject($"draft_prod"));

            var post_dev = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.POST, "_dev")).FullName;
            var post_test = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.POST, "_test")).FullName;
            var post_prod = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.POST, "_prod")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(post_dev, $"post_dev.sql"), _testDataService.GetSqlForCreateDbObject($"post_dev"));
            _testDataService.CreateScriptFile(Path.Combine(post_test, $"post_test.sql"), _testDataService.GetSqlForCreateDbObject($"post_test"));
            _testDataService.CreateScriptFile(Path.Combine(post_prod, $"post_prod.sql"), _testDataService.GetSqlForCreateDbObject($"post_prod"));

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            configuration.TargetVersion = "v0.00";
            configuration.Environment = "test";

            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

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
            var directoryService = new DirectoryService();
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

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
            var configuration = _testConfiguration.GetFreshConfiguration();
            configuration.TargetVersion = "v0.00";
            configuration.Environment = "test";

            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

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
            var directoryService = new DirectoryService();
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

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
            var configuration = _testConfiguration.GetFreshConfiguration();
            configuration.TargetVersion = "v0.00";
            configuration.Environment = "test";

            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

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
