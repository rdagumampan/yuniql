using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Shouldly;
using Yuniql.Core;
using Moq;
using Yuniql.Extensibility;
using System.Data;
using System.IO;
using System;
using Yuniql.PlatformTests;
using Yuniql.CLI;

namespace Yuniql.UnitTests
{
    [TestClass]
    public class MigrationServiceTests
    {
        [TestMethod]
        public void Test_Run_()
        {
            //arrange
            var transaction = new Mock<IDbTransaction>();

            var connection = new Mock<IDbConnection>();
            connection.Setup(s => s.BeginTransaction()).Returns(transaction.Object);

            var localVersionService = new Mock<ILocalVersionService>();
            localVersionService.Setup(s => s.Validate(@"c:\temp")).Verifiable();

            var metadataService = new Mock<IMetadataService>();
            metadataService.Setup(s => s.IsDatabaseExists(null)).Returns(false);
            metadataService.Setup(s => s.CreateDatabase(null));
            metadataService.Setup(s => s.IsDatabaseConfigured(null, null, null)).Returns(false);
            metadataService.Setup(s => s.ConfigureDatabase(null, null, null));
            metadataService.Setup(s => s.GetAllAppliedVersions(null, null, null)).Returns(new List<DbVersion> { });
            metadataService.Setup(s => s.GetAllVersions(null, null, null)).Returns(new List<DbVersion> { });
            metadataService.Setup(s => s.GetCurrentVersion(null, null, null)).Returns(string.Empty);
            metadataService.Setup(s => s.InsertVersion(connection.Object, transaction.Object, "v0.00", null,null, null, null, null, null, null, null));

            metadataService.Setup(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 1", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));
            metadataService.Setup(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'init'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));
            metadataService.Setup(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'pre'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));
            metadataService.Setup(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'post'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));
            metadataService.Setup(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'draft'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));
            metadataService.Setup(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'v0.00'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));

            var dataService = new Mock<IDataService>();
            dataService.Setup(s => s.IsTransactionalDdlSupported).Returns(true);
            dataService.Setup(s => s.GetConnectionInfo()).Returns(new ConnectionInfo { DataSource = "server", Database = "db" });
            dataService.Setup(s => s.CreateConnection()).Returns(connection.Object);

            dataService.Setup(s => s.BreakStatements("SELECT 'init'")).Returns(new List<string> { "SELECT 'init'" });
            dataService.Setup(s => s.BreakStatements("SELECT 'pre'")).Returns(new List<string> { "SELECT 'pre'" });
            dataService.Setup(s => s.BreakStatements("SELECT 'post'")).Returns(new List<string> { "SELECT 'post'" });
            dataService.Setup(s => s.BreakStatements("SELECT 'draft'")).Returns(new List<string> { "SELECT 'draft'" });
            dataService.Setup(s => s.BreakStatements("SELECT 'v0.00'")).Returns(new List<string> { "SELECT 'v0.00'" });

            var bulkImportService = new Mock<IBulkImportService>();
            bulkImportService.Setup(s => s.Run(connection.Object, transaction.Object, "file.csv", DEFAULT_CONSTANTS.BULK_SEPARATOR, DEFAULT_CONSTANTS.BULK_BATCH_SIZE, DEFAULT_CONSTANTS.COMMAND_TIMEOUT_SECS));

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
            var environmentService = new Mock<IEnvironmentService>();

            var tokenKeyPairs = new List<KeyValuePair<string, string>> {
               new KeyValuePair<string, string>("Token1","TokenValue1"),
               new KeyValuePair<string, string>("Token2","TokenValue2"),
               new KeyValuePair<string, string>("Token3","TokenValue3"),
            };

            var configuration = new Configuration { 
                WorkspacePath = @"c:\temp",
                Platform  = SUPPORTED_DATABASES.SQLSERVER,
                TargetVersion = "v0.00",
                AutoCreateDatabase = true,
                Tokens = tokenKeyPairs,
                VerifyOnly = false
            };

            var configurationService = new Mock<IConfigurationService>();
            configurationService.Setup(s => s.GetConfiguration()).Returns(configuration);

            //act
            var sut = new MigrationServiceTransactional(
                localVersionService.Object,
                dataService.Object,
                bulkImportService.Object,
                metadataService.Object,
                tokenReplacementService.Object,
                directoryService.Object,
                fileService.Object,
                traceService.Object,
                configurationService.Object);
            sut.Initialize(configuration);
            sut.Run();

            //asset
            localVersionService.Verify(s => s.Validate(@"c:\temp"));

            metadataService.Verify(s => s.IsDatabaseExists(null));
            metadataService.Verify(s => s.CreateDatabase(null));
            metadataService.Verify(s => s.IsDatabaseConfigured(null, null, null));;
            metadataService.Verify(s => s.ConfigureDatabase(null, null, null));
            metadataService.Verify(s => s.GetAllVersions(null, null, null));
            metadataService.Verify(s => s.GetCurrentVersion(null, null, null));;
            metadataService.Verify(s => s.InsertVersion(It.IsAny<IDbConnection>(), It.IsAny<IDbTransaction>(), "v0.00", null, null, null, null, null, null, null, null));

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

            metadataService.Verify(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'init'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));
            metadataService.Verify(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'pre'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));
            metadataService.Verify(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'post'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));
            metadataService.Verify(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'draft'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));
            metadataService.Verify(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'v0.00'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));

            bulkImportService.Verify(s => s.Run(It.IsAny<IDbConnection>(), It.IsAny<IDbTransaction>(), @"c:\temp\v0.00\file.csv", null, null, null));

            connection.Verify(s => s.Open());
            connection.Verify(s => s.BeginTransaction());
            transaction.Verify(s => s.Commit());
        }

        [TestMethod]
        public void Test_Run_For_Non_Transactional_Platform()
        {
            //arrange
            var transaction = new Mock<IDbTransaction>();

            var connection = new Mock<IDbConnection>();
            connection.Setup(s => s.BeginTransaction()).Returns(transaction.Object);

            var localVersionService = new Mock<ILocalVersionService>();
            localVersionService.Setup(s => s.Validate(@"c:\temp")).Verifiable();

            var metadataService = new Mock<IMetadataService>();
            metadataService.Setup(s => s.IsDatabaseExists(null)).Returns(false);
            metadataService.Setup(s => s.CreateDatabase(null));
            metadataService.Setup(s => s.IsDatabaseConfigured(null, null, null)).Returns(false);
            metadataService.Setup(s => s.ConfigureDatabase(null, null, null));
            metadataService.Setup(s => s.GetAllAppliedVersions(null, null, null)).Returns(new List<DbVersion> { });
            metadataService.Setup(s => s.GetAllVersions(null, null, null)).Returns(new List<DbVersion> { });
            metadataService.Setup(s => s.GetCurrentVersion(null, null, null)).Returns(string.Empty);
            metadataService.Setup(s => s.InsertVersion(connection.Object, transaction.Object, "v0.00", null, null, null, null, null, null, null, null));

            metadataService.Setup(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 1", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));
            metadataService.Setup(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'init'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));
            metadataService.Setup(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'pre'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));
            metadataService.Setup(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'post'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));
            metadataService.Setup(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'draft'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));
            metadataService.Setup(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'v0.00'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));

            var dataService = new Mock<IDataService>();
            dataService.Setup(s => s.GetConnectionInfo()).Returns(new ConnectionInfo { DataSource = "server", Database = "db" });
            dataService.Setup(s => s.CreateConnection()).Returns(connection.Object);

            dataService.Setup(s => s.BreakStatements("SELECT 'init'")).Returns(new List<string> { "SELECT 'init'" });
            dataService.Setup(s => s.BreakStatements("SELECT 'pre'")).Returns(new List<string> { "SELECT 'pre'" });
            dataService.Setup(s => s.BreakStatements("SELECT 'post'")).Returns(new List<string> { "SELECT 'post'" });
            dataService.Setup(s => s.BreakStatements("SELECT 'draft'")).Returns(new List<string> { "SELECT 'draft'" });
            dataService.Setup(s => s.BreakStatements("SELECT 'v0.00'")).Returns(new List<string> { "SELECT 'v0.00'" });

            var bulkImportService = new Mock<IBulkImportService>();
            bulkImportService.Setup(s => s.Run(connection.Object, transaction.Object, "file.csv", DEFAULT_CONSTANTS.BULK_SEPARATOR, DEFAULT_CONSTANTS.BULK_BATCH_SIZE, DEFAULT_CONSTANTS.COMMAND_TIMEOUT_SECS));

            var directoryService = new Mock<IDirectoryService>();
            var fileService = new Mock<IFileService>();

            directoryService.Setup(s => s.GetDirectories(@"c:\temp", "v*.*")).Returns(new string[] { @"c:\temp\v0.00" });

            directoryService.Setup(s => s.GetAllFiles(@"c:\temp\_init", "*.sql")).Returns(new string[] { @"c:\temp\_init\sql_init.sql" });
            directoryService.Setup(s => s.FilterFiles(@"c:\temp\_init", null, It.Is<List<string>>(f => f.Contains(@"c:\temp\_init\sql_init.sql")))).Returns(new string[] { @"c:\temp\_init\sql_init.sql" });

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
            var environmentService = new Mock<IEnvironmentService>();
            var tokenKeyPairs = new List<KeyValuePair<string, string>> {
               new KeyValuePair<string, string>("Token1","TokenValue1"),
               new KeyValuePair<string, string>("Token2","TokenValue2"),
               new KeyValuePair<string, string>("Token3","TokenValue3"),
            };

            var configuration = new Configuration
            {
                WorkspacePath = @"c:\temp",
                Platform = SUPPORTED_DATABASES.SQLSERVER,
                TargetVersion = "v0.00",
                AutoCreateDatabase = true,
                Tokens = tokenKeyPairs,
                VerifyOnly = false
            };

            var configurationService = new Mock<IConfigurationService>();
            configurationService.Setup(s => s.GetConfiguration()).Returns(configuration);

            //act
            var sut = new MigrationServiceTransactional(
                localVersionService.Object,
                dataService.Object,
                bulkImportService.Object,
                metadataService.Object,
                tokenReplacementService.Object,
                directoryService.Object,
                fileService.Object,
                traceService.Object,
                configurationService.Object);
            sut.Initialize(configuration);
            sut.Run();

            //asset
            localVersionService.Verify(s => s.Validate(@"c:\temp"));

            metadataService.Verify(s => s.IsDatabaseExists(null));
            metadataService.Verify(s => s.CreateDatabase(null));
            metadataService.Verify(s => s.IsDatabaseConfigured(null, null, null)); ;
            metadataService.Verify(s => s.ConfigureDatabase(null, null, null));
            metadataService.Verify(s => s.GetAllVersions(null, null, null));
            metadataService.Verify(s => s.GetCurrentVersion(null, null, null)); ;
            metadataService.Verify(s => s.InsertVersion(It.IsAny<IDbConnection>(), It.IsAny<IDbTransaction>(), "v0.00", null, null, null, null, null, null, null, null));

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

            metadataService.Verify(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'init'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));
            metadataService.Verify(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'pre'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));
            metadataService.Verify(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'post'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));
            metadataService.Verify(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'draft'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));
            metadataService.Verify(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'v0.00'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));

            bulkImportService.Verify(s => s.Run(It.IsAny<IDbConnection>(), It.IsAny<IDbTransaction>(), @"c:\temp\v0.00\file.csv", null, null, null));

            connection.Verify(s => s.Open());
        }

        [TestMethod]
        public void Test_Erase()
        {
            //arrange
            var transaction = new Mock<IDbTransaction>();

            var connection = new Mock<IDbConnection>();
            connection.Setup(s => s.BeginTransaction()).Returns(transaction.Object);

            var localVersionService = new Mock<ILocalVersionService>();

            var metadataService = new Mock<IMetadataService>();
            metadataService.Setup(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'erase'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));

            var dataService = new Mock<IDataService>();
            dataService.Setup(s => s.IsTransactionalDdlSupported).Returns(true);
            dataService.Setup(s => s.CreateConnection()).Returns(connection.Object);
            dataService.Setup(s => s.BreakStatements(It.IsAny<string>())).Returns(new List<string> { "SELECT 'erase'" });

            var bulkImportService = new Mock<IBulkImportService>();
            var directoryService = new Mock<IDirectoryService>();
            var fileService = new Mock<IFileService>();

            directoryService.Setup(s => s.GetAllFiles(It.IsAny<string>(), "*.sql")).Returns(new string[] { @"c:\temp\_erase\sql_erase.sql" });
            directoryService.Setup(s => s.FilterFiles(It.IsAny<string>(), null, It.IsAny<List<string>>())).Returns(new string[] { @"c:\temp\_erase\sql_erase.sql" });

            fileService.Setup(s => s.ReadAllText(It.IsAny<string>())).Returns("SELECT 'erase'");

            var tokenReplacementService = new Mock<ITokenReplacementService>();
            tokenReplacementService.Setup(s => s.Replace(It.IsAny<List<KeyValuePair<string, string>>>(), It.IsAny<string>())).Returns("SELECT 'erase'");

            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            var configuration = new Configuration { WorkspacePath = @"c:\temp" };
            var configurationService = new Mock<IConfigurationService>();
            configurationService.Setup(s => s.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_TARGET_PLATFORM, SUPPORTED_DATABASES.SQLSERVER)).Returns(SUPPORTED_DATABASES.SQLSERVER);
            configurationService.Setup(s => s.GetConfiguration()).Returns(configuration);

            //act
            var sut = new MigrationServiceTransactional(
                localVersionService.Object,
                dataService.Object,
                bulkImportService.Object,
                metadataService.Object,
                tokenReplacementService.Object,
                directoryService.Object,
                fileService.Object,
                traceService.Object,
                configurationService.Object);
            sut.Initialize(configuration);
            sut.Erase();

            //assert
            metadataService.Verify(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'erase'", It.IsAny<int>(), It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));
            dataService.Verify(s => s.CreateConnection());
            directoryService.Verify(s => s.GetAllFiles(It.IsAny<string>(), "*.sql"));
            fileService.Verify(s => s.ReadAllText(It.IsAny<string>()));
            dataService.Verify(s => s.BreakStatements(It.IsAny<string>()));
            tokenReplacementService.Verify(s => s.Replace(It.IsAny<List<KeyValuePair<string, string>>>(), It.IsAny<string>()));
            connection.Verify(s => s.Open());
            connection.Verify(s => s.BeginTransaction());
            transaction.Verify(s => s.Commit());
        }

        [TestMethod]
        public void Test_Erase_For_Non_Transactional_Platform()
        {
            //arrange
            var transaction = new Mock<IDbTransaction>();

            var connection = new Mock<IDbConnection>();
            connection.Setup(s => s.BeginTransaction()).Returns(transaction.Object);
            
            var localVersionService = new Mock<ILocalVersionService>();

            var metadataService = new Mock<IMetadataService>();
            metadataService.Setup(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'erase'", null, It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));

            var dataService = new Mock<IDataService>();
            dataService.Setup(s => s.CreateConnection()).Returns(connection.Object);
            dataService.Setup(s => s.BreakStatements(It.IsAny<string>())).Returns(new List<string> { "SELECT 'erase'" });

            var bulkImportService = new Mock<IBulkImportService>();
            var fileService = new Mock<IFileService>();

            var directoryService = new Mock<IDirectoryService>();
            directoryService.Setup(s => s.GetAllFiles(It.IsAny<string>(), "*.sql")).Returns(new string[] { @"c:\temp\_erase\sql_erase.sql" });
            directoryService.Setup(s => s.FilterFiles(It.IsAny<string>(), null, It.IsAny<List<string>>())).Returns(new string[] { @"c:\temp\_erase\sql_erase.sql" });

            fileService.Setup(s => s.ReadAllText(It.IsAny<string>())).Returns("SELECT 'erase'");

            var tokenReplacementService = new Mock<ITokenReplacementService>();
            tokenReplacementService.Setup(s => s.Replace(It.IsAny<List<KeyValuePair<string, string>>>(), It.IsAny<string>())).Returns("SELECT 'erase'");

            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();

            var configuration = new Configuration
            {
                WorkspacePath = @"C:\temp",
                Platform = SUPPORTED_DATABASES.SQLSERVER,
            };

            var configurationService = new Mock<IConfigurationService>();
            configurationService.Setup(s => s.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_TARGET_PLATFORM, SUPPORTED_DATABASES.SQLSERVER)).Returns(SUPPORTED_DATABASES.SQLSERVER);
            configurationService.Setup(s => s.GetConfiguration()).Returns(configuration);

            //act
            var sut = new MigrationServiceNonTransactional(
                localVersionService.Object,
                dataService.Object,
                bulkImportService.Object,
                metadataService.Object,
                tokenReplacementService.Object,
                directoryService.Object,
                fileService.Object,
                traceService.Object,
                configurationService.Object);
            sut.Initialize(configuration);
            sut.Erase();

            //assert
            metadataService.Verify(s => s.ExecuteSql(It.IsAny<IDbConnection>(), "SELECT 'erase'", It.IsAny<int>(), It.IsAny<IDbTransaction>(), It.IsAny<ITraceService>()));
            dataService.Verify(s => s.CreateConnection());
            directoryService.Verify(s => s.GetAllFiles(It.IsAny<string>(), "*.sql"));
            fileService.Verify(s => s.ReadAllText(It.IsAny<string>()));
            dataService.Verify(s => s.BreakStatements(It.IsAny<string>()));
            tokenReplacementService.Verify(s => s.Replace(It.IsAny<List<KeyValuePair<string, string>>>(), It.IsAny<string>()));
            connection.Verify(s => s.Open());
        }

        [TestMethod]
        public void Test_Erase_With_Error_Must_Rollback()
        {
            //arrange
            var transaction = new Mock<IDbTransaction>();

            var connection = new Mock<IDbConnection>();
            connection.Setup(s => s.BeginTransaction()).Returns(transaction.Object);

            var localVersionService = new Mock<ILocalVersionService>();

            var metadataService = new Mock<IMetadataService>();

            var dataService = new Mock<IDataService>();
            dataService.Setup(s => s.IsTransactionalDdlSupported).Returns(true);
            dataService.Setup(s => s.CreateConnection()).Returns(connection.Object);
            dataService.Setup(s => s.BreakStatements(It.IsAny<string>())).Returns(new List<string> { "SELECT 'erase'" });
            //dataService.Setup(s => s.ExecuteNonQuery("sql-connection-string", "SELECT erase", DefaultConstants.CommandTimeoutSecs));

            var bulkImportService = new Mock<IBulkImportService>();
            var directoryService = new Mock<IDirectoryService>();
            var fileService = new Mock<IFileService>();

            directoryService.Setup(s => s.GetAllFiles(It.IsAny<string>(), "*.sql")).Returns(new string[] { @"c:\temp\_erase\sql_erase.sql" });
            directoryService.Setup(s => s.FilterFiles(It.IsAny<string>(), null, It.IsAny<List<string>>())).Returns(new string[] { @"c:\temp\_erase\sql_erase.sql" });

            //simulates that an exception happens while erase is executing
            fileService.Setup(s => s.ReadAllText(It.IsAny<string>())).Throws(new ApplicationException("Fake exception"));

            var tokenReplacementService = new Mock<ITokenReplacementService>();
            tokenReplacementService.Setup(s => s.Replace(It.IsAny<List<KeyValuePair<string, string>>>(), It.IsAny<string>())).Returns("SELECT 'erase'");

            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();

            var configuration = new Configuration
            {
                WorkspacePath = @"C:\temp",
                Platform = SUPPORTED_DATABASES.SQLSERVER,
            };

            var configurationService = new Mock<IConfigurationService>();
            configurationService.Setup(s => s.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_TARGET_PLATFORM, SUPPORTED_DATABASES.SQLSERVER)).Returns(SUPPORTED_DATABASES.SQLSERVER);
            configurationService.Setup(s => s.GetConfiguration()).Returns(configuration);

            //act
            Assert.ThrowsException<ApplicationException>(() =>
            {
                var configuration = new Configuration { WorkspacePath = @"c:\temp" };
                var sut = new MigrationServiceTransactional(
                    localVersionService.Object,
                    dataService.Object,
                    bulkImportService.Object,
                    metadataService.Object,
                    tokenReplacementService.Object,
                    directoryService.Object,
                    fileService.Object,
                    traceService.Object,
                    configurationService.Object);
                sut.Initialize(configuration);
                sut.Erase();
            }).Message.ShouldBe("Fake exception");

            //assert
            dataService.Verify(s => s.CreateConnection());
            directoryService.Verify(s => s.GetAllFiles(It.IsAny<string>(), "*.sql"));
            fileService.Verify(s => s.ReadAllText(It.IsAny<string>()));
            connection.Verify(s => s.Open());
            connection.Verify(s => s.BeginTransaction());
            transaction.Verify(s => s.Rollback());
        }

        [TestMethod]
        public void Test_Run_Transaction_Mode_Session()
        {
            //arrange
            var transaction = new Mock<IDbTransaction>();
            var connection = new Mock<IDbConnection>();
            connection.Setup(c => c.BeginTransaction())
                      .Returns(transaction.Object);

            var dataService = new Mock<IDataService>();
            dataService.Setup(s => s.CreateConnection())
                                    .Returns(connection.Object);
            dataService.Setup(s => s.GetConnectionInfo())
                                    .Returns(() => new ConnectionInfo {  Database = "test", DataSource = "test"});
            var configurationDataService = new Mock<IMetadataService>();
            configurationDataService.Setup(s => s.IsDatabaseConfigured(It.IsAny<string>(), It.IsAny<string>(), null))
                                    .Returns(true);
            configurationDataService.Setup(s => s.GetCurrentVersion(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                                    .Returns(string.Empty);
            configurationDataService.Setup(s => s.GetAllVersions(It.IsAny<string>(), It.IsAny<string>(), null))
                                    .Returns(new List<DbVersion>());

            var environmentService = new Mock<IEnvironmentService>();
            var configurationService = new Mock<IConfigurationService>();

            var transactionMode = TRANSACTION_MODE.SESSION;

            //act
            var sut = new MigrationServiceTransactional(
                new Mock<ILocalVersionService>().Object, 
                dataService.Object, 
                new Mock<IBulkImportService>().Object, 
                configurationDataService.Object, 
                new Mock<ITokenReplacementService>().Object, 
                new Mock<IDirectoryService>().Object, 
                new Mock<IFileService>().Object, 
                new Mock<ITraceService>().Object,
                configurationService.Object);
            sut.Run(string.Empty, transactionMode: transactionMode);

            // assert
            connection.Verify(c => c.BeginTransaction(), Times.Once());
            transaction.Verify(t => t.Commit(), Times.Once());
            transaction.Verify(t => t.Rollback(), Times.Never());
        }

        [TestMethod]
        public void Test_Run_Transaction_Mode_Version()
        {
            //arrange
            var transaction = new Mock<IDbTransaction>();
            var connection = new Mock<IDbConnection>();
            connection.Setup(c => c.BeginTransaction())
                      .Returns(transaction.Object);

            var dataService = new Mock<IDataService>();
            dataService.Setup(s => s.CreateConnection())
                                    .Returns(connection.Object);
            dataService.Setup(s => s.GetConnectionInfo())
                                    .Returns(() => new ConnectionInfo { Database = "test", DataSource = "test" });
            var metadataService = new Mock<IMetadataService>();
            metadataService.Setup(s => s.IsDatabaseConfigured(It.IsAny<string>(), It.IsAny<string>(), null))
                                    .Returns(true);
            metadataService.Setup(s => s.GetCurrentVersion(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                                    .Returns(string.Empty);
            metadataService.Setup(s => s.GetAllVersions(It.IsAny<string>(), It.IsAny<string>(), null))
                                    .Returns(new List<DbVersion>());

            var environmentService = new Mock<IEnvironmentService>();
            var configurationService = new Mock<IConfigurationService>();

            var transactionMode = TRANSACTION_MODE.VERSION;

            //act
            var sut = new MigrationServiceTransactional(
                new Mock<ILocalVersionService>().Object,
                dataService.Object,
                new Mock<IBulkImportService>().Object,
                metadataService.Object,
                new Mock<ITokenReplacementService>().Object,
                new Mock<IDirectoryService>().Object,
                new Mock<IFileService>().Object,
                new Mock<ITraceService>().Object,
                configurationService.Object);
            sut.Run(string.Empty, transactionMode: transactionMode);

            // assert
            connection.Verify(c => c.BeginTransaction(), Times.AtLeast(3));
            transaction.Verify(t => t.Commit(), Times.AtLeast(3));
            transaction.Verify(t => t.Rollback(), Times.Never());
        }

        [TestMethod]
        public void Test_Run_Transaction_Mode_None()
        {
            //arrange
            var transaction = new Mock<IDbTransaction>();
            var connection = new Mock<IDbConnection>();
            connection.Setup(c => c.BeginTransaction())
                      .Returns(transaction.Object);

            var dataService = new Mock<IDataService>();
            dataService.Setup(s => s.CreateConnection())
                                    .Returns(connection.Object);
            dataService.Setup(s => s.GetConnectionInfo())
                                    .Returns(() => new ConnectionInfo { Database = "test", DataSource = "test" });
            var metadataService = new Mock<IMetadataService>();
            metadataService.Setup(s => s.IsDatabaseConfigured(It.IsAny<string>(), It.IsAny<string>(), null))
                                    .Returns(true);
            metadataService.Setup(s => s.GetCurrentVersion(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                                    .Returns(string.Empty);
            metadataService.Setup(s => s.GetAllVersions(It.IsAny<string>(), It.IsAny<string>(), null))
                                    .Returns(new List<DbVersion>());

            var environmentService = new Mock<IEnvironmentService>();
            var configurationService = new Mock<IConfigurationService>();

            var transactionMode = TRANSACTION_MODE.NONE;

            //act
            var sut = new MigrationServiceTransactional(
                new Mock<ILocalVersionService>().Object,
                dataService.Object,
                new Mock<IBulkImportService>().Object,
                metadataService.Object,
                new Mock<ITokenReplacementService>().Object,
                new Mock<IDirectoryService>().Object,
                new Mock<IFileService>().Object,
                new Mock<ITraceService>().Object,
                configurationService.Object);
            sut.Run(string.Empty, transactionMode: transactionMode);

            // assert
            connection.Verify(c => c.BeginTransaction(), Times.Never());
            transaction.Verify(t => t.Commit(), Times.Never());
            transaction.Verify(t => t.Rollback(), Times.Never());
        }

    }
}
