using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Yuniql.CLI;
using Moq;
using Yuniql.Extensibility;
using Yuniql.Core;
using Shouldly;
using CommandLine;
using System.Data;
using System.Linq;

namespace Yuniql.UnitTests
{

    [TestClass]
    public class TestBase
    {
        public bool AreEqual(List<KeyValuePair<string, string>> kvp1, IEnumerable<string> kvs)
        {
            var kvp2 = kvs.Select(t => new KeyValuePair<string, string>(t.Split("=")[0], t.Split("=")[1])).ToList();
            return AreEqual(kvp1, kvp2);
        }

        public bool AreEqual(List<KeyValuePair<string, string>> kvp1, List<KeyValuePair<string, string>> kvp2)
        {
            var result = kvp1.Count == kvp2.Count;
            if (result)
                result = kvp1.TrueForAll(kv1 => kvp2.Exists(kv2 => kv2.Key == kv1.Key && kv2.Value == kv1.Value));

            return result;
        }
    }

    [TestClass]
    public class CommandLineServiceTests : TestBase
    {
        [TestMethod]
        public void Test_Init_Option_No_Explicit_Options()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetCurrentDirectory()).Returns(@"c:\temp\yuniql");
            var localVersionService = new Mock<ILocalVersionService>();
            var configurationService = new Mock<IConfigurationService>();
            configurationService.Setup(s => s.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_WORKSPACE, @"c:\temp\yuniql")).Returns(@"c:\temp\yuniql");

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            //act
            var option = new InitOption();
            var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object, configurationService.Object);
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
            environmentService.Setup(s => s.GetCurrentDirectory()).Returns(@"c:\temp\yuniql");
            var localVersionService = new Mock<ILocalVersionService>();
            var configurationService = new Mock<IConfigurationService>();
            configurationService.Setup(s => s.GetValueOrDefault(@"c:\temp\yuniql", ENVIRONMENT_VARIABLE.YUNIQL_WORKSPACE, @"c:\temp\yuniql")).Returns(@"c:\temp\yuniql");

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            //act
            var option = new InitOption { Path = @"c:\temp\yuniql" };
            var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object, configurationService.Object);
            sut.RunInitOption(option);

            //assert
            localVersionService.Verify(s => s.Init(@"c:\temp\yuniql"));
        }

        [TestMethod]
        public void Test_IncrementVersion_Option_Major_No_Explicit_Workspace_Path()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetCurrentDirectory()).Returns(@"c:\temp\yuniql");
            var localVersionService = new Mock<ILocalVersionService>();
            var configurationService = new Mock<IConfigurationService>();
            configurationService.Setup(s => s.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_WORKSPACE, @"c:\temp\yuniql")).Returns(@"c:\temp\yuniql");

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            //act
            var option = new NextVersionOption { IncrementMajorVersion = true };
            var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object, configurationService.Object);
            sut.RunNextVersionOption(option);

            //assert
            localVersionService.Verify(s => s.IncrementMajorVersion(@"c:\temp\yuniql", null));
        }

        [TestMethod]
        public void Test_IncrementVersion_Option_Major_With_Explicit_Workspace_Path()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetCurrentDirectory()).Returns(@"c:\temp\yuniql");

            var localVersionService = new Mock<ILocalVersionService>();
            var configurationService = new Mock<IConfigurationService>();
            configurationService.Setup(s => s.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_WORKSPACE, @"c:\temp\yuniql")).Returns(@"c:\temp\yuniql");

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            //act
            var option = new NextVersionOption { IncrementMajorVersion = true };
            var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object, configurationService.Object);
            sut.RunNextVersionOption(option);

            //assert
            localVersionService.Verify(s => s.IncrementMajorVersion(@"c:\temp\yuniql", null));
        }

        [TestMethod]
        public void Test_IncrementVersion_Option_Minor_No_Explicit_Workspace_Path()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetCurrentDirectory()).Returns(@"c:\temp\yuniql");
            var localVersionService = new Mock<ILocalVersionService>();
            var configurationService = new Mock<IConfigurationService>();
            configurationService.Setup(s => s.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_WORKSPACE, @"c:\temp\yuniql")).Returns(@"c:\temp\yuniql");

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            //act
            var option = new NextVersionOption { IncrementMinorVersion = true };
            var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object, configurationService.Object);
            sut.RunNextVersionOption(option);

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
            var configurationService = new Mock<IConfigurationService>();
            configurationService.Setup(s => s.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_WORKSPACE, @"c:\temp\yuniql")).Returns(@"c:\temp\yuniql");

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            //act
            var option = new NextVersionOption { };
            var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object, configurationService.Object);
            sut.RunNextVersionOption(option);

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
            var configurationService = new Mock<IConfigurationService>();

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            //act
            var option = new BaselineOption { };
            var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object, configurationService.Object);
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
            var configurationService = new Mock<IConfigurationService>();

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            //act
            var option = new RebaseOption { };
            var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object, configurationService.Object);
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
            var configurationService = new Mock<IConfigurationService>();

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            //act
            var option = new ArchiveOption { };
            var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object, configurationService.Object);
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

            var configuration = new Configuration
            {
                WorkspacePath = @"C:\temp\yuniql",
                Platform = SUPPORTED_DATABASES.SQLSERVER,
                ConnectionString = "sqlserver-connection-string",
                CommandTimeout = DEFAULT_CONSTANTS.COMMAND_TIMEOUT_SECS
            };

            var configurationService = new Mock<IConfigurationService>();
            configurationService.Setup(s => s.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_TARGET_PLATFORM, SUPPORTED_DATABASES.SQLSERVER)).Returns(SUPPORTED_DATABASES.SQLSERVER);
            configurationService.Setup(s => s.Initialize(It.IsAny<Configuration>())).Returns(configuration);

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            //act
            var option = new EraseOption { };
            var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object, configurationService.Object);
            sut.RunEraseOption(option);

            //assert
            migrationService.Verify(s => s.Initialize(It.Is<Configuration>(x =>
                x.WorkspacePath == configuration.WorkspacePath
                && x.DebugTraceMode == configuration.DebugTraceMode
                && x.Platform == configuration.Platform
                && x.ConnectionString == configuration.ConnectionString
                && x.CommandTimeout == configuration.CommandTimeout
                && AreEqual(x.Tokens, configuration.Tokens)
                && x.IsForced == configuration.IsForced
                && x.Environment == configuration.Environment
            )));
            migrationService.Verify(s => s.Erase());
        }

        [DataTestMethod]
        [DataRow("--force", 0)]
        [DataRow("-f", 0)]
        [DataRow("", 1)]
        public void Test_Erase_Require_Force_Flag(string forceFlag, int expectedResultCode)
        {
            // arrange
            var eraseVerbAttribute = Attribute.GetCustomAttribute(typeof(EraseOption), typeof(VerbAttribute));
            var eraseVerbName = ((VerbAttribute)eraseVerbAttribute).Name;
            var args = new string[] { eraseVerbName, forceFlag };

            // act
            var resultCode = Parser.Default.ParseArguments<EraseOption>(args)
                                    .MapResult((EraseOption sut) => 0,
                                                errs => 1);

            // assert
            resultCode.ShouldBeEquivalentTo(expectedResultCode);
        }

        [TestMethod]
        public void Test_List_Option_No_Explicit_Options()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetEnvironmentVariable("YUNIQL_CONNECTION_STRING")).Returns("sqlserver-connection-string");
            var localVersionService = new Mock<ILocalVersionService>();

            var configuration = new Configuration
            {
                WorkspacePath = @"C:\temp\yuniql",
                Platform = SUPPORTED_DATABASES.SQLSERVER,
                ConnectionString = "sqlserver-connection-string",
                CommandTimeout = DEFAULT_CONSTANTS.COMMAND_TIMEOUT_SECS
            };

            var configurationService = new Mock<IConfigurationService>();
            configurationService.Setup(s => s.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_TARGET_PLATFORM, SUPPORTED_DATABASES.SQLSERVER)).Returns(SUPPORTED_DATABASES.SQLSERVER);
            configurationService.Setup(s => s.Initialize(It.IsAny<Configuration>())).Returns(configuration);

            var migrationService = new Mock<IMigrationService>();
            migrationService.Setup(s => s.GetAllVersions(null, null)).Returns(new List<DbVersion> { new DbVersion { Version = "v0.00", AppliedOnUtc = DateTime.UtcNow, AppliedByUser = "user" } });
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            //act
            var option = new ListOption { };
            var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object, configurationService.Object);
            sut.RunListOption(option);

            //assert
            migrationService.Verify(s => s.Initialize(It.Is<Configuration>(x =>
                x.WorkspacePath == configuration.WorkspacePath
                && x.Platform == configuration.Platform
                && x.ConnectionString == configuration.ConnectionString
                && x.CommandTimeout == configuration.CommandTimeout
            )));
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

            var configuration = new Configuration
            {
                WorkspacePath = @"C:\temp\yuniql",
                Platform = SUPPORTED_DATABASES.SQLSERVER,
                ConnectionString = "sqlserver-connection-string",
                CommandTimeout = DEFAULT_CONSTANTS.COMMAND_TIMEOUT_SECS
            };

            var configurationService = new Mock<IConfigurationService>();
            configurationService.Setup(s => s.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_TARGET_PLATFORM, SUPPORTED_DATABASES.SQLSERVER)).Returns(SUPPORTED_DATABASES.SQLSERVER);
            configurationService.Setup(s => s.Initialize(It.IsAny<Configuration>())).Returns(configuration);

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            //act
            var option = new VerifyOption { };
            var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object, configurationService.Object);
            sut.RunVerifyOption(option);

            //assert
            var toolName = "yuniql-cli";
            var toolVersion = typeof(CommandLineService).Assembly.GetName().Version.ToString();

            migrationService.Verify(s => s.Initialize(It.Is<Configuration>(x =>
                x.WorkspacePath == configuration.WorkspacePath
                && x.Platform == configuration.Platform
                && x.ConnectionString == configuration.ConnectionString
                && x.CommandTimeout == configuration.CommandTimeout
            )));
            migrationService.Verify(s=>s.Run());
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

            var configuration = new Configuration
            {
                WorkspacePath = @"C:\temp\yuniql",
                Platform = SUPPORTED_DATABASES.SQLSERVER,
                ConnectionString = "sqlserver-connection-string",
                CommandTimeout = DEFAULT_CONSTANTS.COMMAND_TIMEOUT_SECS
            };

            var configurationService = new Mock<IConfigurationService>();
            configurationService.Setup(s => s.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_TARGET_PLATFORM, SUPPORTED_DATABASES.SQLSERVER)).Returns(SUPPORTED_DATABASES.SQLSERVER);
            configurationService.Setup(s => s.Initialize(It.IsAny<Configuration>())).Returns(configuration);

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            //act
            var option = new VerifyOption { Tokens = new List<string> { "Token1=TokenValue1", "Token2=TokenValue2", "Token3=TokenValue3" } };
            var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object, configurationService.Object);
            sut.RunVerifyOption(option);

            //assert
            var toolName = "yuniql-cli";
            var toolVersion = typeof(CommandLineService).Assembly.GetName().Version.ToString();

            migrationService.Verify(s => s.Initialize(It.Is<Configuration>(x =>
                x.WorkspacePath == configuration.WorkspacePath
                && x.Platform == configuration.Platform
                && x.ConnectionString == configuration.ConnectionString
                && x.CommandTimeout == configuration.CommandTimeout
                && AreEqual(x.Tokens, configuration.Tokens)
            )));
            migrationService.Verify(s => s.Run());
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

            var configuration = new Configuration
            {
                WorkspacePath = @"C:\temp\yuniql",
                Platform = SUPPORTED_DATABASES.SQLSERVER,
                ConnectionString = "sqlserver-connection-string",
                CommandTimeout = DEFAULT_CONSTANTS.COMMAND_TIMEOUT_SECS
            };

            var configurationService = new Mock<IConfigurationService>();
            configurationService.Setup(s => s.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_TARGET_PLATFORM, SUPPORTED_DATABASES.SQLSERVER)).Returns(SUPPORTED_DATABASES.SQLSERVER);
            configurationService.Setup(s => s.Initialize(It.IsAny<Configuration>())).Returns(configuration);

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            //act
            var option = new RunOption { };
            var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object, configurationService.Object);
            sut.RunRunOption(option);

            //assert
            var toolName = "yuniql-cli";
            var toolVersion = typeof(CommandLineService).Assembly.GetName().Version.ToString();

            migrationService.Verify(s => s.Initialize(It.Is<Configuration>(x =>
                x.WorkspacePath == configuration.WorkspacePath
                && x.Platform == configuration.Platform
                && x.ConnectionString == configuration.ConnectionString
                && x.CommandTimeout == configuration.CommandTimeout
            )));
            migrationService.Verify(s => s.Run());
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

            var configuration = new Configuration
            {
                WorkspacePath = @"C:\temp\yuniql",
                Platform = SUPPORTED_DATABASES.SQLSERVER,
                ConnectionString = "sqlserver-connection-string",
                CommandTimeout = DEFAULT_CONSTANTS.COMMAND_TIMEOUT_SECS
            };

            var configurationService = new Mock<IConfigurationService>();
            configurationService.Setup(s => s.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_TARGET_PLATFORM, SUPPORTED_DATABASES.SQLSERVER)).Returns(SUPPORTED_DATABASES.SQLSERVER);
            configurationService.Setup(s => s.Initialize(It.IsAny<Configuration>())).Returns(configuration);

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            //act
            var option = new RunOption { Tokens = new List<string> { "Token1=TokenValue1", "Token2=TokenValue2", "Token3=TokenValue3" } };
            var sut = new CommandLineService(migrationServiceFactory.Object, localVersionService.Object, environmentService.Object, traceService.Object, configurationService.Object);
            sut.RunRunOption(option);

            //assert
            var toolName = "yuniql-cli";
            var toolVersion = typeof(CommandLineService).Assembly.GetName().Version.ToString();

            migrationService.Verify(s => s.Initialize(It.Is<Configuration>(x =>
                x.WorkspacePath == configuration.WorkspacePath
                && x.Platform == configuration.Platform
                && x.ConnectionString == configuration.ConnectionString
                && x.CommandTimeout == configuration.CommandTimeout
                && AreEqual(x.Tokens, configuration.Tokens)
            )));
            migrationService.Verify(s => s.Run());
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Test_StackTrace_Shown_Depending_On_Debug_Flag(bool debugEnabled)
        {
            //arrange
            var errorTraceMsg = string.Empty;
            var traceService = new Mock<ITraceService>();
            traceService.Setup(s => s.Error(It.IsAny<string>(), null))
                        .Callback<string, object>((msg, o) => errorTraceMsg = msg);
            var environmentService = new Mock<IEnvironmentService>();      
            var configurationService = new Mock<IConfigurationService>();
            configurationService.Setup(s => s.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_TARGET_PLATFORM, SUPPORTED_DATABASES.SQLSERVER)).Returns(SUPPORTED_DATABASES.SQLSERVER);

            var fakeException = new Exception("Fake exception");
            configurationService.Setup(s => s.Initialize(It.IsAny<Configuration>())).Throws(fakeException);

            //act
            var option = new RunOption { Debug = debugEnabled };
            var sut = new CommandLineService(null, null, environmentService.Object, traceService.Object, configurationService.Object);
            var returnCode = sut.RunRunOption(option);

            //assert
            if (debugEnabled)
            {
                errorTraceMsg.ShouldContain(fakeException.StackTrace);
            }
            else
            {
                errorTraceMsg.ShouldNotContain(fakeException.StackTrace);
            }
        }
    }

}
