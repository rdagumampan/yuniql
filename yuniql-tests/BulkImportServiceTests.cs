using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;
using System.IO;
using Shouldly;
using System;
using Yuniql.Core;
using Yuniql.Extensibility;

namespace Yuniql.SqlServer.Tests
{
    [TestClass]
    public class BulkImportServiceTests
    {
        private IMigrationServiceFactory _migrationServiceFactory;
        private ITraceService _traceService;

        [TestInitialize]
        public void Setup()
        {
            _traceService = new TraceService();
            _migrationServiceFactory = new MigrationServiceFactory(_traceService);

            var workingPath = TestDbHelper.GetWorkingPath();
            if (!Directory.Exists(workingPath))
            {
                Directory.CreateDirectory(workingPath);
            }
        }

        [TestMethod]
        public void TestImport()
        {
            //arrange
            var workingPath = TestDbHelper.GetWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = TestDbHelper.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);

            localVersionService.IncrementMajorVersion(workingPath, null);
            string v100Directory = Path.Combine(workingPath, "v1.00");
            TestDbHelper.CreateScriptFile(Path.Combine(v100Directory, $"test_v1_00.sql"), TestDbHelper.CreateScript($"test_v1_00"));

            localVersionService.IncrementMinorVersion(workingPath, null);
            string v101Directory = Path.Combine(workingPath, "v1.01");
            TestDbHelper.CreateScriptFile(Path.Combine(v101Directory, $"test_v1_01.sql"), TestDbHelper.CreateScript($"test_v1_01"));

            //act
            var migrationService = _migrationServiceFactory.Create("sqlserver");
            migrationService.Initialize(connectionString);
            migrationService.Run(workingPath, "v1.01", autoCreateDatabase: true);

            //assert
            TestDbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestDbHelper.CreateCheckObjectExistScript("test_v1_00")).ShouldBeTrue();
            TestDbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestDbHelper.CreateCheckObjectExistScript("test_v1_01")).ShouldBeTrue();

            //arrange - add new version with csv files
            localVersionService.IncrementMinorVersion(workingPath, null);
            string v102Directory = Path.Combine(workingPath, "v1.02");
            TestDbHelper.CreateScriptFile(Path.Combine(v102Directory, $"test_v1_02.sql"), TestDbHelper.CreateScript($"test_v1_02"));

            TestDbHelper.CreateScriptFile(Path.Combine(v102Directory, $"test_v1_02_TestCsv.sql"), TestDbHelper.CreateCsvTableScript("TestCsv"));
            File.Copy(Path.Combine(Environment.CurrentDirectory, "TestCsv.csv"), Path.Combine(v102Directory, "TestCsv.csv"));

            //act - bulk load csv files
            migrationService.Run(workingPath, "v1.02", autoCreateDatabase: true);

            //assert
            TestDbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestDbHelper.CreateCheckObjectExistScript("test_v1_02")).ShouldBeTrue();
            TestDbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestDbHelper.CreateCheckObjectExistScript("TestCsv")).ShouldBeTrue();
        }
    }
}
