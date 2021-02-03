using System.Text.Json;
using Yuniql.Extensibility;

namespace Yuniql.Core
{
    ///<inheritdoc/>
    public class ConfigurationHelper
    {
        ///<inheritdoc/>
        public static void Initialize(AspNetCore.Configuration configuration)
        {
            var _configuration = Configuration.Instance;

            //deep copy all the properties and post process defaults based on this hierarchy
            //session property -> environment variables -> defaults from internal core logic

            //base properties
            _configuration.Workspace = configuration.Workspace;
            _configuration.IsDebug = configuration.IsDebug;

            //base platform properties
            _configuration.Platform = configuration.Platform;
            _configuration.ConnectionString = configuration.ConnectionString;
            _configuration.CommandTimeout = configuration.CommandTimeout;

            //base run platform properties (refer to run, verify)
            _configuration.TargetVersion = configuration.TargetVersion;
            _configuration.IsAutoCreateDatabase = configuration.IsAutoCreateDatabase;
            _configuration.Tokens = configuration.Tokens;
            _configuration.BulkSeparator = configuration.BulkSeparator;
            _configuration.BulkBatchSize = configuration.BulkBatchSize;
            _configuration.Environment = configuration.Environment;
            _configuration.MetaSchemaName = configuration.MetaSchemaName;
            _configuration.MetaTableName = configuration.MetaTableName;
            _configuration.TransactionMode = configuration.TransactionMode;
            _configuration.IsContinueAfterFailure = configuration.IsContinueAfterFailure;
            _configuration.IsRequiredClearedDraft = configuration.IsRequiredClearedDraft;

            //erase
            _configuration.IsForced = configuration.IsForced;

            //misc properties
            _configuration.IsVerifyOnly = configuration.IsVerifyOnly;
            _configuration.AppliedByTool = configuration.AppliedByTool;
            _configuration.AppliedByToolVersion = configuration.AppliedByToolVersion;

            //this must be set to false in this project
            //setting this to true to break the logic in Yuniql.MigrationService.Run()
            _configuration.IsInitialized = false;
        }

        ///<inheritdoc/>
        public static string PrintAsJson(bool redactSensitiveText = true)
        {
            var _configuration = Configuration.Instance;
            var configurationString = JsonSerializer.Serialize(_configuration, new JsonSerializerOptions { WriteIndented = true, IgnoreReadOnlyProperties = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            if (redactSensitiveText)
                configurationString = configurationString.Replace(_configuration.ConnectionString, "<sensitive-data-redacted>");

            return configurationString;
        }

    }

}
