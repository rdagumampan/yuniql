using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;
using System.IO;
using Shouldly;
using Yuniql.Core;
using Yuniql.Extensibility;

namespace Yuniql.SqlServer.Tests
{
    [TestClass]
    public class BreakdownScriptTests
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
        public void Test_Single_Run_Empty()
        {
            //arrange
            var workingPath = TestDbHelper.GetWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = TestDbHelper.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);
            localVersionService.IncrementMajorVersion(workingPath, null);

            string sqlStatement = $@"
";
            TestDbHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.00"), $"Test_Single_Run_Empty.sql"), sqlStatement);

            //act
            var migrationService = _migrationServiceFactory.Create("sqlserver");
            migrationService.Initialize(connectionString);
            migrationService.Run(workingPath, "v1.00", autoCreateDatabase: true);

            //assert
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript("Test_Single_Run_Empty")).ShouldBeFalse();
        }

        [TestMethod]
        public void Test_Single_Run_Single_Standard()
        {
            //arrange
            var workingPath = TestDbHelper.GetWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = TestDbHelper.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);
            localVersionService.IncrementMajorVersion(workingPath, null);

            string sqlObjectName = "Test_Single_Run_Single_Standard";
            string sqlStatement = $@"
CREATE PROC [dbo].[{sqlObjectName}]
AS
    SELECT 1;
GO
";
            TestDbHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.00"), $"{sqlObjectName}.sql"), sqlStatement);

            //act
            var migrationService = _migrationServiceFactory.Create("sqlserver");
            migrationService.Initialize(connectionString);
            migrationService.Run(workingPath, "v1.00", autoCreateDatabase: true);

            //assert
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript($"{sqlObjectName}")).ShouldBeTrue();
        }
        [TestMethod]
        public void Test_Run_Single_Without_GO()
        {
            //arrange
            var workingPath = TestDbHelper.GetWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = TestDbHelper.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);
            localVersionService.IncrementMajorVersion(workingPath, null);

            string sqlObjectName = "Test_Single_Run_Single_Standard";
            string sqlStatement = $@"
CREATE PROC [dbo].[{sqlObjectName}]
AS
    SELECT 1;
";
            TestDbHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.00"), $"{sqlObjectName}.sql"), sqlStatement);

            //act
            var migrationService = _migrationServiceFactory.Create("sqlserver");
            migrationService.Initialize(connectionString);
            migrationService.Run(workingPath, "v1.00", autoCreateDatabase: true);

            //assert
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript($"{sqlObjectName}")).ShouldBeTrue();
        }
        [TestMethod]
        public void Test_Run_Multiple_Without_GO_In_Last_Line()
        {
            //arrange
            var workingPath = TestDbHelper.GetWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = TestDbHelper.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);
            localVersionService.IncrementMajorVersion(workingPath, null);

            string sqlFileName = "Test_Single_Run_Single_Standard";
            string sqlObjectName1 = "Test_Single_Run_Single_Standard_1";
            string sqlObjectName2 = "Test_Single_Run_Single_Standard_2";
            string sqlObjectName3 = "Test_Single_Run_Single_Standard_3";

            string sqlStatement = $@"
CREATE PROC [dbo].[{sqlObjectName1}]
AS
    SELECT 1;
GO

CREATE PROC [dbo].[{sqlObjectName2}]
AS
    SELECT 1;
GO

CREATE PROC [dbo].[{sqlObjectName3}]
AS
    SELECT 1;
";
            TestDbHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.00"), $"{sqlFileName}.sql"), sqlStatement);

            //act
            var migrationService = _migrationServiceFactory.Create("sqlserver");
            migrationService.Initialize(connectionString);
            migrationService.Run(workingPath, "v1.00", autoCreateDatabase: true);

            //assert
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript($"{sqlObjectName1}")).ShouldBeTrue();
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript($"{sqlObjectName2}")).ShouldBeTrue();
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript($"{sqlObjectName3}")).ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Run_Multiple_With_GO_In_The_Sql_Statement()
        {
            //arrange
            var workingPath = TestDbHelper.GetWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = TestDbHelper.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);
            localVersionService.IncrementMajorVersion(workingPath, null);

            string sqlFileName = "Test_Single_Run_Single_Standard";
            string sqlObjectName1 = "Test_Single_Run_Single_Standard_1";
            string sqlObjectName2 = "Test_Single_Run_Single_Standard_2";
            string sqlObjectName3 = "Test_Single_Run_Single_Standard_3";

            string sqlStatement = $@"
CREATE PROC [dbo].[{sqlObjectName1}]
AS
    --this is a comment with GO as part of the sentence (ALL CAPS)
    SELECT 1;
GO

CREATE PROC [dbo].[{sqlObjectName2}]
AS
    --this is a comment with go as part of the sentence (small caps)
    SELECT 1;
GO

CREATE PROC [dbo].[{sqlObjectName3}]
AS
    --this is a comment with Go as part of the sentence (Pascal)
    SELECT 1;
";
            TestDbHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.00"), $"{sqlFileName}.sql"), sqlStatement);

            //act
            var migrationService = _migrationServiceFactory.Create("sqlserver");
            migrationService.Initialize(connectionString);
            migrationService.Run(workingPath, "v1.00", autoCreateDatabase: true);

            //assert
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript($"{sqlObjectName1}")).ShouldBeTrue();
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript($"{sqlObjectName2}")).ShouldBeTrue();
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript($"{sqlObjectName3}")).ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Single_Run_Failed_Script_Must_Rollback()
        {
            //arrange
            var workingPath = TestDbHelper.GetWorkingPath();
            var databaseName = new DirectoryInfo(workingPath).Name;
            var connectionString = TestDbHelper.GetConnectionString(databaseName);

            var localVersionService = new LocalVersionService(_traceService);
            localVersionService.Init(workingPath);
            localVersionService.IncrementMajorVersion(workingPath, null);

            string sqlFileName = "Test_Single_Run_Failed_Script_Must_Rollback";
            string sqlStatement = $@"
CREATE TABLE [dbo].[_TestTable](        
    [TestId][INT] IDENTITY(1, 1) NOT NULL,        
    [TestColumn] [DECIMAL] NOT NULL
)
GO

CREATE PROC [dbo].[_TestStoredProcedure]
AS
    SELECT 1;
GO

INSERT INTO [dbo].[_TestTable] (TestColumn) VALUES (1);
INSERT INTO [dbo].[_TestTable] (TestColumn) VALUES (2);
GO

--throws divide by zero error
INSERT INTO [dbo].[_TestTable] (TestColumn) VALUES (3/0);
GO
";
            TestDbHelper.CreateScriptFile(Path.Combine(Path.Combine(workingPath, "v1.00"), $"{sqlFileName}.sql"), sqlStatement);

            //act
            var migrationService = _migrationServiceFactory.Create("sqlserver");
            migrationService.Initialize(connectionString);
            Assert.ThrowsException<SqlException>(() =>
            {
                migrationService.Run(workingPath, "v1.00", autoCreateDatabase: true);
            }).Message.ShouldContain("Divide by zero error encountered");

            //assert
            TestDbHelper.GetCurrentVersion(connectionString).ShouldBeNull();
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript($"TestTable")).ShouldBeFalse();
            TestDbHelper.QuerySingleBool(connectionString, TestDbHelper.CreateCheckDbObjectExistScript($"TestStoredProcedure")).ShouldBeFalse();
        }

    }
}
