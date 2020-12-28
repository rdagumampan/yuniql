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
                //if no path provided, we default into current directory
                if (string.IsNullOrEmpty(opts.Path))
                {
                    opts.Path = _environmentService.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_WORKSPACE);
                    if (string.IsNullOrEmpty(opts.Path))
                    {
                        opts.Path = _environmentService.GetCurrentDirectory();
                    }
                }

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
                //if no path provided, we default into current directory
                if (string.IsNullOrEmpty(opts.Path))
                {
                    opts.Path = _environmentService.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_WORKSPACE);
                    if (string.IsNullOrEmpty(opts.Path))
                    {
                        opts.Path = _environmentService.GetCurrentDirectory();
                    }
                }

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

        public int RunRunOption(RunOption opts)
        {
            try
            {
                //prepare session configuration and validate
                var tokens = opts.Tokens.Select(t => new KeyValuePair<string, string>(t.Split("=")[0], t.Split("=")[1])).ToList();
                var configuration = new Configuration
                {
                    WorkspacePath = opts.Path,
                    TargetVersion = opts.TargetVersion,
                    AutoCreateDatabase= opts.AutoCreateDatabase,
                    Tokens = tokens,
                    VerifyOnly= false,
                    BulkSeparator = opts.BulkSeparator,
                    BulkBatchSize = opts.BulkBatchSize,
                    MetaSchemaName = opts.MetaSchema,
                    MetaTableName = opts.MetaTable,
                    CommandTimeout = opts.CommandTimeout,
                    Environment = opts.Environment,
                    ContinueAfterFailure =  opts.ContinueAfterFailure,
                    TransactionMode = opts.TransactionMode,
                    RequiredClearedDraft =  opts.RequiredClearedDraft,
                    AppliedByTool = "yuniql-cli",
                    AppliedByToolVersion = this.GetType().Assembly.GetName().Version.ToString(),
                };
                _configurationService.AssignDefaults(configuration);

                //run the migration
                var migrationService = _migrationServiceFactory.Create(opts.Platform);
                migrationService.Initialize(opts.ConnectionString, opts.CommandTimeout);
                migrationService.Run(configuration);

                _traceService.Success($"Schema migration completed successfuly on {opts.Path}.");
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
                //prepare session configuration and validate
                var tokens = opts.Tokens.Select(t => new KeyValuePair<string, string>(t.Split("=")[0], t.Split("=")[1])).ToList();
                var configuration = new Configuration
                {
                    WorkspacePath = opts.Path,
                    TargetVersion = opts.TargetVersion,
                    AutoCreateDatabase = opts.AutoCreateDatabase,
                    Tokens = tokens,
                    VerifyOnly = true,
                    BulkSeparator = opts.BulkSeparator,
                    BulkBatchSize = opts.BulkBatchSize,
                    MetaSchemaName = opts.MetaSchema,
                    MetaTableName = opts.MetaTable,
                    CommandTimeout = opts.CommandTimeout,
                    Environment = opts.Environment,
                    TransactionMode = opts.TransactionMode,
                    AppliedByTool = "yuniql-cli",
                    AppliedByToolVersion = this.GetType().Assembly.GetName().Version.ToString(),
                };
                _configurationService.AssignDefaults(configuration);

                var migrationService = _migrationServiceFactory.Create(opts.Platform);
                migrationService.Initialize(opts.ConnectionString, opts.CommandTimeout);
                migrationService.Run(configuration);

                _traceService.Success($"Schema migration verification completed successfuly on {opts.Path}.");
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
                //if no target platform provided, we default into sqlserver
                if (string.IsNullOrEmpty(opts.Platform))
                {
                    opts.Platform = _environmentService.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_TARGET_PLATFORM);
                    if (string.IsNullOrEmpty(opts.Platform))
                    {
                        opts.Platform = SUPPORTED_DATABASES.SQLSERVER;
                    }
                }

                //if no connection string provided, we default into environment variable or throw exception
                if (string.IsNullOrEmpty(opts.ConnectionString))
                {
                    opts.ConnectionString = _environmentService.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_CONNECTION_STRING);
                }

                //get all exsiting db versions
                var migrationService = _migrationServiceFactory.Create(opts.Platform);
                migrationService.Initialize(opts.ConnectionString, opts.CommandTimeout);
                var versions = migrationService.GetAllVersions(opts.MetaSchema, opts.MetaTable);

                var versionPrettyPrint = new TablePrinter("SchemaVersion", "AppliedOnUtc", "Status", "AppliedByUser", "AppliedByTool");
                versions.ForEach(v => versionPrettyPrint.AddRow(v.Version, v.AppliedOnUtc.ToString("u"), v.Status, v.AppliedByUser, $"{v.AppliedByTool} {v.AppliedByToolVersion}"));
                versionPrettyPrint.Print();

                _traceService.Success($"Listed all schema versions applied to database on {opts.Path} workspace.{Environment.NewLine}" +
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
                //if no path provided, we default into current directory
                if (string.IsNullOrEmpty(opts.Path))
                {
                    opts.Path = _environmentService.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_WORKSPACE);
                    if (string.IsNullOrEmpty(opts.Path))
                    {
                        opts.Path = _environmentService.GetCurrentDirectory();
                    }
                }

                //if no target platform provided, we default into sqlserver
                if (string.IsNullOrEmpty(opts.Platform))
                {
                    opts.Platform = _environmentService.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_TARGET_PLATFORM);
                    if (string.IsNullOrEmpty(opts.Platform))
                    {
                        opts.Platform = SUPPORTED_DATABASES.SQLSERVER;
                    }
                }

                //if no connection string provided, we default into environment variable or throw exception
                if (string.IsNullOrEmpty(opts.ConnectionString))
                {
                    opts.ConnectionString = _environmentService.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_CONNECTION_STRING);
                }

                //parse tokens
                var tokens = opts.Tokens.Select(t => new KeyValuePair<string, string>(t.Split("=")[0], t.Split("=")[1])).ToList();

                //run all erase scripts
                var migrationService = _migrationServiceFactory.Create(opts.Platform);
                migrationService.Initialize(opts.ConnectionString, opts.CommandTimeout);
                migrationService.Erase(opts.Path, tokens, opts.CommandTimeout, opts.Environment);

                _traceService.Success($"Schema erase completed successfuly on {opts.Path}.");
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
                string platforms = @"
                                    SqlServer: 
                                       Supported versions:
                                       Usage: yuniql run -c <your-connection-string> --platform sqlserver
                                       Samples: https://github.com/rdagumampan/yuniql/tree/master/samples/basic-sqlserver-sample

                                    PostgreSql: 
                                       Supported versions:
                                       Usage: yuniql run -c <your-connection-string> --platform postgresql
                                       Samples: https://github.com/rdagumampan/yuniql/tree/master/samples/basic-postgresql-sample

                                    MySql: 
                                       Supported versions:
                                       Usage: yuniql run -c <your-connection-string> --platform mysql
                                       Samples: https://github.com/rdagumampan/yuniql/tree/master/samples/basic-mysql-sample

                                    MariaDB: 
                                       Supported versions:
                                       Usage: yuniql run -c <your-connection-string> --platform mariadb
                                       Samples: https://yuniql.io/docs/get-started/";

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
