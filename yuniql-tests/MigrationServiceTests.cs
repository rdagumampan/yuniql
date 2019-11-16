using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;
using System.IO;
using Shouldly;
using System.Collections.Generic;
using System;
using System.Data;
using Yuniql.Core;
using Yuniql.Extensibility;

namespace Yuniql.SqlServer.Tests
{
    [TestClass]
    public class MigrationServiceTests
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
        public void Test_Run_Without_AutocreateDB_Throws_Exception()
        {
            //arrange
            var workingPath = TestDbHelper.GetWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = TestDbHelper.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);

            //act and assert
            Assert.ThrowsException<SqlException>(() =>
            {
                var migrationService = _migrationServiceFactory.Create("sqlserver");
                migrationService.Initialize(connectionString);
                migrationService.Run(workingPath, null, autoCreateDatabase: false);
            }).Message.Contains($"Cannot open database \"{databaseName}\"").ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_With_AutocreateDB()
        {
            //arrange
            var workingPath = TestDbHelper.GetWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = TestDbHelper.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);
            localVersionService.IncrementMajorVersion(workingPath, null);
            localVersionService.IncrementMinorVersion(workingPath, null);

            //act
            var migrationService = _migrationServiceFactory.Create("sqlserver");
            migrationService.Initialize(connectionString);
            migrationService.Run(workingPath, "v1.01", autoCreateDatabase: true);

            //assert
            TestDbHelper.CheckDbExist(connectionString).ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_Database_Already_Updated()
        {
            //arrange
            var workingPath = TestDbHelper.GetWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = TestDbHelper.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);
            localVersionService.IncrementMajorVersion(workingPath, null);
            localVersionService.IncrementMinorVersion(workingPath, null);

            //act
            var migrationService = _migrationServiceFactory.Create("sqlserver");
            migrationService.Initialize(connectionString);
            migrationService.Run(workingPath, "v1.01", autoCreateDatabase: true);
            var versions = TestDbHelper.GetAllDbVersions(connectionString);

            versions.Count.ShouldBe(3);
            versions[0].Version.ShouldBe("v0.00");
            versions[1].Version.ShouldBe("v1.00");
            versions[2].Version.ShouldBe("v1.01");

            migrationService.Run(workingPath, "v1.01", autoCreateDatabase: true);
            migrationService.Run(workingPath, "v1.01", autoCreateDatabase: true);
            versions.Count.ShouldBe(3);
            versions[0].Version.ShouldBe("v0.00");
            versions[1].Version.ShouldBe("v1.00");
            versions[2].Version.ShouldBe("v1.01");
        }

        [DataTestMethod()]
        [DataRow("_init")]
        [DataRow("_pre")]
        [DataRow("_post")]
        [DataRow("_draft")]
        public void Test_Run_Init_Scripts_Executed(string scriptFolder)
        {
            //arrange
            var workingPath = TestDbHelper.GetWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = TestDbHelper.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);
            TestDbHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, scriptFolder), $"test_{scriptFolder}.sql"), TestDbHelper.CreateDbObjectScript($"test_{scriptFolder}"));

            //act
            var migrationService = _migrationServiceFactory.Create("sqlserver");
            migrationService.Initialize(connectionString);
            migrationService.Run(workingPath, "v1.00", autoCreateDatabase: true);

            //assert
            string sqlAssertStatement = $"SELECT ISNULL(OBJECT_ID('[dbo].[test_{scriptFolder}]'), 0) AS ObjectID";
            TestDbHelper.QuerySingleBool(connectionString, sqlAssertStatement).ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_All_Version_Scripts_Executed()
        {
            //arrange
            var workingPath = TestDbHelper.GetWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = TestDbHelper.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);

            localVersionService.IncrementMajorVersion(workingPath, null);
            TestDbHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.00"), $"test_v1_00.sql"), TestDbHelper.CreateDbObjectScript($"test_v1_00"));

            localVersionService.IncrementMinorVersion(workingPath, null);
            TestDbHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.01"), $"test_v1_01.sql"), TestDbHelper.CreateDbObjectScript($"test_v1_01"));

            localVersionService.IncrementMinorVersion(workingPath, null);
            TestDbHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.02"), $"test_v1_02.sql"), TestDbHelper.CreateDbObjectScript($"test_v1_02"));

            //act
            var migrationService = _migrationServiceFactory.Create("sqlserver");
            migrationService.Initialize(connectionString);
            migrationService.Run(workingPath, "v1.02", autoCreateDatabase: true);

            //assert
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript("test_v1_00")).ShouldBeTrue();
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript("test_v1_01")).ShouldBeTrue();
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript("test_v1_02")).ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_Skipped_Versions_Lower_Or_Same_As_Latest()
        {
            //arrange
            var workingPath = TestDbHelper.GetWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = TestDbHelper.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);

            localVersionService.IncrementMajorVersion(workingPath, null);
            TestDbHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.00"), $"test_v1_00.sql"), TestDbHelper.CreateDbObjectScript($"test_v1_00"));

            localVersionService.IncrementMinorVersion(workingPath, null);
            TestDbHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.01"), $"test_v1_01.sql"), TestDbHelper.CreateDbObjectScript($"test_v1_01"));

            //act
            var migrationService = _migrationServiceFactory.Create("sqlserver");
            migrationService.Initialize(connectionString);
            migrationService.Run(workingPath, "v1.01", autoCreateDatabase: true);

            //assert
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript("test_v1_00")).ShouldBeTrue();
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript("test_v1_01")).ShouldBeTrue();

            //act
            TestDbHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.00"), $"test_v1_00_added_later.sql"), TestDbHelper.CreateDbObjectScript($"test_v1_00_added_later"));
            TestDbHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.01"), $"test_v1_01_added_later.sql"), TestDbHelper.CreateDbObjectScript($"test_v1_01_added_later"));

            localVersionService.IncrementMinorVersion(workingPath, null);
            TestDbHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.02"), $"test_v1_02.sql"), TestDbHelper.CreateDbObjectScript($"test_v1_02"));

            migrationService.Run(workingPath, "v1.02", autoCreateDatabase: true);

            //assert again
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript("test_v1_00_added_later")).ShouldBeFalse();
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript("test_v1_01_added_later")).ShouldBeFalse();
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript("test_v1_02")).ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_With_Target_Version_Skipped_Versions_Higher_Than_Target_Version()
        {
            //arrange
            var workingPath = TestDbHelper.GetWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = TestDbHelper.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);

            localVersionService.IncrementMajorVersion(workingPath, null);
            TestDbHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.00"), $"test_v1_00.sql"), TestDbHelper.CreateDbObjectScript($"test_v1_00"));

            localVersionService.IncrementMinorVersion(workingPath, null);
            TestDbHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.01"), $"test_v1_01.sql"), TestDbHelper.CreateDbObjectScript($"test_v1_01"));

            localVersionService.IncrementMinorVersion(workingPath, null);
            TestDbHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.02"), $"test_v1_02.sql"), TestDbHelper.CreateDbObjectScript($"test_v1_02"));

            localVersionService.IncrementMajorVersion(workingPath, null);
            TestDbHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v2.00"), $"test_v2_00.sql"), TestDbHelper.CreateDbObjectScript($"test_v2_00"));

            //act
            var migrationService = _migrationServiceFactory.Create("sqlserver");
            migrationService.Initialize(connectionString);
            migrationService.Run(workingPath, "v1.01", autoCreateDatabase: true);

            //assert
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript("test_v1_00")).ShouldBeTrue();
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript("test_v1_01")).ShouldBeTrue();
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript("test_v1_02")).ShouldBeFalse();
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript("test_v1_03")).ShouldBeFalse();
        }

        [DataTestMethod()]
        [DataRow("_init", "_init")]
        [DataRow("_pre", "_pre")]
        [DataRow("v1.00", "v1_00")]
        [DataRow("_post", "_post")]
        [DataRow("_draft", "_draft")]
        public void Test_Run_With_Parameterized_Tokens(string versionFolder, string scriptName)
        {
            //arrange
            var workingPath = TestDbHelper.GetWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = TestDbHelper.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);

            localVersionService.IncrementMajorVersion(workingPath, null);
            TestDbHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, versionFolder), $"test_{scriptName}.sql"), TestDbHelper.CreateTokenizedDbObjectScript($"test_{scriptName}"));

            //act
            var migrationService = _migrationServiceFactory.Create("sqlserver");
            migrationService.Initialize(connectionString);
            List<KeyValuePair<string, string>> tokens = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Token1","Token1Value"),
                new KeyValuePair<string, string>("Token2","Token2Value"),
                new KeyValuePair<string, string>("Token3","Token3Value"),
            };
            migrationService.Run(workingPath, "v1.00", autoCreateDatabase: true, tokens: tokens);

            //assert
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript($"test_{scriptName}_Token1Value_Token2Value_Token3Value")).ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_All_Version_SubDirectories_Executed()
        {
            //arrange
            var workingPath = TestDbHelper.GetWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = TestDbHelper.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);

            localVersionService.IncrementMajorVersion(workingPath, null);
            string v1rootDirectory = Path.Combine(Path.Combine(workingPath, "v1.00"));
            TestDbHelper.CreateScriptFile(Path.Combine(v1rootDirectory, $"test_v1_00.sql"), TestDbHelper.CreateDbObjectScript($"test_v1_00")); ;

            string v1level1Directory = Path.Combine(v1rootDirectory, "v1.00-level1");
            Directory.CreateDirectory(v1level1Directory);
            TestDbHelper.CreateScriptFile(Path.Combine(v1level1Directory, $"test_v1_00_level1.sql"), TestDbHelper.CreateDbObjectScript($"test_v1_00_level1"));

            string v1level1SubDirectory = Path.Combine(v1level1Directory, "v1.00-level1-sublevel1");
            Directory.CreateDirectory(v1level1SubDirectory);
            TestDbHelper.CreateScriptFile(Path.Combine(v1level1SubDirectory, $"test_v1_00_level1_sublevel1.sql"), TestDbHelper.CreateDbObjectScript($"test_v1_00_level1_sublevel1"));

            localVersionService.IncrementMajorVersion(workingPath, null);
            string v2rootDirectory = Path.Combine(Path.Combine(workingPath, "v2.00"));
            TestDbHelper.CreateScriptFile(Path.Combine(v2rootDirectory, $"test_v2_00.sql"), TestDbHelper.CreateDbObjectScript($"test_v2_00")); ;

            string v2level1Directory = Path.Combine(v2rootDirectory, "v2.00-level1");
            Directory.CreateDirectory(v2level1Directory);
            TestDbHelper.CreateScriptFile(Path.Combine(v2level1Directory, $"test_v2_00_level1.sql"), TestDbHelper.CreateDbObjectScript($"test_v2_00_level1"));

            string v2level1SubDirectory = Path.Combine(v2level1Directory, "v2.00-level1-sublevel1");
            Directory.CreateDirectory(v2level1SubDirectory);
            TestDbHelper.CreateScriptFile(Path.Combine(v2level1SubDirectory, $"test_v2_00_level1_sublevel1.sql"), TestDbHelper.CreateDbObjectScript($"test_v2_00_level1_sublevel1"));

            //act
            var migrationService = _migrationServiceFactory.Create("sqlserver");
            migrationService.Initialize(connectionString);
            migrationService.Run(workingPath, "v2.00", autoCreateDatabase: true);

            //assert
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript("test_v1_00")).ShouldBeTrue();
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript("test_v1_00_level1")).ShouldBeTrue();
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript("test_v1_00_level1_sublevel1")).ShouldBeTrue();

            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript("test_v2_00")).ShouldBeTrue();
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript("test_v2_00_level1")).ShouldBeTrue();
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript("test_v2_00_level1_sublevel1")).ShouldBeTrue();
        }
        
        [TestMethod]
        public void Test_Run_Migration_Throws_Error_Must_Rollback_All_Changes()
        {
            //arrange
            var workingPath = TestDbHelper.GetWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = TestDbHelper.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);

            localVersionService.IncrementMajorVersion(workingPath, null);
            TestDbHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.00"), $"test_v1_00.sql"), TestDbHelper.CreateDbObjectScript($"test_v1_00"));
            TestDbHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.00"), $"test_v1_00_TestCsv.sql"), TestDbHelper.CreateBulkTableScript("TestCsv"));
            File.Copy(Path.Combine(Environment.CurrentDirectory, "TestCsv.csv"), Path.Combine(Path.Combine(workingPath, "v1.00"), "TestCsvDifferentName.csv"));

            //act
            Assert.ThrowsException<InvalidOperationException>(() => {
                var migrationService = _migrationServiceFactory.Create("sqlserver");
                migrationService.Initialize(connectionString);
                migrationService.Run(workingPath, "v1.00", autoCreateDatabase: true);
            }).Message.ShouldContain("Cannot access destination table 'TestCsvDifferentName'");

            //assert
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript("test_v1_00")).ShouldBeFalse();
        }

        [TestMethod]
        public void Test_Verify()
        {
            //arrange
            var workingPath = TestDbHelper.GetWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = TestDbHelper.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);

            localVersionService.IncrementMajorVersion(workingPath, null);
            TestDbHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.00"), $"test_v1_00.sql"), TestDbHelper.CreateDbObjectScript($"test_v1_00"));

            localVersionService.IncrementMinorVersion(workingPath, null);
            TestDbHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.01"), $"test_v1_01.sql"), TestDbHelper.CreateDbObjectScript($"test_v1_01"));

            var migrationService = _migrationServiceFactory.Create("sqlserver");
            migrationService.Initialize(connectionString);
            migrationService.Run(workingPath, "v1.01", autoCreateDatabase: true);

            localVersionService.IncrementMinorVersion(workingPath, null);
            TestDbHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.02"), $"test_v1_02.sql"), TestDbHelper.CreateDbObjectScript($"test_v1_02"));

            //act
            migrationService.Run(workingPath, "v1.02", autoCreateDatabase: false, verifyOnly: true);

            //assert
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript("test_v1_00")).ShouldBeTrue();
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript("test_v1_01")).ShouldBeTrue();
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript("test_v1_02")).ShouldBeFalse();
        }

        [TestMethod]
        public void Test_Erase()
        {
            //arrange
            var workingPath = TestDbHelper.GetWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = TestDbHelper.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);

            localVersionService.IncrementMajorVersion(workingPath, null);
            TestDbHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.00"), $"script1.sql"), TestDbHelper.CreateDbObjectScript($"script1"));
            TestDbHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.00"), $"script2.sql"), TestDbHelper.CreateDbObjectScript($"script2"));
            TestDbHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.00"), $"script3.sql"), TestDbHelper.CreateDbObjectScript($"script3"));

            //act
            var migrationService = _migrationServiceFactory.Create("sqlserver");
            migrationService.Initialize(connectionString);
            migrationService.Run(workingPath, "v1.00", autoCreateDatabase: true);

            //assert
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript("script1")).ShouldBeTrue();
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript("script2")).ShouldBeTrue();
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript("script3")).ShouldBeTrue();

            //arrange
            TestDbHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "_erase"), $"erase.sql"), TestDbHelper.CreateCleanupScript());

            //act
            migrationService.Erase(workingPath);

            //assert
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript("script1")).ShouldBeFalse();
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript("script2")).ShouldBeFalse();
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript("script3")).ShouldBeFalse();
        }

        [TestMethod]
        public void Test_Run_Migration_Unsupported_Platform_Throws_Exception()
        {
            //arrange
            var workingPath = TestDbHelper.GetWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = TestDbHelper.GetConnectionString(databaseName);

            //act
            Assert.ThrowsException<NotSupportedException>(() => {
                var migrationService = _migrationServiceFactory.Create("oracle");
                migrationService.Initialize(connectionString);
                migrationService.Run(workingPath, "v1.00", autoCreateDatabase: true);
            }).Message.ShouldContain($"The target database platform oracle is not yet supported");
        }
    }
}
