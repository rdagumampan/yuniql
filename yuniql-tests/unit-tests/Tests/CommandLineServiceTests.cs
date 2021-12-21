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

namespace Yuniql.UnitTests
{

    [TestClass]
    public class CommandLineServiceTests : TestClassBase
    {
        [TestMethod]
        public void Test_Init_Option_No_Explicit_Options()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetCurrentDirectory()).Returns(@"c:\temp\yuniql");
            var workspaceService = new Mock<IWorkspaceService>();
            var configurationService = new Mock<IConfigurationService>();
            configurationService.Setup(s => s.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_WORKSPACE, @"c:\temp\yuniql")).Returns(@"c:\temp\yuniql");

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            var dataService = new Mock<IDataService>();
            var dataServiceFactory = new Mock<CLI.IDataServiceFactory>();
            dataServiceFactory.Setup(s => s.Create("sqlserver")).Returns(dataService.Object);

            var manifest = new Mock<ManifestData>();
            var manifestFactory = new Mock<IManifestFactory>();
            manifestFactory.Setup(s => s.Create("sqlserver")).Returns(manifest.Object);

            //act
            var option = new InitOption();
            var sut = new CommandLineService(migrationServiceFactory.Object, dataServiceFactory.Object, manifestFactory.Object, workspaceService.Object, environmentService.Object, traceService.Object, configurationService.Object);
            sut.RunInitOption(option);

            //assert
            workspaceService.Verify(s => s.Init(@"c:\temp\yuniql"));
        }

        [TestMethod]
        public void Test_Init_Option_With_Explicit_Workspace_Path()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetCurrentDirectory()).Returns(@"c:\temp\yuniql");
            var workspaceService = new Mock<IWorkspaceService>();
            var configurationService = new Mock<IConfigurationService>();
            configurationService.Setup(s => s.GetValueOrDefault(@"c:\temp\yuniql", ENVIRONMENT_VARIABLE.YUNIQL_WORKSPACE, @"c:\temp\yuniql")).Returns(@"c:\temp\yuniql");

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            var dataService = new Mock<IDataService>();
            var dataServiceFactory = new Mock<CLI.IDataServiceFactory>();
            dataServiceFactory.Setup(s => s.Create("sqlserver")).Returns(dataService.Object);

            var manifest = new Mock<ManifestData>();
            var manifestFactory = new Mock<IManifestFactory>();
            manifestFactory.Setup(s => s.Create("sqlserver")).Returns(manifest.Object);

            //act
            var option = new InitOption { Workspace = @"c:\temp\yuniql" };
            var sut = new CommandLineService(migrationServiceFactory.Object, dataServiceFactory.Object, manifestFactory.Object, workspaceService.Object, environmentService.Object, traceService.Object, configurationService.Object);
            sut.RunInitOption(option);

            //assert
            workspaceService.Verify(s => s.Init(@"c:\temp\yuniql"));
        }

        [TestMethod]
        public void Test_IncrementVersion_Option_Major_No_Explicit_Workspace_Path()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetCurrentDirectory()).Returns(@"c:\temp\yuniql");
            var workspaceService = new Mock<IWorkspaceService>();
            var configurationService = new Mock<IConfigurationService>();
            configurationService.Setup(s => s.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_WORKSPACE, @"c:\temp\yuniql")).Returns(@"c:\temp\yuniql");

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            var dataService = new Mock<IDataService>();
            var dataServiceFactory = new Mock<CLI.IDataServiceFactory>();
            dataServiceFactory.Setup(s => s.Create("sqlserver")).Returns(dataService.Object);

            var manifest = new Mock<ManifestData>();
            var manifestFactory = new Mock<IManifestFactory>();
            manifestFactory.Setup(s => s.Create("sqlserver")).Returns(manifest.Object);

            //act
            var option = new NextVersionOption { IncrementMajorVersion = true };
            var sut = new CommandLineService(migrationServiceFactory.Object, dataServiceFactory.Object, manifestFactory.Object, workspaceService.Object, environmentService.Object, traceService.Object, configurationService.Object);
            sut.RunNextVersionOption(option);

            //assert
            workspaceService.Verify(s => s.IncrementMajorVersion(@"c:\temp\yuniql", null));
        }

        [TestMethod]
        public void Test_IncrementVersion_Option_Major_With_Explicit_Workspace_Path()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetCurrentDirectory()).Returns(@"c:\temp\yuniql");

            var workspaceService = new Mock<IWorkspaceService>();
            var configurationService = new Mock<IConfigurationService>();
            configurationService.Setup(s => s.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_WORKSPACE, @"c:\temp\yuniql")).Returns(@"c:\temp\yuniql");

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            var dataService = new Mock<IDataService>();
            var dataServiceFactory = new Mock<CLI.IDataServiceFactory>();
            dataServiceFactory.Setup(s => s.Create("sqlserver")).Returns(dataService.Object);

            var manifest = new Mock<ManifestData>();
            var manifestFactory = new Mock<IManifestFactory>();
            manifestFactory.Setup(s => s.Create("sqlserver")).Returns(manifest.Object);

            //act
            var option = new NextVersionOption { IncrementMajorVersion = true };
            var sut = new CommandLineService(migrationServiceFactory.Object, dataServiceFactory.Object, manifestFactory.Object, workspaceService.Object, environmentService.Object, traceService.Object, configurationService.Object);
            sut.RunNextVersionOption(option);

            //assert
            workspaceService.Verify(s => s.IncrementMajorVersion(@"c:\temp\yuniql", null));
        }

        [TestMethod]
        public void Test_IncrementVersion_Option_Minor_No_Explicit_Workspace_Path()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetCurrentDirectory()).Returns(@"c:\temp\yuniql");
            var workspaceService = new Mock<IWorkspaceService>();
            var configurationService = new Mock<IConfigurationService>();
            configurationService.Setup(s => s.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_WORKSPACE, @"c:\temp\yuniql")).Returns(@"c:\temp\yuniql");

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            var dataService = new Mock<IDataService>();
            var dataServiceFactory = new Mock<CLI.IDataServiceFactory>();
            dataServiceFactory.Setup(s => s.Create("sqlserver")).Returns(dataService.Object);

            var manifest = new Mock<ManifestData>();
            var manifestFactory = new Mock<IManifestFactory>();
            manifestFactory.Setup(s => s.Create("sqlserver")).Returns(manifest.Object);

            //act
            var option = new NextVersionOption { IncrementMinorVersion = true };
            var sut = new CommandLineService(migrationServiceFactory.Object, dataServiceFactory.Object, manifestFactory.Object, workspaceService.Object, environmentService.Object, traceService.Object, configurationService.Object);
            sut.RunNextVersionOption(option);

            //assert
            workspaceService.Verify(s => s.IncrementMinorVersion(@"c:\temp\yuniql", null));
        }

        [TestMethod]
        public void Test_IncrementVersion_Option_No_Explicit_Options()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetCurrentDirectory()).Returns(@"c:\temp\yuniql");
            var workspaceService = new Mock<IWorkspaceService>();
            var configurationService = new Mock<IConfigurationService>();
            configurationService.Setup(s => s.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_WORKSPACE, @"c:\temp\yuniql")).Returns(@"c:\temp\yuniql");

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            var dataService = new Mock<IDataService>();
            var dataServiceFactory = new Mock<CLI.IDataServiceFactory>();
            dataServiceFactory.Setup(s => s.Create("sqlserver")).Returns(dataService.Object);

            var manifest = new Mock<ManifestData>();
            var manifestFactory = new Mock<IManifestFactory>();
            manifestFactory.Setup(s => s.Create("sqlserver")).Returns(manifest.Object);

            //act
            var option = new NextVersionOption { };
            var sut = new CommandLineService(migrationServiceFactory.Object, dataServiceFactory.Object, manifestFactory.Object, workspaceService.Object, environmentService.Object, traceService.Object, configurationService.Object);
            sut.RunNextVersionOption(option);

            //assert
            workspaceService.Verify(s => s.IncrementMinorVersion(@"c:\temp\yuniql", null));
        }

        [TestMethod]
        public void Test_Baseline_Option_Throws_Not_Implemented_Exception()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            var workspaceService = new Mock<IWorkspaceService>();
            var configurationService = new Mock<IConfigurationService>();

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            var dataService = new Mock<IDataService>();
            var dataServiceFactory = new Mock<CLI.IDataServiceFactory>();
            dataServiceFactory.Setup(s => s.Create("sqlserver")).Returns(dataService.Object);

            var manifest = new Mock<ManifestData>();
            var manifestFactory = new Mock<IManifestFactory>();
            manifestFactory.Setup(s => s.Create("sqlserver")).Returns(manifest.Object);

            //act
            var option = new BaselineOption { };
            var sut = new CommandLineService(migrationServiceFactory.Object, dataServiceFactory.Object, manifestFactory.Object, workspaceService.Object, environmentService.Object, traceService.Object, configurationService.Object);
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
            var workspaceService = new Mock<IWorkspaceService>();
            var configurationService = new Mock<IConfigurationService>();

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            var dataService = new Mock<IDataService>();
            var dataServiceFactory = new Mock<CLI.IDataServiceFactory>();
            dataServiceFactory.Setup(s => s.Create("sqlserver")).Returns(dataService.Object);

            var manifest = new Mock<ManifestData>();
            var manifestFactory = new Mock<IManifestFactory>();
            manifestFactory.Setup(s => s.Create("sqlserver")).Returns(manifest.Object);

            //act
            var option = new RebaseOption { };
            var sut = new CommandLineService(migrationServiceFactory.Object, dataServiceFactory.Object, manifestFactory.Object, workspaceService.Object, environmentService.Object, traceService.Object, configurationService.Object);
            var returnCode = sut.RunRebaseOption(option);

            returnCode.ShouldNotBe(0);
        }

        [TestMethod]
        public void Test_Archive_Option_Throws_Not_Implemented_Exception()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            var workspaceService = new Mock<IWorkspaceService>();
            var configurationService = new Mock<IConfigurationService>();

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            var dataService = new Mock<IDataService>();
            var dataServiceFactory = new Mock<CLI.IDataServiceFactory>();
            dataServiceFactory.Setup(s => s.Create("sqlserver")).Returns(dataService.Object);

            var manifest = new Mock<ManifestData>();
            var manifestFactory = new Mock<IManifestFactory>();
            manifestFactory.Setup(s => s.Create("sqlserver")).Returns(manifest.Object);

            //act
            var option = new ArchiveOption { };
            var sut = new CommandLineService(migrationServiceFactory.Object, dataServiceFactory.Object, manifestFactory.Object, workspaceService.Object, environmentService.Object, traceService.Object, configurationService.Object);
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
            var workspaceService = new Mock<IWorkspaceService>();

            var configuration = Configuration.Instance;
            configuration.Workspace = @"C:\temp\yuniql";
            configuration.Platform = SUPPORTED_DATABASES.SQLSERVER;
            configuration.ConnectionString = "sqlserver-connection-string";

            var configurationService = new Mock<IConfigurationService>();
            configurationService.Setup(s => s.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_PLATFORM, SUPPORTED_DATABASES.SQLSERVER)).Returns(SUPPORTED_DATABASES.SQLSERVER);
            configurationService.Setup(s => s.Initialize());

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            var dataService = new Mock<IDataService>();
            var dataServiceFactory = new Mock<CLI.IDataServiceFactory>();
            dataServiceFactory.Setup(s => s.Create("sqlserver")).Returns(dataService.Object);

            var manifest = new Mock<ManifestData>();
            var manifestFactory = new Mock<IManifestFactory>();
            manifestFactory.Setup(s => s.Create("sqlserver")).Returns(manifest.Object);

            //act
            var option = new EraseOption { };
            var sut = new CommandLineService(migrationServiceFactory.Object, dataServiceFactory.Object, manifestFactory.Object, workspaceService.Object, environmentService.Object, traceService.Object, configurationService.Object);
            sut.RunEraseOption(option);

            //assert
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
            var workspaceService = new Mock<IWorkspaceService>();

            var configuration = Configuration.Instance;
            configuration.Workspace = @"C:\temp\yuniql";
            configuration.Platform = SUPPORTED_DATABASES.SQLSERVER;
            configuration.ConnectionString = "sqlserver-connection-string";

            var configurationService = new Mock<IConfigurationService>();
            configurationService.Setup(s => s.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_PLATFORM, SUPPORTED_DATABASES.SQLSERVER)).Returns(SUPPORTED_DATABASES.SQLSERVER);

            var migrationService = new Mock<IMigrationService>();
            migrationService.Setup(s => s.GetAllVersions(null, null)).Returns(new List<DbVersion> { new DbVersion { Version = "v0.00", AppliedOnUtc = DateTime.UtcNow, AppliedByUser = "user" } });
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            var dataService = new Mock<IDataService>();
            var dataServiceFactory = new Mock<CLI.IDataServiceFactory>();
            dataServiceFactory.Setup(s => s.Create("sqlserver")).Returns(dataService.Object);

            var manifest = new Mock<ManifestData>();
            var manifestFactory = new Mock<IManifestFactory>();
            manifestFactory.Setup(s => s.Create("sqlserver")).Returns(manifest.Object);

            //act
            var option = new ListOption { };
            var sut = new CommandLineService(migrationServiceFactory.Object, dataServiceFactory.Object, manifestFactory.Object, workspaceService.Object, environmentService.Object, traceService.Object, configurationService.Object);
            sut.RunListOption(option);

            //assert
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

            var workspaceService = new Mock<IWorkspaceService>();
            workspaceService.Setup(s => s.GetLatestVersion(@"c:\temp\yuniql")).Returns("v1.00");

            var configuration = Configuration.Instance;
            configuration.Workspace = @"C:\temp\yuniql";
            configuration.Platform = SUPPORTED_DATABASES.SQLSERVER;
            configuration.ConnectionString = "sqlserver-connection-string";

            var configurationService = new Mock<IConfigurationService>();
            configurationService.Setup(s => s.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_PLATFORM, SUPPORTED_DATABASES.SQLSERVER)).Returns(SUPPORTED_DATABASES.SQLSERVER);
            
            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            var dataService = new Mock<IDataService>();
            var dataServiceFactory = new Mock<CLI.IDataServiceFactory>();
            dataServiceFactory.Setup(s => s.Create("sqlserver")).Returns(dataService.Object);

            var manifest = new Mock<ManifestData>();
            var manifestFactory = new Mock<IManifestFactory>();
            manifestFactory.Setup(s => s.Create("sqlserver")).Returns(manifest.Object);

            //act
            var option = new VerifyOption { };
            var sut = new CommandLineService(migrationServiceFactory.Object, dataServiceFactory.Object, manifestFactory.Object, workspaceService.Object, environmentService.Object, traceService.Object, configurationService.Object);
            sut.RunVerifyOption(option);

            //assert
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

            var workspaceService = new Mock<IWorkspaceService>();
            workspaceService.Setup(s => s.GetLatestVersion(@"c:\temp\yuniql")).Returns("v1.00");

            var configuration = Configuration.Instance;
            configuration.Workspace = @"C:\temp\yuniql";
            configuration.Platform = SUPPORTED_DATABASES.SQLSERVER;
            configuration.ConnectionString = "sqlserver-connection-string";
            
            var configurationService = new Mock<IConfigurationService>();
            configurationService.Setup(s => s.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_PLATFORM, SUPPORTED_DATABASES.SQLSERVER)).Returns(SUPPORTED_DATABASES.SQLSERVER);

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            var dataService = new Mock<IDataService>();
            var dataServiceFactory = new Mock<CLI.IDataServiceFactory>();
            dataServiceFactory.Setup(s => s.Create("sqlserver")).Returns(dataService.Object);

            var manifest = new Mock<ManifestData>();
            var manifestFactory = new Mock<IManifestFactory>();
            manifestFactory.Setup(s => s.Create("sqlserver")).Returns(manifest.Object);

            //act
            var option = new VerifyOption { Tokens = new List<string> { "Token1=TokenValue1", "Token2=TokenValue2", "Token3=TokenValue3" } };
            var sut = new CommandLineService(migrationServiceFactory.Object, dataServiceFactory.Object, manifestFactory.Object, workspaceService.Object, environmentService.Object, traceService.Object, configurationService.Object);
            sut.RunVerifyOption(option);

            //assert
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

            var workspaceService = new Mock<IWorkspaceService>();
            workspaceService.Setup(s => s.GetLatestVersion(@"c:\temp\yuniql")).Returns("v1.00");

            var configurationService = new Mock<IConfigurationService>();
            configurationService.Setup(s => s.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_PLATFORM, SUPPORTED_DATABASES.SQLSERVER)).Returns(SUPPORTED_DATABASES.SQLSERVER);

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            var dataService = new Mock<IDataService>();
            var dataServiceFactory = new Mock<CLI.IDataServiceFactory>();
            dataServiceFactory.Setup(s => s.Create("sqlserver")).Returns(dataService.Object);

            var manifest = new Mock<ManifestData>();
            var manifestFactory = new Mock<IManifestFactory>();
            manifestFactory.Setup(s => s.Create("sqlserver")).Returns(manifest.Object);

            //act
            var option = new RunOption { };
            var sut = new CommandLineService(migrationServiceFactory.Object, dataServiceFactory.Object, manifestFactory.Object, workspaceService.Object, environmentService.Object, traceService.Object, configurationService.Object);
            sut.RunRunOption(option);

            //assert
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

            var workspaceService = new Mock<IWorkspaceService>();
            workspaceService.Setup(s => s.GetLatestVersion(@"c:\temp\yuniql")).Returns("v1.00");

            var configurationService = new Mock<IConfigurationService>();
            configurationService.Setup(s => s.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_PLATFORM, SUPPORTED_DATABASES.SQLSERVER)).Returns(SUPPORTED_DATABASES.SQLSERVER);

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Returns(migrationService.Object);

            var dataService = new Mock<IDataService>();
            var dataServiceFactory = new Mock<CLI.IDataServiceFactory>();
            dataServiceFactory.Setup(s => s.Create("sqlserver")).Returns(dataService.Object);

            var manifest = new Mock<ManifestData>();
            var manifestFactory = new Mock<IManifestFactory>();
            manifestFactory.Setup(s => s.Create("sqlserver")).Returns(manifest.Object);

            //act
            var option = new RunOption { Tokens = new List<string> { "Token1=TokenValue1", "Token2=TokenValue2", "Token3=TokenValue3" } };
            var sut = new CommandLineService(migrationServiceFactory.Object, dataServiceFactory.Object, manifestFactory.Object, workspaceService.Object, environmentService.Object, traceService.Object, configurationService.Object);
            sut.RunRunOption(option);

            //assert
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
            configurationService.Setup(s => s.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_PLATFORM, SUPPORTED_DATABASES.SQLSERVER)).Returns(SUPPORTED_DATABASES.SQLSERVER);
            configurationService.Setup(s => s.GetConfiguration()).Returns(Configuration.Instance);

            var migrationService = new Mock<IMigrationService>();
            var migrationServiceFactory = new Mock<CLI.IMigrationServiceFactory>();

            var fakeException = new Exception("Fake exception");
            migrationServiceFactory.Setup(s => s.Create("sqlserver")).Throws(fakeException);

            var dataService = new Mock<IDataService>();
            var dataServiceFactory = new Mock<CLI.IDataServiceFactory>();
            dataServiceFactory.Setup(s => s.Create("sqlserver")).Returns(dataService.Object);

            var manifest = new Mock<ManifestData>();
            var manifestFactory = new Mock<IManifestFactory>();
            manifestFactory.Setup(s => s.Create("sqlserver")).Returns(manifest.Object);

            var workspaceService = new Mock<IWorkspaceService>();

            //act
            var option = new RunOption { IsDebug = debugEnabled };
            var sut = new CommandLineService(migrationServiceFactory.Object, dataServiceFactory.Object, manifestFactory.Object, workspaceService.Object, environmentService.Object, traceService.Object, configurationService.Object);
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
