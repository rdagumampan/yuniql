using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;
using System.IO;
using Shouldly;
using Yuniql.Core;
using Yuniql.Extensibility;

namespace Yuniql.SqlServer.Tests
{
    [TestClass]
    public class BreakdownScriptTests: TestBase
    {
        private string _targetPlatform;
        private ITestDataService _testDataService;

        private IMigrationServiceFactory _migrationServiceFactory;
        private ITraceService _traceService;

        [TestInitialize]
        public void Setup()
        {
            //get target platform to tests from environment variable
            _targetPlatform = EnvironmentHelper.GetEnvironmentVariable("YUNIQL_TEST_TARGET_PLATFORM");
            if (string.IsNullOrEmpty(_targetPlatform))
            {
                _targetPlatform = "sqlserver";
            }

            //create test data service provider
            var testDataServiceFactory = new TestDataServiceFactory();
            _testDataService = testDataServiceFactory.Create(_targetPlatform);

            //create data service factory for migration proper
            _traceService = new TraceService();
            _migrationServiceFactory = new MigrationServiceFactory(_traceService);
        }

        [TestMethod]
        public void Test_Single_Run_Empty()
        {
            //arrange
            var workingPath = GetOrCreateWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = _testDataService.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);
            localVersionService.IncrementMajorVersion(workingPath, null);

            string sqlStatement = $@"
";
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.00"), $"Test_Single_Run_Empty.sql"), sqlStatement);

            //act
            var migrationService = _migrationServiceFactory.Create(_targetPlatform);
            migrationService.Initialize(connectionString);
            migrationService.Run(workingPath, "v1.00", autoCreateDatabase: true);

            //assert
            _testDataService.QuerySingleBool(connectionString, _testDataService.CreateCheckDbObjectExistScript("Test_Single_Run_Empty")).ShouldBeFalse();
        }

        [TestMethod]
        public void Test_Single_Run_Single_Standard()
        {
            //arrange
            var workingPath = GetOrCreateWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = _testDataService.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);
            localVersionService.IncrementMajorVersion(workingPath, null);

            string sqlObjectName = "Test_Object_1";
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.00"), $"{sqlObjectName}.sql"), _testDataService.CreateSingleLineScript(sqlObjectName));

            //act
            var migrationService = _migrationServiceFactory.Create(_targetPlatform);
            migrationService.Initialize(connectionString);
            migrationService.Run(workingPath, "v1.00", autoCreateDatabase: true);

            //assert
            _testDataService.QuerySingleBool(connectionString, _testDataService.CreateCheckDbObjectExistScript($"{sqlObjectName}")).ShouldBeTrue();
        }
        [TestMethod]
        public void Test_Run_Single_Without_GO()
        {
            //arrange
            var workingPath = GetOrCreateWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = _testDataService.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);
            localVersionService.IncrementMajorVersion(workingPath, null);

            string sqlObjectName = "Test_Object_1";
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.00"), $"{sqlObjectName}.sql"), _testDataService.CreateSingleLineScriptWithoutTerminator(sqlObjectName));

            //act
            var migrationService = _migrationServiceFactory.Create(_targetPlatform);
            migrationService.Initialize(connectionString);
            migrationService.Run(workingPath, "v1.00", autoCreateDatabase: true);

            //assert
            _testDataService.QuerySingleBool(connectionString, _testDataService.CreateCheckDbObjectExistScript($"{sqlObjectName}")).ShouldBeTrue();
        }
        [TestMethod]
        public void Test_Run_Multiple_Without_GO_In_Last_Line()
        {
            //arrange
            var workingPath = GetOrCreateWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = _testDataService.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);
            localVersionService.IncrementMajorVersion(workingPath, null);

            string sqlFileName = "Test_Single_Run_Single_Standard";
            string sqlObjectName1 = "Test_Object_1";
            string sqlObjectName2 = "Test_Object_2";
            string sqlObjectName3 = "Test_Object_3";

            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.00"), $"{sqlFileName}.sql"), _testDataService.CreateMultilineScriptWithoutTerminatorInLastLine(sqlObjectName1, sqlObjectName2, sqlObjectName3));

            //act
            var migrationService = _migrationServiceFactory.Create(_targetPlatform);
            migrationService.Initialize(connectionString);
            migrationService.Run(workingPath, "v1.00", autoCreateDatabase: true);

            //assert
            _testDataService.QuerySingleBool(connectionString, _testDataService.CreateCheckDbObjectExistScript($"{sqlObjectName1}")).ShouldBeTrue();
            _testDataService.QuerySingleBool(connectionString, _testDataService.CreateCheckDbObjectExistScript($"{sqlObjectName2}")).ShouldBeTrue();
            _testDataService.QuerySingleBool(connectionString, _testDataService.CreateCheckDbObjectExistScript($"{sqlObjectName3}")).ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_Multiple_With_GO_In_The_Sql_Statement()
        {
            //arrange
            var workingPath = GetOrCreateWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = _testDataService.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);
            localVersionService.IncrementMajorVersion(workingPath, null);

            string sqlFileName = "Test_Single_Run_Single_Standard";
            string sqlObjectName1 = "Test_Object_1";
            string sqlObjectName2 = "Test_Object_2";
            string sqlObjectName3 = "Test_Object_3";

            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.00"), $"{sqlFileName}.sql"), _testDataService.CreateMultilineScriptWithTerminatorInsideStatements(sqlObjectName1, sqlObjectName2, sqlObjectName3));

            //act
            var migrationService = _migrationServiceFactory.Create(_targetPlatform);
            migrationService.Initialize(connectionString);
            migrationService.Run(workingPath, "v1.00", autoCreateDatabase: true);

            //assert
            _testDataService.QuerySingleBool(connectionString, _testDataService.CreateCheckDbObjectExistScript($"{sqlObjectName1}")).ShouldBeTrue();
            _testDataService.QuerySingleBool(connectionString, _testDataService.CreateCheckDbObjectExistScript($"{sqlObjectName2}")).ShouldBeTrue();
            _testDataService.QuerySingleBool(connectionString, _testDataService.CreateCheckDbObjectExistScript($"{sqlObjectName3}")).ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Single_Run_Failed_Script_Must_Rollback()
        {
            //arrange
            var workingPath = GetOrCreateWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = _testDataService.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);
            localVersionService.IncrementMajorVersion(workingPath, null);

            string sqlFileName = "Test_Single_Run_Failed_Script_Must_Rollback";
            string sqlObjectName1 = "Test_Object_1";
            string sqlObjectName2 = "Test_Object_2";
            _testDataService.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.00"), $"{sqlFileName}.sql"), _testDataService.CreateMultilineScriptWithError(sqlObjectName1, sqlObjectName2));

            //act
            var migrationService = _migrationServiceFactory.Create(_targetPlatform);
            migrationService.Initialize(connectionString);
            Assert.ThrowsException<SqlException>(() =>
            {
                migrationService.Run(workingPath, "v1.00", autoCreateDatabase: true);
            }).Message.ShouldContain("Divide by zero error encountered");

            //assert
            _testDataService.GetCurrentVersion(connectionString).ShouldBeNull();
            _testDataService.QuerySingleBool(connectionString, _testDataService.CreateCheckDbObjectExistScript($"{sqlObjectName1}")).ShouldBeFalse();
            _testDataService.QuerySingleBool(connectionString, _testDataService.CreateCheckDbObjectExistScript($"{sqlObjectName2}")).ShouldBeFalse();
        }

    }
}
