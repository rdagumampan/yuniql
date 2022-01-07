using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Yuniql.Core;
using Yuniql.Extensibility;

namespace Yuniql.CLI
{
    public class CommandLineService : ICommandLineService
    {
        private IMigrationServiceFactory _migrationServiceFactory;
        private readonly IDataServiceFactory _dataServiceFactory;
        private readonly IManifestFactory _manifestFactory;
        private readonly IWorkspaceService _workspaceService;
        private readonly IEnvironmentService _environmentService;
        private ITraceService _traceService;
        private readonly IConfigurationService _configurationService;

        public CommandLineService(
            IMigrationServiceFactory migrationServiceFactory,
            IDataServiceFactory dataServiceFactory,
            IManifestFactory manifestFactory,
            IWorkspaceService workspaceService,
            IEnvironmentService environmentService,
            ITraceService traceService,
            IConfigurationService configurationService)
        {
            this._workspaceService = workspaceService;
            this._environmentService = environmentService;
            this._traceService = traceService;
            this._configurationService = configurationService;
            this._migrationServiceFactory = migrationServiceFactory;
            this._dataServiceFactory = dataServiceFactory;
            this._manifestFactory = manifestFactory;
        }

        private Configuration SetupRunConfiguration(BaseRunPlatformOption opts, bool isVerifyOnly = false)
        {
            var configuration = Configuration.Instance;

            var platform = _configurationService.GetValueOrDefault(opts.Platform, ENVIRONMENT_VARIABLE.YUNIQL_PLATFORM, defaultValue: SUPPORTED_DATABASES.SQLSERVER);
            var tokens = opts.Tokens.Select(t => new KeyValuePair<string, string>(t.Split("=")[0], t.Split("=")[1])).ToList();

            if (!string.IsNullOrEmpty(opts.MetaSchemaName))
                tokens.Add(new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_SCHEMA_NAME, opts.MetaSchemaName));

            if (!string.IsNullOrEmpty(opts.MetaTableName))
                tokens.Add(new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_TABLE_NAME, opts.MetaTableName));

            tokens.AddRange(new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_APPLIED_BY_TOOL, "yuniql-cli"),
                    new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_APPLIED_BY_TOOL_VERSION, this.GetType().Assembly.GetName().Version.ToString())
                });

            configuration.Workspace = opts.Workspace;
            configuration.IsDebug = opts.IsDebug;

            configuration.Platform = platform;
            configuration.ConnectionString = opts.ConnectionString;
            configuration.CommandTimeout = opts.CommandTimeout;

            configuration.TargetVersion = opts.TargetVersion;
            configuration.IsAutoCreateDatabase = opts.IsAutoCreateDatabase;
            configuration.Tokens = tokens;
            configuration.BulkSeparator = opts.BulkSeparator;
            configuration.BulkBatchSize = opts.BulkBatchSize;
            configuration.MetaSchemaName = opts.MetaSchemaName;
            configuration.MetaTableName = opts.MetaTableName;
            configuration.Environment = opts.Environment;
            configuration.TransactionMode = opts.TransactionMode;
            configuration.IsContinueAfterFailure = opts.IsContinueAfterFailure;
            configuration.IsRequiredClearedDraft = opts.IsRequiredClearedDraft;

            configuration.IsVerifyOnly = isVerifyOnly;

            configuration.AppliedByTool = "yuniql-cli";
            configuration.AppliedByToolVersion = this.GetType().Assembly.GetName().Version.ToString();

            return configuration;
        }

        public int RunCheckOption(CheckOption opts)
        {
            //run the migration
            string connectionString = _configurationService.GetValueOrDefault(opts.ConnectionString, ENVIRONMENT_VARIABLE.YUNIQL_CONNECTION_STRING); ;
            string platform = _configurationService.GetValueOrDefault(opts.Platform, ENVIRONMENT_VARIABLE.YUNIQL_PLATFORM, defaultValue: SUPPORTED_DATABASES.SQLSERVER);

            try
            {
                var dataService = _dataServiceFactory.Create(platform);
                dataService.Initialize(connectionString);

                var connectivityService = new ConnectivityService(dataService, _traceService);
                connectivityService.CheckConnectivity();
            }
            catch (Exception ex)
            {
                return OnException(ex, "Failed to execute ping function", opts.IsDebug);
            }

            return 0;
        }

        public int RunInitOption(InitOption opts)
        {
            try
            {
                opts.Workspace = _configurationService.GetValueOrDefault(opts.Workspace, ENVIRONMENT_VARIABLE.YUNIQL_WORKSPACE, defaultValue: _environmentService.GetCurrentDirectory());
                _workspaceService.Init(opts.Workspace);
                _traceService.Success($"Initialized {opts.Workspace}.");
                return 0;
            }
            catch (Exception ex)
            {
                return OnException(ex, "Failed to execute init function", opts.IsDebug);
            }
        }

        public int RunNextVersionOption(NextVersionOption opts)
        {
            try
            {
                opts.Workspace = _configurationService.GetValueOrDefault(opts.Workspace, ENVIRONMENT_VARIABLE.YUNIQL_WORKSPACE, defaultValue: _environmentService.GetCurrentDirectory());
                if (opts.IncrementMajorVersion)
                {
                    var nextVersion = _workspaceService.IncrementMajorVersion(opts.Workspace, opts.File);
                    _traceService.Success($"New major version created {nextVersion} on {opts.Workspace}.");
                }
                else if (opts.IncrementMinorVersion || (!opts.IncrementMajorVersion && !opts.IncrementMinorVersion))
                {
                    var nextVersion = _workspaceService.IncrementMinorVersion(opts.Workspace, opts.File);
                    _traceService.Success($"New minor version created {nextVersion} on {opts.Workspace}.");
                }

                return 0;
            }
            catch (Exception ex)
            {
                return OnException(ex, "Failed to execute vnext function", opts.IsDebug);
            }
        }

        public int RunRunOption(RunOption opts)
        {
            try
            {
                //run the migration
                var configuration = SetupRunConfiguration(opts, isVerifyOnly: false);
                var migrationService = _migrationServiceFactory.Create(configuration.Platform);
                migrationService.Run();

                _traceService.Success($"Schema migration completed successfuly on {configuration.Workspace}.");
                return 0;
            }
            catch (Exception ex)
            {
                return OnException(ex, "Failed to execute run function", opts.IsDebug);
            }
        }

        public int RunApplyOption(ApplyOption opts)
        {
            try
            {
                //run the migration
                var configuration = SetupRunConfiguration(opts, isVerifyOnly: false);
                var migrationService = _migrationServiceFactory.Create(configuration.Platform);
                migrationService.Run();

                _traceService.Success($"Schema migration completed successfuly on {configuration.Workspace}.");
                return 0;
            }
            catch (Exception ex)
            {
                return OnException(ex, "Failed to execute apply function", opts.IsDebug);
            }
        }

        public int RunVerifyOption(VerifyOption opts)
        {
            try
            {
                //run the migration
                var configuration = SetupRunConfiguration(opts, isVerifyOnly: true);
                var migrationService = _migrationServiceFactory.Create(configuration.Platform);
                migrationService.Run();

                _traceService.Success($"Schema migration verification completed successfuly on {configuration.Workspace}.");
                return 0;
            }
            catch (Exception ex)
            {
                return OnException(ex, "Failed to execute verification function. Target database will be rolled back to its previous state", opts.IsDebug);
            }
        }

        public int RunListOption(ListOption opts)
        {
            try
            {
                var platform = _configurationService.GetValueOrDefault(opts.Platform, ENVIRONMENT_VARIABLE.YUNIQL_PLATFORM, defaultValue: SUPPORTED_DATABASES.SQLSERVER);

                var configuration = Configuration.Instance;
                configuration.Workspace = opts.Workspace;
                configuration.IsDebug = opts.IsDebug;

                configuration.Platform = platform;
                configuration.ConnectionString = opts.ConnectionString;
                configuration.CommandTimeout = opts.CommandTimeout;

                configuration.MetaSchemaName = opts.MetaSchema;
                configuration.MetaTableName = opts.MetaTable;

                //get all exsiting db versions
                var migrationService = _migrationServiceFactory.Create(configuration.Platform);
                var versions = migrationService.GetAllVersions(configuration.MetaSchemaName, configuration.MetaTableName);

                var versionPrettyPrint = new TablePrinter("SchemaVersion", "AppliedOnUtc", "Status", "AppliedByUser", "AppliedByTool", "Duration");
                versions.ForEach(v => versionPrettyPrint.AddRow(v.Version, v.AppliedOnUtc.ToString("u"), v.Status, v.AppliedByUser, $"{v.AppliedByTool} {v.AppliedByToolVersion}", $"{v.DurationMs} ms / {v.DurationMs / 1000} s"));
                versionPrettyPrint.Print();

                var failedVersion = versions.LastOrDefault(v => v.Status == Status.Failed);
                if (null != failedVersion)
                {
                    var failedVersionMessage = $"Previous run was not successful, see details below:{Environment.NewLine}" +
                        $"Last failed version: {failedVersion.Version}{Environment.NewLine}" +
                        $"Last failed script: {failedVersion.FailedScriptPath}{Environment.NewLine}" +
                        $"Last error message: {failedVersion.FailedScriptError}{Environment.NewLine}" +
                        $"Suggested action: Fix the failed script and run manually outside of yuniql." +
                        @$"After that, try to issue ""yuniql run"" command again with ""--continue-after-failure"" parameter.{Environment.NewLine}";
                    _traceService.Warn(failedVersionMessage);
                }

                _traceService.Success($"Listed all schema versions applied to database on {configuration.Workspace} workspace.{Environment.NewLine}" +
                    $"For platforms not supporting full transactional DDL operations (ex. MySql, Snowflake, CockroachDB), unsuccessful migrations will show the status as Failed and you can look for FailedScriptPath and FailedScriptError in the schema version tracking table.");

                return 0;
            }
            catch (Exception ex)
            {
                return OnException(ex, "Failed to execute list function", opts.IsDebug);
            }
        }

        public int RunEraseOption(EraseOption opts)
        {
            try
            {
                //parse tokens
                var platform = _configurationService.GetValueOrDefault(opts.Platform, ENVIRONMENT_VARIABLE.YUNIQL_PLATFORM, defaultValue: SUPPORTED_DATABASES.SQLSERVER);
                var tokens = opts.Tokens.Select(t => new KeyValuePair<string, string>(t.Split("=")[0], t.Split("=")[1])).ToList();
                if (!string.IsNullOrEmpty(opts.MetaSchemaName))
                    tokens.Add(new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_SCHEMA_NAME, opts.MetaSchemaName));
                if (!string.IsNullOrEmpty(opts.MetaTableName))
                    tokens.Add(new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_TABLE_NAME, opts.MetaTableName));
                tokens.AddRange(new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_APPLIED_BY_TOOL, "yuniql-cli"),
                    new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_APPLIED_BY_TOOL_VERSION, this.GetType().Assembly.GetName().Version.ToString())
                });

                var configuration = Configuration.Instance;
                configuration.Workspace = opts.Workspace;
                configuration.IsDebug = opts.IsDebug;

                configuration.Platform = platform;
                configuration.ConnectionString = opts.ConnectionString;
                configuration.CommandTimeout = opts.CommandTimeout;

                configuration.Tokens = tokens;
                configuration.IsForced = opts.Force;
                configuration.Environment = opts.Environment;
                configuration.MetaSchemaName = opts.MetaSchemaName;
                configuration.MetaTableName = opts.MetaTableName;

                //run all erase scripts
                var migrationService = _migrationServiceFactory.Create(platform);
                migrationService.Erase();

                _traceService.Success($"Schema erase completed successfuly on {configuration.Workspace}.");
                return 0;
            }
            catch (Exception ex)
            {
                return OnException(ex, "Failed to execute erase function", opts.IsDebug);
            }
        }

        public int RunDestroyOption(DestroyOption opts)
        {
            try
            {
                //parse tokens
                var platform = _configurationService.GetValueOrDefault(opts.Platform, ENVIRONMENT_VARIABLE.YUNIQL_PLATFORM, defaultValue: SUPPORTED_DATABASES.SQLSERVER);
                var connectionString = _configurationService.GetValueOrDefault(opts.ConnectionString, ENVIRONMENT_VARIABLE.YUNIQL_CONNECTION_STRING);

                var configuration = Configuration.Instance;
                configuration.Workspace = opts.Workspace;
                configuration.IsDebug = opts.IsDebug;

                configuration.Platform = platform;
                configuration.ConnectionString = connectionString;
                configuration.CommandTimeout = opts.CommandTimeout;

                configuration.IsForced = opts.Force;

                var dataService = _dataServiceFactory.Create(platform);
                dataService.Initialize(configuration.ConnectionString);
                var connectionInfo = dataService.GetConnectionInfo();

                //run all erase scripts
                var migrationService = _migrationServiceFactory.Create(platform);
                migrationService.Destroy();

                _traceService.Success($"Database {connectionInfo.Database} destroyed successfuly from {connectionInfo.DataSource}.");
                return 0;
            }
            catch (Exception ex)
            {
                return OnException(ex, "Failed to execute destroy function", opts.IsDebug);
            }
        }

        public int RunPlatformsOption(PlatformsOption opts)
        {
            try
            {
                Console.WriteLine(@"Supported database platforms and available samples. For specific versions, please refer to latest documentation pages.");
                 
                //iterate through supported db and get manifest data
                var fields = typeof(SUPPORTED_DATABASES).GetFields();
                foreach (var field in fields)
                {
                    var platformManifest = _manifestFactory.Create(field.Name.ToLower());
                    var message = string.Format(@"
Name: {0}
Usage:{1}
Getting started: {2}
Samples: {3}", platformManifest.Name, platformManifest.Usage, platformManifest.DocumentationUrl, platformManifest.SamplesUrl);
                    Console.WriteLine(message);
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                return OnException(ex, "Failed to execute platforms function", opts.IsDebug);
            }
        }

        public int RunBaselineOption(BaselineOption opts)
        {
            try
            {
                throw new NotImplementedException("Not yet implemented, stay tune!");
            }
            catch (Exception ex)
            {
                return OnException(ex, "Failed to execute baseline function", opts.IsDebug);

            }
        }

        public int RunRebaseOption(RebaseOption opts)
        {
            try
            {
                throw new NotImplementedException("Not yet implemented, stay tune!");
            }
            catch (Exception ex)
            {
                return OnException(ex, "Failed to execute rebase function", opts.IsDebug);

            }
        }

        public int RunArchiveOption(ArchiveOption opts)
        {
            try
            {
                throw new NotImplementedException("Not yet implemented, stay tune!");
            }
            catch (Exception ex)
            {
                return OnException(ex, "Failed to execute archive function", opts.IsDebug);
            }
        }

        private int OnException(Exception exception, string headerMessage, bool debug)
        {
            var stackTraceMessage = debug ? exception.ToString().Replace(exception.Message, string.Empty)
                : $"{exception.Message} {exception.InnerException?.Message}";

            _traceService.Error($"{headerMessage}. {exception.Message}{Environment.NewLine}" +
                $"Diagnostics stack trace captured a {stackTraceMessage}{Environment.NewLine}" +
                $"If you think this is a bug, please report an issue here https://github.com/rdagumampan/yuniql/issues."); //TODO: create global constants for url
            return 1;
        }

    }
}
