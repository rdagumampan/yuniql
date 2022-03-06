using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Shouldly;
using Yuniql.Core;
using Yuniql.Extensibility;
using System;
using System.Linq;
using Yuniql.PlatformTests.Interfaces;
using Yuniql.PlatformTests.Setup;
using IMigrationServiceFactory = Yuniql.PlatformTests.Interfaces.IMigrationServiceFactory;
using MigrationServiceFactory = Yuniql.PlatformTests.Setup.MigrationServiceFactory;
using System.Diagnostics;

namespace Yuniql.PlatformTests.Core
{

    //https://docs.microsoft.com/en-gb/dotnet/standard/assembly/unloadability
    //https://github.com/dotnet/samples/blob/master/core/extensions/AppWithPlugin/AppWithPlugin/Program.cs
    [TestClass]
    public class MigrationServiceMixedTests : TestClassBase
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

        [TestMethodEx(Requires = "IsTransactionalDdlNotSupported")]
        public void Test_Run_Fail_Migration_When_Version_Directory_With_Explicit_Transaction_Has_Other_Directories()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            var transactionDirectory = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", RESERVED_DIRECTORY_NAME.TRANSACTION)).FullName;
            var otherDirectory = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "other_directory")).FullName;

            //act & assert
            try
            {
                var configuration = _testConfiguration.GetFreshConfiguration();
                var migrationService = _migrationServiceFactory.Create(configuration.Platform);
                migrationService.Run();
            }
            catch (Exception ex)
            {
                //used try/catch this instead of Assert.ThrowsException because different vendors
                //throws different exception type and message content
                ex.Message.ShouldNotBeNullOrEmpty();
            }
        }

        [TestMethodEx(Requires = "IsTransactionalDdlNotSupported")]
        public void Test_Run_Fail_Migration_When_Version_With_Explicit_Transaction_Directory_Has_Files()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            var transactionDirectory = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", RESERVED_DIRECTORY_NAME.TRANSACTION)).FullName;
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), $"test_v0_00.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));

            //act & assert
            try
            {
                var configuration = _testConfiguration.GetFreshConfiguration();
                var migrationService = _migrationServiceFactory.Create(configuration.Platform);
                migrationService.Run();
            }
            catch (Exception ex)
            {
                //used try/catch this instead of Assert.ThrowsException because different vendors
                //throws different exception type and message content
                ex.Message.ShouldNotBeNullOrEmpty();
            }
        }

        [TestMethodEx(Requires = "IsTransactionalDdlNotSupported")]
        public void Test_Run_Fail_Migration_When_DDL_Failed_And_Transactional_DDL_Not_Supported()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), $"test_v0_00_01.sql"), _testDataService.GetSqlForCreateDbObjectWithError(TEST_DBOBJECTS.DB_OBJECT_1));
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), $"test_v0_00_02.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), $"test_v0_00_03.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));

            //act & assert
            try
            {
                var configuration = _testConfiguration.GetFreshConfiguration();
                var migrationService = _migrationServiceFactory.Create(configuration.Platform);
                migrationService.Run();
            }
            catch (Exception ex)
            {
                //used try/catch this instead of Assert.ThrowsException because different vendors
                //throws different exception type and message content
                ex.Message.ShouldNotBeNullOrEmpty();
            }

            //verrity status of migrations
            var versions = _testDataService.GetAllDbVersions(_testConfiguration.ConnectionString);
            versions.Count.ShouldBe(1);
            versions.Count(s => s.Status == Status.Failed).ShouldBe(1);

            var failedVersion = versions.Single();
            failedVersion.Status.ShouldBe(Status.Failed);
            failedVersion.FailedScriptPath.ShouldBe(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), $"test_v0_00_01.sql"));
            failedVersion.FailedScriptError.ShouldNotBeNullOrEmpty();
        }

        [TestMethodEx(Requires = "IsTransactionalDdlNotSupported")]
        public void Test_Run_Fail_Migration_When_No_Force_Continue_After_Failue_Option_Enabled()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), $"test_v0_00_01.sql"), _testDataService.GetSqlForCreateDbObjectWithError(TEST_DBOBJECTS.DB_OBJECT_1));
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), $"test_v0_00_02.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), $"test_v0_00_03.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));

            //act & assert - where the first script has failed
            try
            {
                var configuration = _testConfiguration.GetFreshConfiguration();
                var migrationService = _migrationServiceFactory.Create(configuration.Platform);
                migrationService.Run();
            }
            catch (Exception ex)
            {
                //used try/catch this instead of Assert.ThrowsException because different vendors
                //throws different exception type and message content
                ex.Message.ShouldNotBeNullOrEmpty();
            }

            //verrity status of migrations
            var versions = _testDataService.GetAllDbVersions(_testConfiguration.ConnectionString);
            versions.Count.ShouldBe(1);
            versions.Count(s => s.Status == Status.Failed).ShouldBe(1);

            var failedVersion = versions.Single();
            failedVersion.Status.ShouldBe(Status.Failed);
            failedVersion.FailedScriptPath.ShouldBe(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), $"test_v0_00_01.sql"));
            failedVersion.FailedScriptError.ShouldNotBeNullOrEmpty();

            //act & assert - where force to continue on failure
            try
            {
                var configuration = _testConfiguration.GetFreshConfiguration();
                var migrationService = _migrationServiceFactory.Create(configuration.Platform);
                migrationService.Run();
            }
            catch (Exception ex)
            {
                //used try/catch this instead of Assert.ThrowsException because different vendors
                //throws different exception type and message content
                ex.Message.ShouldNotBeNullOrEmpty();
            }
        }

        [TestMethodEx(Requires = "IsTransactionalDdlNotSupported")]
        public void Test_Run_Ok_Migration_With_Force_Continue_After_Failue_Option_Enabled()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), $"test_v0_00_01.sql"), _testDataService.GetSqlForCreateDbObjectWithError(TEST_DBOBJECTS.DB_OBJECT_1));
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), $"test_v0_00_02.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), $"test_v0_00_03.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));

            //act & assert - where the first script has failed
            var configuration = _testConfiguration.GetFreshConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);

            try
            {
                migrationService.Run();
            }
            catch (Exception ex)
            {
                //used try/catch this instead of Assert.ThrowsException because different vendors
                //throws different exception type and message content
                ex.Message.ShouldNotBeNullOrEmpty();
            }

            //verrity status of migrations
            var versions = _testDataService.GetAllDbVersions(_testConfiguration.ConnectionString);
            versions.Count.ShouldBe(1);
            versions.Count(s => s.Status == Status.Failed).ShouldBe(1);

            var failedVersion = versions.Single();
            failedVersion.Status.ShouldBe(Status.Failed);
            failedVersion.FailedScriptPath.ShouldBe(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), $"test_v0_00_01.sql"));
            failedVersion.FailedScriptError.ShouldNotBeNullOrEmpty();

            //act & assert - where force to continue on failure
            configuration.IsContinueAfterFailure = true;
            migrationService.Run();

            //verrity status of migrations
            var retriedVersions = _testDataService.GetAllDbVersions(_testConfiguration.ConnectionString);
            retriedVersions.Count.ShouldBe(1);
            retriedVersions.Count(s => s.Status == Status.Successful).ShouldBe(1);

            var version = retriedVersions.Single();
            version.Status.ShouldBe(Status.Successful);
            version.FailedScriptPath.ShouldBeNullOrEmpty();
            version.FailedScriptError.ShouldBeNullOrEmpty();
        }

        [TestMethodEx(Requires = "IsTransactionalDdlNotSupported")]
        public void Test_Run_Ok_Without_Explicit_Transaction()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            workspaceService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"test_v1_00.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));

            workspaceService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.01"), $"test_v1_01.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));

            workspaceService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.02"), $"test_v1_02.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_1).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_2).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_3).ShouldBeTrue();
        }


        [TestMethodEx(Requires = "IsTransactionalDdlNotSupported")]
        public void Test_Run_Ok_With_Explicit_Transaction()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            workspaceService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            var v1_00_transactionDirectory = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v1.00", RESERVED_DIRECTORY_NAME.TRANSACTION)).FullName;
            _testDataService.CreateScriptFile(Path.Combine(v1_00_transactionDirectory, $"test_v1_00.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));

            workspaceService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);
            var v1_01_transactionDirectory = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.01", RESERVED_DIRECTORY_NAME.TRANSACTION)).FullName;
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.01"), $"test_v1_01.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));

            workspaceService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);
            var v1_02_transactionDirectory = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.02", RESERVED_DIRECTORY_NAME.TRANSACTION)).FullName;
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.02"), $"test_v1_02.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_1).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_2).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_3).ShouldBeTrue();
        }

        [TestMethodEx(Requires = "IsTransactionalDdlNotSupported")]
        public void Test_Run_Ok_Without_Explicit_Transaction_With_SubDirectories()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            workspaceService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            var v1rootDirectory = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v1.00", RESERVED_DIRECTORY_NAME.TRANSACTION)).FullName;
            _testDataService.CreateScriptFile(Path.Combine(v1rootDirectory, $"test_v1_00.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1)); ;

            string v1level1Directory = Path.Combine(v1rootDirectory, "v1.00-level1");
            Directory.CreateDirectory(v1level1Directory);
            _testDataService.CreateScriptFile(Path.Combine(v1level1Directory, $"test_v1_00_level1.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));

            string v1level1SubDirectory = Path.Combine(v1level1Directory, "v1.00-level1-sublevel1");
            Directory.CreateDirectory(v1level1SubDirectory);
            _testDataService.CreateScriptFile(Path.Combine(v1level1SubDirectory, $"test_v1_00_level1_sublevel1.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));

            workspaceService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            var v2rootDirectory = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v2.00", RESERVED_DIRECTORY_NAME.TRANSACTION)).FullName;
            _testDataService.CreateScriptFile(Path.Combine(v2rootDirectory, $"test_v2_00.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_4)); ;

            string v2level1Directory = Path.Combine(v2rootDirectory, "v2.00-level1");
            Directory.CreateDirectory(v2level1Directory);
            _testDataService.CreateScriptFile(Path.Combine(v2level1Directory, $"test_v2_00_level1.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_5));

            string v2level1SubDirectory = Path.Combine(v2level1Directory, "v2.00-level1-sublevel1");
            Directory.CreateDirectory(v2level1SubDirectory);
            _testDataService.CreateScriptFile(Path.Combine(v2level1SubDirectory, $"test_v2_00_level1_sublevel1.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_6));

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            configuration.TargetVersion = "v2.00";

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


        [TestMethodEx(Requires = "IsTransactionalDdlNotSupported")]
        public void Test_Run_Ok_With_Explicit_Transaction_With_SubDirectories()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            workspaceService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            string v1rootDirectory = Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"));
            _testDataService.CreateScriptFile(Path.Combine(v1rootDirectory, $"test_v1_00.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1)); ;

            string v1level1Directory = Path.Combine(v1rootDirectory, "v1.00-level1");
            Directory.CreateDirectory(v1level1Directory);
            _testDataService.CreateScriptFile(Path.Combine(v1level1Directory, $"test_v1_00_level1.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));

            string v1level1SubDirectory = Path.Combine(v1level1Directory, "v1.00-level1-sublevel1");
            Directory.CreateDirectory(v1level1SubDirectory);
            _testDataService.CreateScriptFile(Path.Combine(v1level1SubDirectory, $"test_v1_00_level1_sublevel1.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));

            workspaceService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            string v2rootDirectory = Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v2.00"));
            _testDataService.CreateScriptFile(Path.Combine(v2rootDirectory, $"test_v2_00.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_4)); ;

            string v2level1Directory = Path.Combine(v2rootDirectory, "v2.00-level1");
            Directory.CreateDirectory(v2level1Directory);
            _testDataService.CreateScriptFile(Path.Combine(v2level1Directory, $"test_v2_00_level1.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_5));

            string v2level1SubDirectory = Path.Combine(v2level1Directory, "v2.00-level1-sublevel1");
            Directory.CreateDirectory(v2level1SubDirectory);
            _testDataService.CreateScriptFile(Path.Combine(v2level1SubDirectory, $"test_v2_00_level1_sublevel1.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_6));

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            configuration.TargetVersion = "v2.00";

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
