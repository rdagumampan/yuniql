using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CommandLine;

namespace ArdiLabs.Yuniql
{
    public class Program
    {
        public static void Main(string[] args)
        {
            TraceService.Debug("Assembly.GetExecutingAssembly().Location: " + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            TraceService.Debug("AppContext.BaseDirectory: " + AppContext.BaseDirectory);
            TraceService.Debug("AppDomain.CurrentDomain.BaseDirectory: " + AppDomain.CurrentDomain.BaseDirectory);
            TraceService.Debug("Environment.CurrentDirectory: " + Environment.CurrentDirectory);
            TraceService.Debug("Directory.GetCurrentDirectory: " + Directory.GetCurrentDirectory());
            TraceService.Debug("Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().Location).LocalPath);: " + Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().Location).LocalPath));
            TraceService.Debug("Path.GetDirectoryName(Assembly.GetEntryAssembly().Location): " + Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

            CommandLine.Parser.Default.ParseArguments<InitOption, RunOption, NextVersionOption, InfoOption>(args)
              .MapResult(
                (InitOption opts) =>
                {
                    TraceSettings.Instance.IsDebugEnabled = opts.Debug;
                    return RunInitOption(opts);
                },
                (RunOption opts) =>
                {
                    TraceSettings.Instance.IsDebugEnabled = opts.Debug;
                    return RunMigration(opts);
                },
                (NextVersionOption opts) =>
                {
                    TraceSettings.Instance.IsDebugEnabled = opts.Debug;
                    return IncrementVersion(opts);
                },
                (InfoOption opts) =>
                {
                    TraceSettings.Instance.IsDebugEnabled = opts.Debug;
                    return RunInfoOption(opts);
                },
                (BaselineOption opts) =>
                {
                    TraceSettings.Instance.IsDebugEnabled = opts.Debug;
                    return RunBaselineOption(opts);
                },
                (RebaseOption opts) =>
                {
                    TraceSettings.Instance.IsDebugEnabled = opts.Debug;
                    return RunRebaseOption(opts);
                },
                errs => 1);
        }

        private static object RunInitOption(InitOption opts)
        {
            try
            {
                var versionService = new LocalVersionService();

                if (string.IsNullOrEmpty(opts.Path))
                {
                    var workingPath = Environment.CurrentDirectory;
                    versionService.Init(workingPath);
                    TraceService.Info($"Initialized {workingPath}.");
                }
                else
                {
                    versionService.Init(opts.Path);
                    TraceService.Info($"Initialized {opts.Path}.");
                }
            }
            catch (Exception ex)
            {
                TraceService.Error($"Failed to execute init function. {Environment.NewLine}{ex.ToString()}");
                throw;
            }

            return 0;
        }

        private static object IncrementVersion(NextVersionOption opts)
        {
            try
            {
                if (string.IsNullOrEmpty(opts.Path))
                {
                    var workingPath = Environment.CurrentDirectory;
                    opts.Path = workingPath;
                }

                var versionService = new LocalVersionService();
                if (opts.IncrementMajorVersion)
                {
                    var nextVersion = versionService.IncrementMajorVersion(opts.Path, opts.File);
                    TraceService.Info($"New major version created {nextVersion} on {opts.Path}.");
                }
                else if (opts.IncrementMinorVersion || (!opts.IncrementMajorVersion && !opts.IncrementMinorVersion))
                {
                    var nextVersion = versionService.IncrementMinorVersion(opts.Path, opts.File);
                    TraceService.Info($"New minor version created {nextVersion} on {opts.Path}.");
                }
            }
            catch (Exception ex)
            {
                TraceService.Error($"Failed to execute vnext function. {Environment.NewLine}{ex.ToString()}");
                throw;
            }

            return 0;
        }

        private static object RunMigration(RunOption opts)
        {
            try
            {
                if (string.IsNullOrEmpty(opts.Path))
                {
                    var workingPath = Environment.CurrentDirectory;
                    opts.Path = workingPath;
                }

                TraceService.Info($"Started migration from {opts.Path}.");

                //if no target version specified, capture the latest from local folder structure
                if (string.IsNullOrEmpty(opts.TargetVersion))
                {
                    var localVersionService = new LocalVersionService();
                    opts.TargetVersion = localVersionService.GetLatestVersion(opts.Path);
                    TraceService.Info($"No explicit target version requested. We'll use latest available locally {opts.TargetVersion} on {opts.Path}.");
                }

                //if no connection string passed, use environment variable or throw exception
                if (string.IsNullOrEmpty(opts.ConnectionString))
                {
                    var environmentService = new EnvironmentService();
                    opts.ConnectionString = environmentService.GetEnvironmentVariable("YUNIQL_CONNECTION_STRING");
                }

                //parse tokens
                var tokens = opts.Tokens.Select(t => new KeyValuePair<string, string>(t.Split("=")[0], t.Split("=")[1])).ToList();

                //run the migration
                var migrationService = new MigrationService(opts.ConnectionString);
                migrationService.Run(opts.Path, opts.TargetVersion, opts.AutoCreateDatabase, tokens);
            }
            catch (Exception ex)
            {
                TraceService.Error($"Failed to execute run function. Target database will be rolled back to its previous state. {Environment.NewLine}{ex.ToString()}");
                throw;
            }

            return 0;
        }

        private static object RunInfoOption(InfoOption opts)
        {
            try
            {
                //if no connection string passed, use environment variable or throw exception
                if (string.IsNullOrEmpty(opts.ConnectionString))
                {
                    var environmentService = new EnvironmentService();
                    opts.ConnectionString = environmentService.GetEnvironmentVariable("YUNIQL_CONNECTION_STRING");
                }

                var versions = new List<DbVersion>();
                var migrationService = new MigrationService(opts.ConnectionString);
                versions = migrationService.GetAllDbVersions();

                var results = new StringBuilder();
                results.AppendLine($"Version\t\tCreated\t\t\t\tCreatedBy");
                versions.ForEach(v =>
                {
                    results.AppendLine($"{v.Version}\t\t{v.DateInsertedUtc.ToString("o")}\t{v.LastUserId}");
                });

                Console.WriteLine(results.ToString());
            }
            catch (Exception ex)
            {
                TraceService.Error($"Failed to execute info function. {Environment.NewLine}{ex.ToString()}");
                throw;
            }

            return 0;
        }

        private static object RunBaselineOption(BaselineOption opts)
        {
            throw new NotImplementedException("Not yet implemented, stay tune!");
        }

        private static object RunRebaseOption(RebaseOption opts)
        {
            throw new NotImplementedException("Not yet implemented, stay tune!");
        }
    }

    public interface IEnvironmentService
    {
        string GetEnvironmentVariable(string name);
    }

    public class EnvironmentService: IEnvironmentService
    {
        public string GetEnvironmentVariable(string name)
        {
            string result = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrEmpty(result) && Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                result = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User);
                if (string.IsNullOrEmpty(result))
                    result = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine);
            }

            return result;
        }
    }
}
