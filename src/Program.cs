using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;

namespace ArdiLabs.Yuniql
{
    public class Program
    {

        //yunisql init
        //yunisql init -p c:\temp\demo | --path c:\temp\demo
        //yunisql vnext
        //yunisql vnext -p c:\temp\demo | --path c:\temp\demo
        //yunisql vnext -M | --major
        //yunisql vnext -m | --minor
        //yunisql vnext -f "Table1.sql"
        //yunisql run
        //yunisql run -a true | --auto-create-db true
        //yunisql run -p c:\temp\demo | --path c:\temp\demo
        //yunisql run -t v1.05 | --target-version v1.05
        //yunisql run -c "<connectiong-string>"
        //yunisql -v | --version
        //yunisql -h | --help

        static void Main(string[] args)
        {
            TraceService.Info("Assembly.GetExecutingAssembly().Location: " + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            TraceService.Info("AppContext.BaseDirectory: " + AppContext.BaseDirectory);
            TraceService.Info("AppDomain.CurrentDomain.BaseDirectory: "+ AppDomain.CurrentDomain.BaseDirectory);
            TraceService.Info("Environment.CurrentDirectory: " + Environment.CurrentDirectory);
            TraceService.Info("Directory.GetCurrentDirectory: " + Directory.GetCurrentDirectory());
            TraceService.Info("Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().Location).LocalPath);: "+ Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().Location).LocalPath));
            TraceService.Info("Path.GetDirectoryName(Assembly.GetEntryAssembly().Location): " + Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

            CommandLine.Parser.Default.ParseArguments<InitOption, RunOption, NextVersionOption>(args)
              .MapResult(
                (InitOption opts) => RunInitOption(opts),
                (RunOption opts) => RunMigration(opts),
                (NextVersionOption opts) => IncrementVersion(opts),
                errs => 1);
        }

        private static object RunInitOption(InitOption opts)
        {
            var versionService = new LocalVersionService();

            if (string.IsNullOrEmpty(opts.Path))
            {
                var workingPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                versionService.Init(workingPath);
                TraceService.Info($"Initialized {opts.Path}.");
            }
            else
            {
                versionService.Init(opts.Path);
                TraceService.Info($"Initialized {opts.Path}.");
            }

            return 0;
        }

        private static object IncrementVersion(NextVersionOption opts)
        {
            if (string.IsNullOrEmpty(opts.Path))
            {
                var workingPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
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

            return 0;
        }

        private static object RunMigration(RunOption opts)
        {
            if (string.IsNullOrEmpty(opts.Path))
            {
                var workingPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                opts.Path = workingPath;
            }

            TraceService.Info($"Started migration from {opts.Path}.");

            //if no target version specified, capture the latest from local folder structure
            if (string.IsNullOrEmpty(opts.TargetVersion))
            {
                var localVersionService = new LocalVersionService();
                opts.TargetVersion = localVersionService.GetLatestVersion(opts.Path);
                TraceService.Info($"No explicit target version requested. Will use latest available locally {opts.TargetVersion} on {opts.Path}.");
            }

            var migrationService = new MigrationService();
            migrationService.Run(opts.Path, opts.ConnectionString, opts.TargetVersion, opts.AutoCreateDatabase);
            TraceService.Info($"Completed migration from {opts.Path}.");

            return 0;
        }
    }
}
