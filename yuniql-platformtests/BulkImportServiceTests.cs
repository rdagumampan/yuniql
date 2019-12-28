using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Shouldly;
using System;
using Yuniql.Core;
using Yuniql.Extensibility;

namespace Yuniql.PlatformTests
{
    [TestClass]
    public class BulkImportServiceTests : TestBase
    {
        private string _targetPlatform;
        private ITestDataService _testDataService;

        private IMigrationServiceFactory _migrationServiceFactory;
        private ITraceService _traceService;

        [TestInitialize]
        public void Setup()
        {
            //get target platform to tests from environment variable
            _targetPlatform = EnvironmentHelper.GetEnvironmentVariable("YUNIQL_TEST_TARGET_PLATFORM");
            if (string.IsNullOrEmpty(_targetPlatform))
            {
                _targetPlatform = "sqlserver";
            }

            //create test data service provider
            var testDataServiceFactory = new TestDataServiceFactory();
            _testDataService = testDataServiceFactory.Create(_targetPlatform);

            //create data service factory for migration proper
            _traceService = new FileTraceService();
            _migrationServiceFactory = new MigrationServiceFactory(_traceService);
        }

        [TestMethod]
        public void Test_Bulk_Import_With_Default_Delimter()
        {
            //arrange
            var workingPath = GetOrCreateWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = _testDataService.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);

            localVersionService.IncrementMajorVersion(workingPath, null);
            string v100Directory = Path.Combine(workingPath, "v1.00");
            _testDataService.CreateScriptFile(Path.Combine(v100Directory, $"test_v1_00.sql"), _testDataService.CreateDbObjectScript($"test_v1_00"));

            localVersionService.IncrementMinorVersion(workingPath, null);
            string v101Directory = Path.Combine(workingPath, "v1.01");
            _testDataService.CreateScriptFile(Path.Combine(v101Directory, $"test_v1_01.sql"), _testDataService.CreateDbObjectScript($"test_v1_01"));
            _testDataService.CreateScriptFile(Path.Combine(v101Directory, $"test_v1_02_TestCsv.sql"), _testDataService.CreateBulkTableScript("TestCsv"));

            //act
            var migrationService = _migrationServiceFactory.Create(_targetPlatform);
            migrationService.Initialize(connectionString);
            migrationService.Run(workingPath, "v1.01", autoCreateDatabase: true);

            //assert
            _testDataService.CheckIfDbObjectExist(connectionString, "test_v1_00").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(connectionString, "test_v1_01").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(connectionString, "TestCsv").ShouldBeTrue();

            //arrange - add new version with csv files
            localVersionService.IncrementMinorVersion(workingPath, null);
            string v102Directory = Path.Combine(workingPath, "v1.02");
            _testDataService.CreateScriptFile(Path.Combine(v102Directory, $"test_v1_02.sql"), _testDataService.CreateDbObjectScript($"test_v1_02"));
            File.Copy(Path.Combine(Environment.CurrentDirectory, "TestCsv.csv"), Path.Combine(v102Directory, "TestCsv.csv"));

            //act - bulk load csv files
            migrationService.Run(workingPath, "v1.02", autoCreateDatabase: true);

            //assert
            _testDataService.CheckIfDbObjectExist(connectionString, "test_v1_02").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(connectionString, "TestCsv").ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Bulk_Import_With_Pipe_Delimter()
        {
            //arrange
            var workingPath = GetOrCreateWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = _testDataService.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);

            string v000Directory = Path.Combine(workingPath, "v0.00");
            _testDataService.CreateScriptFile(Path.Combine(v000Directory, $"test_v0_00_TestCsvPipeDelimited.sql"), _testDataService.CreateBulkTableScript("test_v0_00_TestCsvPipeDelimited"));
            File.Copy(Path.Combine(Environment.CurrentDirectory, "TestCsvPipeDelimited.csv"), Path.Combine(v000Directory, "test_v0_00_TestCsvPipeDelimited.csv"));

            //act - bulk load csv files and change the default delimieted to |
            var migrationService = _migrationServiceFactory.Create(_targetPlatform);
            migrationService.Initialize(connectionString);
            migrationService.Run(workingPath, "v1.00", autoCreateDatabase: true, delimeter: "|");

            //assert
            _testDataService.CheckIfDbObjectExist(connectionString, "test_v0_00_TestCsvPipeDelimited").ShouldBeTrue();
        }


        [TestMethod]
        public void Test_Bulk_Import_With_Utf8()
        {
            //arrange
            var workingPath = GetOrCreateWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = _testDataService.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);

            string v000Directory = Path.Combine(workingPath, "v0.00");
            _testDataService.CreateScriptFile(Path.Combine(v000Directory, $"test_v0_00_TestCsvUtf8.sql"), _testDataService.CreateBulkTableScript("test_v0_00_TestCsvUtf8"));
            File.Copy(Path.Combine(Environment.CurrentDirectory, "TestCsvUtf8.csv"), Path.Combine(v000Directory, "test_v0_00_TestCsvUtf8.csv"));

            //act - bulk load csv files
            var migrationService = _migrationServiceFactory.Create(_targetPlatform);
            migrationService.Initialize(connectionString);
            migrationService.Run(workingPath, "v1.00", autoCreateDatabase: true);

            //assert
            _testDataService.CheckIfDbObjectExist(connectionString, "test_v0_00_TestCsvUtf8").ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Bulk_Import_With_Null_Columns()
        {
            //arrange
            var workingPath = GetOrCreateWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = _testDataService.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);

            string v000Directory = Path.Combine(workingPath, "v0.00");
            _testDataService.CreateScriptFile(Path.Combine(v000Directory, $"test_v0_00_TestCsvNullColumn.sql"), _testDataService.CreateBulkTableScript("test_v0_00_TestCsvNullColumn"));
            File.Copy(Path.Combine(Environment.CurrentDirectory, "TestCsvNullColumn.csv"), Path.Combine(v000Directory, "test_v0_00_TestCsvNullColumn.csv"));

            //act - bulk load csv files
            var migrationService = _migrationServiceFactory.Create(_targetPlatform);
            migrationService.Initialize(connectionString);
            migrationService.Run(workingPath, "v1.00", autoCreateDatabase: true);

            //assert
            _testDataService.CheckIfDbObjectExist(connectionString, "test_v0_00_TestCsvNullColumn").ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Bulk_Import_With_Unquoted()
        {
            //arrange
            var workingPath = GetOrCreateWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = _testDataService.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);

            string v000Directory = Path.Combine(workingPath, "v0.00");
            _testDataService.CreateScriptFile(Path.Combine(v000Directory, $"test_v0_00_TestCsvUnquoted.sql"), _testDataService.CreateBulkTableScript("test_v0_00_TestCsvUnquoted"));
            File.Copy(Path.Combine(Environment.CurrentDirectory, "TestCsvUnquoted.csv"), Path.Combine(v000Directory, "test_v0_00_TestCsvUnquoted.csv"));

            //act - bulk load csv files
            var migrationService = _migrationServiceFactory.Create(_targetPlatform);
            migrationService.Initialize(connectionString);
            migrationService.Run(workingPath, "v1.00", autoCreateDatabase: true);

            //assert
            _testDataService.CheckIfDbObjectExist(connectionString, "test_v0_00_TestCsvUnquoted").ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Bulk_Import_Destination_Table_Does_Not_Exist_Throws_Error()
        {
            //arrange
            var workingPath = GetOrCreateWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = _testDataService.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);

            string v000Directory = Path.Combine(workingPath, "v0.00");
            _testDataService.CreateScriptFile(Path.Combine(v000Directory, $"test_v0_00_TestCsvBulkTableOld.sql"), _testDataService.CreateBulkTableScript("test_v0_00_TestCsvBulkTableOld"));

            //we simulate a importing data into TestCsvBulkTable that doesnt exist
            File.Copy(Path.Combine(Environment.CurrentDirectory, "TestCsv.csv"), Path.Combine(v000Directory, "test_v0_00_TestCsvBulkTable.csv"));

            //act - bulk load csv files
            try
            {
                var migrationService = _migrationServiceFactory.Create(_targetPlatform);
                migrationService.Initialize(connectionString);
                migrationService.Run(workingPath, "v1.00", autoCreateDatabase: true);
            }
            catch (Exception ex)
            {
                ex.Message.ShouldNotBeEmpty();

                //asset all changes were rolled back
                _testDataService.CheckIfDbObjectExist(connectionString, "test_v0_00_TestCsvBulkTableOld").ShouldBeFalse();
                _testDataService.CheckIfDbObjectExist(connectionString, "test_v0_00_TestCsvBulkTable").ShouldBeFalse();
            }
        }

        [TestMethod]
        public void Test_Bulk_Import_Mismatch_Columns_But_Nullable()
        {
            //arrange
            var workingPath = GetOrCreateWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = _testDataService.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);

            string v000Directory = Path.Combine(workingPath, "v0.00");
            _testDataService.CreateScriptFile(Path.Combine(v000Directory, $"test_v0_00_TestCsvMismatchColumn.sql"), _testDataService.CreateBulkTableScript("test_v0_00_TestCsvMismatchColumn"));
            File.Copy(Path.Combine(Environment.CurrentDirectory, "TestCsvMismatchColumn.csv"), Path.Combine(v000Directory, "test_v0_00_TestCsvMismatchColumn.csv"));

            //act - bulk load csv files
            var migrationService = _migrationServiceFactory.Create(_targetPlatform);
            migrationService.Initialize(connectionString);
            migrationService.Run(workingPath, "v1.00", autoCreateDatabase: true);

            //assert
            _testDataService.CheckIfDbObjectExist(connectionString, "test_v0_00_TestCsvMismatchColumn").ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Bulk_Import_Mismatch_Columns_But_Not_Nullable()
        {
            //ignore if atomic ddl transaction not supported in target platforms
            if (!_testDataService.IsAtomicDDLSupported)
            {
                Assert.Inconclusive("Target database platform or version does not support atomic DDL operations. DDL operations like CREATE TABLE, CREATE VIEW are not gauranteed to be executed transactional.");
            }

            //arrange
            var workingPath = GetOrCreateWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = _testDataService.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);

            string v000Directory = Path.Combine(workingPath, "v0.00");
            _testDataService.CreateScriptFile(Path.Combine(v000Directory, $"test_v0_00.sql"), _testDataService.CreateDbObjectScript($"test_v0_00"));
            _testDataService.CreateScriptFile(Path.Combine(v000Directory, $"test_v0_00_TestCsvMismatchColumnNotNullable.sql"), _testDataService.CreateBulkTableScript("test_v0_00_TestCsvMismatchColumnNotNullable"));
            File.Copy(Path.Combine(Environment.CurrentDirectory, "TestCsvMismatchColumnNotNullable.csv"), Path.Combine(v000Directory, "test_v0_00_TestCsvMismatchColumnNotNullable.csv"));

            //act - bulk load csv files
            try
            {
                var migrationService = _migrationServiceFactory.Create(_targetPlatform);
                migrationService.Initialize(connectionString);
                migrationService.Run(workingPath, "v1.00", autoCreateDatabase: true);
            }
            catch (Exception ex)
            {
                ex.Message.ShouldNotBeEmpty();

                //asset all changes were rolled back
                _testDataService.CheckIfDbObjectExist(connectionString, "test_v0_00").ShouldBeFalse();
                _testDataService.CheckIfDbObjectExist(connectionString, "test_v0_00_TestCsvMismatchColumnNotNullable").ShouldBeFalse();
            }
        }

    }
}
