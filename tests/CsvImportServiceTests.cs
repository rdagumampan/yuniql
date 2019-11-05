using ArdiLabs.Yuniql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;
using System.IO;
using Shouldly;
using System;
using ArdiLabs.Yuniql.SqlServer;

namespace Yuniql.Tests
{
    [TestClass]
    public class CsvImportServiceTests
    {
        [TestInitialize]
        public void Setup()
        {
            var workingPath = TestHelper.GetWorkingPath();
            if (!Directory.Exists(workingPath))
            {
                Directory.CreateDirectory(workingPath);
            }
        }

        [TestMethod]
        public void TestImport()
        {
            //arrange
            var workingPath = TestHelper.GetWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = TestHelper.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService();
            localVersionService.Init(workingPath);

            localVersionService.IncrementMajorVersion(workingPath, null);
            string v100Directory = Path.Combine(workingPath, "v1.00");
            TestHelper.CreateScriptFile(Path.Combine(v100Directory, $"test_v1_00.sql"), TestHelper.CreateScript($"test_v1_00"));

            localVersionService.IncrementMinorVersion(workingPath, null);
            string v101Directory = Path.Combine(workingPath, "v1.01");
            TestHelper.CreateScriptFile(Path.Combine(v101Directory, $"test_v1_01.sql"), TestHelper.CreateScript($"test_v1_01"));

            //act
            var csvImportService = new SqlServerCsvImportService();
            var dataService = new SqlServerDataService(connectionString);
            var migrationService = new MigrationService(connectionString, dataService, csvImportService);
            migrationService.Run(workingPath, "v1.01", autoCreateDatabase: true);

            //assert
            TestDbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v1_00")).ShouldBeTrue();
            TestDbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v1_01")).ShouldBeTrue();

            //arrange - add new version with csv files
            localVersionService.IncrementMinorVersion(workingPath, null);
            string v102Directory = Path.Combine(workingPath, "v1.02");
            TestHelper.CreateScriptFile(Path.Combine(v102Directory, $"test_v1_02.sql"), TestHelper.CreateScript($"test_v1_02"));

            TestHelper.CreateScriptFile(Path.Combine(v102Directory, $"test_v1_02_TestCsv.sql"), TestHelper.CreateCsvTableScript("TestCsv"));
            File.Copy(Path.Combine(Environment.CurrentDirectory, "TestCsv.csv"), Path.Combine(v102Directory, "TestCsv.csv"));

            //act - bulk load csv files
            migrationService.Run(workingPath, "v1.02", autoCreateDatabase: true);

            //assert
            TestDbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v1_02")).ShouldBeTrue();
            TestDbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("TestCsv")).ShouldBeTrue();
        }
    }
}
