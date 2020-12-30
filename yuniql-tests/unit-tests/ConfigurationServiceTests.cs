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
    public class ConfigurationServiceTests
    {
        private Configuration CreateTestParameter()
        {
            return new Configuration
            {
                WorkspacePath = @"c:\temp\yuniql",
                DebugTraceMode = true,
                Platform = SUPPORTED_DATABASES.SQLSERVER,
                ConnectionString = "sqlserver-connectionstring",
                CommandTimeout = 30,
                TargetVersion = "v0.00",
                AutoCreateDatabase = true,
                Tokens = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("token1", "value1"),
                    new KeyValuePair<string, string>("token2", "value2"),
                    new KeyValuePair<string, string>("token3", "value3")
                },
                BulkSeparator = ",",
                BulkBatchSize = 1000,
                Environment = "dev",
                MetaSchemaName = "yuniql_schema",
                MetaTableName = "yuniql_table",
                TransactionMode = TRANSACTION_MODE.SESSION,
                ContinueAfterFailure = true,
                RequiredClearedDraft = true,
                IsForced = true,
                VerifyOnly = true,
                AppliedByTool = "yuniql-cli",
                AppliedByToolVersion = "v0.0.0.0"
            };
        }

        [TestMethod]
        public void Test_Paremeters_Mapped_To_Configuration()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var localVersionService = new Mock<ILocalVersionService>();

            var parameters = CreateTestParameter();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_WORKSPACE)).Returns(parameters.WorkspacePath);
            environmentService.Setup(s => s.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_TARGET_PLATFORM)).Returns(parameters.Platform);
            environmentService.Setup(s => s.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_CONNECTION_STRING)).Returns(parameters.ConnectionString);

            //act
            var sut = new ConfigurationService(environmentService.Object, localVersionService.Object, traceService.Object);
            sut.Initialize(parameters);
            var configuration = sut.GetConfiguration();

            //assert
            var parameterJson = JsonSerializer.Serialize(parameters, new JsonSerializerOptions { WriteIndented = true, IgnoreReadOnlyProperties = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var configurationJson = JsonSerializer.Serialize(configuration, new JsonSerializerOptions { WriteIndented = true, IgnoreReadOnlyProperties = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            parameterJson.ShouldBe(configurationJson);
        }

        [TestMethod]
        public void Test_Print_Redaction_Enabled()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var localVersionService = new Mock<ILocalVersionService>();

            var parameters = CreateTestParameter();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_WORKSPACE)).Returns(parameters.WorkspacePath);
            environmentService.Setup(s => s.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_TARGET_PLATFORM)).Returns(parameters.Platform);
            environmentService.Setup(s => s.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_CONNECTION_STRING)).Returns(parameters.ConnectionString);

            //act
            var sut = new ConfigurationService(environmentService.Object, localVersionService.Object, traceService.Object);
            sut.Initialize(parameters);
            var configurationJson = sut.PrintAsJson(redactSensitiveText:true);

            //assert
            var configuration = JsonSerializer.Deserialize<Configuration>(configurationJson, new JsonSerializerOptions {PropertyNameCaseInsensitive = true });
            configuration.ConnectionString.ShouldBe("<sensitive-data-redacted>");
        }

        [TestMethod]
        public void Test_Print_Redaction_Disabled()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var localVersionService = new Mock<ILocalVersionService>();

            var parameters = CreateTestParameter();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_WORKSPACE)).Returns(parameters.WorkspacePath);
            environmentService.Setup(s => s.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_TARGET_PLATFORM)).Returns(parameters.Platform);
            environmentService.Setup(s => s.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_CONNECTION_STRING)).Returns(parameters.ConnectionString);

            //act
            var sut = new ConfigurationService(environmentService.Object, localVersionService.Object, traceService.Object);
            sut.Initialize(parameters);
            var configurationJson = sut.PrintAsJson(redactSensitiveText: false);

            //assert
            var configuration = JsonSerializer.Deserialize<Configuration>(configurationJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            configuration.ConnectionString.ShouldBe(parameters.ConnectionString);
        }

        [TestMethod]
        public void Test_GetValueOrDefault_With_Use_Original_Value()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            var localVersionService = new Mock<ILocalVersionService>();

            //act
            var sut = new ConfigurationService(environmentService.Object, localVersionService.Object, traceService.Object);
            var result = sut.GetValueOrDefault("mariadb", ENVIRONMENT_VARIABLE.YUNIQL_TARGET_PLATFORM, SUPPORTED_DATABASES.SQLSERVER);

            //assert
            result.ShouldBe("mariadb");
        }

        [TestMethod]
        public void Test_GetValueOrDefault_Use_Environment_Variable()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            environmentService.Setup(s => s.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_TARGET_PLATFORM)).Returns(SUPPORTED_DATABASES.MARIADB);
            var localVersionService = new Mock<ILocalVersionService>();

            //act
            var sut = new ConfigurationService(environmentService.Object, localVersionService.Object, traceService.Object);
            var result = sut.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_TARGET_PLATFORM, SUPPORTED_DATABASES.SQLSERVER);

            //assert
            result.ShouldBe(SUPPORTED_DATABASES.MARIADB);
        }

        [TestMethod]
        public void Test_GetValueOrDefault_Use_Default_Value()
        {
            //arrange
            var traceService = new Mock<ITraceService>();
            var environmentService = new Mock<IEnvironmentService>();
            var localVersionService = new Mock<ILocalVersionService>();

            //act
            var sut = new ConfigurationService(environmentService.Object, localVersionService.Object, traceService.Object);
            var result = sut.GetValueOrDefault(null, ENVIRONMENT_VARIABLE.YUNIQL_TARGET_PLATFORM, SUPPORTED_DATABASES.SQLSERVER);

            //assert
            result.ShouldBe(SUPPORTED_DATABASES.SQLSERVER);
        }

    }

}
