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
        //yuniql init
        //yuniql init -p c:\temp\demo | --path c:\temp\demo
        //yuniql vnext
        //yuniql vnext -p c:\temp\demo | --path c:\temp\demo
        //yuniql vnext -M | --major
        //yuniql vnext -m | --minor
        //yuniql vnext -f "Table1.sql"
        //yuniql run
        //yuniql run -a true | --auto-create-db true
        //yuniql run -p c:\temp\demo | --path c:\temp\demo
        //yuniql run -t v1.05 | --target-version v1.05
        //yuniql run -c "<connectiong-string>"
        //yuniql info -c "<connectiong-string>" 
        //yuniql -v | --version
        //yuniql -h | --help
        //yuniql -d | --debug

        static void Main(string[] args)
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
                (NextVersionOption opts) => {
                    TraceSettings.Instance.IsDebugEnabled = opts.Debug;
                    return IncrementVersion(opts);
                },
                (InfoOption opts) => {
                    TraceSettings.Instance.IsDebugEnabled = opts.Debug;
                    return RunInfoOption(opts);
                },
                (BaselineOption opts) => {
                    TraceSettings.Instance.IsDebugEnabled = opts.Debug;
                    return RunBaselineOption(opts);
                },
                (RebaseOption opts) => {
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
                    TraceService.Info($"Initialized {opts.Path}.");
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

                //parse tokens
                var tokens = opts.Tokens.Select(t => new KeyValuePair<string, string>(t.Split("=")[0], t.Split("=")[1])).ToList();

                var migrationService = new MigrationService();
                migrationService.Run(opts.Path, opts.ConnectionString, opts.TargetVersion, opts.AutoCreateDatabase, tokens);
                TraceService.Info($"Completed migration from {opts.Path}.");
            }
            catch (Exception ex)
            {
                TraceService.Error($"Failed to execute run function. {Environment.NewLine}{ex.ToString()}");
                throw;
            }

            return 0;
        }

        private static object RunInfoOption(InfoOption opts)
        {
            try
            {
                var migrationService = new MigrationService();
                var versions = migrationService.GetAllDbVersions(new SqlConnectionStringBuilder(opts.ConnectionString));
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
}
