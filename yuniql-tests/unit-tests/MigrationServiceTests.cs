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

            var localVersionService = new Mock<ILocalVersionService>();
            localVersionService.Setup(s => s.Validate(@"c:\temp")).Verifiable();

            var configurationService = new Mock<IConfigurationDataService>();
            configurationService.Setup(s => s.IsDatabaseExists(null)).Returns(false);
            configurationService.Setup(s => s.CreateDatabase(null));
            configurationService.Setup(s => s.IsDatabaseConfigured(null)).Returns(false);
            configurationService.Setup(s => s.ConfigureDatabase(null));
            configurationService.Setup(s => s.GetAllAppliedVersions(null)).Returns(new List<DbVersion> { });
            configurationService.Setup(s => s.GetCurrentVersion(null)).Returns(string.Empty);
            configurationService.Setup(s => s.InsertVersion(connection.Object, transaction.Object, "v0.00", null, null, null, null, null));

            configurationService.Setup(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 1", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));
            configurationService.Setup(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'init'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));
            configurationService.Setup(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'pre'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));
            configurationService.Setup(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'post'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));
            configurationService.Setup(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'draft'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));
            configurationService.Setup(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'v0.00'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));

            var dataService = new Mock<IDataService>();
            dataService.Setup(s => s.GetConnectionInfo()).Returns(new ConnectionInfo { DataSource = "server", Database = "db" });
            dataService.Setup(s => s.CreateConnection()).Returns(connection.Object);

            dataService.Setup(s => s.BreakStatements("SELECT 'init'")).Returns(new List<string> { "SELECT 'init'" });
            dataService.Setup(s => s.BreakStatements("SELECT 'pre'")).Returns(new List<string> { "SELECT 'pre'" });
            dataService.Setup(s => s.BreakStatements("SELECT 'post'")).Returns(new List<string> { "SELECT 'post'" });
            dataService.Setup(s => s.BreakStatements("SELECT 'draft'")).Returns(new List<string> { "SELECT 'draft'" });
            dataService.Setup(s => s.BreakStatements("SELECT 'v0.00'")).Returns(new List<string> { "SELECT 'v0.00'" });

            var bulkImportService = new Mock<IBulkImportService>();
            bulkImportService.Setup(s => s.Run(connection.Object, transaction.Object, "file.csv", DefaultConstants.Delimiter, DefaultConstants.BatchSize, DefaultConstants.CommandTimeoutSecs));

            var directoryService = new Mock<IDirectoryService>();
            var fileService = new Mock<IFileService>();

            directoryService.Setup(s => s.GetDirectories(@"c:\temp", "v*.*")).Returns(new string[] { @"c:\temp\v0.00" });

            directoryService.Setup(s => s.GetAllFiles(@"c:\temp\_init", "*.sql")).Returns(new string[] { @"c:\temp\_init\sql_init.sql" });
            directoryService.Setup(s => s.FilterFiles(@"c:\temp\_init", null, It.Is<List<string>>(f=> f.Contains(@"c:\temp\_init\sql_init.sql")))).Returns(new string[] { @"c:\temp\_init\sql_init.sql" });

            directoryService.Setup(s => s.GetAllFiles(@"c:\temp\_pre", "*.sql")).Returns(new string[] { @"c:\temp\_pre\sql_pre.sql" });
            directoryService.Setup(s => s.FilterFiles(@"c:\temp\_pre", null, It.Is<List<string>>(f => f.Contains(@"c:\temp\_pre\sql_pre.sql")))).Returns(new string[] { @"c:\temp\_pre\sql_pre.sql" });

            directoryService.Setup(s => s.GetAllFiles(@"c:\temp\_post", "*.sql")).Returns(new string[] { @"c:\temp\_post\sql_post.sql" });
            directoryService.Setup(s => s.FilterFiles(@"c:\temp\_post", null, It.Is<List<string>>(f => f.Contains(@"c:\temp\_post\sql_post.sql")))).Returns(new string[] { @"c:\temp\_post\sql_post.sql" });

            directoryService.Setup(s => s.GetAllFiles(@"c:\temp\_draft", "*.sql")).Returns(new string[] { @"c:\temp\_draft\sql_draft.sql" });
            directoryService.Setup(s => s.FilterFiles(@"c:\temp\_draft", null, It.Is<List<string>>(f => f.Contains(@"c:\temp\_draft\sql_draft.sql")))).Returns(new string[] { @"c:\temp\_draft\sql_draft.sql" });

            directoryService.Setup(s => s.GetFiles(@"c:\temp\v0.00", "*.sql")).Returns(new string[] { @"c:\temp\v0.00\sql_v0_00.sql" });
            directoryService.Setup(s => s.FilterFiles(@"c:\temp", null, It.Is<List<string>>(f => f.Contains(@"c:\temp\v0.00\sql_v0_00.sql")))).Returns(new string[] { @"c:\temp\v0.00\sql_v0_00.sql" });

            directoryService.Setup(s => s.GetAllDirectories(@"c:\temp\v0.00", "*")).Returns(new string[] { });

            directoryService.Setup(s => s.GetFiles(@"c:\temp\v0.00", "*.csv")).Returns(new string[] { @"c:\temp\v0.00\file.csv" });
            directoryService.Setup(s => s.FilterFiles(@"c:\temp", null, It.Is<List<string>>(f => f.Contains(@"c:\temp\v0.00\file.csv")))).Returns(new string[] { @"c:\temp\v0.00\file.csv" });

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
                localVersionService.Object,
                dataService.Object,
                bulkImportService.Object,
                configurationService.Object,
                tokenReplacementService.Object,
                directoryService.Object,
                fileService.Object,
                traceService.Object);
            sut.Run(workingPath: @"c:\temp",
                targetVersion: "v0.00",
                autoCreateDatabase: true,
                tokenKeyPairs: tokenKeyPairs,
                verifyOnly: false);

            //asset
            localVersionService.Verify(s => s.Validate(@"c:\temp"));

            configurationService.Verify(s => s.IsDatabaseExists(null));
            configurationService.Verify(s => s.CreateDatabase(null));
            configurationService.Verify(s => s.IsDatabaseConfigured(null));
            configurationService.Verify(s => s.ConfigureDatabase(null));
            configurationService.Verify(s => s.GetAllAppliedVersions(null));
            configurationService.Verify(s => s.GetCurrentVersion(null));
            configurationService.Verify(s => s.InsertVersion(It.IsAny<IDbConnection>(), It.IsAny<IDbTransaction>(), "v0.00", null, null, null, null, null));

            dataService.Verify(s => s.GetConnectionInfo());
            dataService.Verify(s => s.CreateConnection());

            directoryService.Verify(s => s.GetDirectories(@"c:\temp", "v*.*"));
            directoryService.Verify(s => s.GetAllFiles(@"c:\temp\_init", "*.sql"));
            directoryService.Verify(s => s.GetAllFiles(@"c:\temp\_pre", "*.sql"));
            directoryService.Verify(s => s.GetAllFiles(@"c:\temp\_post", "*.sql"));
            directoryService.Verify(s => s.GetAllFiles(@"c:\temp\_draft", "*.sql"));
            directoryService.Verify(s => s.GetFiles(@"c:\temp\v0.00", "*.sql"));
            directoryService.Verify(s => s.GetAllDirectories(@"c:\temp\v0.00", "*"));
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

            configurationService.Verify(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'init'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));
            configurationService.Verify(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'pre'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));
            configurationService.Verify(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'post'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));
            configurationService.Verify(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'draft'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));
            configurationService.Verify(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'v0.00'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));

            bulkImportService.Verify(s => s.Run(It.IsAny<IDbConnection>(), It.IsAny<IDbTransaction>(), @"c:\temp\v0.00\file.csv", null, null, null));

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

            var localVersionService = new Mock<ILocalVersionService>();

            var configurationService = new Mock<IConfigurationDataService>();
            configurationService.Setup(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'erase'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));

            var dataService = new Mock<IDataService>();
            dataService.Setup(s => s.CreateConnection()).Returns(connection.Object);
            dataService.Setup(s => s.BreakStatements("SELECT 'erase'")).Returns(new List<string> { "SELECT 'erase'" });

            var bulkImportService = new Mock<IBulkImportService>();
            var directoryService = new Mock<IDirectoryService>();
            var fileService = new Mock<IFileService>();

            directoryService.Setup(s => s.GetAllFiles(@"c:\temp\_erase", "*.sql")).Returns(new string[] { @"c:\temp\_erase\sql_erase.sql" });
            directoryService.Setup(s => s.FilterFiles(@"c:\temp\_erase", null, It.IsAny<List<string>>())).Returns(new string[] { @"c:\temp\_erase\sql_erase.sql" });

            fileService.Setup(s => s.ReadAllText(@"c:\temp\_erase\sql_erase.sql")).Returns("SELECT 'erase'");

            var tokenReplacementService = new Mock<ITokenReplacementService>();
            tokenReplacementService.Setup(s => s.Replace(null, "SELECT 'erase'")).Returns("SELECT 'erase'");

            var traceService = new Mock<ITraceService>();

            //act
            var sut = new MigrationService(
                localVersionService.Object,
                dataService.Object,
                bulkImportService.Object,
                configurationService.Object,
                tokenReplacementService.Object,
                directoryService.Object,
                fileService.Object,
                traceService.Object);
            sut.Erase(workingPath: @"c:\temp");

            //assert
            configurationService.Verify(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'erase'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));

            dataService.Verify(s => s.CreateConnection());
            directoryService.Verify(s => s.GetAllFiles(@"c:\temp\_erase", "*.sql"));
            fileService.Verify(s => s.ReadAllText(@"c:\temp\_erase\sql_erase.sql"));
            dataService.Verify(s => s.BreakStatements("SELECT 'erase'"));
            tokenReplacementService.Verify(s => s.Replace(null, "SELECT 'erase'"));

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

            var localVersionService = new Mock<ILocalVersionService>();

            var configurationService = new Mock<IConfigurationDataService>();

            var dataService = new Mock<IDataService>();
            dataService.Setup(s => s.CreateConnection()).Returns(connection.Object);
            dataService.Setup(s => s.BreakStatements("SELECT 'erase'")).Returns(new List<string> { "SELECT 'erase'" });
            //dataService.Setup(s => s.ExecuteNonQuery("sql-connection-string", "SELECT erase", DefaultConstants.CommandTimeoutSecs));

            var bulkImportService = new Mock<IBulkImportService>();
            var directoryService = new Mock<IDirectoryService>();
            var fileService = new Mock<IFileService>();

            directoryService.Setup(s => s.GetAllFiles(@"c:\temp\_erase", "*.sql")).Returns(new string[] { @"c:\temp\_erase\sql_erase.sql" });
            directoryService.Setup(s => s.FilterFiles(@"c:\temp\_erase", null, It.IsAny<List<string>>())).Returns(new string[] { @"c:\temp\_erase\sql_erase.sql" });

            //simulates that an exception happens while erase is executing
            fileService.Setup(s => s.ReadAllText(@"c:\temp\_erase\sql_erase.sql")).Throws(new ApplicationException("Fake exception"));

            var tokenReplacementService = new Mock<ITokenReplacementService>();
            tokenReplacementService.Setup(s => s.Replace(null, "SELECT 'erase'")).Returns("SELECT 'erase'");

            var traceService = new Mock<ITraceService>();

            //act
            Assert.ThrowsException<ApplicationException>(() =>
            {
                var sut = new MigrationService(
                    localVersionService.Object,
                    dataService.Object,
                    bulkImportService.Object,
                    configurationService.Object,
                    tokenReplacementService.Object,
                    directoryService.Object,
                    fileService.Object,
                    traceService.Object);
                sut.Erase(workingPath: @"c:\temp");
            }).Message.ShouldBe("Fake exception");

            //assert
            dataService.Verify(s => s.CreateConnection());
            directoryService.Verify(s => s.GetAllFiles(@"c:\temp\_erase", "*.sql"));
            fileService.Verify(s => s.ReadAllText(@"c:\temp\_erase\sql_erase.sql"));
            connection.Verify(s => s.Open());
            connection.Verify(s => s.BeginTransaction());
            transaction.Verify(s => s.Rollback());
        }
    }
}
