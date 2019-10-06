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
        //https://github.com/commandlineparser/commandline
        //https://github.com/dotnet/command-line-api

        //yunisql init
        [Verb("init", HelpText = "Initialize migration structure to target folder")]
        public class InitOption
        {
            //yunisql init -p c:\temp\demo | --path c:\temp\demo
            [Option('p', "path", Required = false, HelpText = "Path to initialize")]
            public string Path { get; set; }
        }

        //yunisql vnext
        [Verb("vnext", HelpText = "Increment to next version")]
        public class NextVersionOption
        {
            [Option('p', "path", Required = false, HelpText = "Path to increment version from")]
            public string Path { get; set; }

            //yunisql vnext -M | --major
            [Option('M', "major", Required = false, HelpText = "Increment major version")]
            public bool IncrementMajorVersion { get; set; }

            //yunisql vnext -m | --minor
            [Option('m', "minor", Required = false, HelpText = "Increment minor version")]
            public bool IncrementMinorVersion { get; set; }

            //yunisql vnext -f "Table1.sql"
            [Option('f', "file", Required = false, HelpText = "Increment version and create empty .sql file")]
            public string File { get; set; }
        }

        //yunisql run
        [Verb("run", HelpText = "Runs migration steps")]
        public class RunOption
        {
            //yunisql run -p c:\temp\demo | --path c:\temp\demo
            [Option('p', "path", Required = false, HelpText = "Path to run migration from")]
            public string Path { get; set; }

            //yunisql run -t v1.05 | --target-version v1.05
            [Option('t', "target-version", Required = false, HelpText = "Target version to migrate into and skipping versions greater")]
            public string TargetVersion { get; set; }

            //yunisql run -c "<connectiong-string>"
            [Option('c', "connection-string", Required = false, HelpText = "Connection string to target sql server instance", Default = "Data Source=.;Integrated Security=SSPI;Initial Catalog=YunisqlDemoDB")]
            public string ConnectionString { get; set; }

            [Option('a', "auto-create-db", Required = false, HelpText = "Create database automatically")]
            public bool AutoCreateDatabase { get; set; }
        }

        //yunisql -v | --version
        //yunisql -h | --help

        static void Main(string[] args)
        {
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
            }
            else
            {
                versionService.Init(opts.Path);
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
                versionService.IncrementMajorVersion(opts.Path, opts.File);
            } else if (opts.IncrementMinorVersion)
            {
                versionService.IncrementMinorVersion(opts.Path, opts.File);
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

            var migrationService = new MigrationService();
            migrationService.Run(opts.Path, opts.ConnectionString, opts.AutoCreateDatabase, opts.TargetVersion);

            return 0;
        }
    }
}
