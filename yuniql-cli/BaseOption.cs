﻿using CommandLine;

namespace Yuniql.CLI
{
    public class BaseOption
    {
        //yuniql <command> -p c:\temp\demo | --path c:\temp\demo
        [Option('p', "path", Required = false, HelpText = "Path to initialize.")]
        public string Workspace { get; set; }

        //yuniql <command> -d | --debug
        [Option('d', "debug", Required = false, HelpText = "Print debug information including all raw scripts.")]
        public bool? IsDebug { get; set; } 

        //yuniql <command> --trace-sensitive-data
        [Option("trace-sensitive-data", Required = false, HelpText = "Include sensitive data like connection string in the log messages.", Default = false)]
        public bool IsTraceSensitiveData { get; set; } = false;

        //https://peter.sh/experiments/chromium-command-line-switches/
        //yuniql <command> --trace-to-file
        [Option("trace-to-file", Required = false, HelpText = "Trace logs are also written on file in addition to console output.", Default = true)]
        public bool IsTraceToFile { get; set; } = true;

        //yuniql <command> --trace-to-directory
        [Option("trace-to-directory", Required = false, HelpText = "Directory path where the log files will be created.")]
        public string TraceToDirectory { get; set; }

    }
}
