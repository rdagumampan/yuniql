using CommandLine;

namespace Yuniql.CLI
{
    //yuniql vnext
    [Verb("vnext", HelpText = "Increment to next version")]
    public class NextVersionOption
    {
        //yuniql vnext -p c:\temp\demo | --path c:\temp\demo
        [Option('p', "path", Required = false, HelpText = "Path to increment version from")]
        public string Path { get; set; }

        //yuniql vnext -M | --major
        [Option('M', "major", Required = false, HelpText = "Increment major version")]
        public bool IncrementMajorVersion { get; set; }

        //yuniql vnext -m | --minor
        [Option('m', "minor", Required = false, HelpText = "Increment minor version")]
        public bool IncrementMinorVersion { get; set; }

        //yuniql vnext -f "Table1.sql"
        [Option('f', "file", Required = false, HelpText = "Increment version and create empty .sql file")]
        public string File { get; set; }

        //yuniql vnext -d | --debug
        [Option('d', "debug", Required = false, HelpText = "Print debug information including all raw scripts")]
        public bool Debug { get; set; }
    }
}
