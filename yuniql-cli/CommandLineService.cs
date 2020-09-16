using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public CommandLineService(IMigrationServiceFactory migrationServiceFactory,
                                  ILocalVersionService localVersionService,
                                  IEnvironmentService environmentService,
                                  ITraceService traceService)
        {
            this._localVersionService = localVersionService;
            this._environmentService = environmentService;
            this._traceService = traceService;
            this._migrationServiceFactory = migrationServiceFactory;
        }

        public int RunInitOption(InitOption opts)
        {
            try
            {
                //if no path provided, we default into current directory
                if(string.IsNullOrEmpty(opts.Path))
                {
                    var workingPath = _environmentService.GetCurrentDirectory();
                    _localVersionService.Init(workingPath);
                    _traceService.Info($"Initialized {workingPath}.");
                } else
                {
                    _localVersionService.Init(opts.Path);
                    _traceService.Success($"Initialized {opts.Path}.");
                }

                return 0;
            } catch(Exception ex)
            {
                return OnException(ex, "Failed to execute init function", opts.Debug, _traceService);
            }
        }

        public int IncrementVersion(NextVersionOption opts)
        {
            try
            {
                //if no path provided, we default into current directory
                if(string.IsNullOrEmpty(opts.Path))
                {
                    var workingPath = _environmentService.GetCurrentDirectory();
                    opts.Path = workingPath;
                }

                if(opts.IncrementMajorVersion)
                {
                    var nextVersion = _localVersionService.IncrementMajorVersion(opts.Path, opts.File);
                    _traceService.Success($"New major version created {nextVersion} on {opts.Path}.");
                } else if(opts.IncrementMinorVersion || (!opts.IncrementMajorVersion && !opts.IncrementMinorVersion))
                {
                    var nextVersion = _localVersionService.IncrementMinorVersion(opts.Path, opts.File);
                    _traceService.Success($"New minor version created {nextVersion} on {opts.Path}.");
                }

                return 0;
            } catch(Exception ex)
            {
                return OnException(ex, "Failed to execute vnext function", opts.Debug, _traceService);
            }
        }

        public int RunMigration(RunOption opts)
        {
            try
            {
                //if no path provided, we default into current directory
                if(string.IsNullOrEmpty(opts.Path))
                {
                    var workingPath = _environmentService.GetCurrentDirectory();
                    opts.Path = workingPath;
                }
                _traceService.Info($"Started migration from {opts.Path}.");

                //if no target platform provided, we default into sqlserver
                if(string.IsNullOrEmpty(opts.Platform))
                {
                    opts.Platform = _environmentService.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_TARGET_PLATFORM);
                    if(string.IsNullOrEmpty(opts.Platform))
                    {
                        opts.Platform = SUPPORTED_DATABASES.SQLSERVER;
                    }
                }

                //if no connection string provided, we default into environment variable or throw exception
                if(string.IsNullOrEmpty(opts.ConnectionString))
                {
                    opts.ConnectionString = _environmentService.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_CONNECTION_STRING);
                }

                //if no target version specified, we capture the latest from local folder structure
                if(string.IsNullOrEmpty(opts.TargetVersion))
                {
                    opts.TargetVersion = _localVersionService.GetLatestVersion(opts.Path);
                    _traceService.Info($"No explicit target version requested. We'll use latest available locally {opts.TargetVersion} on {opts.Path}.");
                }

                //parse tokens
                var tokens = opts.Tokens
                    .Select(t => new KeyValuePair<string, string>(t.Split("=")[0], t.Split("=")[1]))
                    .ToList();

                //run the migration
                var toolName = "yuniql-cli";
                var toolVersion = this.GetType().Assembly.GetName().Version.ToString();

                var migrationService = _migrationServiceFactory.Create(opts.Platform);
                migrationService.Initialize(opts.ConnectionString, opts.CommandTimeout);
                migrationService.Run(opts.Path,
                                     opts.TargetVersion,
                                     autoCreateDatabase: opts.AutoCreateDatabase,
                                     tokens: tokens,
                                     verifyOnly: false,
                                     bulkSeparator: opts.BulkSeparator,
                                     metaSchemaName: opts.MetaSchema,
                                     metaTableName: opts.MetaTable,
                                     commandTimeout: opts.CommandTimeout,
                                     bulkBatchSize: opts.BulkBatchSize,
                                     appliedByTool: toolName,
                                     appliedByToolVersion: toolVersion,
                                     environmentCode: opts.Environment,
                                     opts.ContinueAfterFailure
                    ? NonTransactionalResolvingOption.ContinueAfterFailure
                    : (NonTransactionalResolvingOption?)null,
                                     opts.NoTransaction);

                _traceService.Success($"Schema migration completed successfuly on {opts.Path}.");
                return 0;
            } catch(Exception ex)
            {
                return OnException(ex, "Failed to execute run function", opts.Debug, _traceService);
            }
        }

        public int RunVerify(VerifyOption opts)
        {
            try
            {
                //if no path provided, we default into current directory
                if(string.IsNullOrEmpty(opts.Path))
                {
                    var workingPath = _environmentService.GetCurrentDirectory();
                    opts.Path = workingPath;
                }
                _traceService.Info($"Started verifcation from {opts.Path}.");

                //if no target platform provided, we default into sqlserver
                if(string.IsNullOrEmpty(opts.Platform))
                {
                    opts.Platform = _environmentService.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_TARGET_PLATFORM);
                    if(string.IsNullOrEmpty(opts.Platform))
                    {
                        opts.Platform = SUPPORTED_DATABASES.SQLSERVER;
                    }
                }

                //if no connection string provided, we default into environment variable or throw exception
                if(string.IsNullOrEmpty(opts.ConnectionString))
                {
                    opts.ConnectionString = _environmentService.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_CONNECTION_STRING);
                }

                //if no target version specified, we capture the latest from local folder structure
                if(string.IsNullOrEmpty(opts.TargetVersion))
                {
                    opts.TargetVersion = _localVersionService.GetLatestVersion(opts.Path);
                    _traceService.Info($"No explicit target version requested. We'll use latest available locally {opts.TargetVersion} on {opts.Path}.");
                }

                //parse tokens
                var tokens = opts.Tokens
                    .Select(t => new KeyValuePair<string, string>(t.Split("=")[0], t.Split("=")[1]))
                    .ToList();

                //run the migration
                var toolName = "yuniql-cli";
                var toolVersion = this.GetType().Assembly.GetName().Version.ToString();

                //run the migration
                var migrationService = _migrationServiceFactory.Create(opts.Platform);
                migrationService.Initialize(opts.ConnectionString, opts.CommandTimeout);
                migrationService.Run(opts.Path,
                                     opts.TargetVersion,
                                     autoCreateDatabase: false,
                                     tokens: tokens,
                                     verifyOnly: true,
                                     bulkSeparator: opts.BulkSeparator,
                                     metaSchemaName: opts.MetaSchema,
                                     metaTableName: opts.MetaTable,
                                     commandTimeout: opts.CommandTimeout,
                                     bulkBatchSize: opts.BulkBatchSize,
                                     appliedByTool: toolName,
                                     appliedByToolVersion: toolVersion,
                                     environmentCode: opts.Environment,
                                     null);

                _traceService.Success($"Schema migration verification completed successfuly on {opts.Path}.");
                return 0;
            } catch(Exception ex)
            {
                return OnException(ex,
                                   "Failed to execute verification function. Target database will be rolled back to its previous state",
                                   opts.Debug,
                                   _traceService);
            }
        }

        public int RunListOption(ListOption opts)
        {
            try
            {
                //if no target platform provided, we default into sqlserver
                if(string.IsNullOrEmpty(opts.Platform))
                {
                    opts.Platform = _environmentService.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_TARGET_PLATFORM);
                    if(string.IsNullOrEmpty(opts.Platform))
                    {
                        opts.Platform = SUPPORTED_DATABASES.SQLSERVER;
                    }
                }

                //if no connection string provided, we default into environment variable or throw exception
                if(string.IsNullOrEmpty(opts.ConnectionString))
                {
                    opts.ConnectionString = _environmentService.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_CONNECTION_STRING);
                }

                //get all exsiting db versions
                var migrationService = _migrationServiceFactory.Create(opts.Platform);
                migrationService.Initialize(opts.ConnectionString, opts.CommandTimeout);
                var versions = migrationService.GetAllVersions(opts.MetaSchema, opts.MetaTable);

                var versionPrettyPrint = new TablePrinter("SchemaVersion",
                                                          "AppliedOnUtc",
                                                          "Status",
                                                          "AppliedByUser",
                                                          "AppliedByTool");
                versions.ForEach(v => versionPrettyPrint.AddRow(v.Version,
                                                                v.AppliedOnUtc.ToString("u"),
                                                                v.Status,
                                                                v.AppliedByUser,
                                                                $"{v.AppliedByTool} {v.AppliedByToolVersion}"));
                versionPrettyPrint.Print();

                _traceService.Success($"Listed all schema versions applied to database on {opts.Path} workspace.{Environment.NewLine}" +
                    $"For platforms not supporting full transactional DDL operations (ex. MySql, CockroachDB, Snowflake), unsuccessful migrations will show the status as Failed and you can look for LastFailedScript and LastScriptError in the schema version tracking table.");

                return 0;
            } catch(Exception ex)
            {
                return OnException(ex, "Failed to execute info function", opts.Debug, _traceService);
            }
        }

        public int RunEraseOption(EraseOption opts)
        {
            try
            {
                //if no path provided, we default into current directory
                if(string.IsNullOrEmpty(opts.Path))
                {
                    var workingPath = _environmentService.GetCurrentDirectory();
                    opts.Path = workingPath;
                }

                //if no connection string provided, we default into environment variable or throw exception
                if(string.IsNullOrEmpty(opts.ConnectionString))
                {
                    opts.ConnectionString = _environmentService.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_CONNECTION_STRING);
                }

                //if no target platform provided, we default into sqlserver
                if(string.IsNullOrEmpty(opts.Platform))
                {
                    opts.Platform = _environmentService.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_TARGET_PLATFORM);
                    if(string.IsNullOrEmpty(opts.Platform))
                    {
                        opts.Platform = SUPPORTED_DATABASES.SQLSERVER;
                    }
                }

                //parse tokens
                var tokens = opts.Tokens
                    .Select(t => new KeyValuePair<string, string>(t.Split("=")[0], t.Split("=")[1]))
                    .ToList();

                //run all erase scripts
                var migrationService = _migrationServiceFactory.Create(opts.Platform);
                migrationService.Initialize(opts.ConnectionString, opts.CommandTimeout);
                migrationService.Erase(opts.Path, tokens, opts.CommandTimeout, opts.Environment);

                _traceService.Success($"Schema erase completed successfuly on {opts.Path}.");
                return 0;
            } catch(Exception ex)
            {
                return OnException(ex, "Failed to execute erase function", opts.Debug, _traceService);
            }
        }

        public int RunBaselineOption(BaselineOption opts)
        {
            try
            {
                throw new NotImplementedException("Not yet implemented, stay tune!");
            } catch(Exception ex)
            {
                return OnException(ex, "Failed to execute baseline function", opts.Debug, _traceService);
            }
        }

        public int RunRebaseOption(RebaseOption opts)
        {
            try
            {
                throw new NotImplementedException("Not yet implemented, stay tune!");
            } catch(Exception ex)
            {
                return OnException(ex, "Failed to execute rebase function", opts.Debug, _traceService);
            }
        }

        public int RunArchiveOption(ArchiveOption opts)
        {
            try
            {
                throw new NotImplementedException("Not yet implemented, stay tune!");
            } catch(Exception ex)
            {
                return OnException(ex, "Failed to archive function", opts.Debug, _traceService);
            }
        }

        private int OnException(Exception exception, string headerMessage, bool debug, ITraceService traceService)
        {
            var userMessage = debug ? exception.ToString() : $"{exception.Message} {exception.InnerException?.Message}";

            if(userMessage.Equals("Format of the initialization string does not conform to specification starting at index 0. "))
            {
                userMessage = "Missing parameter or incorrect format: connectionString. When running from CLI, you need to pass -c {your-connection-string} or --connection-string {your-connection-string}. If you need help formatting the connection string, visit https://www.connectionstrings.com.";
            }

            traceService.Error($"{headerMessage}. Arrg... something seems broken.{Environment.NewLine}" +
                $"Internal error message: {userMessage}.{Environment.NewLine}" +
                $"If you think this is a bug, please report an issue here https://github.com/rdagumampan/yuniql/issues.");
            return 1;
        }
    }
}
