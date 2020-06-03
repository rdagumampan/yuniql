using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Yuniql.CLI;
using Moq;
using Yuniql.Extensibility;
using Yuniql.Core;
using Shouldly;
using CommandLine;

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
            var option = new BaselineOption { };
            var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object);
            var returnCode = sut.RunBaselineOption(option);

            //assert
            returnCode.ShouldNotBe(0);
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
            var option = new RebaseOption { };
            var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object);
            var returnCode = sut.RunRebaseOption(option);

            returnCode.ShouldNotBe(0);
        }

        [TestMethod]
        public void Test_Archive_Option_Throws_Not_Implemented_Exception()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            var localVersionService = new Mock<ILocalVersionService>();
            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            //act
            var option = new ArchiveOption { };
            var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object);
            var returnCode = sut.RunArchiveOption(option);

            //assert
            returnCode.ShouldNotBe(0);
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
            migrationService.Verify(s => s.Initialize("sqlserver-connection-string", DEFAULT_CONSTANTS.COMMAND_TIMEOUT_SECS));
            migrationService.Verify(s => s.Erase(@"c:\temp\yuniql",
                   It.Is<List<KeyValuePair<string, string>>>(x =>
                    x[0].Key == "Token1" && x[0].Value == "TokenValue1"
                    && x[1].Key == "Token2" && x[1].Value == "TokenValue2"
                    && x[2].Key == "Token3" && x[2].Value == "TokenValue3"
                ), DEFAULT_CONSTANTS.COMMAND_TIMEOUT_SECS, null));
        }

        [DataTestMethod]
        [DataRow("--force", 0)]
        [DataRow("-f", 0)]
        [DataRow("", 1)]
        public void Test_Erase_Require_Force_Flag(string forceFlag, int expectedResultCode) {
            // arrange
            var eraseVerbAttribute = Attribute.GetCustomAttribute(typeof(EraseOption), typeof(VerbAttribute));
            var eraseVerbName = ((VerbAttribute)eraseVerbAttribute).Name;
            var args = new string[] {eraseVerbName, forceFlag};
            
            // act
            var resultCode = Parser.Default.ParseArguments<EraseOption>(args)
                                    .MapResult((EraseOption sut) => 0,
                                                errs => 1);
            
            // assert
            resultCode.ShouldBeEquivalentTo(expectedResultCode);
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
            migrationService.Setup(s => s.GetAllVersions(null, null)).Returns(new List<DbVersion> { new DbVersion { Version = "v0.00", AppliedOnUtc = DateTime.UtcNow, AppliedByUser = "user" } });
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            //act
            var option = new InfoOption { };
            var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object);
            sut.RunInfoOption(option);

            //assert
            migrationService.Verify(s => s.Initialize("sqlserver-connection-string", DEFAULT_CONSTANTS.COMMAND_TIMEOUT_SECS));
            migrationService.Verify(s => s.GetAllVersions(null, null));
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

            migrationService.Verify(s => s.Initialize("sqlserver-connection-string", DEFAULT_CONSTANTS.COMMAND_TIMEOUT_SECS));
            migrationService.Verify(s => s.Run(@"c:\temp\yuniql", "v1.00", false, It.Is<List<KeyValuePair<string, string>>>(x => x.Count == 0), true, DEFAULT_CONSTANTS.BULK_SEPARATOR, null, null, DEFAULT_CONSTANTS.COMMAND_TIMEOUT_SECS, 0, toolName, toolVersion, null, null));
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

            migrationService.Verify(s => s.Initialize("sqlserver-connection-string", DEFAULT_CONSTANTS.COMMAND_TIMEOUT_SECS));
            migrationService.Verify(s => s.Run(@"c:\temp\yuniql", "v1.00", false,
                It.Is<List<KeyValuePair<string, string>>>(x =>
                    x[0].Key == "Token1" && x[0].Value == "TokenValue1"
                    && x[1].Key == "Token2" && x[1].Value == "TokenValue2"
                    && x[2].Key == "Token3" && x[2].Value == "TokenValue3"
                ), true, DEFAULT_CONSTANTS.BULK_SEPARATOR, null, null, DEFAULT_CONSTANTS.COMMAND_TIMEOUT_SECS, 0, toolName, toolVersion, null, null));
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

            migrationService.Verify(s => s.Initialize("sqlserver-connection-string", DEFAULT_CONSTANTS.COMMAND_TIMEOUT_SECS));
            migrationService.Verify(s => s.Run(@"c:\temp\yuniql", "v1.00", false, It.Is<List<KeyValuePair<string, string>>>(x => x.Count == 0), false, DEFAULT_CONSTANTS.BULK_SEPARATOR, null, null, DEFAULT_CONSTANTS.COMMAND_TIMEOUT_SECS, 0, toolName, toolVersion, null, null));
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

            migrationService.Verify(s => s.Initialize("sqlserver-connection-string", DEFAULT_CONSTANTS.COMMAND_TIMEOUT_SECS));
            migrationService.Verify(s => s.Run(@"c:\temp\yuniql", "v1.00", false,
                It.Is<List<KeyValuePair<string, string>>>(x =>
                    x[0].Key == "Token1" && x[0].Value == "TokenValue1"
                    && x[1].Key == "Token2" && x[1].Value == "TokenValue2"
                    && x[2].Key == "Token3" && x[2].Value == "TokenValue3"
                ), false, DEFAULT_CONSTANTS.BULK_SEPARATOR, null, null, DEFAULT_CONSTANTS.COMMAND_TIMEOUT_SECS, 0, toolName, toolVersion, null, null));
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Test_StackTrace_Shown_Depending_On_Debug_Flag(bool isDebug)
        {
            //arrange
            var errorTraceMsg = string.Empty;
            var traceService = new Mock<ITraceService>();
            traceService.Setup(s => s.Error(It.IsAny<string>(), null))
                        .Callback<string, object>((msg, o) => errorTraceMsg = msg);
            
            var exc = new Exception("Fake exception");
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetCurrentDirectory()).Throws(exc);

            //act
            var option = new RunOption{ Debug = isDebug};
            var sut = new CommandLineService(null, null, environmentService.Object, traceService.Object);

            var returnCode = sut.RunMigration(option);

            //assert
            if (isDebug) {
                errorTraceMsg.ShouldContain(exc.StackTrace);
            }
            else { 
                errorTraceMsg.ShouldNotContain(exc.StackTrace);
            }
        }
    }
}
