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

        public CommandLineService(
            IMigrationServiceFactory migrationServiceFactory,
            ILocalVersionService localVersionService,
            IEnvironmentService environmentService,
            ITraceService traceService)
        {
            this._localVersionService = localVersionService;
            this._environmentService = environmentService;
            this._traceService = traceService;
            this._migrationServiceFactory = migrationServiceFactory;
        }

        public object RunInitOption(InitOption opts)
        {
            try
            {
                //if no path provided, we default into current directory
                if (string.IsNullOrEmpty(opts.Path))
                {
                    var workingPath = _environmentService.GetCurrentDirectory();
                    _localVersionService.Init(workingPath);
                    _traceService.Info($"Initialized {workingPath}.");
                }
                else
                {
                    _localVersionService.Init(opts.Path);
                    _traceService.Info($"Initialized {opts.Path}.");
                }
            }
            catch (Exception ex)
            {
                _traceService.Error($"Failed to execute init function. {Environment.NewLine}{ex.ToString()}");
                throw;
            }

            return 0;
        }

        public object IncrementVersion(NextVersionOption opts)
        {
            try
            {
                //if no path provided, we default into current directory
                if (string.IsNullOrEmpty(opts.Path))
                {
                    var workingPath = _environmentService.GetCurrentDirectory();
                    opts.Path = workingPath;
                }

                if (opts.IncrementMajorVersion)
                {
                    var nextVersion = _localVersionService.IncrementMajorVersion(opts.Path, opts.File);
                    _traceService.Info($"New major version created {nextVersion} on {opts.Path}.");
                }
                else if (opts.IncrementMinorVersion || (!opts.IncrementMajorVersion && !opts.IncrementMinorVersion))
                {
                    var nextVersion = _localVersionService.IncrementMinorVersion(opts.Path, opts.File);
                    _traceService.Info($"New minor version created {nextVersion} on {opts.Path}.");
                }
            }
            catch (Exception ex)
            {
                _traceService.Error($"Failed to execute vnext function. {Environment.NewLine}{ex.ToString()}");
                throw;
            }

            return 0;
        }

        public object RunMigration(RunOption opts)
        {
            try
            {
                //if no path provided, we default into current directory
                if (string.IsNullOrEmpty(opts.Path))
                {
                    var workingPath = _environmentService.GetCurrentDirectory();
                    opts.Path = workingPath;
                }
                _traceService.Info($"Started migration from {opts.Path}.");

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

                //if no target version specified, we capture the latest from local folder structure
                if (string.IsNullOrEmpty(opts.TargetVersion))
                {
                    opts.TargetVersion = _localVersionService.GetLatestVersion(opts.Path);
                    _traceService.Info($"No explicit target version requested. We'll use latest available locally {opts.TargetVersion} on {opts.Path}.");
                }

                //parse tokens
                var tokens = opts.Tokens.Select(t => new KeyValuePair<string, string>(t.Split("=")[0], t.Split("=")[1])).ToList();

                //run the migration
                var toolName = "yuniql-cli";
                var toolVersion = this.GetType().Assembly.GetName().Version.ToString();

                var migrationService = _migrationServiceFactory.Create(opts.Platform);
                migrationService.Initialize(opts.ConnectionString, opts.CommandTimeout);
                migrationService.Run(
                    opts.Path,
                    opts.TargetVersion,
                    autoCreateDatabase: opts.AutoCreateDatabase,
                    tokens: tokens,
                    verifyOnly: false,
                    delimiter: opts.Delimiter,
                    schemaName: opts.Schema,
                    tableName: opts.Table,
                    commandTimeout: opts.CommandTimeout,
                    batchSize: null,
                    appliedByTool: toolName,
                    appliedByToolVersion: toolVersion,
                    environmentCode: opts.Environment,
                    opts.ContinueAfterFailure ? NonTransactionalResolvingOption.ContinueAfterFailure : (NonTransactionalResolvingOption?) null
                    );
            }
            catch (Exception ex)
            {
                _traceService.Error($"Failed to execute run function. {Environment.NewLine}{ex.ToString()}");
                throw;
            }

            return 0;
        }

        public object RunVerify(VerifyOption opts)
        {
            try
            {
                //if no path provided, we default into current directory
                if (string.IsNullOrEmpty(opts.Path))
                {
                    var workingPath = _environmentService.GetCurrentDirectory();
                    opts.Path = workingPath;
                }
                _traceService.Info($"Started verifcation from {opts.Path}.");

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

                //if no target version specified, we capture the latest from local folder structure
                if (string.IsNullOrEmpty(opts.TargetVersion))
                {
                    opts.TargetVersion = _localVersionService.GetLatestVersion(opts.Path);
                    _traceService.Info($"No explicit target version requested. We'll use latest available locally {opts.TargetVersion} on {opts.Path}.");
                }

                //parse tokens
                var tokens = opts.Tokens.Select(t => new KeyValuePair<string, string>(t.Split("=")[0], t.Split("=")[1])).ToList();

                //run the migration
                var toolName = "yuniql-cli";
                var toolVersion = this.GetType().Assembly.GetName().Version.ToString();

                //run the migration
                var migrationService = _migrationServiceFactory.Create(opts.Platform);
                migrationService.Initialize(opts.ConnectionString, opts.CommandTimeout);
                migrationService.Run(
                    opts.Path,
                    opts.TargetVersion,
                    autoCreateDatabase: false,
                    tokens: tokens,
                    verifyOnly: true,
                    delimiter: opts.Delimiter,
                    schemaName: opts.Schema,
                    tableName: opts.Table,
                    commandTimeout: opts.CommandTimeout,
                    batchSize: null,
                    appliedByTool: toolName,
                    appliedByToolVersion: toolVersion,
                    environmentCode: opts.Environment,
                    null
                    );

                _traceService.Info("Verification run successful.");
            }
            catch (Exception ex)
            {
                _traceService.Error($"Failed to execute verification function. Target database will be rolled back to its previous state. {Environment.NewLine}{ex.ToString()}");
                throw;
            }

            return 0;
        }

        public object RunInfoOption(InfoOption opts)
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
                var versions = migrationService.GetAllVersions(opts.Schema, opts.Table);

                var results = new StringBuilder();
                results.AppendLine($"Version\t\tCreated\t\t\t\tCreatedBy");
                versions.ForEach(v =>
                {
                    results.AppendLine($"{v.Version}\t\t{v.AppliedOnUtc.ToString("o")}\t{v.AppliedByUser}");
                });

                Console.WriteLine(results.ToString());
            }
            catch (Exception ex)
            {
                _traceService.Error($"Failed to execute info function. {Environment.NewLine}{ex.ToString()}");
                throw;
            }

            return 0;
        }

        public object RunEraseOption(EraseOption opts)
        {
            try
            {
                //if no path provided, we default into current directory
                if (string.IsNullOrEmpty(opts.Path))
                {
                    var workingPath = _environmentService.GetCurrentDirectory();
                    opts.Path = workingPath;
                }

                //if no connection string provided, we default into environment variable or throw exception
                if (string.IsNullOrEmpty(opts.ConnectionString))
                {
                    opts.ConnectionString = _environmentService.GetEnvironmentVariable(ENVIRONMENT_VARIABLE.YUNIQL_CONNECTION_STRING);
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

                //parse tokens
                var tokens = opts.Tokens.Select(t => new KeyValuePair<string, string>(t.Split("=")[0], t.Split("=")[1])).ToList();

                //run all erase scripts
                var migrationService = _migrationServiceFactory.Create(opts.Platform);
                migrationService.Initialize(opts.ConnectionString, opts.CommandTimeout);
                migrationService.Erase(opts.Path, tokens, opts.CommandTimeout, opts.Environment);
            }
            catch (Exception ex)
            {
                _traceService.Error($"Failed to execute info function. {Environment.NewLine}{ex.ToString()}");
                throw;
            }

            return 0;
        }

        public object RunBaselineOption(BaselineOption opts)
        {
            throw new NotImplementedException("Not yet implemented, stay tune!");
        }

        public object RunRebaseOption(RebaseOption opts)
        {
            throw new NotImplementedException("Not yet implemented, stay tune!");
        }

        public object RunArchiveOption(ArchiveOption Opts) 
        {
            throw new NotImplementedException("Not yet implemented, stay tune!");
        }
    }
}
