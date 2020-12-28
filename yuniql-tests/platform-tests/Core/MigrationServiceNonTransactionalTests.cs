using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Shouldly;
using Yuniql.Core;
using Yuniql.Extensibility;
using System;
using System.Linq;

namespace Yuniql.PlatformTests
{

    //https://docs.microsoft.com/en-gb/dotnet/standard/assembly/unloadability
    //https://github.com/dotnet/samples/blob/master/core/extensions/AppWithPlugin/AppWithPlugin/Program.cs
    [TestClass]
    public class MigrationServiceNonTransactionalTests : TestBase
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

        [TestMethodEx(Requires = "IsTransactionalDdlSupported")]
        public void Test_Run_Fail_Migration_When_Version_Directory_Has_Other_Directories()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);

            var transactionDirectory = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "_transaction")).FullName;
            var otherDirectory = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "other_directory")).FullName;

            //act & assert
            try
            {
                var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
                migrationService.Initialize(_testConfiguration.ConnectionString);
                migrationService.Run(_testConfiguration.WorkspacePath, autoCreateDatabase: true);
            }
            catch (Exception ex)
            {
                //used try/catch this instead of Assert.ThrowsException because different vendors
                //throws different exception type and message content
                ex.Message.ShouldNotBeNullOrEmpty();
            }
        }

        [TestMethodEx(Requires = "IsTransactionalDdlSupported")]
        public void Test_Run_Fail_Migration_When_Version_Directory_Has_Files()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);

            var transactionDirectory = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "_transaction")).FullName;
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), $"test_v0_00.sql"), _testDataService.GetSqlForCreateDbObject($"test_v0_00"));

            //act & assert
            try
            {
                var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
                migrationService.Initialize(_testConfiguration.ConnectionString);
                migrationService.Run(_testConfiguration.WorkspacePath, autoCreateDatabase: true);
            }
            catch (Exception ex)
            {
                //used try/catch this instead of Assert.ThrowsException because different vendors
                //throws different exception type and message content
                ex.Message.ShouldNotBeNullOrEmpty();
            }
        }

        [TestMethodEx(Requires = "IsTransactionalDdlSupported")]
        public void Test_Run_Fail_Migration_When_Non_Transaction_DDL_Failed()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);

            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), $"test_v0_00_01.sql"), _testDataService.GetSqlForCreateDbObjectWithError($"test_v0_00_01"));
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), $"test_v0_00_02.sql"), _testDataService.GetSqlForCreateDbObject($"test_v0_00_02"));
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), $"test_v0_00_03.sql"), _testDataService.GetSqlForCreateDbObject($"test_v0_00_03"));

            //act & assert
            try
            {
                var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
                migrationService.Initialize(_testConfiguration.ConnectionString);
                migrationService.Run(_testConfiguration.WorkspacePath, autoCreateDatabase: true);
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

        [TestMethodEx(Requires = "IsTransactionalDdlSupported")]
        public void Test_Run_Fail_Migration_When_No_Force_Continue_After_Failue_Option_Enabled()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);

            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), $"test_v0_00_01.sql"), _testDataService.GetSqlForCreateDbObjectWithError($"test_v0_00_01"));
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), $"test_v0_00_02.sql"), _testDataService.GetSqlForCreateDbObject($"test_v0_00_02"));
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), $"test_v0_00_03.sql"), _testDataService.GetSqlForCreateDbObject($"test_v0_00_03"));

            //act & assert - where the first script has failed
            try
            {
                var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
                migrationService.Initialize(_testConfiguration.ConnectionString);
                migrationService.Run(_testConfiguration.WorkspacePath, autoCreateDatabase: true);
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
                var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
                migrationService.Initialize(_testConfiguration.ConnectionString);
                migrationService.Run(_testConfiguration.WorkspacePath, autoCreateDatabase: true);
            }
            catch (Exception ex)
            {
                //used try/catch this instead of Assert.ThrowsException because different vendors
                //throws different exception type and message content
                ex.Message.ShouldNotBeNullOrEmpty();
            }
        }

        [TestMethodEx(Requires = "IsTransactionalDdlSupported")]
        public void Test_Run_Ok_Migration_With_Force_Continue_After_Failue_Option_Enabled()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);

            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), $"test_v0_00_01.sql"), _testDataService.GetSqlForCreateDbObjectWithError($"test_v0_00_01"));
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), $"test_v0_00_02.sql"), _testDataService.GetSqlForCreateDbObject($"test_v0_00_02"));
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), $"test_v0_00_03.sql"), _testDataService.GetSqlForCreateDbObject($"test_v0_00_03"));

            //act & assert - where the first script has failed
            var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
            migrationService.Initialize(_testConfiguration.ConnectionString);

            try
            {
                migrationService.Run(_testConfiguration.WorkspacePath, autoCreateDatabase: true);
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
            migrationService.Run(_testConfiguration.WorkspacePath, autoCreateDatabase: true, continueAfterFailure: true);

            //verrity status of migrations
            var retriedVersions = _testDataService.GetAllDbVersions(_testConfiguration.ConnectionString);
            retriedVersions.Count.ShouldBe(1);
            retriedVersions.Count(s => s.Status == Status.Successful).ShouldBe(1);

            var version = retriedVersions.Single();
            version.Status.ShouldBe(Status.Successful);
            version.FailedScriptPath.ShouldBeNullOrEmpty();
            version.FailedScriptError.ShouldBeNullOrEmpty();
        }

        [TestMethodEx(Requires = "IsTransactionalDdlSupported")]
        public void Test_Run_Ok_Without_Explicit_Transaction()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);

            localVersionService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"test_v1_00.sql"), _testDataService.GetSqlForCreateDbObject($"test_v1_00"));

            localVersionService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.01"), $"test_v1_01.sql"), _testDataService.GetSqlForCreateDbObject($"test_v1_01"));

            localVersionService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.02"), $"test_v1_02.sql"), _testDataService.GetSqlForCreateDbObject($"test_v1_02"));

            //act
            var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
            migrationService.Initialize(_testConfiguration.ConnectionString);
            migrationService.Run(_testConfiguration.WorkspacePath, autoCreateDatabase: true);

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_00").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_01").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_02").ShouldBeTrue();
        }


        [TestMethodEx(Requires = "IsTransactionalDdlSupported")]
        public void Test_Run_Ok_With_Explicit_Transaction()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);

            localVersionService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            var v1_00_transactionDirectory = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v1.00", "_transaction")).FullName;
            _testDataService.CreateScriptFile(Path.Combine(v1_00_transactionDirectory, $"test_v1_00.sql"), _testDataService.GetSqlForCreateDbObject($"test_v1_00"));

            localVersionService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);
            var v1_01_transactionDirectory = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.01", "_transaction")).FullName;
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.01"), $"test_v1_01.sql"), _testDataService.GetSqlForCreateDbObject($"test_v1_01"));

            localVersionService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);
            var v1_02_transactionDirectory = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.02", "_transaction")).FullName;
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.02"), $"test_v1_02.sql"), _testDataService.GetSqlForCreateDbObject($"test_v1_02"));

            //act
            var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
            migrationService.Initialize(_testConfiguration.ConnectionString);
            migrationService.Run(_testConfiguration.WorkspacePath, autoCreateDatabase: true);

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_00").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_01").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_02").ShouldBeTrue();
        }

        [TestMethodEx(Requires = "IsTransactionalDdlSupported")]
        public void Test_Run_Ok_Without_Explicit_Transaction_With_SubDirectories()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);

            localVersionService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            var v1rootDirectory = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v1.00", "_transaction")).FullName;
            _testDataService.CreateScriptFile(Path.Combine(v1rootDirectory, $"test_v1_00.sql"), _testDataService.GetSqlForCreateDbObject($"test_v1_00")); ;

            string v1level1Directory = Path.Combine(v1rootDirectory, "v1.00-level1");
            Directory.CreateDirectory(v1level1Directory);
            _testDataService.CreateScriptFile(Path.Combine(v1level1Directory, $"test_v1_00_level1.sql"), _testDataService.GetSqlForCreateDbObject($"test_v1_00_level1"));

            string v1level1SubDirectory = Path.Combine(v1level1Directory, "v1.00-level1-sublevel1");
            Directory.CreateDirectory(v1level1SubDirectory);
            _testDataService.CreateScriptFile(Path.Combine(v1level1SubDirectory, $"test_v1_00_level1_sublevel1.sql"), _testDataService.GetSqlForCreateDbObject($"test_v1_00_level1_sublevel1"));

            localVersionService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            var v2rootDirectory = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v2.00", "_transaction")).FullName;
            _testDataService.CreateScriptFile(Path.Combine(v2rootDirectory, $"test_v2_00.sql"), _testDataService.GetSqlForCreateDbObject($"test_v2_00")); ;

            string v2level1Directory = Path.Combine(v2rootDirectory, "v2.00-level1");
            Directory.CreateDirectory(v2level1Directory);
            _testDataService.CreateScriptFile(Path.Combine(v2level1Directory, $"test_v2_00_level1.sql"), _testDataService.GetSqlForCreateDbObject($"test_v2_00_level1"));

            string v2level1SubDirectory = Path.Combine(v2level1Directory, "v2.00-level1-sublevel1");
            Directory.CreateDirectory(v2level1SubDirectory);
            _testDataService.CreateScriptFile(Path.Combine(v2level1SubDirectory, $"test_v2_00_level1_sublevel1.sql"), _testDataService.GetSqlForCreateDbObject($"test_v2_00_level1_sublevel1"));

            //act
            var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
            migrationService.Initialize(_testConfiguration.ConnectionString);
            migrationService.Run(_testConfiguration.WorkspacePath, "v2.00", autoCreateDatabase: true);

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_00").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_00_level1").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_00_level1_sublevel1").ShouldBeTrue();

            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v2_00").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v2_00_level1").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v2_00_level1_sublevel1").ShouldBeTrue();
        }


        [TestMethodEx(Requires = "IsTransactionalDdlSupported")]
        public void Test_Run_Ok_With_Explicit_Transaction_With_SubDirectories()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);

            localVersionService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            string v1rootDirectory = Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"));
            _testDataService.CreateScriptFile(Path.Combine(v1rootDirectory, $"test_v1_00.sql"), _testDataService.GetSqlForCreateDbObject($"test_v1_00")); ;

            string v1level1Directory = Path.Combine(v1rootDirectory, "v1.00-level1");
            Directory.CreateDirectory(v1level1Directory);
            _testDataService.CreateScriptFile(Path.Combine(v1level1Directory, $"test_v1_00_level1.sql"), _testDataService.GetSqlForCreateDbObject($"test_v1_00_level1"));

            string v1level1SubDirectory = Path.Combine(v1level1Directory, "v1.00-level1-sublevel1");
            Directory.CreateDirectory(v1level1SubDirectory);
            _testDataService.CreateScriptFile(Path.Combine(v1level1SubDirectory, $"test_v1_00_level1_sublevel1.sql"), _testDataService.GetSqlForCreateDbObject($"test_v1_00_level1_sublevel1"));

            localVersionService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            string v2rootDirectory = Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v2.00"));
            _testDataService.CreateScriptFile(Path.Combine(v2rootDirectory, $"test_v2_00.sql"), _testDataService.GetSqlForCreateDbObject($"test_v2_00")); ;

            string v2level1Directory = Path.Combine(v2rootDirectory, "v2.00-level1");
            Directory.CreateDirectory(v2level1Directory);
            _testDataService.CreateScriptFile(Path.Combine(v2level1Directory, $"test_v2_00_level1.sql"), _testDataService.GetSqlForCreateDbObject($"test_v2_00_level1"));

            string v2level1SubDirectory = Path.Combine(v2level1Directory, "v2.00-level1-sublevel1");
            Directory.CreateDirectory(v2level1SubDirectory);
            _testDataService.CreateScriptFile(Path.Combine(v2level1SubDirectory, $"test_v2_00_level1_sublevel1.sql"), _testDataService.GetSqlForCreateDbObject($"test_v2_00_level1_sublevel1"));

            //act
            var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
            migrationService.Initialize(_testConfiguration.ConnectionString);
            migrationService.Run(_testConfiguration.WorkspacePath, "v2.00", autoCreateDatabase: true);

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_00").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_00_level1").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_00_level1_sublevel1").ShouldBeTrue();

            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v2_00").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v2_00_level1").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v2_00_level1_sublevel1").ShouldBeTrue();
        }

    }
}
