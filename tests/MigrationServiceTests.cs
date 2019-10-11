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
            var migrationService = new MigrationService();
            Assert.ThrowsException<SqlException>(() =>
            {
                migrationService.Run(workingPath, connectionString, null, autoCreateDatabase: false);
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
            var migrationService = new MigrationService();
            migrationService.Run(workingPath, connectionString, "v1.01", autoCreateDatabase: true);

            //assert
            migrationService.IsTargetDatabaseExists(new SqlConnectionStringBuilder(connectionString), databaseName);
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
            var migrationService = new MigrationService();
            migrationService.Run(workingPath, connectionString, "v1.01", autoCreateDatabase: true);
            var versions = GetAllDbVersions(connectionString);
            versions.Count.ShouldBe(3);
            versions[0].Version.ShouldBe("v0.00");
            versions[1].Version.ShouldBe("v1.00");
            versions[2].Version.ShouldBe("v1.01");

            migrationService.Run(workingPath, connectionString, "v1.01", autoCreateDatabase: true);
            migrationService.Run(workingPath, connectionString, "v1.01", autoCreateDatabase: true);
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
            var migrationService = new MigrationService();
            migrationService.Run(workingPath, connectionString, "v1.00", autoCreateDatabase: true);

            //assert
            string sqlAssertStatement = $"SELECT ISNULL(OBJECT_ID('[dbo].[test_{scriptFolder}]'), 0) AS ObjectID";
            DbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), sqlAssertStatement).ShouldBeTrue();
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
            var migrationService = new MigrationService();
            migrationService.Run(workingPath, connectionString, "v1.02", autoCreateDatabase: true);

            //assert
            DbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v1_00")).ShouldBeTrue();
            DbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v1_01")).ShouldBeTrue();
            DbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v1_02")).ShouldBeTrue();
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
            var migrationService = new MigrationService();
            migrationService.Run(workingPath, connectionString, "v1.01", autoCreateDatabase: true);

            //assert
            DbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v1_00")).ShouldBeTrue();
            DbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v1_01")).ShouldBeTrue();

            //act
            TestHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.00"), $"test_v1_00_added_later.sql"), TestHelper.CreateScript($"test_v1_00_added_later"));
            TestHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.01"), $"test_v1_01_added_later.sql"), TestHelper.CreateScript($"test_v1_01_added_later"));

            localVersionService.IncrementMinorVersion(workingPath, null);
            TestHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.02"), $"test_v1_02.sql"), TestHelper.CreateScript($"test_v1_02"));

            migrationService.Run(workingPath, connectionString, "v1.02", autoCreateDatabase: true);

            //assert again
            DbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v1_00_added_later")).ShouldBeFalse();
            DbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v1_01_added_later")).ShouldBeFalse();
            DbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v1_02")).ShouldBeTrue();
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
            var migrationService = new MigrationService();
            migrationService.Run(workingPath, connectionString, "v1.01", autoCreateDatabase: true);

            //assert
            DbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v1_00")).ShouldBeTrue();
            DbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v1_01")).ShouldBeTrue();
            DbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v1_02")).ShouldBeFalse();
            DbHelper.QuerySingleBool(new SqlConnectionStringBuilder(connectionString), TestHelper.CreateAssetScript("test_v1_03")).ShouldBeFalse();
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
