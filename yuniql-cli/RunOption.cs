using CommandLine;
using System.Collections.Generic;

namespace Yuniql.CLI
{
    //yuniql run
    [Verb("run", HelpText = "Runs migration steps")]
    public class RunOption : BaseOption
    {
        //yuniql run -p c:\temp\demo | --path c:\temp\demo
        [Option('p', "path", Required = false, HelpText = "Path to run migration from")]
        public string Path { get; set; }

        //yuniql run -t v1.05 | --target-version v1.05
        [Option('t', "target-version", Required = false, HelpText = "Target version to migrate into and skipping versions greater")]
        public string TargetVersion { get; set; }

        //yuniql run -c "<connectiong-string>"
        [Option('c', "connection-string", Required = false, HelpText = "Connection string to target sql server instance")]
        public string ConnectionString { get; set; }

        //yuniql run -a true | --auto-create-db true
        [Option('a', "auto-create-db", Required = false, HelpText = "Create database automatically")]
        public bool AutoCreateDatabase { get; set; }

        //yuniql run -d | --debug
        [Option('d', "debug", Required = false, HelpText = "Print debug information including all raw scripts")]
        public bool Debug { get; set; }

        //yuniql run -k "Token1=TokenValue1" -k "Token2=TokenValue2" -k "Token3=TokenValue3" | --token "..." --token "..." --token "..."
        //yuniql run -k "Token1=TokenValue1,Token2=TokenValue2,Token3=TokenValue3" | --token "...,...,..."
        [Option('k', "token", Required = false, HelpText = "Replace tokens using the passed key-value pairs", Separator = ',')]
        public IEnumerable<string> Tokens { get; set; } = new List<string>();

        //yuniql run --delimeter "," | --delimeter "|"
        [Option("delimiter", Required = false, HelpText = "Bulk import file delimiter", Default = ",")]
        public string Delimiter { get; set; } = ",";

        //yuniql run --plugins-path "," | --plugins-path "|"
        [Option("plugins-path", Required = false, HelpText = "The location of plugins. The default location is current location of the yuniql assemblies.")]
        public string PluginsPath { get; set; }
    }
}
