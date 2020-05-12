using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Shouldly;
using Yuniql.Core;
using Yuniql.Extensibility;
using System;

namespace Yuniql.PlatformTests
{
    [TestClass]
    public class BreakdownScriptTests : TestBase
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

        [TestMethodEx(Requires = nameof(TestDataServiceBase.IsBatchSqlSupported))]
        public void Test_Create_SingleLine_Empty_Script()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);
            localVersionService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);

            string sqlStatement = $@"
";
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"Test_Single_Run_Empty.sql"), sqlStatement);

            //act
            var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
            migrationService.Initialize(_testConfiguration.ConnectionString);
            migrationService.Run(_testConfiguration.WorkspacePath, "v1.00", autoCreateDatabase: true);

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, "Test_Single_Run_Empty").ShouldBeFalse();
        }

        [TestMethodEx(Requires = nameof(TestDataServiceBase.IsBatchSqlSupported))]
        public void Test_Create_SingleLine_Script_With_Terminator()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);
            localVersionService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);

            string sqlObjectName = "Test_Object_1";
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"{sqlObjectName}.sql"), _testDataService.GetSqlForSingleLine(sqlObjectName));

            //act
            var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
            migrationService.Initialize(_testConfiguration.ConnectionString);
            migrationService.Run(_testConfiguration.WorkspacePath, "v1.00", autoCreateDatabase: true);

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, $"{sqlObjectName}").ShouldBeTrue();
        }

        [TestMethodEx(Requires = nameof(TestDataServiceBase.IsBatchSqlSupported))]
        public void Test_Create_SingleLine_Script_Without_Terminator()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);
            localVersionService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);

            string sqlObjectName = "Test_Object_1";
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"{sqlObjectName}.sql"), _testDataService.GetSqlForSingleLineWithoutTerminator(sqlObjectName));

            //act
            var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
            migrationService.Initialize(_testConfiguration.ConnectionString);
            migrationService.Run(workingPath: _testConfiguration.WorkspacePath, targetVersion: "v1.00", autoCreateDatabase: true);

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, $"{sqlObjectName}").ShouldBeTrue();
        }

        [TestMethodEx(Requires = nameof(TestDataServiceBase.IsBatchSqlSupported))]
        public void Test_Create_Multiline_Script_Without_Terminator_In_LastLine()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);
            localVersionService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);

            string sqlFileName = "Test_Single_Run_Single_Standard";
            string sqlObjectName1 = "Test_Object_1";
            string sqlObjectName2 = "Test_Object_2";
            string sqlObjectName3 = "Test_Object_3";

            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"{sqlFileName}.sql"), _testDataService.GetSqlForMultilineWithoutTerminatorInLastLine(sqlObjectName1, sqlObjectName2, sqlObjectName3));

            //act
            var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
            migrationService.Initialize(_testConfiguration.ConnectionString);
            migrationService.Run(_testConfiguration.WorkspacePath, "v1.00", autoCreateDatabase: true);

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, $"{sqlObjectName1}").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, $"{sqlObjectName2}").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, $"{sqlObjectName3}").ShouldBeTrue();
        }

        [TestMethodEx(Requires = nameof(TestDataServiceBase.IsBatchSqlSupported))]

        public void Test_Create_Multiline_Script_With_Terminator_Inside_Statements()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);
            localVersionService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);

            string sqlFileName = "Test_Single_Run_Single_Standard";
            string sqlObjectName1 = "Test_Object_1";
            string sqlObjectName2 = "Test_Object_2";
            string sqlObjectName3 = "Test_Object_3";

            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"{sqlFileName}.sql"), _testDataService.GetSqlForMultilineWithTerminatorInsideStatements(sqlObjectName1, sqlObjectName2, sqlObjectName3));

            //act
            var migrationService = _migrationServiceFactory.Create(_testConfiguration.Platform);
            migrationService.Initialize(_testConfiguration.ConnectionString);
            migrationService.Run(_testConfiguration.WorkspacePath, "v1.00", autoCreateDatabase: true);

            //assert
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, $"{sqlObjectName1}").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, $"{sqlObjectName2}").ShouldBeTrue();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, $"{sqlObjectName3}").ShouldBeTrue();
        }

        [TestMethodEx(Requires = nameof(TestDataServiceBase.IsBatchSqlSupported))]
        public void Test_Create_Multiline_Script_With_Error_Must_Rollback()
        {
            //arrange
            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(_testConfiguration.WorkspacePath);
            localVersionService.IncrementMajorVersion(_testConfiguration.WorkspacePath, null);

            string sqlFileName = "Test_Single_Run_Failed_Script_Must_Rollback";
            string sqlObjectName1 = "Test_Object_1";
            string sqlObjectName2 = "Test_Object_2";
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(_testConfiguration.WorkspacePath, "v1.00"), $"{sqlFileName}.sql"), _testDataService.GetSqlForMultilineWithError(sqlObjectName1, sqlObjectName2));

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
            _testDataService.GetCurrentDbVersion(_testConfiguration.ConnectionString).ShouldBeNull();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, $"{sqlObjectName1}").ShouldBeFalse();
            _testDataService.CheckIfDbObjectExist(_testConfiguration.ConnectionString, $"{sqlObjectName2}").ShouldBeFalse();
        }

    }
}
