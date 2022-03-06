using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Shouldly;
using System;
using Yuniql.Core;
using Yuniql.Extensibility;
using Yuniql.PlatformTests.Interfaces;
using Yuniql.PlatformTests.Setup;
using IMigrationServiceFactory = Yuniql.PlatformTests.Interfaces.IMigrationServiceFactory;
using MigrationServiceFactory = Yuniql.PlatformTests.Setup.MigrationServiceFactory;
using System.Diagnostics;

namespace Yuniql.PlatformTests.Core
{
    [TestClass]
    public class MigrationServiceFileSortTests : TestClassBase
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
            _directoryService = new DirectoryService();
            _traceService = new FileTraceService(_directoryService) { IsDebugEnabled = true };
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
        public void Test_Run_With_Sort_Order_Manifest()
        {
            //arrange
            var directoryService = new DirectoryService();
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            _testDataService.CreateScriptFile(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "test_v0_00_01.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));
            _testDataService.CreateScriptFile(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "test_v0_00_02.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));
            _testDataService.CreateScriptFile(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "test_v0_00_03.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));
            _testDataService.CreateScriptFile(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "test_v0_00_04.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_4));
            _testDataService.CreateScriptFile(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "test_v0_00_05.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_5));

            var sortOrderFile = $"test_v0_00_03.sql{Environment.NewLine}" +
                                $"test_v0_00_01.sql{Environment.NewLine}" +
                                $"test_v0_00_05.sql{Environment.NewLine}" +
                                $"test_v0_00_02.sql{Environment.NewLine}" +
                                $"test_v0_00_04.sql";
            _testDataService.CreateScriptFile(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "_manifest.ini"), sortOrderFile);

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_3).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_1).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_5).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_2).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_4).ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_With_Sort_Order_Manifest_Some_Files_Not_Listed()
        {
            //arrange
            var directoryService = new DirectoryService();
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            _testDataService.CreateScriptFile(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "test_v0_00_01.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));
            _testDataService.CreateScriptFile(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "test_v0_00_02.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));
            _testDataService.CreateScriptFile(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "test_v0_00_03.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));
            _testDataService.CreateScriptFile(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "test_v0_00_04.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_4));
            _testDataService.CreateScriptFile(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "test_v0_00_05.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_5));

            var sortOrderFile = $"test_v0_00_03.sql{Environment.NewLine}" +
                                $"test_v0_00_01.sql{Environment.NewLine}" +
                                $"test_v0_00_05.sql";
            _testDataService.CreateScriptFile(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "_manifest.ini"), sortOrderFile);

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_3).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_1).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_5).ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_With_Sort_Order_Manifest_Some_Listed_File_Not_Present()
        {
            //arrange
            var directoryService = new DirectoryService();
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            _testDataService.CreateScriptFile(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "test_v0_00_02.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));
            _testDataService.CreateScriptFile(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "test_v0_00_04.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_4));

            var sortOrderFile = $"test_v0_00_03.sql{Environment.NewLine}" +
                                $"test_v0_00_01.sql{Environment.NewLine}" +
                                $"test_v0_00_05.sql{Environment.NewLine}" +
                                $"test_v0_00_02.sql{Environment.NewLine}" +
                                $"test_v0_00_04.sql";
            _testDataService.CreateScriptFile(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "_manifest.ini"), sortOrderFile);

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_3).ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_1).ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_5).ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_2).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_4).ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_With_Sort_Order_Manifest_Blank_Lines_Ignored()
        {
            //arrange
            var directoryService = new DirectoryService();
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            _testDataService.CreateScriptFile(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "test_v0_00_01.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));
            _testDataService.CreateScriptFile(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "test_v0_00_02.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));
            _testDataService.CreateScriptFile(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "test_v0_00_03.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));
            _testDataService.CreateScriptFile(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "test_v0_00_04.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_4));
            _testDataService.CreateScriptFile(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "test_v0_00_05.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_5));

            var sortOrderFile = $"test_v0_00_03.sql{Environment.NewLine}" +
                                $"{Environment.NewLine}" +
                                $"test_v0_00_01.sql{Environment.NewLine}" +
                                $"{Environment.NewLine}" +
                                $"test_v0_00_05.sql{Environment.NewLine}" +
                                $"{Environment.NewLine}" +
                                $"test_v0_00_02.sql{Environment.NewLine}" +
                                $"{Environment.NewLine}" +
                                $"test_v0_00_04.sql" +
                                $"{Environment.NewLine}";
            _testDataService.CreateScriptFile(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "_manifest.ini"), sortOrderFile);

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_3).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_1).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_5).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_2).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_4).ShouldBeTrue();
        }

    }
}
