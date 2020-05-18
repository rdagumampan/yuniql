using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Yuniql.CLI;
using Moq;
using Yuniql.Extensibility;
using Yuniql.Core;

namespace Yuniql.UnitTests
{
    [TestClass]
    public class CommandLineServiceTests
    {
        [TestMethod]
        public void Test_Init_Option_No_Explicit_Options()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetCurrentDirectory()).Returns(@"c:\temp\yuniql");
            var localVersionService = new Mock<ILocalVersionService>();

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            //act
            var option = new InitOption();
            var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object);
            sut.RunInitOption(option);

            //assert
            localVersionService.Verify(s => s.Init(@"c:\temp\yuniql"));
        }

        [TestMethod]
        public void Test_Init_Option_With_Explicit_Workspace_Path()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            var localVersionService = new Mock<ILocalVersionService>();
            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            //act
            var option = new InitOption { Path = @"c:\temp\yuniql-ex" };
            var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object);
            sut.RunInitOption(option);

            //assert
            localVersionService.Verify(s => s.Init(@"c:\temp\yuniql-ex"));
        }

        [TestMethod]
        public void Test_IncrementVersion_Option_Major_No_Explicit_Workspace_Path()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetCurrentDirectory()).Returns(@"c:\temp\yuniql");
            var localVersionService = new Mock<ILocalVersionService>();
            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            //act
            var option = new NextVersionOption { IncrementMajorVersion = true };
            var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object);
            sut.IncrementVersion(option);

            //assert
            localVersionService.Verify(s => s.IncrementMajorVersion(@"c:\temp\yuniql", null));
        }

        [TestMethod]
        public void Test_IncrementVersion_Option_Major_With_Explicit_Workspace_Path()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            var localVersionService = new Mock<ILocalVersionService>();
            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            //act
            var option = new NextVersionOption { IncrementMajorVersion = true, Path = @"c:\temp\yuniql-ex" };
            var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object);
            sut.IncrementVersion(option);

            //assert
            localVersionService.Verify(s => s.IncrementMajorVersion(@"c:\temp\yuniql-ex", null));
        }

        [TestMethod]
        public void Test_IncrementVersion_Option_Minor_No_Explicit_Workspace_Path()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetCurrentDirectory()).Returns(@"c:\temp\yuniql");
            var localVersionService = new Mock<ILocalVersionService>();
            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            //act
            var option = new NextVersionOption { IncrementMinorVersion = true };
            var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object);
            sut.IncrementVersion(option);

            //assert
            localVersionService.Verify(s => s.IncrementMinorVersion(@"c:\temp\yuniql", null));
        }

        [TestMethod]
        public void Test_IncrementVersion_Option_No_Explicit_Options()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetCurrentDirectory()).Returns(@"c:\temp\yuniql");
            var localVersionService = new Mock<ILocalVersionService>();
            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            //act
            var option = new NextVersionOption { };
            var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object);
            sut.IncrementVersion(option);

            //assert
            localVersionService.Verify(s => s.IncrementMinorVersion(@"c:\temp\yuniql", null));
        }

        [TestMethod]
        public void Test_Baseline_Option_Throws_Not_Implemented_Exception()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            var localVersionService = new Mock<ILocalVersionService>();
            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            //act
            Assert.ThrowsException<NotImplementedException>(() =>
            {
                var option = new BaselineOption { };
                var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object);
                sut.RunBaselineOption(option);
            });
        }

        [TestMethod]
        public void Test_Rebase_Option_Throws_Not_Implemented_Exception()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            var localVersionService = new Mock<ILocalVersionService>();
            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            //act
            Assert.ThrowsException<NotImplementedException>(() =>
            {
                var option = new RebaseOption { };
                var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object);
                sut.RunRebaseOption(option);
            });
        }

        [TestMethod]
        public void Test_Erase_Option_No_Explicit_Options()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetCurrentDirectory()).Returns(@"c:\temp\yuniql");
            environmentService.Setup(s => s.GetEnvironmentVariable("YUNIQL_CONNECTION_STRING")).Returns("sqlserver-connection-string");
            var localVersionService = new Mock<ILocalVersionService>();

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            //act
            var option = new EraseOption { Tokens = new List<string> { "Token1=TokenValue1", "Token2=TokenValue2", "Token3=TokenValue3" } };
            var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object);
            sut.RunEraseOption(option);

            //assert
            migrationService.Verify(s => s.Initialize("sqlserver-connection-string", DefaultConstants.CommandTimeoutSecs));
            migrationService.Verify(s => s.Erase(@"c:\temp\yuniql",
                   It.Is<List<KeyValuePair<string, string>>>(x =>
                    x[0].Key == "Token1" && x[0].Value == "TokenValue1"
                    && x[1].Key == "Token2" && x[1].Value == "TokenValue2"
                    && x[2].Key == "Token3" && x[2].Value == "TokenValue3"
                ), DefaultConstants.CommandTimeoutSecs, null));
        }

        [TestMethod]
        public void Test_Info_Option_No_Explicit_Options()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetEnvironmentVariable("YUNIQL_CONNECTION_STRING")).Returns("sqlserver-connection-string");
            var localVersionService = new Mock<ILocalVersionService>();
            var migrationService = new Mock<IMigrationService>();
            migrationService.Setup(s => s.GetAllVersions()).Returns(new List<DbVersion> { new DbVersion { Version = "v0.00", AppliedOnUtc = DateTime.UtcNow, AppliedByUser = "user" } });
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            //act
            var option = new InfoOption { };
            var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object);
            sut.RunInfoOption(option);

            //assert
            migrationService.Verify(s => s.Initialize("sqlserver-connection-string", DefaultConstants.CommandTimeoutSecs));
            migrationService.Verify(s => s.GetAllVersions());
        }

        [TestMethod]
        public void Test_Verify_Option_No_Explicit_Options()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetCurrentDirectory()).Returns(@"c:\temp\yuniql");
            environmentService.Setup(s => s.GetEnvironmentVariable("YUNIQL_CONNECTION_STRING")).Returns("sqlserver-connection-string");

            var localVersionService = new Mock<ILocalVersionService>();
            localVersionService.Setup(s => s.GetLatestVersion(@"c:\temp\yuniql")).Returns("v1.00");

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            //act
            var option = new VerifyOption { };
            var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object);
            sut.RunVerify(option);

            //assert
            var toolName = "yuniql-cli";
            var toolVersion = typeof(CommandLineService).Assembly.GetName().Version.ToString();

            migrationService.Verify(s => s.Initialize("sqlserver-connection-string", DefaultConstants.CommandTimeoutSecs));
            migrationService.Verify(s => s.Run(@"c:\temp\yuniql", "v1.00", false, It.Is<List<KeyValuePair<string, string>>>(x => x.Count == 0), true, DefaultConstants.Delimiter, DefaultConstants.CommandTimeoutSecs, null, toolName, toolVersion, null, null));
        }

        [TestMethod]
        public void Test_Verify_Option_With_Tokens()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetCurrentDirectory()).Returns(@"c:\temp\yuniql");
            environmentService.Setup(s => s.GetEnvironmentVariable("YUNIQL_CONNECTION_STRING")).Returns("sqlserver-connection-string");

            var localVersionService = new Mock<ILocalVersionService>();
            localVersionService.Setup(s => s.GetLatestVersion(@"c:\temp\yuniql")).Returns("v1.00");

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            //act
            var option = new VerifyOption { Tokens = new List<string> { "Token1=TokenValue1", "Token2=TokenValue2", "Token3=TokenValue3" } };
            var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object);
            sut.RunVerify(option);

            //assert
            var toolName = "yuniql-cli";
            var toolVersion = typeof(CommandLineService).Assembly.GetName().Version.ToString();

            migrationService.Verify(s => s.Initialize("sqlserver-connection-string", DefaultConstants.CommandTimeoutSecs));
            migrationService.Verify(s => s.Run(@"c:\temp\yuniql", "v1.00", false,
                It.Is<List<KeyValuePair<string, string>>>(x =>
                    x[0].Key == "Token1" && x[0].Value == "TokenValue1"
                    && x[1].Key == "Token2" && x[1].Value == "TokenValue2"
                    && x[2].Key == "Token3" && x[2].Value == "TokenValue3"
                ), true, DefaultConstants.Delimiter, DefaultConstants.CommandTimeoutSecs, null, toolName, toolVersion, null, null));
        }


        [TestMethod]
        public void Test_Run_Option_No_Explicit_Options()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetCurrentDirectory()).Returns(@"c:\temp\yuniql");
            environmentService.Setup(s => s.GetEnvironmentVariable("YUNIQL_CONNECTION_STRING")).Returns("sqlserver-connection-string");

            var localVersionService = new Mock<ILocalVersionService>();
            localVersionService.Setup(s => s.GetLatestVersion(@"c:\temp\yuniql")).Returns("v1.00");

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            //act
            var option = new RunOption { };
            var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object);
            sut.RunMigration(option);

            //assert
            var toolName = "yuniql-cli";
            var toolVersion = typeof(CommandLineService).Assembly.GetName().Version.ToString();

            migrationService.Verify(s => s.Initialize("sqlserver-connection-string", DefaultConstants.CommandTimeoutSecs));
            migrationService.Verify(s => s.Run(@"c:\temp\yuniql", "v1.00", false, It.Is<List<KeyValuePair<string, string>>>(x => x.Count == 0), false, DefaultConstants.Delimiter, DefaultConstants.CommandTimeoutSecs, null, toolName, toolVersion, null, null));
        }

        [TestMethod]
        public void Test_Run_Option_With_Tokens()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetCurrentDirectory()).Returns(@"c:\temp\yuniql");
            environmentService.Setup(s => s.GetEnvironmentVariable("YUNIQL_CONNECTION_STRING")).Returns("sqlserver-connection-string");

            var localVersionService = new Mock<ILocalVersionService>();
            localVersionService.Setup(s => s.GetLatestVersion(@"c:\temp\yuniql")).Returns("v1.00");

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            //act
            var option = new RunOption { Tokens = new List<string> { "Token1=TokenValue1", "Token2=TokenValue2", "Token3=TokenValue3" } };
            var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object);
            sut.RunMigration(option);

            //assert
            var toolName = "yuniql-cli";
            var toolVersion = typeof(CommandLineService).Assembly.GetName().Version.ToString();

            migrationService.Verify(s => s.Initialize("sqlserver-connection-string", DefaultConstants.CommandTimeoutSecs));
            migrationService.Verify(s => s.Run(@"c:\temp\yuniql", "v1.00", false,
                It.Is<List<KeyValuePair<string, string>>>(x =>
                    x[0].Key == "Token1" && x[0].Value == "TokenValue1"
                    && x[1].Key == "Token2" && x[1].Value == "TokenValue2"
                    && x[2].Key == "Token3" && x[2].Value == "TokenValue3"
                ), false, DefaultConstants.Delimiter, DefaultConstants.CommandTimeoutSecs, null, toolName, toolVersion, null, null));
        }
    }
}
