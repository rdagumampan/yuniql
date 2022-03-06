using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Shouldly;
using System.Collections.Generic;
using System;
using System.Data;
using Yuniql.Core;
using Yuniql.Extensibility;
using Yuniql.PlatformTests.Interfaces;
using Yuniql.PlatformTests.Setup;
using IMigrationServiceFactory = Yuniql.PlatformTests.Interfaces.IMigrationServiceFactory;
using MigrationServiceFactory = Yuniql.PlatformTests.Setup.MigrationServiceFactory;
using System.Diagnostics;

namespace Yuniql.PlatformTests.Core
{

    //https://docs.microsoft.com/en-gb/dotnet/standard/assembly/unloadability
    //https://github.com/dotnet/samples/blob/master/core/extensions/AppWithPlugin/AppWithPlugin/Program.cs
    [TestClass]
    public class MigrationServiceTests : TestClassBase
    {
        private ITestDataService _testDataService;
        private IMigrationServiceFactory _migrationServiceFactory;
        private ITraceService _traceService;
        private IDirectoryService _directoryService;
        private TestConfiguration _testConfiguration;

        [TestInitialize]
        public void Setup()
        {
            _testConfiguration = ConfigureWithEmptyWorkspace();

            //create test data service provider
            var testDataServiceFactory = new TestDataServiceFactory();
            _testDataService = testDataServiceFactory.Create(_testConfiguration.Platform);

            //create data service factory for migration proper
            _directoryService = new DirectoryService(_traceService);
            _traceService = new FileTraceService() { IsDebugEnabled = true };
            _migrationServiceFactory = new MigrationServiceFactory(_traceService);
        }

        [TestCleanup]
        public void Cleanup()
        {
            //drop the test directory
            try
            {
                if (Directory.Exists(_testConfiguration.WorkspacePath))
                    Directory.Delete(_testConfiguration.WorkspacePath, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            //drop test database
            try
            {
                _testDataService.CleanupDbObjects(_testConfiguration.ConnectionString);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        [TestMethod]
        public void Test_Run_Without_AutocreateDB_Throws_Exception()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            //act and assert
            try
            {
                var configuration = _testConfiguration.GetFreshConfiguration();
                configuration.IsAutoCreateDatabase = false;

                var migrationService = _migrationServiceFactory.Create(configuration.Platform);
                migrationService.Run();
            }
            catch (Exception ex)
            {
                //used try/catch this instead of Assert.ThrowsException because different vendors
                //throws different exception type and message content
                ex.Message.ShouldNotBeNullOrEmpty();
            }
        }

        [TestMethodEx(Requires = "IsMultiTenancySupported")]
        public void Test_Run_With_AutocreateDB()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);
            workspaceService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            workspaceService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbExist(_testConfiguration.ConnectionString).ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_Database_Already_Updated()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);
            workspaceService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            workspaceService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            configuration.TargetVersion = "v1.01";
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
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
        [DataRow(RESERVED_DIRECTORY_NAME.INIT)]
        [DataRow(RESERVED_DIRECTORY_NAME.PRE)]
        [DataRow(RESERVED_DIRECTORY_NAME.POST)]
        [DataRow(RESERVED_DIRECTORY_NAME.DRAFT)]
        public void Test_Run_All_NonVersion_Scripts_Executed(string scriptFolder)
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, scriptFolder), $"test_{scriptFolder}.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_1).ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_All_Version_Scripts_Executed()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            workspaceService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"test_v1_00.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));

            workspaceService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.01"), $"test_v1_01.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));

            workspaceService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.02"), $"test_v1_02.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_1).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_2).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_3).ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_Skipped_Versions_Lower_Or_Same_As_Latest()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            workspaceService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"test_v1_00.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));

            workspaceService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.01"), $"test_v1_01.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_1).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_2).ShouldBeTrue();

            //act
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"test_v1_00_added_later.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.01"), $"test_v1_01_added_later.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_4));

            workspaceService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.02"), $"test_v1_02.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_5));

            migrationService.Run();

            //assert again
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_3).ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_4).ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_5).ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_With_Target_Version_Skipped_Versions_Higher_Than_Target_Version()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            workspaceService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"test_v1_00.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));

            workspaceService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.01"), $"test_v1_01.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));

            workspaceService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.02"), $"test_v1_02.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));

            workspaceService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v2.00"), $"test_v2_00.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_4));

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            configuration.TargetVersion = "v1.01";
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_1).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_2).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_3).ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_4).ShouldBeFalse();
        }

        [DataTestMethod()]
        [DataRow(RESERVED_DIRECTORY_NAME.INIT, RESERVED_DIRECTORY_NAME.INIT)]
        [DataRow(RESERVED_DIRECTORY_NAME.PRE, RESERVED_DIRECTORY_NAME.PRE)]
        [DataRow("v1.00", "v1_00")]
        [DataRow(RESERVED_DIRECTORY_NAME.POST, RESERVED_DIRECTORY_NAME.POST)]
        [DataRow(RESERVED_DIRECTORY_NAME.DRAFT, RESERVED_DIRECTORY_NAME.DRAFT)]
        public void Test_Run_With_Parameterized_Tokens(string versionFolder, string scriptName)
        {
            //arrange           
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            workspaceService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, versionFolder), $"{scriptName}.sql"), _testDataService.GetSqlForCreateDbObjectWithTokens(TEST_DBOBJECTS.DB_OBJECT_1));

            //act
            List<KeyValuePair<string, string>> tokens = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Token1","Token1Value"),
                new KeyValuePair<string, string>("Token2","Token2Value"),
                new KeyValuePair<string, string>("Token3","Token3Value"),
            };

            var configuration = _testConfiguration.GetFreshConfiguration();
            configuration.Tokens = tokens;
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, $"{TEST_DBOBJECTS.DB_OBJECT_1}_Token1Value_Token2Value_Token3Value").ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_All_Version_SubDirectories_Executed()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            workspaceService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            string v1rootDirectory = Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"));
            _testDataService.CreateScriptFile(Path.Combine(v1rootDirectory, $"test_v1_00.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1)); ;

            string v1level1Directory = Path.Combine(v1rootDirectory, "v1.00-level1");
            Directory.CreateDirectory(v1level1Directory);
            _testDataService.CreateScriptFile(Path.Combine(v1level1Directory, $"test_v1_00_level1.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));

            string v1level1SubDirectory = Path.Combine(v1level1Directory, "v1.00-level1-sublevel1");
            Directory.CreateDirectory(v1level1SubDirectory);
            _testDataService.CreateScriptFile(Path.Combine(v1level1SubDirectory, $"test_v1_00_level1_sublevel1.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));

            workspaceService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            string v2rootDirectory = Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v2.00"));
            _testDataService.CreateScriptFile(Path.Combine(v2rootDirectory, $"test_v2_00.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_4)); ;

            string v2level1Directory = Path.Combine(v2rootDirectory, "v2.00-level1");
            Directory.CreateDirectory(v2level1Directory);
            _testDataService.CreateScriptFile(Path.Combine(v2level1Directory, $"test_v2_00_level1.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_5));

            string v2level1SubDirectory = Path.Combine(v2level1Directory, "v2.00-level1-sublevel1");
            Directory.CreateDirectory(v2level1SubDirectory);
            _testDataService.CreateScriptFile(Path.Combine(v2level1SubDirectory, $"test_v2_00_level1_sublevel1.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_6));

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_1).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_2).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_3).ShouldBeTrue();

            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_4).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_5).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_6).ShouldBeTrue();
        }

        [TestMethodEx(Requires = "IsTransactionalDdlSupported")]
        public void Test_Run_With_Faulty_Script_Throws_Error_Must_Rollback_All_Changes()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            workspaceService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"test_v1_00.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $".sql"), _testDataService.GetSqlForCreateBulkTable(TEST_DBOBJECTS.TestCsv));
            File.Copy(Path.Combine(Path.Combine(Environment.CurrentDirectory, "Data"), "TestCsv.csv"), Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), "TestCsv.csv"));
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"test_v1_00_error.sql"), _testDataService.GetSqlForCreateDbObjectWithError(TEST_DBOBJECTS.DB_OBJECT_2));

            //act
            try
            {
                var configuration = _testConfiguration.GetFreshConfiguration();
                var migrationService = _migrationServiceFactory.Create(configuration.Platform);
                migrationService.Run();
            }
            catch (Exception ex)
            {
                //used try/catch this instead of Assert.ThrowsException because different vendors
                //throws different exception type and message content
                ex.Message.ShouldNotBeNullOrEmpty();
            }

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_1).ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.TestCsv).ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_2).ShouldBeFalse();
        }

        [TestMethodEx(Requires = "IsTransactionalDdlSupported")]
        public void Test_Verify()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            workspaceService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"test_v1_00.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));

            workspaceService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.01"), $"test_v1_01.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));

            var configuration = _testConfiguration.GetFreshConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            workspaceService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.02"), $"test_v1_02.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));

            //act
            configuration.IsVerifyOnly = true;
            configuration.IsAutoCreateDatabase = false;
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_1).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_2).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_3).ShouldBeFalse();
        }

        [TestMethod]
        public void Test_Erase()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            workspaceService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"script1.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"script2.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"script3.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_1).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_2).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_3).ShouldBeTrue();

            //arrange
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.ERASE), $"erase.sql"), _testDataService.GetSqlForEraseDbObjects());

            //act
            migrationService.Erase();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_1).ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_2).ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_3).ShouldBeFalse();
        }

        [TestMethod]
        public void Test_Run_With_Unsupported_Platform_Throws_Exception()
        {
            //arrange

            //act
            Assert.ThrowsException<NotSupportedException>(() =>
            {
                var configuration = _testConfiguration.GetFreshConfiguration();
                var migrationService = _migrationServiceFactory.Create("sqlserver-vnext");
                migrationService.Run();
            }).Message.ShouldContain($"The target database platform sqlserver-vnext is not supported or plugins location was not correctly configured.");
        }

        [TestMethod]
        public void Test_Run_With_Missing_Directories_In_Workspace_Must_Throw_Exception()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), $"test_v0_00.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));

            Directory.Delete(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.INIT), true);
            Directory.Delete(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.POST), true);

            //act
            var exception = Assert.ThrowsException<YuniqlMigrationException>(() =>
            {
                var configuration = _testConfiguration.GetFreshConfiguration();
                var migrationService = _migrationServiceFactory.Create(configuration.Platform);
                migrationService.Run();
            });

            //assert
            exception.Message.Contains("At least one required yuniql directory/folder is missing in your workspace").ShouldBeTrue();
            exception.Message.Contains($"{Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.INIT)} / Missing").ShouldBeTrue();
            exception.Message.Contains($"{Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.PRE)} / Found").ShouldBeTrue();
            exception.Message.Contains($"{Path.Combine(_testConfiguration.WorkspacePath, "v0.00*")} / Found").ShouldBeTrue();
            exception.Message.Contains($"{Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.DRAFT)} / Found").ShouldBeTrue();
            exception.Message.Contains($"{Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.POST)} / Missing").ShouldBeTrue();
            exception.Message.Contains($"{Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.ERASE)} / Found").ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_With_User_Custom_Directories_In_Workspace()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), $"test_v0_00.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));

            Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "user_created_folder"));
            Directory.CreateDirectory(Path.Combine(_testConfiguration.WorkspacePath, "_another_user_created_folder"));

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_1).ShouldBeTrue();
        }

        [TestMethodEx(Requires = "IsMultiTenancySupported")]
        public void Test_Run_Dash_Character_In_Database_Name()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), $"test_v0_00.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));

            //use database name with dash
            var databaseName = "yuniqldb-raw-dev";
            _testConfiguration.ConnectionString = _testConfiguration.ConnectionString.Replace(_testConfiguration.DatabaseName, databaseName);
            _testConfiguration.DatabaseName = databaseName;

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_1).ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_Draft_Always_Executed()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), $"test_v0_00.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));

            //act, run with _draft is empty at this point
            var configuration = _testConfiguration.GetFreshConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_1).ShouldBeTrue();

            //arrange
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.DRAFT), $"test_draft_01.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, RESERVED_DIRECTORY_NAME.DRAFT), $"test_draft_02.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));

            //act - runs again with _draft holding two script files
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_2).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_3).ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_Transaction_Mode_Every_Session()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            workspaceService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"test_v1_00.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));

            workspaceService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.01"), $"test_v1_01.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));

            workspaceService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.02"), $"test_v1_02.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            configuration.TransactionMode = TRANSACTION_MODE.SESSION;
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_1).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_2).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_3).ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_Transaction_Mode_Every_Version()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            workspaceService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"test_v1_00.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));

            workspaceService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.01"), $"test_v1_01.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));

            workspaceService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.02"), $"test_v1_02.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            configuration.TransactionMode = TRANSACTION_MODE.VERSION;
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_1).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_2).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_3).ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_Transaction_Mode_None()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);

            workspaceService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"test_v1_00.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_1));

            workspaceService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.01"), $"test_v1_01.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_2));

            workspaceService.IncrementMinorVersion(_testConfiguration.WorkspacePath, null);
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.02"), $"test_v1_02.sql"), _testDataService.GetSqlForCreateDbObject(TEST_DBOBJECTS.DB_OBJECT_3));

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            configuration.TransactionMode = TRANSACTION_MODE.NONE;
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_1).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_2).ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, TEST_DBOBJECTS.DB_OBJECT_3).ShouldBeTrue();
        }
    }
}
