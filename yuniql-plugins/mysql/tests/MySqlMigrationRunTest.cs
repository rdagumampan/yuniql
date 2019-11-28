using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Yuniql.MySql;
using Yuniql.Core;
using Yuniql.Extensibility;
using Shouldly;

namespace Yuniql.PosgreSQL.Tests
{
    [TestClass]
    public class MySqlMigrationRunTest
    {
        [TestMethod]
        public void TestBasicRun()
        {
            //arrange
            //uses the samples project in the same directory as this test project
            var workspacePath = Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory).ToString()).ToString()).ToString()).ToString(), "samples");
            var connectionString = @$"Host=localhost;Port=5432;Username=app;Password=app;Database=yuniqldb_{Guid.NewGuid().ToString().Substring(0,4)}";

            var traceService = new ConsoleTraceService();
            var dataService = new MySqlDataService(traceService);
            dataService.Initialize(connectionString);

            var bulkImportService = new MySqlBulkImportService(traceService);
            bulkImportService.Initialize(connectionString);

            var testDataService = new MySqlTestDataService(dataService);

            //act
            var migrationService = new MigrationService(dataService, bulkImportService, traceService);
            migrationService.Run(workspacePath, null, true, tokenKeyPairs: null, verifyOnly: false);

            //assert
            testDataService.CheckIfDbExist(connectionString).ShouldBeTrue();
            testDataService.CheckIfDbObjectExist(connectionString, "company").ShouldBeTrue();
            testDataService.CheckIfDbObjectExist(connectionString, "company_view").ShouldBeTrue();
        }
    }
}
