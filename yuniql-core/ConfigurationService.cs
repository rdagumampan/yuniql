using System;
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
        private readonly Configuration _configuration = new Configuration();

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
        public Configuration Initialize(Configuration configuration)
        {

            //deep copy all the properties and post process defaults based on this hierarchy
            //cli parameters -> environment variables -> defaults from internal core logic

            //BaseOption
            _configuration.WorkspacePath = GetValueOrDefault(configuration.WorkspacePath, ENVIRONMENT_VARIABLE.YUNIQL_WORKSPACE, defaultValue: _environmentService.GetCurrentDirectory());
            _configuration.DebugTraceMode = configuration.DebugTraceMode;

            //BasePlatformOption
            _configuration.Platform = GetValueOrDefault(configuration.Platform, ENVIRONMENT_VARIABLE.YUNIQL_TARGET_PLATFORM, defaultValue: SUPPORTED_DATABASES.SQLSERVER);
            _configuration.ConnectionString = GetValueOrDefault(configuration.ConnectionString, ENVIRONMENT_VARIABLE.YUNIQL_CONNECTION_STRING);
            _configuration.CommandTimeout = configuration.CommandTimeout;

            //BaseRunPlatformOption (Runption, VerifyOption)
            _configuration.TargetVersion = configuration.TargetVersion;
            _configuration.AutoCreateDatabase = configuration.AutoCreateDatabase;
            _configuration.Tokens = configuration.Tokens;
            _configuration.BulkSeparator = configuration.BulkSeparator;
            _configuration.BulkBatchSize = configuration.BulkBatchSize;
            _configuration.Environment = configuration.Environment;
            _configuration.MetaSchemaName = configuration.MetaSchemaName;
            _configuration.MetaTableName = configuration.MetaTableName;
            _configuration.TransactionMode = configuration.TransactionMode;
            _configuration.ContinueAfterFailure = configuration.ContinueAfterFailure;
            _configuration.RequiredClearedDraft = configuration.RequiredClearedDraft;

            //EraseOption
            _configuration.IsForced = configuration.IsForced;

            //Non-cli captured configuration
            _configuration.VerifyOnly = configuration.VerifyOnly;
            _configuration.AppliedByTool = configuration.AppliedByTool;
            _configuration.AppliedByToolVersion = configuration.AppliedByToolVersion;

            return configuration;
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
        public Configuration GetConfiguration()
        {
            return _configuration;
        }

        ///<inheritdoc/>
        public void Validate()
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        public string PrintAsJson()
        {
            var configurationString = JsonSerializer.Serialize(_configuration, new JsonSerializerOptions { WriteIndented = true, IgnoreReadOnlyProperties = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            configurationString = configurationString.Replace(_configuration.ConnectionString, "<redacted>");

            return configurationString;
        }

    }

}
