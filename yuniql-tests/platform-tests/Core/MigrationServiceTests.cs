using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Shouldly;
using System.Collections.Generic;
using System;
using System.Data;
using Yuniql.Core;
using Yuniql.Extensibility;

namespace Yuniql.PlatformTests
{

    //https://docs.microsoft.com/en-gb/dotnet/standard/assembly/unloadability
    //https://github.com/dotnet/samples/blob/master/core/extensions/AppWithPlugin/AppWithPlugin/Program.cs
    [TestClass]
    public class MigrationServiceTests : TestBase
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
            _traceService = new FileTraceService();
            _migrationServiceFactory = new MigrationServiceFactory(_traceService);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_testConfiguration.WorkspacePath))
                Directory.Delete(_testConfiguration.WorkspacePath, true);
        }

        [TestMethod]
        public void Test_Run_Without_AutocreateDB_Throws_Exception()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);

            //act and assert
            try
            {
                var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
                migrationService.Initialize(_testConfiguration.ConnectionString);
                migrationService.Run(_testConfiguration.WorkspacePath, null, autoCreateDatabase: false);
            }
            catch (Exception ex)
            {
                //used try/catch this instead of Assert.ThrowsException because different vendors
                //throws different exception type and message content
                ex.Message.ShouldNotBeNullOrEmpty();
            }
        }

        [TestMethod]
        public void Test_Run_With_AutocreateDB()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);
            localVersionService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            localVersionService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);

            //act
            var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
            migrationService.Initialize(_testConfiguration.ConnectionString);
            migrationService.Run(_testConfiguration.WorkspacePath, "v1.01", autoCreateDatabase: true);

            //assert
            _testDataService.CheckIfDbExist(_testConfiguration.ConnectionString).ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_Database_Already_Updated()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);
            localVersionService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            localVersionService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);

            //act
            var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
            migrationService.Initialize(_testConfiguration.ConnectionString);
            migrationService.Run(_testConfiguration.WorkspacePath, "v1.01", autoCreateDatabase: true);
            var versions = _testDataService.GetAllDbVersions(_testConfiguration.ConnectionString);

            versions.Count.ShouldBe(3);
            versions[0].Version.ShouldBe("v0.00");
            versions[1].Version.ShouldBe("v1.00");
            versions[2].Version.ShouldBe("v1.01");

            migrationService.Run(_testConfiguration.WorkspacePath, "v1.01", autoCreateDatabase: true);
            migrationService.Run(_testConfiguration.WorkspacePath, "v1.01", autoCreateDatabase: true);
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
        public void Test_Run_All_NonVersion_Scripts_Executed(string scriptFolder)
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, scriptFolder), $"test_{scriptFolder}.sql"), _testDataService.GetSqlForCreateDbObject($"test_{scriptFolder}"));

            //act
            var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
            migrationService.Initialize(_testConfiguration.ConnectionString);
            migrationService.Run(_testConfiguration.WorkspacePath, "v1.00", autoCreateDatabase: true);

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, @$"test_{scriptFolder}").ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_All_Version_Scripts_Executed()
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
            migrationService.Run(_testConfiguration.WorkspacePath, "v1.02", autoCreateDatabase: true);

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_00").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_01").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_02").ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_Skipped_Versions_Lower_Or_Same_As_Latest()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);

            localVersionService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"test_v1_00.sql"), _testDataService.GetSqlForCreateDbObject($"test_v1_00"));

            localVersionService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.01"), $"test_v1_01.sql"), _testDataService.GetSqlForCreateDbObject($"test_v1_01"));

            //act
            var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
            migrationService.Initialize(_testConfiguration.ConnectionString);
            migrationService.Run(_testConfiguration.WorkspacePath, "v1.01", autoCreateDatabase: true);

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_00").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_01").ShouldBeTrue();

            //act
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"test_v1_00_added_later.sql"), _testDataService.GetSqlForCreateDbObject($"test_v1_00_added_later"));
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.01"), $"test_v1_01_added_later.sql"), _testDataService.GetSqlForCreateDbObject($"test_v1_01_added_later"));

            localVersionService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.02"), $"test_v1_02.sql"), _testDataService.GetSqlForCreateDbObject($"test_v1_02"));

            migrationService.Run(_testConfiguration.WorkspacePath, "v1.02", autoCreateDatabase: true);

            //assert again
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_00_added_later").ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_01_added_later").ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_02").ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_With_Target_Version_Skipped_Versions_Higher_Than_Target_Version()
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

            localVersionService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v2.00"), $"test_v2_00.sql"), _testDataService.GetSqlForCreateDbObject($"test_v2_00"));

            //act
            var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
            migrationService.Initialize(_testConfiguration.ConnectionString);
            migrationService.Run(_testConfiguration.WorkspacePath, "v1.01", autoCreateDatabase: true);

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_00").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_01").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_02").ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_03").ShouldBeFalse();
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
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);

            localVersionService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, versionFolder), $"{scriptName}.sql"), _testDataService.GetSqlForCreateDbObjectWithTokens($"{scriptName}"));

            //act
            var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
            migrationService.Initialize(_testConfiguration.ConnectionString);
            List<KeyValuePair<string, string>> tokens = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Token1","Token1Value"),
                new KeyValuePair<string, string>("Token2","Token2Value"),
                new KeyValuePair<string, string>("Token3","Token3Value"),
            };
            migrationService.Run(_testConfiguration.WorkspacePath, "v1.00", autoCreateDatabase: true, tokens: tokens);

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, $"{scriptName}_Token1Value_Token2Value_Token3Value").ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_All_Version_SubDirectories_Executed()
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

        [TestMethodEx(Requires = "IsAtomicDDLSupported")]
        public void Test_Run_With_Faulty_Script_Throws_Error_Must_Rollback_All_Changes()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);

            localVersionService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"test_v1_00.sql"), _testDataService.GetSqlForCreateDbObject($"test_v1_00"));
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $".sql"), _testDataService.GetSqlForCreateBulkTable("TestCsv"));
            File.Copy(Path.Combine(Path.Combine(Environment.CurrentDirectory, "Core"), "TestCsv.csv"), Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), "TestCsv.csv"));
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"test_v1_00_error.sql"), _testDataService.GetSqlForCreateDbObjectWithError($"test_v1_00_error"));

            //act
            try
            {
                var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
                migrationService.Initialize(_testConfiguration.ConnectionString);
                migrationService.Run(_testConfiguration.WorkspacePath, "v1.00", autoCreateDatabase: true);
            }
            catch (Exception ex)
            {
                //used try/catch this instead of Assert.ThrowsException because different vendors
                //throws different exception type and message content
                ex.Message.ShouldNotBeNullOrEmpty();
            }

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_00").ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "TestCsv").ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_00_error").ShouldBeFalse();
        }

        [TestMethodEx(Requires = "IsAtomicDDLSupported")]
        public void Test_Verify()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);

            localVersionService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"test_v1_00.sql"), _testDataService.GetSqlForCreateDbObject($"test_v1_00"));

            localVersionService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.01"), $"test_v1_01.sql"), _testDataService.GetSqlForCreateDbObject($"test_v1_01"));

            var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
            migrationService.Initialize(_testConfiguration.ConnectionString);
            migrationService.Run(_testConfiguration.WorkspacePath, "v1.01", autoCreateDatabase: true);

            localVersionService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.02"), $"test_v1_02.sql"), _testDataService.GetSqlForCreateDbObject($"test_v1_02"));

            //act
            migrationService.Run(_testConfiguration.WorkspacePath, "v1.02", autoCreateDatabase: false, verifyOnly: true);

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_00").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_01").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_02").ShouldBeFalse();
        }

        [TestMethod]
        public void Test_Erase()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);

            localVersionService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"script1.sql"), _testDataService.GetSqlForCreateDbObject($"script1"));
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"script2.sql"), _testDataService.GetSqlForCreateDbObject($"script2"));
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"script3.sql"), _testDataService.GetSqlForCreateDbObject($"script3"));

            //act
            var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
            migrationService.Initialize(_testConfiguration.ConnectionString);
            migrationService.Run(_testConfiguration.WorkspacePath, "v1.00", autoCreateDatabase: true);

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "script1").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "script2").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "script3").ShouldBeTrue();

            //arrange
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "_erase"), $"erase.sql"), _testDataService.GetSqlForCleanup());

            //act
            migrationService.Erase(_testConfiguration.WorkspacePath);

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "script1").ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "script2").ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "script3").ShouldBeFalse();
        }

        [TestMethod]
        public void Test_Run_With_Unsupported_Platform_Throws_Exception()
        {
            //arrange

            //act
            Assert.ThrowsException<NotSupportedException>(() =>
            {
                var migrationService = _migrationServiceFactory.Create("oracle");
                migrationService.Initialize(_testConfiguration.ConnectionString);
                migrationService.Run(_testConfiguration.WorkspacePath, "v1.00", autoCreateDatabase: true);
            }).Message.ShouldContain($"The target database platform oracle is not supported or plugins location was not correctly configured.");
        }

        [TestMethod]
        public void Test_Run_With_Missing_Directories_In_Workspace_Must_Throw_Exception()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), $"test_v0_00.sql"), _testDataService.GetSqlForCreateDbObject($"test_v0_00"));

            Directory.Delete(Path.Combine(_testConfiguration.WorkspacePath, "_init"), true);
            Directory.Delete(Path.Combine(_testConfiguration.WorkspacePath, "_post"), true);

            //act
            var exception = Assert.ThrowsException<YuniqlMigrationException>(() =>
            {
                var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
                migrationService.Initialize(_testConfiguration.ConnectionString);
                migrationService.Run(_testConfiguration.WorkspacePath, "v0.00", autoCreateDatabase: true);
            });

            //assert
            exception.Message.Contains("At least one Yuniql directory is missing in your project.").ShouldBeTrue();
            exception.Message.Contains($"{Path.Combine(_testConfiguration.WorkspacePath, "_init")} / Missing").ShouldBeTrue();
            exception.Message.Contains($"{Path.Combine(_testConfiguration.WorkspacePath, "_pre")} / Found").ShouldBeTrue();
            exception.Message.Contains($"{Path.Combine(_testConfiguration.WorkspacePath, "v0.00")} / Found").ShouldBeTrue();
            exception.Message.Contains($"{Path.Combine(_testConfiguration.WorkspacePath, "_draft")} / Found").ShouldBeTrue();
            exception.Message.Contains($"{Path.Combine(_testConfiguration.WorkspacePath, "_post")} / Missing").ShouldBeTrue();
            exception.Message.Contains($"{Path.Combine(_testConfiguration.WorkspacePath, "_erase")} / Found").ShouldBeTrue();

            _testDataService.CheckIfDbExist(_testConfiguration.ConnectionString).ShouldBeFalse();
        }

        [TestMethod]
        public void Test_Run_With_User_Custom_Directories_In_Workspace()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), $"test_v0_00.sql"), _testDataService.GetSqlForCreateDbObject($"test_v0_00"));

            Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "user_created_folder"));
            Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "_another_user_created_folder"));

            //act
            var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
            migrationService.Initialize(_testConfiguration.ConnectionString);
            migrationService.Run(_testConfiguration.WorkspacePath, "v0.00", autoCreateDatabase: true);

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v0_00").ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_Dash_Character_In_Database_Name()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), $"test_v0_00.sql"), _testDataService.GetSqlForCreateDbObject($"test_v0_00"));

            //use database name with dash
            var databaseName = "yuniqldb-raw-dev";
            _testConfiguration.ConnectionString = _testConfiguration.ConnectionString.Replace(_testConfiguration.DatabaseName, databaseName);
            _testConfiguration.DatabaseName = databaseName;

            //act
            var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
            migrationService.Initialize(_testConfiguration.ConnectionString);
            migrationService.Run(_testConfiguration.WorkspacePath, "v0.00", autoCreateDatabase: true);

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v0_00").ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_Environment_Aware_Scripts_All_Directories()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);

            //creare environment-aware directories
            var init_dev = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "_init", "_dev")).FullName;
            var init_test = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "_init", "_test")).FullName;
            var init_prod = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "_init", "_prod")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(init_dev, $"init_dev.sql"), _testDataService.GetSqlForCreateDbObject($"init_dev"));
            _testDataService.CreateScriptFile(Path.Combine(init_test, $"init_test.sql"), _testDataService.GetSqlForCreateDbObject($"init_test"));
            _testDataService.CreateScriptFile(Path.Combine(init_prod, $"init_prod.sql"), _testDataService.GetSqlForCreateDbObject($"init_prod"));

            var pre_dev = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "_pre", "_dev")).FullName;
            var pre_test = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "_pre", "_test")).FullName;
            var pre_prod = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "_pre", "_prod")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(pre_dev, $"pre_dev.sql"), _testDataService.GetSqlForCreateDbObject($"pre_dev"));
            _testDataService.CreateScriptFile(Path.Combine(pre_test, $"pre_test.sql"), _testDataService.GetSqlForCreateDbObject($"pre_test"));
            _testDataService.CreateScriptFile(Path.Combine(pre_prod, $"pre_prod.sql"), _testDataService.GetSqlForCreateDbObject($"pre_prod"));

            var v00_dev = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "_dev")).FullName;
            var v00_test = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "_test")).FullName;
            var v00_prod = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "_prod")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(v00_dev, $"v00_dev.sql"), _testDataService.GetSqlForCreateDbObject($"v00_dev"));
            _testDataService.CreateScriptFile(Path.Combine(v00_test, $"v00_test.sql"), _testDataService.GetSqlForCreateDbObject($"v00_test"));
            _testDataService.CreateScriptFile(Path.Combine(v00_prod, $"v00_prod.sql"), _testDataService.GetSqlForCreateDbObject($"v00_prod"));

            var draft_dev = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "_draft", "_dev")).FullName;
            var draft_test = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "_draft", "_test")).FullName;
            var draft_prod = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "_draft", "_prod")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(draft_dev, $"draft_dev.sql"), _testDataService.GetSqlForCreateDbObject($"draft_dev"));
            _testDataService.CreateScriptFile(Path.Combine(draft_test, $"draft_test.sql"), _testDataService.GetSqlForCreateDbObject($"draft_test"));
            _testDataService.CreateScriptFile(Path.Combine(draft_prod, $"draft_prod.sql"), _testDataService.GetSqlForCreateDbObject($"draft_prod"));

            var post_dev = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "_post", "_dev")).FullName;
            var post_test = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "_post", "_test")).FullName;
            var post_prod = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "_post", "_prod")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(post_dev, $"post_dev.sql"), _testDataService.GetSqlForCreateDbObject($"post_dev"));
            _testDataService.CreateScriptFile(Path.Combine(post_test, $"post_test.sql"), _testDataService.GetSqlForCreateDbObject($"post_test"));
            _testDataService.CreateScriptFile(Path.Combine(post_prod, $"post_prod.sql"), _testDataService.GetSqlForCreateDbObject($"post_prod"));

            //act
            var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
            migrationService.Initialize(_testConfiguration.ConnectionString);
            migrationService.Run(_testConfiguration.WorkspacePath, "v0.00", autoCreateDatabase: true, environmentCode: "test");

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "init_dev").ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "init_test").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "init_prod").ShouldBeFalse();

            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "pre_dev").ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "pre_test").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "pre_prod").ShouldBeFalse();

            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "v00_dev").ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "v00_test").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "v00_prod").ShouldBeFalse();

            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "draft_dev").ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "draft_test").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "draft_prod").ShouldBeFalse();

            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "post_dev").ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "post_test").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "post_prod").ShouldBeFalse();
        }

        [TestMethod]
        public void Test_Run_Environment_Aware_Scripts_Placed_In_Sub_Directory()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);

            //creare environment-aware directories
            var v00_dev = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "tables", "_dev")).FullName;
            var v00_test = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "tables", "_test")).FullName;
            var v00_prod = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "tables", "_prod")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(v00_dev, $"v00_dev.sql"), _testDataService.GetSqlForCreateDbObject($"v00_dev"));
            _testDataService.CreateScriptFile(Path.Combine(v00_test, $"v00_test_01.sql"), _testDataService.GetSqlForCreateDbObject($"v00_test_01"));
            _testDataService.CreateScriptFile(Path.Combine(v00_test, $"v00_test_02.sql"), _testDataService.GetSqlForCreateDbObject($"v00_test_02"));
            _testDataService.CreateScriptFile(Path.Combine(v00_test, $"v00_test_03.sql"), _testDataService.GetSqlForCreateDbObject($"v00_test_03"));
            _testDataService.CreateScriptFile(Path.Combine(v00_prod, $"v00_prod.sql"), _testDataService.GetSqlForCreateDbObject($"v00_prod"));

            //act
            var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
            migrationService.Initialize(_testConfiguration.ConnectionString);
            migrationService.Run(_testConfiguration.WorkspacePath, "v0.00", autoCreateDatabase: true, environmentCode: "test");

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "v00_dev").ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "v00_test_01").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "v00_test_02").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "v00_test_03").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "v00_prod").ShouldBeFalse();
        }


        [TestMethod]
        public void Test_Run_Environment_Aware_Scripts_Placed_In_Sub_Directory_With_Scripts_Outside()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);

            //creare environment-aware directories
            var v00 = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00")).FullName;
            var v00_dev = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "tables", "_dev")).FullName;
            var v00_test = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "tables", "_test")).FullName;
            var v00_prod = Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "v0.00", "tables", "_prod")).FullName;

            _testDataService.CreateScriptFile(Path.Combine(v00, $"v00_01.sql"), _testDataService.GetSqlForCreateDbObject($"v00_01"));
            _testDataService.CreateScriptFile(Path.Combine(v00, $"v00_02.sql"), _testDataService.GetSqlForCreateDbObject($"v00_02"));
            _testDataService.CreateScriptFile(Path.Combine(v00, $"v00_03.sql"), _testDataService.GetSqlForCreateDbObject($"v00_03"));

            _testDataService.CreateScriptFile(Path.Combine(v00_dev, $"v00_dev.sql"), _testDataService.GetSqlForCreateDbObject($"v00_dev"));
            _testDataService.CreateScriptFile(Path.Combine(v00_test, $"v00_test_01.sql"), _testDataService.GetSqlForCreateDbObject($"v00_test_01"));
            _testDataService.CreateScriptFile(Path.Combine(v00_test, $"v00_test_02.sql"), _testDataService.GetSqlForCreateDbObject($"v00_test_02"));
            _testDataService.CreateScriptFile(Path.Combine(v00_test, $"v00_test_03.sql"), _testDataService.GetSqlForCreateDbObject($"v00_test_03"));
            _testDataService.CreateScriptFile(Path.Combine(v00_prod, $"v00_prod.sql"), _testDataService.GetSqlForCreateDbObject($"v00_prod"));

            //act
            var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
            migrationService.Initialize(_testConfiguration.ConnectionString);
            migrationService.Run(_testConfiguration.WorkspacePath, "v0.00", autoCreateDatabase: true, environmentCode: "test");

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "v00_01").ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "v00_02").ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "v00_03").ShouldBeFalse();

            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "v00_dev").ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "v00_test_01").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "v00_test_02").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "v00_test_03").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "v00_prod").ShouldBeFalse();
        }
    }
}
