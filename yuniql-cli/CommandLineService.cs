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
        private readonly ILocalVersionService _localVersionService;
        private readonly IEnvironmentService _environmentService;
        private ITraceService _traceService;
        private readonly IConfigurationService _configurationService;

        public CommandLineService(
            IMigrationServiceFactory migrationServiceFactory,
            ILocalVersionService localVersionService,
            IEnvironmentService environmentService,
            ITraceService traceService,
            IConfigurationService configurationService)
        {
            this._localVersionService = localVersionService;
            this._environmentService = environmentService;
            this._traceService = traceService;
            this._configurationService = configurationService;
            this._migrationServiceFactory = migrationServiceFactory;
        }

        public int RunInitOption(InitOption opts)
        {
            try
            {
                opts.Path = _configurationService.GetValueOrDefault(opts.Path, ENVIRONMENT_VARIABLE.YUNIQL_WORKSPACE, defaultValue: _environmentService.GetCurrentDirectory());
                _localVersionService.Init(opts.Path);
                _traceService.Success($"Initialized {opts.Path}.");
                return 0;
            }
            catch (Exception ex)
            {
                return OnException(ex, "Failed to execute init function", opts.Debug, _traceService);
            }
        }

        public int RunNextVersionOption(NextVersionOption opts)
        {
            try
            {
                opts.Path = _configurationService.GetValueOrDefault(opts.Path, ENVIRONMENT_VARIABLE.YUNIQL_WORKSPACE, defaultValue: _environmentService.GetCurrentDirectory());
                if (opts.IncrementMajorVersion)
                {
                    var nextVersion = _localVersionService.IncrementMajorVersion(opts.Path, opts.File);
                    _traceService.Success($"New major version created {nextVersion} on {opts.Path}.");
                }
                else if (opts.IncrementMinorVersion || (!opts.IncrementMajorVersion && !opts.IncrementMinorVersion))
                {
                    var nextVersion = _localVersionService.IncrementMinorVersion(opts.Path, opts.File);
                    _traceService.Success($"New minor version created {nextVersion} on {opts.Path}.");
                }

                return 0;
            }
            catch (Exception ex)
            {
                return OnException(ex, "Failed to execute vnext function", opts.Debug, _traceService);
            }
        }

        private Configuration SetupRunConfiguration(BaseRunPlatformOption opts, bool verifyOnly = false)
        {
            var configuration = Configuration.Instance;

            var platform = _configurationService.GetValueOrDefault(opts.Platform, ENVIRONMENT_VARIABLE.YUNIQL_PLATFORM, defaultValue: SUPPORTED_DATABASES.SQLSERVER);
            var tokens = opts.Tokens.Select(t => new KeyValuePair<string, string>(t.Split("=")[0], t.Split("=")[1])).ToList();

            configuration.Platform = platform;
            configuration.Workspace = opts.Path;
            configuration.ConnectionString = opts.ConnectionString;
            configuration.TargetVersion = opts.TargetVersion;
            configuration.IsAutoCreateDatabase = opts.AutoCreateDatabase;
            configuration.Tokens = tokens;
            configuration.IsVerifyOnly = verifyOnly;
            configuration.BulkSeparator = opts.BulkSeparator;
            configuration.BulkBatchSize = opts.BulkBatchSize;
            configuration.MetaSchemaName = opts.MetaSchema;
            configuration.MetaTableName = opts.MetaTable;
            configuration.CommandTimeout = opts.CommandTimeout;
            configuration.Environment = opts.Environment;
            configuration.IsContinueAfterFailure = opts.ContinueAfterFailure;
            configuration.TransactionMode = opts.TransactionMode;
            configuration.IsRequiredClearedDraft = opts.RequiredClearedDraft;
            configuration.AppliedByTool = "yuniql-cli";
            configuration.AppliedByToolVersion = this.GetType().Assembly.GetName().Version.ToString();

            return configuration;
        }

        public int RunRunOption(RunOption opts)
        {
            try
            {
                //run the migration
                var configuration = SetupRunConfiguration(opts, verifyOnly: false);
                var migrationService = _migrationServiceFactory.Create(configuration.Platform);
                migrationService.Run();

                _traceService.Success($"Schema migration completed successfuly on {configuration.Workspace}.");
                return 0;
            }
            catch (Exception ex)
            {
                return OnException(ex, "Failed to execute run function", opts.Debug, _traceService);
            }
        }
        
        public int RunVerifyOption(VerifyOption opts)
        {
            try
            {
                //run the migration
                var configuration = SetupRunConfiguration(opts, verifyOnly: true);
                var migrationService = _migrationServiceFactory.Create(configuration.Platform);
                migrationService.Run();

                _traceService.Success($"Schema migration verification completed successfuly on {configuration.Workspace}.");
                return 0;
            }
            catch (Exception ex)
            {
                return OnException(ex, "Failed to execute verification function. Target database will be rolled back to its previous state", opts.Debug, _traceService);
            }
        }

        public int RunListOption(ListOption opts)
        {
            try
            {
                var platform = _configurationService.GetValueOrDefault(opts.Platform, ENVIRONMENT_VARIABLE.YUNIQL_PLATFORM, defaultValue: SUPPORTED_DATABASES.SQLSERVER);

                var configuration = Configuration.Instance;
                configuration.Platform = platform;
                configuration.Workspace = opts.Path;
                configuration.ConnectionString = opts.ConnectionString;
                configuration.MetaSchemaName = opts.MetaSchema;
                configuration.MetaTableName = opts.MetaTable;
                configuration.CommandTimeout = opts.CommandTimeout;

                //get all exsiting db versions
                var migrationService = _migrationServiceFactory.Create(configuration.Platform);
                var versions = migrationService.GetAllVersions(configuration.MetaSchemaName, configuration.MetaTableName);

                var versionPrettyPrint = new TablePrinter("SchemaVersion", "AppliedOnUtc", "Status", "AppliedByUser", "AppliedByTool");
                versions.ForEach(v => versionPrettyPrint.AddRow(v.Version, v.AppliedOnUtc.ToString("u"), v.Status, v.AppliedByUser, $"{v.AppliedByTool} {v.AppliedByToolVersion}"));
                versionPrettyPrint.Print();

                _traceService.Success($"Listed all schema versions applied to database on {configuration.Workspace} workspace.{Environment.NewLine}" +
                    $"For platforms not supporting full transactional DDL operations (ex. MySql, CockroachDB, Snowflake), unsuccessful migrations will show the status as Failed and you can look for LastFailedScript and LastScriptError in the schema version tracking table.");

                return 0;
            }
            catch (Exception ex)
            {
                return OnException(ex, "Failed to execute info function", opts.Debug, _traceService);
            }
        }

        public int RunEraseOption(EraseOption opts)
        {
            try
            {
                //parse tokens
                var platform = _configurationService.GetValueOrDefault(opts.Platform, ENVIRONMENT_VARIABLE.YUNIQL_PLATFORM, defaultValue: SUPPORTED_DATABASES.SQLSERVER);
                var tokens = opts.Tokens.Select(t => new KeyValuePair<string, string>(t.Split("=")[0], t.Split("=")[1])).ToList();

                var configuration = Configuration.Instance;
                configuration.Platform = platform;
                configuration.Workspace = opts.Path;
                configuration.IsDebug = opts.Debug;
                configuration.ConnectionString = opts.ConnectionString;
                configuration.CommandTimeout = opts.CommandTimeout;
                configuration.Tokens = tokens;
                configuration.IsForced = opts.Force;
                configuration.Environment = opts.Environment;

                //run all erase scripts
                var migrationService = _migrationServiceFactory.Create(platform);
                migrationService.Erase();

                _traceService.Success($"Schema erase completed successfuly on {configuration.Workspace}.");
                return 0;
            }
            catch (Exception ex)
            {
                return OnException(ex, "Failed to execute erase function", opts.Debug, _traceService);
            }
        }

        public int RunPlatformsOption(PlatformsOption opts)
        {
            try
            {
                string platforms = @"Supported database platforms and available samples. For specific versions, please refer to latest documentation pages.

    SqlServer: 
        Supported versions: https://yuniql.io/docs/supported-platforms/
        Usage: yuniql run -a -c <your-connection-string> --platform sqlserver
        Samples: https://github.com/rdagumampan/yuniql/tree/master/samples/basic-sqlserver-sample

    PostgreSql: 
        Supported versions: https://yuniql.io/docs/supported-platforms/
        Usage: yuniql run -a -c <your-connection-string> --platform postgresql
        Samples: https://github.com/rdagumampan/yuniql/tree/master/samples/basic-postgresql-sample

    MySql: 
        Supported versions: https://yuniql.io/docs/supported-platforms/
        Usage: yuniql run -a -c <your-connection-string> --platform mysql
        Samples: https://github.com/rdagumampan/yuniql/tree/master/samples/basic-mysql-sample

    MariaDb: 
        Supported versions: https://yuniql.io/docs/supported-platforms/
        Supported versions: 
        Usage: yuniql run -a -c <your-connection-string> --platform mariadb
        Samples: https://github.com/rdagumampan/yuniql/tree/master/samples/basic-mysql-sample";

                Console.WriteLine(platforms);

                return 0;
            }
            catch (Exception ex)
            {
                return OnException(ex, "Failed to execute RunPlatformsOption function", opts.Debug, _traceService);
            }
        }

        private int OnException(Exception exception, string headerMessage, bool debug, ITraceService traceService)
        {
            var userMessage = debug ? exception.ToString() : $"{exception.Message} {exception.InnerException?.Message}";
            traceService.Error($"{headerMessage}. Arrg... something seems broken.{Environment.NewLine}" +
                $"Internal error message: {userMessage}.{Environment.NewLine}" +
                $"If you think this is a bug, please report an issue here https://github.com/rdagumampan/yuniql/issues.");
            return 1;
        }

        public int RunBaselineOption(BaselineOption opts)
        {
            try
            {
                throw new NotImplementedException("Not yet implemented, stay tune!");
            }
            catch (Exception ex)
            {
                return OnException(ex, "Failed to execute baseline function", opts.Debug, _traceService);

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
                return OnException(ex, "Failed to execute rebase function", opts.Debug, _traceService);

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
                return OnException(ex, "Failed to archive function", opts.Debug, _traceService);
            }
        }
    }
}
