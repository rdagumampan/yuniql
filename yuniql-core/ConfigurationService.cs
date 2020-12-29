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
        public void Initialize(Configuration configuration)
        {

            //deep copy all the properties and post process defaults based on this hierarchy
            //cli parameters -> environment variables -> defaults from internal core logic
            _configuration.WorkspacePath = configuration.WorkspacePath;
            _configuration.Platform = configuration.Platform;
            _configuration.ConnectionString = configuration.ConnectionString;
            _configuration.AutoCreateDatabase = configuration.AutoCreateDatabase;
            _configuration.TargetVersion = configuration.TargetVersion;
            _configuration.Tokens = configuration.Tokens;
            _configuration.VerifyOnly = configuration.VerifyOnly;
            _configuration.BulkSeparator = configuration.BulkSeparator;
            _configuration.BulkBatchSize = configuration.BulkBatchSize;
            _configuration.CommandTimeout = configuration.CommandTimeout;
            _configuration.DebugTraceMode = configuration.DebugTraceMode;
            _configuration.AppliedByTool = configuration.AppliedByTool;
            _configuration.AppliedByToolVersion = configuration.AppliedByToolVersion;
            _configuration.Environment = configuration.Environment;
            _configuration.MetaSchemaName = configuration.MetaSchemaName;
            _configuration.MetaTableName = configuration.MetaTableName;
            _configuration.ContinueAfterFailure = configuration.ContinueAfterFailure;
            _configuration.TransactionMode = configuration.TransactionMode;
            _configuration.RequiredClearedDraft = configuration.RequiredClearedDraft;

            //if no path provided, we default into environment variable
            if (string.IsNullOrEmpty(_configuration.WorkspacePath))
            {
                _configuration.WorkspacePath = _environmentService.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_WORKSPACE);

                //if no path provided, we default into current directory
                if (string.IsNullOrEmpty(_configuration.WorkspacePath))
                {
                    _configuration.WorkspacePath = _environmentService.GetCurrentDirectory();
                }
            }
            _traceService.Info($"Started migration from {_configuration.WorkspacePath}.");

            //if no target platform provided, we default into sqlserver
            if (string.IsNullOrEmpty(_configuration.Platform))
            {
                _configuration.Platform = _environmentService.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_TARGET_PLATFORM);
                if (string.IsNullOrEmpty(_configuration.Platform))
                {
                    _configuration.Platform = SUPPORTED_DATABASES.SQLSERVER;
                }
            }

            //if no connection string provided, we default into environment variable or throw exception
            if (string.IsNullOrEmpty(_configuration.ConnectionString))
            {
                _configuration.ConnectionString = _environmentService.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_CONNECTION_STRING);
            }
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
