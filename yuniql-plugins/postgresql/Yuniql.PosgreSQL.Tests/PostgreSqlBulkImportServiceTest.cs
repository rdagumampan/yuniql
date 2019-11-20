using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Yuniql.Extensibility;
using Yuniql.PostgreSql;

namespace Yuniql.PosgreSQL.Tests
{
    [TestClass]
    public class PostgreSqlBulkImportServiceTest
    {
        [TestMethod]
        public void TestBulkImport()
        {
            //arrange
            var workingPath = GetOrCreateWorkingPath();
            var traceService = new TraceService();

            var bulkFile = Path.Combine(Environment.CurrentDirectory, "TestCsv.csv");
            var connectionString = "Host=localhost;Port=5432;Username=app;Password=app;Database=mydbname";

            //act
            var sut = new PostgreSqlBulkImportService(traceService);
            sut.Initialize(connectionString);

            using(var connection = new Npgsql.NpgsqlConnection(connectionString))
            {
                connection.Open();
                sut.Run(connection, null, bulkFile);
            }


            //assert
        }

        public string GetOrCreateWorkingPath()
        {
            var workingPath = Path.Combine(Environment.CurrentDirectory, @$"yuniql_testdb_{Guid.NewGuid().ToString().Substring(0, 4)}"); ;
            if (!Directory.Exists(workingPath))
            {
                Directory.CreateDirectory(workingPath);
            }

            return workingPath;
        }
    }

    public class TraceService : ITraceService
    {
        public bool IsDebugEnabled { get; set; }

        public void Debug(string message, object payload = null)
        {
            Console.WriteLine($"DBG {message}");
        }

        public void Error(string message, object payload = null)
        {
            Console.WriteLine($"ERR {message}");
        }

        public void Info(string message, object payload = null)
        {
            Console.WriteLine($"INF {message}");
        }
    }
}
