using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shouldly;
using System.Collections.Generic;
using System.Text.Json;
using Yuniql.Core;
using Yuniql.Extensibility;

namespace Yuniql.UnitTests
{
    [TestClass]
    public class ConfigurationServiceTests: TestClassBase
    {
        private Configuration GetFreshConfiguration()
        {
            var configuration = Configuration.Instance;
            configuration.Workspace = @"c:\temp\yuniql";
            configuration.IsDebug = true;
            configuration.Platform = SUPPORTED_DATABASES.SQLSERVER;
            configuration.ConnectionString = "sqlserver-connectionstring";
            configuration.CommandTimeout = 30;
            configuration.TargetVersion = "v0.00";
            configuration.IsAutoCreateDatabase = true;
            configuration.Tokens = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string> ("token1", "value1"),
                new KeyValuePair<string, string> ("token2", "value2"),
                new KeyValuePair<string, string> ("token3", "value3")
            };
            configuration.BulkSeparator = ";";
            configuration.BulkBatchSize = 1000;
            configuration.Environment = "dev";
            configuration.MetaSchemaName = "yuniql_schema";
            configuration.MetaTableName = "yuniql_table";
            configuration.TransactionMode = TRANSACTION_MODE.SESSION;
            configuration.IsContinueAfterFailure = true;
            configuration.IsRequiredClearedDraft = true;
            configuration.IsForced = true;
            configuration.IsVerifyOnly = true;
            configuration.AppliedByTool = "yuniql-cli";
            configuration.AppliedByToolVersion = "1.0.0.0";

            return configuration;
    }

        [TestMethod]
        public void Test_Paremeters_Mapped_To_Configuration()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var workspaceService = new Mock<IWorkspaceService>();

            var parameters = GetFreshConfiguration();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_WORKSPACE)).Returns(parameters.Workspace);
            environmentService.Setup(s => s.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_PLATFORM)).Returns(parameters.Platform);
            environmentService.Setup(s => s.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_CONNECTION_STRING)).Returns(parameters.ConnectionString);

            //act
            var sut = new ConfigurationService(environmentService.Object, workspaceService.Object, traceService.Object);
            sut.Initialize();
            var configuration = sut.GetConfiguration();

            //assert
            var parameterJson = JsonSerializer.Serialize(parameters, new JsonSerializerOptions { WriteIndented = true, IgnoreReadOnlyProperties = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var configurationJson = JsonSerializer.Serialize(configuration, new JsonSerializerOptions { WriteIndented = true, IgnoreReadOnlyProperties = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            parameterJson.ShouldBe(configurationJson);
        }

        [TestMethod]
        public void Test_Trace_Sensitive_Data_Disabled()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var workspaceService = new Mock<IWorkspaceService>();

            //disable IsTraceSensitiveData to log plain text connection strings
            traceService.Setup(property => property.IsTraceSensitiveData).Returns(false);

            var parameters = GetFreshConfiguration();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_WORKSPACE)).Returns(parameters.Workspace);
            environmentService.Setup(s => s.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_PLATFORM)).Returns(parameters.Platform);
            environmentService.Setup(s => s.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_CONNECTION_STRING)).Returns(parameters.ConnectionString);

            //act
            var sut = new ConfigurationService(environmentService.Object, workspaceService.Object, traceService.Object);
            sut.Initialize();
            var configurationJson = sut.PrintAsJson();

            //assert
            configurationJson.Contains("\"connectionString\": \"<sensitive-data-redacted>\"").ShouldBeTrue();
        }

        [TestMethod]
        public void Test_Trace_Sensitive_Data_Enabled()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var workspaceService = new Mock<IWorkspaceService>();

            //enable IsTraceSensitiveData to log plain text connection strings
            traceService.Setup(property => property.IsTraceSensitiveData).Returns(true); 

            var parameters = GetFreshConfiguration();

            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_WORKSPACE)).Returns(parameters.Workspace);
            environmentService.Setup(s => s.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_PLATFORM)).Returns(parameters.Platform);
            environmentService.Setup(s => s.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_CONNECTION_STRING)).Returns(parameters.ConnectionString);

            //act
            var sut = new ConfigurationService(environmentService.Object, workspaceService.Object, traceService.Object);
            sut.Initialize();
            var configurationJson = sut.PrintAsJson();

            //assert
            configurationJson.Contains("\"connectionString\": \"<sensitive-data-redacted>\"").ShouldBeFalse ();
        }

        [TestMethod]
        public void Test_GetValueOrDefault_With_Use_Original_Value()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            var workspaceService = new Mock<IWorkspaceService>();

            //act
            var sut = new ConfigurationService(environmentService.Object, workspaceService.Object, traceService.Object);
            var result = sut.GetValueOrDefault("mariadb", ENVIRONMENT_VARIABLE.YUNIQL_PLATFORM, SUPPORTED_DATABASES.SQLSERVER);

            //assert
            result.ShouldBe("mariadb");
        }

        [TestMethod]
        public void Test_GetValueOrDefault_Use_Environment_Variable()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_PLATFORM)).Returns(SUPPORTED_DATABASES.MARIADB);
            var workspaceService = new Mock<IWorkspaceService>();

            //act
            var sut = new ConfigurationService(environmentService.Object, workspaceService.Object, traceService.Object);
            var result = sut.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_PLATFORM, SUPPORTED_DATABASES.SQLSERVER);

            //assert
            result.ShouldBe(SUPPORTED_DATABASES.MARIADB);
        }

        [TestMethod]
        public void Test_GetValueOrDefault_Use_Default_Value()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            var workspaceService = new Mock<IWorkspaceService>();

            //act
            var sut = new ConfigurationService(environmentService.Object, workspaceService.Object, traceService.Object);
            var result = sut.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_PLATFORM, SUPPORTED_DATABASES.SQLSERVER);

            //assert
            result.ShouldBe(SUPPORTED_DATABASES.SQLSERVER);
        }

    }

}
