using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Yuniql.MySql;
using Yuniql.Core;
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
            var connectionString = @$"Server=localhost;Port=3306;Database=yuniqldb_{Guid.NewGuid().ToString().Substring(0, 4)};Uid=root;Pwd=P@ssw0rd!;";

            var traceService = new ConsoleTraceService();
            var tokenReplacementService = new TokenReplacementService(traceService);

            var dataService = new MySqlDataService(traceService);
            dataService.Initialize(connectionString);

            var bulkImportService = new MySqlBulkImportService(traceService);
            bulkImportService.Initialize(connectionString);

            //act
            var migrationService = new MigrationService(dataService, bulkImportService, tokenReplacementService, new DirectoryService(), new FileService(), traceService);
            migrationService.Run(workspacePath, null, true, tokenKeyPairs: null, verifyOnly: false);
        }
    }
}
