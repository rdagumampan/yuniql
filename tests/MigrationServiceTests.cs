using ArdiLabs.Yuniql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;
using System.IO;
using Shouldly;
using System.Collections.Generic;
using System;
using System.Data;

namespace Yuniql.Tests
{
    [TestClass]
    public class MigrationServiceTests
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
        public void Test_Run_Without_AutocreateDB_Throws_Exception()
        {
            //arrange
            var workingPath = TestHelper.GetWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = TestHelper.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService();
            localVersionService.Init(workingPath);

            //act and assert
            Assert.ThrowsException<SqlException>(() =>
            {
                var migrationService = new MigrationService(connectionString);
                migrationService.Run(workingPath, null, autoCreateDatabase: false);
            }).Message.Contains($"Cannot open database \"{databaseName}\"").ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_With_AutocreateDB()
        {
            //arrange
            var workingPath = TestHelper.GetWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = TestHelper.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService();
            localVersionService.Init(workingPath);
            localVersionService.IncrementMajorVersion(workingPath, null);
            localVersionService.IncrementMinorVersion(workingPath, null);

            //act
            var migrationService = new MigrationService(connectionString);
            migrationService.Run(workingPath, "v1.01", autoCreateDatabase: true);

            //assert
            var sqlStatement = $"SELECT ISNULL(DB_ID (N'{databaseName}'),0);";

            //check if database exists and auto-create when its not
            var masterSqlDbConnectionString = new SqlConnectionStringBuilder(connectionString);
            masterSqlDbConnectionString.InitialCatalog = "master";

            TestDbHelper.QuerySingleBool(masterSqlDbConnectionString, sqlStatement).ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_Database_Already_Updated()
        {
            //arrange
            var workingPath = TestHelper.GetWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = TestHelper.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService();
            localVersionService.Init(workingPath);
            localVersionService.IncrementMajorVersion(workingPath, null);
            localVersionService.IncrementMinorVersion(workingPath, null);

            //act
            var migrationService = new MigrationService(connectionString);
            migrationService.Run(workingPath, "v1.01", autoCreateDatabase: true);
            var versions = GetAllDbVersions(connectionString);
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
        public void Test_Run_Database_Init_Scripts_Executed(string scriptFolder)
        {
            //arrange
            var workingPath = TestHelper.GetWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = TestHelper.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService();
            localVersionService.Init(workingPath);
            TestHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, scriptFolder), $"test_{scriptFolder}.sql"), TestHelper.CreateScript($"test_{scriptFolder}"));

            //act
            var migrationService = new MigrationService(connectionString);
            migrationService.Run(workingPath, "v1.00", autoCreateDatabase: true);

            //assert
            string sqlAssertStatement = $"SELECT ISNULL(OBJECT_ID('[dbo].[test_{scriptFolder}]'), 0) AS ObjectID";
            TestDbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), sqlAssertStatement).ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_Database_Executed_All_Versions()
        {
            //arrange
            var workingPath = TestHelper.GetWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = TestHelper.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService();
            localVersionService.Init(workingPath);

            localVersionService.IncrementMajorVersion(workingPath, null);
            TestHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.00"), $"test_v1_00.sql"), TestHelper.CreateScript($"test_v1_00"));

            localVersionService.IncrementMinorVersion(workingPath, null);
            TestHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.01"), $"test_v1_01.sql"), TestHelper.CreateScript($"test_v1_01"));

            localVersionService.IncrementMinorVersion(workingPath, null);
            TestHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.02"), $"test_v1_02.sql"), TestHelper.CreateScript($"test_v1_02"));

            //act
            var migrationService = new MigrationService(connectionString);
            migrationService.Run(workingPath, "v1.02", autoCreateDatabase: true);

            //assert
            TestDbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v1_00")).ShouldBeTrue();
            TestDbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v1_01")).ShouldBeTrue();
            TestDbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v1_02")).ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_Database_Skipped_Versions_Lower_Or_Same_As_Latest()
        {
            //arrange
            var workingPath = TestHelper.GetWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = TestHelper.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService();
            localVersionService.Init(workingPath);

            localVersionService.IncrementMajorVersion(workingPath, null);
            TestHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.00"), $"test_v1_00.sql"), TestHelper.CreateScript($"test_v1_00"));

            localVersionService.IncrementMinorVersion(workingPath, null);
            TestHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.01"), $"test_v1_01.sql"), TestHelper.CreateScript($"test_v1_01"));

            //act
            var migrationService = new MigrationService(connectionString);
            migrationService.Run(workingPath, "v1.01", autoCreateDatabase: true);

            //assert
            TestDbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v1_00")).ShouldBeTrue();
            TestDbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v1_01")).ShouldBeTrue();

            //act
            TestHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.00"), $"test_v1_00_added_later.sql"), TestHelper.CreateScript($"test_v1_00_added_later"));
            TestHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.01"), $"test_v1_01_added_later.sql"), TestHelper.CreateScript($"test_v1_01_added_later"));

            localVersionService.IncrementMinorVersion(workingPath, null);
            TestHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.02"), $"test_v1_02.sql"), TestHelper.CreateScript($"test_v1_02"));

            migrationService.Run(workingPath, "v1.02", autoCreateDatabase: true);

            //assert again
            TestDbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v1_00_added_later")).ShouldBeFalse();
            TestDbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v1_01_added_later")).ShouldBeFalse();
            TestDbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v1_02")).ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_Database_Skipped_Versions_Higher_Than_Target_Version()
        {
            //arrange
            var workingPath = TestHelper.GetWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = TestHelper.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService();
            localVersionService.Init(workingPath);

            localVersionService.IncrementMajorVersion(workingPath, null);
            TestHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.00"), $"test_v1_00.sql"), TestHelper.CreateScript($"test_v1_00"));

            localVersionService.IncrementMinorVersion(workingPath, null);
            TestHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.01"), $"test_v1_01.sql"), TestHelper.CreateScript($"test_v1_01"));

            localVersionService.IncrementMinorVersion(workingPath, null);
            TestHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.02"), $"test_v1_02.sql"), TestHelper.CreateScript($"test_v1_02"));

            localVersionService.IncrementMajorVersion(workingPath, null);
            TestHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v2.00"), $"test_v2_00.sql"), TestHelper.CreateScript($"test_v2_00"));

            //act
            var migrationService = new MigrationService(connectionString);
            migrationService.Run(workingPath, "v1.01", autoCreateDatabase: true);

            //assert
            TestDbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v1_00")).ShouldBeTrue();
            TestDbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v1_01")).ShouldBeTrue();
            TestDbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v1_02")).ShouldBeFalse();
            TestDbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v1_03")).ShouldBeFalse();
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
            var workingPath = TestHelper.GetWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = TestHelper.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService();
            localVersionService.Init(workingPath);

            localVersionService.IncrementMajorVersion(workingPath, null);
            TestHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, versionFolder), $"test_{scriptName}.sql"), TestHelper.CreateTokenizedScript($"test_{scriptName}"));

            //act
            var migrationService = new MigrationService(connectionString);
            List<KeyValuePair<string, string>> tokens = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Token1","Token1Value"),
                new KeyValuePair<string, string>("Token2","Token2Value"),
                new KeyValuePair<string, string>("Token3","Token3Value"),
            };
            migrationService.Run(workingPath, "v1.00", autoCreateDatabase: true,  tokens: tokens);

            //assert
            TestDbHelper.QuerySingleString(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateSpHelpTextScript($"test_{scriptName}").TrimEnd()).Contains("Token1Value.Token2Value.Token3Value");
        }

        [TestMethod]
        public void Test_Run_Database_Executed_All_Version_SubDirectories()
        {
            //arrange
            var workingPath = TestHelper.GetWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = TestHelper.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService();
            localVersionService.Init(workingPath);

            localVersionService.IncrementMajorVersion(workingPath, null);
            string v1rootDirectory = Path.Combine(Path.Combine(workingPath, "v1.00"));
            TestHelper.CreateScriptFile(Path.Combine(v1rootDirectory, $"test_v1_00.sql"), TestHelper.CreateScript($"test_v1_00")); ;

            string v1level1Directory = Path.Combine(v1rootDirectory, "v1.00-level1");
            Directory.CreateDirectory(v1level1Directory);
            TestHelper.CreateScriptFile(Path.Combine(v1level1Directory, $"test_v1_00_level1.sql"), TestHelper.CreateScript($"test_v1_00_level1"));

            string v1level1SubDirectory = Path.Combine(v1level1Directory, "v1.00-level1-sublevel1");
            Directory.CreateDirectory(v1level1SubDirectory);
            TestHelper.CreateScriptFile(Path.Combine(v1level1SubDirectory, $"test_v1_00_level1_sublevel1.sql"), TestHelper.CreateScript($"test_v1_00_level1_sublevel1"));

            localVersionService.IncrementMajorVersion(workingPath, null);
            string v2rootDirectory = Path.Combine(Path.Combine(workingPath, "v2.00"));
            TestHelper.CreateScriptFile(Path.Combine(v2rootDirectory, $"test_v2_00.sql"), TestHelper.CreateScript($"test_v2_00")); ;

            string v2level1Directory = Path.Combine(v2rootDirectory, "v2.00-level1");
            Directory.CreateDirectory(v2level1Directory);
            TestHelper.CreateScriptFile(Path.Combine(v2level1Directory, $"test_v2_00_level1.sql"), TestHelper.CreateScript($"test_v2_00_level1"));

            string v2level1SubDirectory = Path.Combine(v2level1Directory, "v2.00-level1-sublevel1");
            Directory.CreateDirectory(v2level1SubDirectory);
            TestHelper.CreateScriptFile(Path.Combine(v2level1SubDirectory, $"test_v2_00_level1_sublevel1.sql"), TestHelper.CreateScript($"test_v2_00_level1_sublevel1"));

            //act
            var migrationService = new MigrationService(connectionString);
            migrationService.Run(workingPath, "v2.00", autoCreateDatabase: true);

            //assert
            TestDbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v1_00")).ShouldBeTrue();
            TestDbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v1_00_level1")).ShouldBeTrue();
            TestDbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v1_00_level1_sublevel1")).ShouldBeTrue();

            TestDbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v2_00")).ShouldBeTrue();
            TestDbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v2_00_level1")).ShouldBeTrue();
            TestDbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v2_00_level1_sublevel1")).ShouldBeTrue();
        }


        [TestMethod]
        public void Test_Run_Database_Throws_Error_Must_Rollback()
        {
            //arrange
            var workingPath = TestHelper.GetWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = TestHelper.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService();
            localVersionService.Init(workingPath);

            localVersionService.IncrementMajorVersion(workingPath, null);
            TestHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.00"), $"test_v1_00.sql"), TestHelper.CreateScript($"test_v1_00"));
            TestHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.00"), $"test_v1_00_TestCsv.sql"), TestHelper.CreateCsvTableScript("TestCsv"));
            File.Copy(Path.Combine(Environment.CurrentDirectory, "TestCsv.csv"), Path.Combine(Path.Combine(workingPath, "v1.00"), "TestCsvDifferentName.csv"));

            //act
            Assert.ThrowsException<InvalidOperationException>(() => {
                var migrationService = new MigrationService(connectionString);
                migrationService.Run(workingPath, "v1.00", autoCreateDatabase: true);
            }).Message.ShouldContain("Cannot access destination table 'TestCsvDifferentName'");

            //assert
            TestDbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v1_00")).ShouldBeFalse();
        }

        private List<DbVersion> GetAllDbVersions(string sqlConnectionString)
        {
            var result = new List<DbVersion>();

            var sqlStatement = $"SELECT Id, Version, DateInsertedUtc, LastUserId FROM [dbo].[__YuniqlDbVersion] ORDER BY Version ASC;";

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlStatement;
                command.CommandTimeout = 0;

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var dbVersion = new DbVersion
                    {
                        Id = reader.GetInt16(0),
                        Version = reader.GetString(1),
                        DateInsertedUtc = reader.GetDateTime(2),
                        LastUserId = reader.GetString(3)
                    };
                    result.Add(dbVersion);
                }
            }
            return result;
        }
    }
}
