using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Shouldly;
using Yuniql.Core;
using Moq;
using Yuniql.Extensibility;
using System.Data;
using System.IO;
using System;

namespace Yuniql.UnitTests
{
    [TestClass]
    public class MigrationServiceTests
    {
        [TestMethod]
        public void Test_Run_Empty_Workspace_All_Working_Scenarios()
        {
            //arrange
            var transaction = new Mock<IDbTransaction>();

            var connection = new Mock<IDbConnection>();
            connection.Setup(s => s.BeginTransaction()).Returns(transaction.Object);

            var dataService = new Mock<IDataService>();
            dataService.Setup(s => s.GetConnectionInfo()).Returns(new ConnectionInfo { DataSource = "server", Database = "db" });
            dataService.Setup(s => s.IsTargetDatabaseExists()).Returns(false);
            dataService.Setup(s => s.CreateDatabase());
            dataService.Setup(s => s.IsTargetDatabaseConfigured()).Returns(false);
            dataService.Setup(s => s.ConfigureDatabase());
            dataService.Setup(s => s.GetAllVersions()).Returns(new List<DbVersion> { });
            dataService.Setup(s => s.GetCurrentVersion()).Returns(string.Empty);
            dataService.Setup(s => s.CreateConnection()).Returns(connection.Object);

            dataService.Setup(s => s.BreakStatements("SELECT 'init'")).Returns(new List<string> { "SELECT 'init'" });
            dataService.Setup(s => s.BreakStatements("SELECT 'pre'")).Returns(new List<string> { "SELECT 'pre'" });
            dataService.Setup(s => s.BreakStatements("SELECT 'post'")).Returns(new List<string> { "SELECT 'post'" });
            dataService.Setup(s => s.BreakStatements("SELECT 'draft'")).Returns(new List<string> { "SELECT 'draft'" });
            dataService.Setup(s => s.BreakStatements("SELECT 'v0.00'")).Returns(new List<string> { "SELECT 'v0.00'" });

            dataService.Setup(s => s.ExecuteNonQuery("sql-connection-string", "SELECT 1"));
            dataService.Setup(s => s.UpdateVersion(connection.Object, transaction.Object, "v0.00"));

            var bulkImportService = new Mock<IBulkImportService>();
            bulkImportService.Setup(s => s.Run(connection.Object, transaction.Object, "file.csv", ","));

            var directoryService = new Mock<IDirectoryService>();
            var fileService = new Mock<IFileService>();

            directoryService.Setup(s => s.GetDirectories(@"c:\temp", "v*.*")).Returns(new string[] { @"c:\temp\v0.00" });

            directoryService.Setup(s => s.GetFiles(@"c:\temp\_init", "*.sql")).Returns(new string[] { @"c:\temp\_init\sql_init.sql" });
            directoryService.Setup(s => s.GetFiles(@"c:\temp\_pre", "*.sql")).Returns(new string[] { @"c:\temp\_pre\sql_pre.sql" });
            directoryService.Setup(s => s.GetFiles(@"c:\temp\_post", "*.sql")).Returns(new string[] { @"c:\temp\_post\sql_post.sql" });
            directoryService.Setup(s => s.GetFiles(@"c:\temp\_draft", "*.sql")).Returns(new string[] { @"c:\temp\_draft\sql_draft.sql" });
            directoryService.Setup(s => s.GetFiles(@"c:\temp\v0.00", "*.sql")).Returns(new string[] { @"c:\temp\v0.00\sql_v0_00.sql" });

            directoryService.Setup(s => s.GetDirectories(@"c:\temp\v0.00", "*", SearchOption.AllDirectories)).Returns(new string[] { });

            directoryService.Setup(s => s.GetFiles(@"c:\temp\v0.00", "*.csv")).Returns(new string[] { @"c:\temp\v0.00\file.csv" });

            fileService.Setup(s => s.ReadAllText(@"c:\temp\_init\sql_init.sql")).Returns("SELECT 'init'");
            fileService.Setup(s => s.ReadAllText(@"c:\temp\_pre\sql_pre.sql")).Returns("SELECT 'pre'");
            fileService.Setup(s => s.ReadAllText(@"c:\temp\_post\sql_post.sql")).Returns("SELECT 'post'");
            fileService.Setup(s => s.ReadAllText(@"c:\temp\_draft\sql_draft.sql")).Returns("SELECT 'draft'");
            fileService.Setup(s => s.ReadAllText(@"c:\temp\v0.00\sql_v0_00.sql")).Returns("SELECT 'v0.00'");

            var tokenReplacementService = new Mock<ITokenReplacementService>();
            tokenReplacementService.Setup(s => s.Replace(It.IsAny<List<KeyValuePair<string, string>>>(), "SELECT 'init'")).Returns("SELECT 'init'");
            tokenReplacementService.Setup(s => s.Replace(It.IsAny<List<KeyValuePair<string, string>>>(), "SELECT 'pre'")).Returns("SELECT 'pre'"); ;
            tokenReplacementService.Setup(s => s.Replace(It.IsAny<List<KeyValuePair<string, string>>>(), "SELECT 'post'")).Returns("SELECT 'post'"); ;
            tokenReplacementService.Setup(s => s.Replace(It.IsAny<List<KeyValuePair<string, string>>>(), "SELECT 'draft'")).Returns("SELECT 'draft'"); ;
            tokenReplacementService.Setup(s => s.Replace(It.IsAny<List<KeyValuePair<string, string>>>(), "SELECT 'v0.00'")).Returns("SELECT 'v0.00'"); ;

            var traceService = new Mock<ITraceService>();

            var tokenKeyPairs = new List<KeyValuePair<string, string>> {
               new KeyValuePair<string, string>("Token1","TokenValue1"),
               new KeyValuePair<string, string>("Token2","TokenValue2"),
               new KeyValuePair<string, string>("Token3","TokenValue3"),
            };

            //act
            var sut = new MigrationService(
                dataService.Object,
                bulkImportService.Object,
                tokenReplacementService.Object,
                directoryService.Object,
                fileService.Object,
                traceService.Object);
            sut.Run(workingPath: @"c:\temp", targetVersion: "v0.00", autoCreateDatabase: true, tokenKeyPairs: tokenKeyPairs, verifyOnly: false);

            //asset
            dataService.Verify(s => s.GetConnectionInfo());
            dataService.Verify(s => s.IsTargetDatabaseExists());
            dataService.Verify(s => s.CreateDatabase());
            dataService.Verify(s => s.IsTargetDatabaseConfigured());
            dataService.Verify(s => s.ConfigureDatabase());

            dataService.Verify(s => s.GetAllVersions());
            dataService.Verify(s => s.GetCurrentVersion());
            dataService.Verify(s => s.CreateConnection());

            directoryService.Verify(s => s.GetDirectories(@"c:\temp", "v*.*"));
            directoryService.Verify(s => s.GetFiles(@"c:\temp\_init", "*.sql"));
            directoryService.Verify(s => s.GetFiles(@"c:\temp\_pre", "*.sql"));
            directoryService.Verify(s => s.GetFiles(@"c:\temp\_post", "*.sql"));
            directoryService.Verify(s => s.GetFiles(@"c:\temp\_draft", "*.sql"));
            directoryService.Verify(s => s.GetFiles(@"c:\temp\v0.00", "*.sql"));
            directoryService.Verify(s => s.GetDirectories(@"c:\temp\v0.00", "*", SearchOption.AllDirectories));
            directoryService.Verify(s => s.GetFiles(@"c:\temp\v0.00", "*.csv"));

            fileService.Verify(s => s.ReadAllText(@"c:\temp\_init\sql_init.sql"));
            fileService.Verify(s => s.ReadAllText(@"c:\temp\_pre\sql_pre.sql"));
            fileService.Verify(s => s.ReadAllText(@"c:\temp\_post\sql_post.sql"));
            fileService.Verify(s => s.ReadAllText(@"c:\temp\_draft\sql_draft.sql"));
            fileService.Verify(s => s.ReadAllText(@"c:\temp\v0.00\sql_v0_00.sql"));

            dataService.Verify(s => s.BreakStatements("SELECT 'init'"));
            dataService.Verify(s => s.BreakStatements("SELECT 'pre'"));
            dataService.Verify(s => s.BreakStatements("SELECT 'post'"));
            dataService.Verify(s => s.BreakStatements("SELECT 'draft'"));
            dataService.Verify(s => s.BreakStatements("SELECT 'v0.00'"));

            tokenReplacementService.Verify(s => s.Replace(It.Is<List<KeyValuePair<string, string>>>(x =>
               x[0].Key == "Token1" && x[0].Value == "TokenValue1"
               && x[1].Key == "Token2" && x[1].Value == "TokenValue2"
               && x[2].Key == "Token3" && x[2].Value == "TokenValue3"
            ), "SELECT 'init'"));

            tokenReplacementService.Verify(s => s.Replace(It.Is<List<KeyValuePair<string, string>>>(x =>
               x[0].Key == "Token1" && x[0].Value == "TokenValue1"
               && x[1].Key == "Token2" && x[1].Value == "TokenValue2"
               && x[2].Key == "Token3" && x[2].Value == "TokenValue3"
            ), "SELECT 'pre'"));


            tokenReplacementService.Verify(s => s.Replace(It.Is<List<KeyValuePair<string, string>>>(x =>
               x[0].Key == "Token1" && x[0].Value == "TokenValue1"
               && x[1].Key == "Token2" && x[1].Value == "TokenValue2"
               && x[2].Key == "Token3" && x[2].Value == "TokenValue3"
            ), "SELECT 'post'"));

            tokenReplacementService.Verify(s => s.Replace(It.Is<List<KeyValuePair<string, string>>>(x =>
               x[0].Key == "Token1" && x[0].Value == "TokenValue1"
               && x[1].Key == "Token2" && x[1].Value == "TokenValue2"
               && x[2].Key == "Token3" && x[2].Value == "TokenValue3"
            ), "SELECT 'draft'"));

            tokenReplacementService.Verify(s => s.Replace(It.Is<List<KeyValuePair<string, string>>>(x =>
               x[0].Key == "Token1" && x[0].Value == "TokenValue1"
               && x[1].Key == "Token2" && x[1].Value == "TokenValue2"
               && x[2].Key == "Token3" && x[2].Value == "TokenValue3"
            ), "SELECT 'v0.00'"));

            dataService.Verify(s => s.ExecuteNonQuery(It.IsAny<IDbConnection>(), "SELECT 'init'", It.IsAny<IDbTransaction>()));
            dataService.Verify(s => s.ExecuteNonQuery(It.IsAny<IDbConnection>(), "SELECT 'pre'", It.IsAny<IDbTransaction>()));
            dataService.Verify(s => s.ExecuteNonQuery(It.IsAny<IDbConnection>(), "SELECT 'post'", It.IsAny<IDbTransaction>()));
            dataService.Verify(s => s.ExecuteNonQuery(It.IsAny<IDbConnection>(), "SELECT 'draft'", It.IsAny<IDbTransaction>()));
            dataService.Verify(s => s.ExecuteNonQuery(It.IsAny<IDbConnection>(), "SELECT 'v0.00'", It.IsAny<IDbTransaction>()));

            bulkImportService.Verify(s => s.Run(It.IsAny<IDbConnection>(), It.IsAny<IDbTransaction>(), @"c:\temp\v0.00\file.csv", ","));

            dataService.Verify(s => s.UpdateVersion(It.IsAny<IDbConnection>(), It.IsAny<IDbTransaction>(), "v0.00"));

            connection.Verify(s => s.Open());
            connection.Verify(s => s.BeginTransaction());
            transaction.Verify(s => s.Commit());
        }

        [TestMethod]
        public void Test_Erase()
        {
            //arrange
            var transaction = new Mock<IDbTransaction>();

            var connection = new Mock<IDbConnection>();
            connection.Setup(s => s.BeginTransaction()).Returns(transaction.Object);

            var dataService = new Mock<IDataService>();
            dataService.Setup(s => s.CreateConnection()).Returns(connection.Object);
            dataService.Setup(s => s.BreakStatements("SELECT 'erase'")).Returns(new List<string> { "SELECT 'erase'" });
            dataService.Setup(s => s.ExecuteNonQuery("sql-connection-string", "SELECT erase"));

            var bulkImportService = new Mock<IBulkImportService>();
            var directoryService = new Mock<IDirectoryService>();
            var fileService = new Mock<IFileService>();

            directoryService.Setup(s => s.GetFiles(@"c:\temp\_erase", "*.sql")).Returns(new string[] { @"c:\temp\_erase\sql_erase.sql" });
            fileService.Setup(s => s.ReadAllText(@"c:\temp\_erase\sql_erase.sql")).Returns("SELECT 'erase'");

            var tokenReplacementService = new Mock<ITokenReplacementService>();
            tokenReplacementService.Setup(s => s.Replace(null, "SELECT 'erase'")).Returns("SELECT 'erase'");

            var traceService = new Mock<ITraceService>();

            //act
            var sut = new MigrationService(
                dataService.Object,
                bulkImportService.Object,
                tokenReplacementService.Object,
                directoryService.Object,
                fileService.Object,
                traceService.Object);
            sut.Erase(workingPath: @"c:\temp");

            //assert
            dataService.Verify(s => s.CreateConnection());
            directoryService.Verify(s => s.GetFiles(@"c:\temp\_erase", "*.sql"));
            fileService.Verify(s => s.ReadAllText(@"c:\temp\_erase\sql_erase.sql"));
            dataService.Verify(s => s.BreakStatements("SELECT 'erase'"));
            tokenReplacementService.Verify(s => s.Replace(null, "SELECT 'erase'"));

            dataService.Verify(s => s.ExecuteNonQuery(It.IsAny<IDbConnection>(), "SELECT 'erase'", It.IsAny<IDbTransaction>()));

            connection.Verify(s => s.Open());
            connection.Verify(s => s.BeginTransaction());
            transaction.Verify(s => s.Commit());
        }

        [TestMethod]
        public void Test_Erase_With_Error_Must_Rollback()
        {
            //arrange
            var transaction = new Mock<IDbTransaction>();

            var connection = new Mock<IDbConnection>();
            connection.Setup(s => s.BeginTransaction()).Returns(transaction.Object);

            var dataService = new Mock<IDataService>();
            dataService.Setup(s => s.CreateConnection()).Returns(connection.Object);
            dataService.Setup(s => s.BreakStatements("SELECT 'erase'")).Returns(new List<string> { "SELECT 'erase'" });
            dataService.Setup(s => s.ExecuteNonQuery("sql-connection-string", "SELECT erase"));

            var bulkImportService = new Mock<IBulkImportService>();
            var directoryService = new Mock<IDirectoryService>();
            var fileService = new Mock<IFileService>();

            directoryService.Setup(s => s.GetFiles(@"c:\temp\_erase", "*.sql")).Returns(new string[] { @"c:\temp\_erase\sql_erase.sql" });

            //simulates that an exception happens while erase is executing
            fileService.Setup(s => s.ReadAllText(@"c:\temp\_erase\sql_erase.sql")).Throws(new ApplicationException("Fake exception"));

            var tokenReplacementService = new Mock<ITokenReplacementService>();
            tokenReplacementService.Setup(s => s.Replace(null, "SELECT 'erase'")).Returns("SELECT 'erase'");

            var traceService = new Mock<ITraceService>();

            //act
            Assert.ThrowsException<ApplicationException>(() =>
            {
                var sut = new MigrationService(
                    dataService.Object,
                    bulkImportService.Object,
                    tokenReplacementService.Object,
                    directoryService.Object,
                    fileService.Object,
                    traceService.Object);
                sut.Erase(workingPath: @"c:\temp");
            }).Message.ShouldBe("Fake exception");

            //assert
            dataService.Verify(s => s.CreateConnection());
            directoryService.Verify(s => s.GetFiles(@"c:\temp\_erase", "*.sql"));
            fileService.Verify(s => s.ReadAllText(@"c:\temp\_erase\sql_erase.sql"));
            connection.Verify(s => s.Open());
            connection.Verify(s => s.BeginTransaction());
            transaction.Verify(s => s.Rollback());
        }
    }
}
