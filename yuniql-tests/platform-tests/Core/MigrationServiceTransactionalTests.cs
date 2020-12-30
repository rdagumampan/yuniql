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
    public class MigrationServiceTransactionalTests : TestBase
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

        [TestMethod]
        public void Test_Run_Without_AutocreateDB_Throws_Exception()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);

            //act and assert
            try
            {
                var configuration = _testConfiguration.GetConfiguration();
                configuration.AutoCreateDatabase = false;

                var migrationService = _migrationServiceFactory.Create(configuration.Platform);
                migrationService.Initialize(configuration);
                migrationService.Run();
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
            var configuration = _testConfiguration.GetConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Initialize(configuration);
            migrationService.Run();

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
            var configuration = _testConfiguration.GetConfiguration();
            configuration.TargetVersion = "v1.01";
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Initialize(configuration);
            migrationService.Run();

            var versions = _testDataService.GetAllDbVersions(_testConfiguration.ConnectionString);

            versions.Count.ShouldBe(3);
            versions[0].Version.ShouldBe("v0.00");
            versions[1].Version.ShouldBe("v1.00");
            versions[2].Version.ShouldBe("v1.01");

            migrationService.Run();
            migrationService.Run();

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
            var configuration = _testConfiguration.GetConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Initialize(configuration);
            migrationService.Run();

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
            var configuration = _testConfiguration.GetConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Initialize(configuration);
            migrationService.Run();

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
            var configuration = _testConfiguration.GetConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Initialize(configuration);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_00").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_01").ShouldBeTrue();

            //act
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"test_v1_00_added_later.sql"), _testDataService.GetSqlForCreateDbObject($"test_v1_00_added_later"));
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.01"), $"test_v1_01_added_later.sql"), _testDataService.GetSqlForCreateDbObject($"test_v1_01_added_later"));

            localVersionService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.02"), $"test_v1_02.sql"), _testDataService.GetSqlForCreateDbObject($"test_v1_02"));

            migrationService.Run();

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
            var configuration = _testConfiguration.GetConfiguration();
            configuration.TargetVersion = "v1.01";
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Initialize(configuration);
            migrationService.Run();

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
            List<KeyValuePair<string, string>> tokens = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Token1","Token1Value"),
                new KeyValuePair<string, string>("Token2","Token2Value"),
                new KeyValuePair<string, string>("Token3","Token3Value"),
            };

            var configuration = _testConfiguration.GetConfiguration();
            configuration.Tokens = tokens;
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Initialize(configuration);
            migrationService.Run();

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
            var configuration = _testConfiguration.GetConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Initialize(configuration);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_00").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_00_level1").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_00_level1_sublevel1").ShouldBeTrue();

            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v2_00").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v2_00_level1").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v2_00_level1_sublevel1").ShouldBeTrue();
        }

        [TestMethodEx(Requires = "IsTransactionalDdlSupported")]
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
                var configuration = _testConfiguration.GetConfiguration();
                var migrationService = _migrationServiceFactory.Create(configuration.Platform);
                migrationService.Initialize(configuration);
                migrationService.Run();
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

        [TestMethodEx(Requires = "IsTransactionalDdlSupported")]
        public void Test_Verify()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);

            localVersionService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"test_v1_00.sql"), _testDataService.GetSqlForCreateDbObject($"test_v1_00"));

            localVersionService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.01"), $"test_v1_01.sql"), _testDataService.GetSqlForCreateDbObject($"test_v1_01"));

            var configuration = _testConfiguration.GetConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Initialize(configuration);
            migrationService.Run();

            localVersionService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.02"), $"test_v1_02.sql"), _testDataService.GetSqlForCreateDbObject($"test_v1_02"));

            //act
            configuration.VerifyOnly = true;
            configuration.AutoCreateDatabase = false;
            migrationService.Initialize(configuration);
            migrationService.Run();

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
            var configuration = _testConfiguration.GetConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Initialize(configuration);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "script1").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "script2").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "script3").ShouldBeTrue();

            //arrange
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "_erase"), $"erase.sql"), _testDataService.GetSqlForCleanup());

            //act
            migrationService.Erase();

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
                var configuration = _testConfiguration.GetConfiguration();
                var migrationService = _migrationServiceFactory.Create("sqlserver-vnext");
                migrationService.Initialize(configuration);
                migrationService.Run();
            }).Message.ShouldContain($"The target database platform sqlserver-vnext is not supported or plugins location was not correctly configured.");
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
                var configuration = _testConfiguration.GetConfiguration();
                var migrationService = _migrationServiceFactory.Create(configuration.Platform);
                migrationService.Initialize(configuration);
                migrationService.Run();
            });

            //assert
            exception.Message.Contains("At least one Yuniql directory is missing in your project.").ShouldBeTrue();
            exception.Message.Contains($"{Path.Combine(_testConfiguration.WorkspacePath, "_init")} / Missing").ShouldBeTrue();
            exception.Message.Contains($"{Path.Combine(_testConfiguration.WorkspacePath, "_pre")} / Found").ShouldBeTrue();
            exception.Message.Contains($"{Path.Combine(_testConfiguration.WorkspacePath, "v0.00*")} / Found").ShouldBeTrue();
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
            var configuration = _testConfiguration.GetConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Initialize(configuration);
            migrationService.Run();

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
            var configuration = _testConfiguration.GetConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Initialize(configuration);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v0_00").ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_Draft_Always_Executed()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), $"test_v0_00.sql"), _testDataService.GetSqlForCreateDbObject($"test_v0_00"));

            //act, run with _draft is empty at this point
            var configuration = _testConfiguration.GetConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Initialize(configuration);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v0_00").ShouldBeTrue();

            //arrange
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "_draft"), $"test_draft_01.sql"), _testDataService.GetSqlForCreateDbObject($"test_draft_01"));
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "_draft"), $"test_draft_02.sql"), _testDataService.GetSqlForCreateDbObject($"test_draft_02"));

            //act - runs again with _draft holding two script files
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_draft_01").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_draft_02").ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_Transaction_Mode_Every_Session()
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
            var configuration = _testConfiguration.GetConfiguration();
            configuration.TransactionMode = TRANSACTION_MODE.SESSION;
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Initialize(configuration);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_00").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_01").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_02").ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_Transaction_Mode_Every_Version()
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
            var configuration = _testConfiguration.GetConfiguration();
            configuration.TransactionMode = TRANSACTION_MODE.VERSION;
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Initialize(configuration);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_00").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_01").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_02").ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_Transaction_Mode_None()
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
            var configuration = _testConfiguration.GetConfiguration();
            configuration.TransactionMode = TRANSACTION_MODE.NONE;
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Initialize(configuration);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_00").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_01").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "test_v1_02").ShouldBeTrue();
        }

    }
}
