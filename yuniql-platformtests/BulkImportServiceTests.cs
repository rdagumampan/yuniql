using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Shouldly;
using System;
using Yuniql.Core;
using Yuniql.Extensibility;
using System.Collections.Generic;
using System.Linq;

namespace Yuniql.PlatformTests
{
    [TestClass]
    public class BulkImportServiceTests : TestBase
    {
        private ITestDataService _testDataService;
        private ITraceService _traceService;
        private IMigrationServiceFactory _migrationServiceFactory;
        private TestConfiguration _testConfiguration;

        [TestInitialize]
        public void Setup()
        {
            //get target platform to tests from environment variable
            var targetPlatform = GetTargetPlatform();

            //create test data service provider
            var testDataServiceFactory = new TestDataServiceFactory();
            _testDataService = testDataServiceFactory.Create(targetPlatform);

            //create data service factory for migration proper
            _traceService = new FileTraceService();
            _migrationServiceFactory = new MigrationServiceFactory(_traceService);

            //create test run configuration
            var workspacePath = GetOrCreateWorkingPath();
            var databaseName = new DirectoryInfo(workspacePath).Name;
            _testConfiguration = new TestConfiguration
            {
                TargetPlatform = targetPlatform,
                WorkspacePath = workspacePath,
                DatabaseName = databaseName,
                ConnectionString = _testDataService.GetConnectionString(databaseName)
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            //drop test database
            _testDataService.DropTestDatabase(_testConfiguration.ConnectionString, _testConfiguration.DatabaseName);

            //drop test directories
            if (Directory.Exists(_testConfiguration.WorkspacePath))
            {
                Directory.Delete(_testConfiguration.WorkspacePath, true);
            }
        }

        [TestMethod]
        public void Test_Bulk_Import_With_Default_Delimter()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);

            localVersionService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            string v100Directory = Path.Combine(_testConfiguration.WorkspacePath, "v1.00");
            _testDataService.CreateScriptFile(Path.Combine(v100Directory, $"test_v1_00.sql"), _testDataService.CreateDbObjectScript($"test_v1_00"));

            localVersionService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);
            string v101Directory = Path.Combine(_testConfiguration.WorkspacePath, "v1.01");
            _testDataService.CreateScriptFile(Path.Combine(v101Directory, $"test_v1_01.sql"), _testDataService.CreateDbObjectScript($"test_v1_01"));
            _testDataService.CreateScriptFile(Path.Combine(v101Directory, $"test_v1_02_TestCsv.sql"), _testDataService.CreateBulkTableScript("TestCsv"));

            //act
            var migrationService = _migrationServiceFactory.Create(_testConfiguration.TargetPlatform);
            migrationService.Initialize(_testConfiguration.ConnectionString);
            migrationService.Run(_testConfiguration.WorkspacePath, "v1.01", autoCreateDatabase: true);

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_00").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_01").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "TestCsv").ShouldBeTrue();

            //arrange - add new version with csv files
            localVersionService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);
            string v102Directory = Path.Combine(_testConfiguration.WorkspacePath, "v1.02");
            _testDataService.CreateScriptFile(Path.Combine(v102Directory, $"test_v1_02.sql"), _testDataService.CreateDbObjectScript($"test_v1_02"));
            File.Copy(Path.Combine(Environment.CurrentDirectory, "TestCsv.csv"), Path.Combine(v102Directory, "TestCsv.csv"));

            //act - bulk load csv files
            migrationService.Run(_testConfiguration.WorkspacePath, "v1.02", autoCreateDatabase: true);

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_02").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "TestCsv").ShouldBeTrue();

            var results = _testDataService.GetBulkTestData(_testConfiguration.ConnectionString, "TestCsv");
            var testDataRows = new List<BulkTestDataRow>
            {
                new BulkTestDataRow { FirstName="Jack", LastName ="Poole", BirthDate = new DateTime(1980,1,1) },
                new BulkTestDataRow { FirstName="Diana", LastName ="Churchill", BirthDate = new DateTime(1980,1,1) },
                new BulkTestDataRow { FirstName="Rebecca", LastName ="Lyman", BirthDate = new DateTime(1980,1,1) },
                new BulkTestDataRow { FirstName="Sam", LastName ="Macdonald", BirthDate = new DateTime(1980,1,1) },
                new BulkTestDataRow { FirstName="Matt", LastName ="Paige", BirthDate = new DateTime(1980,1,1) },
            };

            results.Count.ShouldBe(5);
            testDataRows.All(t => results.Exists(r => 
                t.FirstName == r.FirstName 
                && t.LastName == r.LastName 
                && t.BirthDate == r.BirthDate
            )).ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Bulk_Import_With_Pipe_Delimter()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);

            string v000Directory = Path.Combine(_testConfiguration.WorkspacePath, "v0.00");
            _testDataService.CreateScriptFile(Path.Combine(v000Directory, $"test_v0_00_TestCsvPipeDelimited.sql"), _testDataService.CreateBulkTableScript("test_v0_00_TestCsvPipeDelimited"));
            File.Copy(Path.Combine(Environment.CurrentDirectory, "TestCsvPipeDelimited.csv"), Path.Combine(v000Directory, "test_v0_00_TestCsvPipeDelimited.csv"));

            //act - bulk load csv files and change the default delimieted to |
            var migrationService = _migrationServiceFactory.Create(_testConfiguration.TargetPlatform);
            migrationService.Initialize(_testConfiguration.ConnectionString);
            migrationService.Run(_testConfiguration.WorkspacePath, "v1.00", autoCreateDatabase: true, delimeter: "|");

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v0_00_TestCsvPipeDelimited").ShouldBeTrue();

            var results = _testDataService.GetBulkTestData(_testConfiguration.ConnectionString, "test_v0_00_TestCsvPipeDelimited");
            var testDataRows = new List<BulkTestDataRow>
            {
                new BulkTestDataRow { FirstName="Jack", LastName ="Poole", BirthDate = new DateTime(1980,1,1) },
                new BulkTestDataRow { FirstName="Diana", LastName ="Churchill", BirthDate = new DateTime(1980,1,1) },
                new BulkTestDataRow { FirstName="Rebecca", LastName ="Lyman", BirthDate = new DateTime(1980,1,1) },
                new BulkTestDataRow { FirstName="Sam", LastName ="Macdonald", BirthDate = new DateTime(1980,1,1) },
                new BulkTestDataRow { FirstName="Matt", LastName ="Paige", BirthDate = new DateTime(1980,1,1) },
            };

            results.Count.ShouldBe(5);
            testDataRows.All(t => results.Exists(r =>
                t.FirstName == r.FirstName
                && t.LastName == r.LastName
                && t.BirthDate == r.BirthDate
            )).ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Bulk_Import_With_Utf8()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);

            string v000Directory = Path.Combine(_testConfiguration.WorkspacePath, "v0.00");
            _testDataService.CreateScriptFile(Path.Combine(v000Directory, $"test_v0_00_TestCsvUtf8.sql"), _testDataService.CreateBulkTableScript("test_v0_00_TestCsvUtf8"));
            File.Copy(Path.Combine(Environment.CurrentDirectory, "TestCsvUtf8.csv"), Path.Combine(v000Directory, "test_v0_00_TestCsvUtf8.csv"));

            //act - bulk load csv files
            var migrationService = _migrationServiceFactory.Create(_testConfiguration.TargetPlatform);
            migrationService.Initialize(_testConfiguration.ConnectionString);
            migrationService.Run(_testConfiguration.WorkspacePath, "v1.00", autoCreateDatabase: true);

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v0_00_TestCsvUtf8").ShouldBeTrue();

            var results = _testDataService.GetBulkTestData(_testConfiguration.ConnectionString, "test_v0_00_TestCsvUtf8");
            var testDataRows = new List<BulkTestDataRow>
            {
                new BulkTestDataRow { FirstName="Allan", LastName ="Søgaard", BirthDate = new DateTime(1980,1,1) },
                new BulkTestDataRow { FirstName="Martin", LastName ="Bæk", BirthDate = new DateTime(1980,1,1) },
                new BulkTestDataRow { FirstName="Gitte", LastName ="Jürgensen", BirthDate = new DateTime(1980,1,1) },
                new BulkTestDataRow { FirstName="Maria", LastName ="Østergård", BirthDate = new DateTime(1980,1,1) },
                new BulkTestDataRow { FirstName="Peter", LastName ="Langkjær", BirthDate = new DateTime(1980,1,1) },
            };

            results.Count.ShouldBe(5);
            testDataRows.All(t => results.Exists(r =>
                t.FirstName == r.FirstName
                && t.LastName == r.LastName
                && t.BirthDate == r.BirthDate
            )).ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Bulk_Import_With_Null_Columns()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);

            string v000Directory = Path.Combine(_testConfiguration.WorkspacePath, "v0.00");
            _testDataService.CreateScriptFile(Path.Combine(v000Directory, $"test_v0_00_TestCsvNullColumn.sql"), _testDataService.CreateBulkTableScript("test_v0_00_TestCsvNullColumn"));
            File.Copy(Path.Combine(Environment.CurrentDirectory, "TestCsvNullColumn.csv"), Path.Combine(v000Directory, "test_v0_00_TestCsvNullColumn.csv"));

            //act - bulk load csv files
            var migrationService = _migrationServiceFactory.Create(_testConfiguration.TargetPlatform);
            migrationService.Initialize(_testConfiguration.ConnectionString);
            migrationService.Run(_testConfiguration.WorkspacePath, "v1.00", autoCreateDatabase: true);

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v0_00_TestCsvNullColumn").ShouldBeTrue();

            var results = _testDataService.GetBulkTestData(_testConfiguration.ConnectionString, "test_v0_00_TestCsvNullColumn");
            var testDataRows = new List<BulkTestDataRow>
            {
                new BulkTestDataRow { FirstName="Jack", LastName ="Poole"},
                new BulkTestDataRow { FirstName="Diana", LastName ="Churchill"},
                new BulkTestDataRow { FirstName="Rebecca", LastName ="Lyman" },
                new BulkTestDataRow { FirstName="Sam", LastName ="Macdonald"},
                new BulkTestDataRow { FirstName="Matt", LastName ="Paige"},
            };

            results.Count.ShouldBe(5);
            testDataRows.All(t => results.Exists(r =>
                t.FirstName == r.FirstName
                && t.LastName == r.LastName
                && !r.BirthDate.HasValue
            )).ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Bulk_Import_With_Unquoted()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);

            string v000Directory = Path.Combine(_testConfiguration.WorkspacePath, "v0.00");
            _testDataService.CreateScriptFile(Path.Combine(v000Directory, $"test_v0_00_TestCsvUnquoted.sql"), _testDataService.CreateBulkTableScript("test_v0_00_TestCsvUnquoted"));
            File.Copy(Path.Combine(Environment.CurrentDirectory, "TestCsvUnquoted.csv"), Path.Combine(v000Directory, "test_v0_00_TestCsvUnquoted.csv"));

            //act - bulk load csv files
            var migrationService = _migrationServiceFactory.Create(_testConfiguration.TargetPlatform);
            migrationService.Initialize(_testConfiguration.ConnectionString);
            migrationService.Run(_testConfiguration.WorkspacePath, "v1.00", autoCreateDatabase: true);

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v0_00_TestCsvUnquoted").ShouldBeTrue();

            var results = _testDataService.GetBulkTestData(_testConfiguration.ConnectionString, "test_v0_00_TestCsvUnquoted");
            var testDataRows = new List<BulkTestDataRow>
            {
                new BulkTestDataRow { FirstName="Jack", LastName ="Poole", BirthDate = new DateTime(1980,1,1) },
                new BulkTestDataRow { FirstName="Diana", LastName ="Churchill", BirthDate = new DateTime(1980,1,1) },
                new BulkTestDataRow { FirstName="Rebecca", LastName ="Lyman", BirthDate = new DateTime(1980,1,1) },
                new BulkTestDataRow { FirstName="Sam", LastName ="Macdonald", BirthDate = new DateTime(1980,1,1) },
                new BulkTestDataRow { FirstName="Matt", LastName ="Paige", BirthDate = new DateTime(1980,1,1) },
            };

            results.Count.ShouldBe(5);
            testDataRows.All(t => results.Exists(r =>
                t.FirstName == r.FirstName
                && t.LastName == r.LastName
                && t.BirthDate == r.BirthDate
            )).ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Bulk_Import_Destination_Table_Does_Not_Exist_Throws_Error()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);

            string v000Directory = Path.Combine(_testConfiguration.WorkspacePath, "v0.00");
            _testDataService.CreateScriptFile(Path.Combine(v000Directory, $"test_v0_00_TestCsvBulkTableOld.sql"), _testDataService.CreateBulkTableScript("test_v0_00_TestCsvBulkTableOld"));

            //we simulate a importing data into TestCsvBulkTable that doesnt exist
            File.Copy(Path.Combine(Environment.CurrentDirectory, "TestCsv.csv"), Path.Combine(v000Directory, "test_v0_00_TestCsvBulkTable.csv"));

            //act - bulk load csv files
            try
            {
                var migrationService = _migrationServiceFactory.Create(_testConfiguration.TargetPlatform);
                migrationService.Initialize(_testConfiguration.ConnectionString);
                migrationService.Run(_testConfiguration.WorkspacePath, "v1.00", autoCreateDatabase: true);
            }
            catch (Exception ex)
            {
                ex.Message.ShouldNotBeEmpty();

                //asset all changes were rolled back
                _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v0_00_TestCsvBulkTableOld").ShouldBeFalse();
                _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v0_00_TestCsvBulkTable").ShouldBeFalse();
            }
        }

        [TestMethod]
        public void Test_Bulk_Import_Mismatch_Columns_But_Nullable()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);

            string v000Directory = Path.Combine(_testConfiguration.WorkspacePath, "v0.00");
            _testDataService.CreateScriptFile(Path.Combine(v000Directory, $"test_v0_00_TestCsvMismatchColumn.sql"), _testDataService.CreateBulkTableScript("test_v0_00_TestCsvMismatchColumn"));
            File.Copy(Path.Combine(Environment.CurrentDirectory, "TestCsvMismatchColumn.csv"), Path.Combine(v000Directory, "test_v0_00_TestCsvMismatchColumn.csv"));

            //act - bulk load csv files
            var migrationService = _migrationServiceFactory.Create(_testConfiguration.TargetPlatform);
            migrationService.Initialize(_testConfiguration.ConnectionString);
            migrationService.Run(_testConfiguration.WorkspacePath, "v1.00", autoCreateDatabase: true);

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v0_00_TestCsvMismatchColumn").ShouldBeTrue();

            var results = _testDataService.GetBulkTestData(_testConfiguration.ConnectionString, "test_v0_00_TestCsvMismatchColumn");
            var testDataRows = new List<BulkTestDataRow>
            {
                new BulkTestDataRow { FirstName="Jack", LastName ="Poole"},
                new BulkTestDataRow { FirstName="Diana", LastName ="Churchill"},
                new BulkTestDataRow { FirstName="Rebecca", LastName ="Lyman" },
                new BulkTestDataRow { FirstName="Sam", LastName ="Macdonald"},
                new BulkTestDataRow { FirstName="Matt", LastName ="Paige"},
            };

            results.Count.ShouldBe(5);
            testDataRows.All(t => results.Exists(r =>
                t.FirstName == r.FirstName
                && t.LastName == r.LastName
                && !r.BirthDate.HasValue
            )).ShouldBeTrue();
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
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);

            string v000Directory = Path.Combine(_testConfiguration.WorkspacePath, "v0.00");
            _testDataService.CreateScriptFile(Path.Combine(v000Directory, $"test_v0_00.sql"), _testDataService.CreateDbObjectScript($"test_v0_00"));
            _testDataService.CreateScriptFile(Path.Combine(v000Directory, $"test_v0_00_TestCsvMismatchColumnNotNullable.sql"), _testDataService.CreateBulkTableScript("test_v0_00_TestCsvMismatchColumnNotNullable"));
            File.Copy(Path.Combine(Environment.CurrentDirectory, "TestCsvMismatchColumnNotNullable.csv"), Path.Combine(v000Directory, "test_v0_00_TestCsvMismatchColumnNotNullable.csv"));

            //act - bulk load csv files
            try
            {
                var migrationService = _migrationServiceFactory.Create(_testConfiguration.TargetPlatform);
                migrationService.Initialize(_testConfiguration.ConnectionString);
                migrationService.Run(_testConfiguration.WorkspacePath, "v1.00", autoCreateDatabase: true);
            }
            catch (Exception ex)
            {
                ex.Message.ShouldNotBeEmpty();

                //asset all changes were rolled back
                _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v0_00").ShouldBeFalse();
                _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v0_00_TestCsvMismatchColumnNotNullable").ShouldBeFalse();
            }
        }

    }
}
