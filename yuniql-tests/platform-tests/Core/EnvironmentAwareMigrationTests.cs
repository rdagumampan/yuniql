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
        private IDirectoryService _directoryService;
        private TestConfiguration _testConfiguration;

        [TestInitialize]
        public void Setup()
        {
            _testConfiguration = ConfigureWithEmptyWorkspace();

            //create test data service provider
            var testDataServiceFactory = new TestDataServiceFactory();
            _testDataService = testDataServiceFactory.Create(_testConfiguration.Platform);

            //create data service factory for migration proper
            _directoryService = new DirectoryService(_traceService);
            _traceService = new FileTraceService() { IsDebugEnabled = true };
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
                _testDataService.CleanupDbObjects(_testConfiguration.ConnectionString);
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
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            //creare environment-aware directories
            var init_dev = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.INIT, "_dev")).FullName;
            var init_test = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.INIT, "_test")).FullName;
            var init_prod = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.INIT, "_prod")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(init_dev, $"init_dev.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));
            _testDataService.CreateScriptFile(Path.Combine(init_test, $"init_test.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));
            _testDataService.CreateScriptFile(Path.Combine(init_prod, $"init_prod.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));

            var pre_dev = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.PRE, "_dev")).FullName;
            var pre_test = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.PRE, "_test")).FullName;
            var pre_prod = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.PRE, "_prod")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(pre_dev, $"pre_dev.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_4));
            _testDataService.CreateScriptFile(Path.Combine(pre_test, $"pre_test.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_5));
            _testDataService.CreateScriptFile(Path.Combine(pre_prod, $"pre_prod.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_6));

            var v00_dev = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "_dev")).FullName;
            var v00_test = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "_test")).FullName;
            var v00_prod = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "_prod")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(v00_dev, $"v00_dev.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_7));
            _testDataService.CreateScriptFile(Path.Combine(v00_test, $"v00_test.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_8));
            _testDataService.CreateScriptFile(Path.Combine(v00_prod, $"v00_prod.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_9));

            var draft_dev = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.DRAFT, "_dev")).FullName;
            var draft_test = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.DRAFT, "_test")).FullName;
            var draft_prod = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.DRAFT, "_prod")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(draft_dev, $"draft_dev.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_10));
            _testDataService.CreateScriptFile(Path.Combine(draft_test, $"draft_test.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_11));
            _testDataService.CreateScriptFile(Path.Combine(draft_prod, $"draft_prod.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));

            var post_dev = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.POST, "_dev")).FullName;
            var post_test = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.POST, "_test")).FullName;
            var post_prod = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.POST, "_prod")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(post_dev, $"post_dev.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_13));
            _testDataService.CreateScriptFile(Path.Combine(post_test, $"post_test.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_14));
            _testDataService.CreateScriptFile(Path.Combine(post_prod, $"post_prod.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_15));

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            configuration.TargetVersion = "v0.00";
            configuration.Environment = "test";

            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_1).ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_2).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_3).ShouldBeFalse();

            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_4).ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_5).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_6).ShouldBeFalse();

            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_7).ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_8).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_9).ShouldBeFalse();

            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_10).ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_11).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_12).ShouldBeFalse();

            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_13).ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_14).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_15).ShouldBeFalse();
        }

        [TestMethod]
        public void Test_Run_Environment_Aware_Scripts_Placed_In_Sub_Directory()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            //creare environment-aware directories
            var v00_dev = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "tables", "_dev")).FullName;
            var v00_test = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "tables", "_test")).FullName;
            var v00_prod = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "tables", "_prod")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(v00_dev, $"v00_dev.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));
            _testDataService.CreateScriptFile(Path.Combine(v00_test, $"v00_test_01.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));
            _testDataService.CreateScriptFile(Path.Combine(v00_test, $"v00_test_02.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));
            _testDataService.CreateScriptFile(Path.Combine(v00_test, $"v00_test_03.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_4));
            _testDataService.CreateScriptFile(Path.Combine(v00_prod, $"v00_prod.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_5));

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            configuration.TargetVersion = "v0.00";
            configuration.Environment = "test";

            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_1).ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_2).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_3).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_4).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_5).ShouldBeFalse();
        }


        [TestMethod]
        public void Test_Run_Environment_Aware_Scripts_Placed_In_Sub_Directory_With_Scripts_Outside()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            //creare environment-aware directories
            var v00 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00")).FullName;
            var v00_dev = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "tables", "_dev")).FullName;
            var v00_test = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "tables", "_test")).FullName;
            var v00_prod = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "tables", "_prod")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(v00, $"v00_01.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));
            _testDataService.CreateScriptFile(Path.Combine(v00, $"v00_02.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));
            _testDataService.CreateScriptFile(Path.Combine(v00, $"v00_03.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));

            _testDataService.CreateScriptFile(Path.Combine(v00_dev, $"v00_dev.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_4));
            _testDataService.CreateScriptFile(Path.Combine(v00_test, $"v00_test_01.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_5));
            _testDataService.CreateScriptFile(Path.Combine(v00_test, $"v00_test_02.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_6));
            _testDataService.CreateScriptFile(Path.Combine(v00_test, $"v00_test_03.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_7));
            _testDataService.CreateScriptFile(Path.Combine(v00_prod, $"v00_prod.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_8));

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            configuration.TargetVersion = "v0.00";
            configuration.Environment = "test";

            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_1).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_2).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_3).ShouldBeTrue();

            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_4).ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_5).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_6).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_7).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_8).ShouldBeFalse();
        }
    }
}
