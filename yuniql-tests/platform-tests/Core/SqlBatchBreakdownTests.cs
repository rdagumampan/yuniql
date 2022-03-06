using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Shouldly;
using Yuniql.Core;
using Yuniql.Extensibility;
using System;
using Yuniql.PlatformTests.Interfaces;
using Yuniql.PlatformTests.Setup;
using IMigrationServiceFactory = Yuniql.PlatformTests.Interfaces.IMigrationServiceFactory;
using MigrationServiceFactory = Yuniql.PlatformTests.Setup.MigrationServiceFactory;
using System.Diagnostics;

namespace Yuniql.PlatformTests.Core
{
    [TestClass]
    public class SqlBatchBreakdownTests : TestClassBase
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

        [TestMethodEx(Requires = nameof(TestDataServiceBase.IsBatchSqlSupported))]
        public void Test_Create_SingleLine_Empty_Script()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);
            workspaceService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);

            string sqlStatement = $@"
";
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"Test_Single_Run_Empty.sql"), sqlStatement);

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "Test_Single_Run_Empty").ShouldBeFalse();
        }

        [TestMethodEx(Requires = nameof(TestDataServiceBase.IsBatchSqlSupported))]
        public void Test_Create_SingleLine_Script_With_Terminator()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);
            workspaceService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);

            string sqlObjectName = TEST_DBOBJECTS.DB_OBJECT_1;
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"{sqlObjectName}.sql"), _testDataService.GetSqlForSingleLine(sqlObjectName));

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, $"{sqlObjectName}").ShouldBeTrue();
        }

        [TestMethodEx(Requires = nameof(TestDataServiceBase.IsBatchSqlSupported))]
        public void Test_Create_SingleLine_Script_Without_Terminator()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);
            workspaceService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);

            string sqlObjectName = TEST_DBOBJECTS.DB_OBJECT_1;
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"{sqlObjectName}.sql"), _testDataService.GetSqlForSingleLineWithoutTerminator(sqlObjectName));

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, $"{sqlObjectName}").ShouldBeTrue();
        }

        [TestMethodEx(Requires = nameof(TestDataServiceBase.IsBatchSqlSupported))]
        public void Test_Create_Multiline_Script_Without_Terminator_In_LastLine()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);
            workspaceService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);

            string sqlFileName = "Test_Single_Run_Single_Standard";
            string sqlObjectName1 = TEST_DBOBJECTS.DB_OBJECT_1;
            string sqlObjectName2 = TEST_DBOBJECTS.DB_OBJECT_2;
            string sqlObjectName3 = TEST_DBOBJECTS.DB_OBJECT_3;

            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"{sqlFileName}.sql"), _testDataService.GetSqlForMultilineWithoutTerminatorInLastLine(sqlObjectName1, sqlObjectName2, sqlObjectName3));

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, $"{sqlObjectName1}").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, $"{sqlObjectName2}").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, $"{sqlObjectName3}").ShouldBeTrue();
        }

        [TestMethodEx(Requires = nameof(TestDataServiceBase.IsBatchSqlSupported))]
        public void Test_Create_Multiline_Script_With_Terminator_In_Comment_Block()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);
            workspaceService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);

            string sqlFileName = "Test_Single_Run_Single_Standard";
            string sqlObjectName1 = TEST_DBOBJECTS.DB_OBJECT_1;
            string sqlObjectName2 = TEST_DBOBJECTS.DB_OBJECT_2;
            string sqlObjectName3 = TEST_DBOBJECTS.DB_OBJECT_3;

            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"{sqlFileName}.sql"), _testDataService.GetSqlForMultilineWithTerminatorInCommentBlock(sqlObjectName1, sqlObjectName2, sqlObjectName3));

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, $"{sqlObjectName1}").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, $"{sqlObjectName2}").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, $"{sqlObjectName3}").ShouldBeTrue();
        }


        [TestMethodEx(Requires = nameof(TestDataServiceBase.IsBatchSqlSupported))]

        public void Test_Create_Multiline_Script_With_Terminator_Inside_Statements()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);
            workspaceService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);

            string sqlFileName = "Test_Single_Run_Single_Standard";
            string sqlObjectName1 = TEST_DBOBJECTS.DB_OBJECT_1;
            string sqlObjectName2 = TEST_DBOBJECTS.DB_OBJECT_2;
            string sqlObjectName3 = TEST_DBOBJECTS.DB_OBJECT_3;

            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"{sqlFileName}.sql"), _testDataService.GetSqlForMultilineWithTerminatorInsideStatements(sqlObjectName1, sqlObjectName2, sqlObjectName3));

            //act
            var configuration = _testConfiguration.GetFreshConfiguration();
            var migrationService = _migrationServiceFactory.Create(configuration.Platform);
            migrationService.Run();

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, $"{sqlObjectName1}").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, $"{sqlObjectName2}").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, $"{sqlObjectName3}").ShouldBeTrue();
        }

        [TestMethodEx(Requires = nameof(TestDataServiceBase.IsBatchSqlSupported)+ "And" + nameof(TestDataServiceBase.IsTransactionalDdlSupported))]
        public void Test_Create_Multiline_Script_With_Error_Must_Rollback()
        {
            //arrange
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            workspaceService.Init(_testConfiguration.WorkspacePath);
            workspaceService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);

            string sqlFileName = "Test_Single_Run_Failed_Script_Must_Rollback";
            string sqlObjectName1 = TEST_DBOBJECTS.DB_OBJECT_1;
            string sqlObjectName2 = TEST_DBOBJECTS.DB_OBJECT_2;
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v0.00"), $"{sqlFileName}.sql"), _testDataService.GetSqlForMultilineWithError(sqlObjectName1, sqlObjectName2));

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
            _testDataService.GetCurrentDbVersion(_testConfiguration.ConnectionString).ShouldBeNull();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, $"{sqlObjectName1}").ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, $"{sqlObjectName2}").ShouldBeFalse();
        }

    }
}
