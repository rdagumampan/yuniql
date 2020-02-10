using CommandLine;

namespace Yuniql.CLI
{
    //yuniql vnext
    [Verb("vnext", HelpText = "Increments to next version.")]
    public class NextVersionOption: BaseOption
    {
        //yuniql vnext -M | --major
        [Option('M', "major", Required = false, HelpText = "Increment major version.")]
        public bool IncrementMajorVersion { get; set; }

        //yuniql vnext -m | --minor
        [Option('m', "minor", Required = false, HelpText = "Increment minor version.")]
        public bool IncrementMinorVersion { get; set; }

        //yuniql vnext -f "Table1.sql"
        [Option('f', "file", Required = false, HelpText = "Increment version and create empty .sql file.")]
        public string File { get; set; }
    }
}
