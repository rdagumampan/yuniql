using CommandLine;

namespace Yuniql.CLI
{
    public class BaseOption
    {
        //yuniql <command> -p c:\temp\demo | --path c:\temp\demo
        [Option('p', "path", Required = false, HelpText = "Path to initialize.")]
        public string Workspace { get; set; }

        //yuniql <command> -d | --debug
        [Option('d', "debug", Required = false, HelpText = "Print debug information including all raw scripts.")]
        public bool IsDebug { get; set; }

        //yuniql <command> --trace-sensitive-data
        [Option("trace-sensitive-data", Required = false, HelpText = "Include sensitive data in the log.", Default = false)]
        public bool TraceSensitiveData { get; set; } = false;

        //yuniql <command> --trace-directory
        [Option("trace-directory", Required = false, HelpText = "Define the directory where the log files will be created.")]
        public string TraceDirectory { get; set; }

        //yuniql <command> --trace-silent
        [Option("trace-silent", Required = false, HelpText = "Disable the creation of log files.", Default = false)]
        public bool IsTraceSilent { get; set; } = false;
    }

}
