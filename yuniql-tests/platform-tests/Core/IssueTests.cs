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
    public class IssueTests : TestClassBase
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

        //https://github.com/rdagumampan/yuniql/issues/249
        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("v10.0.0")]
        public void Test_Issue_249_A(string targetVersion)
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            //creare environment-aware directories
            var v0_00 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00")).FullName;
            var v1_0_1 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v1.0.1")).FullName;
            var v2_2_3 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v2.2.3")).FullName;
            var v9_1_3 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v9.1.3")).FullName;
            var v10_0_0 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v10.0.0")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(v0_00, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));
            _testDataService.CreateScriptFile(Path.Combine(v1_0_1, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));
            _testDataService.CreateScriptFile(Path.Combine(v2_2_3, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));
            _testDataService.CreateScriptFile(Path.Combine(v9_1_3, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_4));
            _testDataService.CreateScriptFile(Path.Combine(v10_0_0, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_5));

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            configuration.TargetVersion = targetVersion;

            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_1).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_2).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_3).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_4).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_5).ShouldBeTrue();
        }

        //https://github.com/rdagumampan/yuniql/issues/249
        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("v11.00")]
        public void Test_Issue_249_B(string targetVersion)
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            //creare environment-aware directories
            var v0_00 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00")).FullName;
            var v1_00 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v1.00")).FullName;
            var v10_00 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v10.00")).FullName;
            var v11_00 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v11.00")).FullName;
            var v2_00 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v2.00")).FullName;
            var v2_01 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v2.01")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(v0_00, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));
            _testDataService.CreateScriptFile(Path.Combine(v1_00, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));
            _testDataService.CreateScriptFile(Path.Combine(v10_00, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));
            _testDataService.CreateScriptFile(Path.Combine(v11_00, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_4));
            _testDataService.CreateScriptFile(Path.Combine(v2_00, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_5));
            _testDataService.CreateScriptFile(Path.Combine(v2_01, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_6));

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            configuration.TargetVersion = targetVersion;

            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_1).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_2).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_3).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_4).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_5).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_6).ShouldBeTrue();
        }

        //https://github.com/rdagumampan/yuniql/issues/249
        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("v0.03.1")]
        public void Test_Issue_252_A(string targetVersion)
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            //creare environment-aware directories
            var v0_00_1 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00.1")).FullName;
            var v0_01_1 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.01.1")).FullName;
            var v0_02_1 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.02.1")).FullName;
            var v0_03_1 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.03.1")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(v0_00_1, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));
            _testDataService.CreateScriptFile(Path.Combine(v0_01_1, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));
            _testDataService.CreateScriptFile(Path.Combine(v0_02_1, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));
            _testDataService.CreateScriptFile(Path.Combine(v0_03_1, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_4));

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            configuration.TargetVersion = targetVersion;

            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_1).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_2).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_3).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_4).ShouldBeTrue();
        }

        //https://github.com/rdagumampan/yuniql/issues/249
        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("v0.03.1.transactions")]
        public void Test_Issue_252_B(string targetVersion)
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            //creare environment-aware directories
            var v0_00_1 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00.1.tables")).FullName;
            var v0_01_1 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.01.1.procedures")).FullName;
            var v0_02_1 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.02.1.initial-data")).FullName;
            var v0_03_1 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.03.1.transactions")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(v0_00_1, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));
            _testDataService.CreateScriptFile(Path.Combine(v0_01_1, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));
            _testDataService.CreateScriptFile(Path.Combine(v0_02_1, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));
            _testDataService.CreateScriptFile(Path.Combine(v0_03_1, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_4));

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            configuration.TargetVersion = targetVersion;

            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_1).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_2).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_3).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_4).ShouldBeTrue();
        }

    }
}
