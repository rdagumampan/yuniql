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
    public class VersionDirectoryTests : TestClassBase
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
        [DataRow("v20")]
        public void Test_Format_vx(string targetVersion)
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            //creare environment-aware directories
            var v0 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0")).FullName;
            var v1 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v1")).FullName;
            var v2 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v2")).FullName;
            var v10 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v10")).FullName;
            var v11 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v11")).FullName;
            var v20 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v20")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(v0, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));
            _testDataService.CreateScriptFile(Path.Combine(v1, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));
            _testDataService.CreateScriptFile(Path.Combine(v2, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));
            _testDataService.CreateScriptFile(Path.Combine(v10, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_4));
            _testDataService.CreateScriptFile(Path.Combine(v11, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_5));
            _testDataService.CreateScriptFile(Path.Combine(v20, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_6));

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
        [DataRow("v20.20")]
        public void Test_Format_vx_x(string targetVersion)
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            //creare environment-aware directories
            var v0 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.0")).FullName;
            var v1 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v1.1")).FullName;
            var v2 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v2.2")).FullName;
            var v10 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v10.10")).FullName;
            var v11 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v11.11")).FullName;
            var v20 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v20.20")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(v0, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));
            _testDataService.CreateScriptFile(Path.Combine(v1, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));
            _testDataService.CreateScriptFile(Path.Combine(v2, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));
            _testDataService.CreateScriptFile(Path.Combine(v10, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_4));
            _testDataService.CreateScriptFile(Path.Combine(v11, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_5));
            _testDataService.CreateScriptFile(Path.Combine(v20, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_6));

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
        [DataRow("v20.20.20")]
        public void Test_Format_vx_x_x(string targetVersion)
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            //creare environment-aware directories
            var v0 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.0.0")).FullName;
            var v1 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v1.1.1")).FullName;
            var v2 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v2.2.2")).FullName;
            var v10 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v10.10.10")).FullName;
            var v11 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v11.11.11")).FullName;
            var v20 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v20.20.20")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(v0, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));
            _testDataService.CreateScriptFile(Path.Combine(v1, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));
            _testDataService.CreateScriptFile(Path.Combine(v2, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));
            _testDataService.CreateScriptFile(Path.Combine(v10, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_4));
            _testDataService.CreateScriptFile(Path.Combine(v11, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_5));
            _testDataService.CreateScriptFile(Path.Combine(v20, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_6));

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
        [DataRow("v20")]
        public void Test_Format_vxx(string targetVersion)
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            //creare environment-aware directories
            var v0 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v00")).FullName;
            var v1 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v01")).FullName;
            var v2 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v02")).FullName;
            var v10 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v10")).FullName;
            var v11 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v11")).FullName;
            var v20 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v20")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(v0, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));
            _testDataService.CreateScriptFile(Path.Combine(v1, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));
            _testDataService.CreateScriptFile(Path.Combine(v2, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));
            _testDataService.CreateScriptFile(Path.Combine(v10, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_4));
            _testDataService.CreateScriptFile(Path.Combine(v11, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_5));
            _testDataService.CreateScriptFile(Path.Combine(v20, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_6));

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
        [DataRow("v20.20")]
        public void Test_Format_vxx_xx(string targetVersion)
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            //creare environment-aware directories
            var v0 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v00.00")).FullName;
            var v1 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v01.01")).FullName;
            var v2 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v02.02")).FullName;
            var v10 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v10.10")).FullName;
            var v11 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v11.11")).FullName;
            var v20 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v20.20")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(v0, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));
            _testDataService.CreateScriptFile(Path.Combine(v1, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));
            _testDataService.CreateScriptFile(Path.Combine(v2, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));
            _testDataService.CreateScriptFile(Path.Combine(v10, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_4));
            _testDataService.CreateScriptFile(Path.Combine(v11, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_5));
            _testDataService.CreateScriptFile(Path.Combine(v20, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_6));

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
        [DataRow("v20.20.20")]
        public void Test_Format_vxx_xx_xx(string targetVersion)
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            //creare environment-aware directories
            var v0 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v00.00.00")).FullName;
            var v1 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v01.01.01")).FullName;
            var v2 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v02.02.02")).FullName;
            var v10 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v10.10.10")).FullName;
            var v11 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v11.11.11")).FullName;
            var v20 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v20.20.20")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(v0, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));
            _testDataService.CreateScriptFile(Path.Combine(v1, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));
            _testDataService.CreateScriptFile(Path.Combine(v2, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));
            _testDataService.CreateScriptFile(Path.Combine(v10, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_4));
            _testDataService.CreateScriptFile(Path.Combine(v11, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_5));
            _testDataService.CreateScriptFile(Path.Combine(v20, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_6));

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
        [DataRow("v25102022.0020")]
        public void Test_Format_vddmmyyy(string targetVersion)
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            //creare environment-aware directories
            var v0 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v25102022.0000")).FullName;
            var v1 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v25102022.0001")).FullName;
            var v2 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v25102022.0002")).FullName;
            var v10 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v25102022.0010")).FullName;
            var v11 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v25102022.0011")).FullName;
            var v20 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v25102022.0020")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(v0, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));
            _testDataService.CreateScriptFile(Path.Combine(v1, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));
            _testDataService.CreateScriptFile(Path.Combine(v2, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));
            _testDataService.CreateScriptFile(Path.Combine(v10, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_4));
            _testDataService.CreateScriptFile(Path.Combine(v11, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_5));
            _testDataService.CreateScriptFile(Path.Combine(v20, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_6));

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
        [DataRow("v20.20-label")]
        public void Test_Format_vx_xx_label (string targetVersion)
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            //creare environment-aware directories
            var v0 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00-label")).FullName;
            var v1 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v1.01-label")).FullName;
            var v2 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v2.02-label")).FullName;
            var v10 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v10.10-label")).FullName;
            var v11 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v11.11-label")).FullName;
            var v20 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v20.20-label")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(v0, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));
            _testDataService.CreateScriptFile(Path.Combine(v1, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));
            _testDataService.CreateScriptFile(Path.Combine(v2, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));
            _testDataService.CreateScriptFile(Path.Combine(v10, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_4));
            _testDataService.CreateScriptFile(Path.Combine(v11, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_5));
            _testDataService.CreateScriptFile(Path.Combine(v20, $"script.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_6));

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


    }
}
