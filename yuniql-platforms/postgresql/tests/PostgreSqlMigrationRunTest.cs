using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Yuniql.PostgreSql;
using Yuniql.Core;
using Shouldly;

namespace Yuniql.PosgreSQL.Tests
{
    [TestClass]
    public class PostgreSqlMigrationRunTest
    {
        [TestMethod]
        public void TestBasicRun()
        {
            //arrange
            //uses the samples project in the same directory as this test project
            var workspacePath = Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory).ToString()).ToString()).ToString()).ToString(), "samples");
            var connectionString = @$"Host=localhost;Port=5432;Username=app;Password=P@ssw0rd!;Database=yuniqldb_{Guid.NewGuid().ToString().Substring(0,4)}";

            var traceService = new ConsoleTraceService();
            var tokenReplacementService = new TokenReplacementService(traceService);

            var dataService = new PostgreSqlDataService(traceService);
            dataService.Initialize(connectionString);

            var bulkImportService = new PostgreSqlBulkImportService(traceService);
            bulkImportService.Initialize(connectionString);

            //act
            var migrationService = new MigrationService(dataService, bulkImportService, tokenReplacementService, new DirectoryService(), new FileService(), traceService);
            migrationService.Run(workspacePath, null, true, tokenKeyPairs: null, verifyOnly: false);
        }
    }
}
