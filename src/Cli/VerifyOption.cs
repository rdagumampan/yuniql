using CommandLine;
using System.Collections.Generic;

namespace ArdiLabs.Yuniql.CLI
{
    //yuniql verify
    [Verb("verify", HelpText = "Runs migration steps")]
    public class VerifyOption : BaseOption
    {
        //yuniql verify -p c:\temp\demo | --path c:\temp\demo
        [Option('p', "path", Required = false, HelpText = "Path to run migration from")]
        public string Path { get; set; }

        //yuniql verify -c "<connectiong-string>"
        [Option('c', "connection-string", Required = false, HelpText = "Connection string to target sql server instance")]
        public string ConnectionString { get; set; }

        //yuniql verify -d | --debug
        [Option('d', "debug", Required = false, HelpText = "Print debug information including all raw scripts")]
        public bool Debug { get; set; }

        //yuniql verify -t v1.05 | --target-version v1.05
        [Option('t', "target-version", Required = false, HelpText = "Target version to migrate into and skipping versions greater")]
        public string TargetVersion { get; set; }

        //yuniql verify -k "Token1=TokenValue1" -k "Token2=TokenValue2" -k "Token3=TokenValue3" | --token "..." --token "..." --token "..."
        //yuniql verify -k "Token1=TokenValue1,Token2=TokenValue2,Token3=TokenValue3" | --token "...,...,..."
        [Option('k', "token", Required = false, HelpText = "Replace tokens using the passed key-value pairs", Separator = ',')]
        public IEnumerable<string> Tokens { get; set; } = new List<string>();
    }
}
