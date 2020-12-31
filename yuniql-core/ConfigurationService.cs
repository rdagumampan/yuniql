using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Yuniql.Extensibility;

namespace Yuniql.Core
{
    ///<inheritdoc/>
    public class ConfigurationService : IConfigurationService
    {
        private readonly IEnvironmentService _environmentService;
        private readonly ILocalVersionService _localVersionService;
        private readonly ITraceService _traceService;

        ///<inheritdoc/>
        public ConfigurationService(
            IEnvironmentService environmentService,
            ILocalVersionService localVersionService,
            ITraceService traceService)
        {
            this._environmentService = environmentService;
            this._localVersionService = localVersionService;
            this._traceService = traceService;
        }

        ///<inheritdoc/>
        public Configuration GetConfiguration() => Configuration.Instance;

        ///<inheritdoc/>
        public void Initialize()
        {
            var configuration = Configuration.Instance;

            //deep copy all the properties and post process defaults based on this hierarchy
            //cli parameters -> environment variables -> defaults from internal core logic

            //BaseOption
            configuration.WorkspacePath = GetValueOrDefault(configuration.WorkspacePath, ENVIRONMENT_VARIABLE.YUNIQL_WORKSPACE, defaultValue: _environmentService.GetCurrentDirectory());
            configuration.DebugTraceMode = configuration.DebugTraceMode;

            //BasePlatformOption
            configuration.Platform = GetValueOrDefault(configuration.Platform, ENVIRONMENT_VARIABLE.YUNIQL_TARGET_PLATFORM, defaultValue: SUPPORTED_DATABASES.SQLSERVER);
            configuration.ConnectionString = GetValueOrDefault(configuration.ConnectionString, ENVIRONMENT_VARIABLE.YUNIQL_CONNECTION_STRING);
            configuration.CommandTimeout = configuration.CommandTimeout;

            //BaseRunPlatformOption (refer to Runption, VerifyOption)
            configuration.TargetVersion = configuration.TargetVersion;
            configuration.AutoCreateDatabase = configuration.AutoCreateDatabase;
            configuration.Tokens = configuration.Tokens;
            configuration.BulkSeparator = configuration.BulkSeparator;
            configuration.BulkBatchSize = configuration.BulkBatchSize;
            configuration.Environment = configuration.Environment;
            configuration.MetaSchemaName = configuration.MetaSchemaName;
            configuration.MetaTableName = configuration.MetaTableName;
            configuration.TransactionMode = configuration.TransactionMode;
            configuration.ContinueAfterFailure = configuration.ContinueAfterFailure;
            configuration.RequiredClearedDraft = configuration.RequiredClearedDraft;

            //EraseOption
            configuration.IsForced = configuration.IsForced;

            //Non-cli captured configuration
            configuration.VerifyOnly = configuration.VerifyOnly;
            configuration.AppliedByTool = configuration.AppliedByTool;
            configuration.AppliedByToolVersion = configuration.AppliedByToolVersion;

            //mark the global configuration as initialized
            configuration.IsInitialized = true;
        }

        ///<inheritdoc/>
        public void Reset()
        {
            var configuration = Configuration.Instance;

            //BaseOption
            configuration.WorkspacePath = null;
            configuration.DebugTraceMode = false;

            //BasePlatformOption
            configuration.Platform = null;
            configuration.ConnectionString = null;
            configuration.CommandTimeout = DEFAULT_CONSTANTS.COMMAND_TIMEOUT_SECS;

            //BaseRunPlatformOption (refer to Runption, VerifyOption)
            configuration.TargetVersion = null;
            configuration.AutoCreateDatabase = false;
            configuration.Tokens = null;
            configuration.BulkSeparator = DEFAULT_CONSTANTS.BULK_SEPARATOR;
            configuration.BulkBatchSize = DEFAULT_CONSTANTS.BULK_BATCH_SIZE;
            configuration.Environment = null;
            configuration.MetaSchemaName = null;
            configuration.MetaTableName = null;
            configuration.TransactionMode = TRANSACTION_MODE.SESSION;
            configuration.ContinueAfterFailure = null;
            configuration.RequiredClearedDraft = false;

            //EraseOption
            configuration.IsForced = configuration.IsForced;

            //Non-cli captured configuration
            configuration.VerifyOnly = configuration.VerifyOnly;
            configuration.AppliedByTool = configuration.AppliedByTool;
            configuration.AppliedByToolVersion = configuration.AppliedByToolVersion;

            //mark the global configuration as initialized
            configuration.IsInitialized = false;
        }

        ///<inheritdoc/>
        public string GetValueOrDefault(string receivedValue, string environmentVariableName, string defaultValue = null)
        {
            var result = receivedValue;
            if (string.IsNullOrEmpty(receivedValue))
            {
                result = _environmentService.GetEnvironmentVariable(environmentVariableName);
                if (string.IsNullOrEmpty(result))
                {
                    result = defaultValue;
                }
            }
            return result;
        }

        ///<inheritdoc/>
        public void Validate()
        {
            var _configuration = GetConfiguration();

            var validationResults = new List<Tuple<string, string, string, string>>();
            var helpLink = "https://yuniql.io/docs/yuniql-cli-command-reference/";

            //platform
            if (string.IsNullOrEmpty(_configuration.Platform))
                validationResults.Add(new Tuple<string, string, string, string>("Platform", "--platform", ENVIRONMENT_VARIABLE.YUNIQL_TARGET_PLATFORM, $"{helpLink}"));

            //workspace
            if (string.IsNullOrEmpty(_configuration.WorkspacePath))
                validationResults.Add(new Tuple<string, string, string, string>("Workspace", "-p | --path", ENVIRONMENT_VARIABLE.YUNIQL_WORKSPACE, $"{helpLink}"));

            //connection string
            if (string.IsNullOrEmpty(_configuration.ConnectionString))
                validationResults.Add(new Tuple<string, string, string, string>("ConnectionString", "-c | connection-string", ENVIRONMENT_VARIABLE.YUNIQL_CONNECTION_STRING, $"{helpLink}"));

            if (validationResults.Any())
            {
                var validationResultJson = JsonSerializer.Serialize(validationResults, new JsonSerializerOptions { WriteIndented = true, IgnoreReadOnlyProperties = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                throw new YuniqlMigrationException($"Missing session configuration values. The following information are required. Data: {validationResultJson}");
            }
        }

        ///<inheritdoc/>
        public string PrintAsJson(bool redactSensitiveText = true)
        {
            var _configuration = GetConfiguration();

            var configurationString = JsonSerializer.Serialize(_configuration, new JsonSerializerOptions { WriteIndented = true, IgnoreReadOnlyProperties = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            if (redactSensitiveText)
                configurationString = configurationString.Replace(_configuration.ConnectionString, "<sensitive-data-redacted>");

            return configurationString;
        }

    }

}
