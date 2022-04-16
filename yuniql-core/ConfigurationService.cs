using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using Yuniql.Extensibility;

namespace Yuniql.Core
{
    ///<inheritdoc/>
    public class ConfigurationService : IConfigurationService
    {
        private readonly IEnvironmentService _environmentService;
        private readonly IWorkspaceService _workspaceService;
        private readonly ITraceService _traceService;

        ///<inheritdoc/>
        public ConfigurationService(
            IEnvironmentService environmentService,
            IWorkspaceService workspaceService,
            ITraceService traceService)
        {
            this._environmentService = environmentService;
            this._workspaceService = workspaceService;
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
            configuration.Workspace = GetValueOrDefault(configuration.Workspace, ENVIRONMENT_VARIABLE.YUNIQL_WORKSPACE, defaultValue: _environmentService.GetCurrentDirectory());
            configuration.IsDebug = configuration.IsDebug;

            //BasePlatformOption
            configuration.Platform = GetValueOrDefault(configuration.Platform, ENVIRONMENT_VARIABLE.YUNIQL_PLATFORM, defaultValue: SUPPORTED_DATABASES.SQLSERVER);
            configuration.ConnectionString = GetValueOrDefault(configuration.ConnectionString, ENVIRONMENT_VARIABLE.YUNIQL_CONNECTION_STRING);
            configuration.CommandTimeout = configuration.CommandTimeout;

            //BaseRunPlatformOption (refer to Runption, VerifyOption)
            configuration.TargetVersion = configuration.TargetVersion;
            configuration.IsAutoCreateDatabase = configuration.IsAutoCreateDatabase;
            configuration.Tokens = configuration.Tokens;
            configuration.BulkSeparator = configuration.BulkSeparator;
            configuration.BulkBatchSize = configuration.BulkBatchSize;
            configuration.Environment = configuration.Environment;
            configuration.MetaSchemaName = configuration.MetaSchemaName;
            configuration.MetaTableName = configuration.MetaTableName;
            configuration.TransactionMode = configuration.TransactionMode;
            configuration.IsContinueAfterFailure = configuration.IsContinueAfterFailure;
            configuration.IsRequiredClearedDraft = configuration.IsRequiredClearedDraft;

            //EraseOption
            configuration.IsForced = configuration.IsForced;

            //Non-cli captured configuration
            configuration.IsVerifyOnly = configuration.IsVerifyOnly;
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
            configuration.Workspace = null;
            configuration.IsDebug = false;

            //BasePlatformOption
            configuration.Platform = null;
            configuration.ConnectionString = null;
            configuration.CommandTimeout = DEFAULT_CONSTANTS.COMMAND_TIMEOUT_SECS;

            //BaseRunPlatformOption (refer to Runption, VerifyOption)
            configuration.TargetVersion = null;
            configuration.IsAutoCreateDatabase = false;
            configuration.Tokens = null;
            configuration.BulkSeparator = DEFAULT_CONSTANTS.BULK_SEPARATOR;
            configuration.BulkBatchSize = DEFAULT_CONSTANTS.BULK_BATCH_SIZE;
            configuration.Environment = null;
            configuration.MetaSchemaName = null;
            configuration.MetaTableName = null;
            configuration.TransactionMode = TRANSACTION_MODE.SESSION;
            configuration.IsContinueAfterFailure = null;
            configuration.IsRequiredClearedDraft = false;

            //EraseOption
            configuration.IsForced = configuration.IsForced;

            //Non-cli captured configuration
            configuration.IsVerifyOnly = false;
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
                validationResults.Add(new Tuple<string, string, string, string>("Platform", "--platform", ENVIRONMENT_VARIABLE.YUNIQL_PLATFORM, $"{helpLink}"));

            ////workspace, this is not needed for erase and destroy commands
            //if (string.IsNullOrEmpty(_configuration.Workspace))
            //    validationResults.Add(new Tuple<string, string, string, string>("Workspace", "-p | --path", ENVIRONMENT_VARIABLE.YUNIQL_WORKSPACE, $"{helpLink}"));

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
        public string PrintAsJson()
        {
            var configuration = GetConfiguration();
            var configurationString = JsonSerializer.Serialize(configuration, new JsonSerializerOptions
            {
                WriteIndented = true,
                IgnoreReadOnlyProperties = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            //if TraceSensitiveData is true, do not redact the sensitive data
            if (!_traceService.IsTraceSensitiveData)
                configurationString = configurationString.Replace(configuration.ConnectionString, "<sensitive-data-redacted>");

            return configurationString;
        }

    }

}
