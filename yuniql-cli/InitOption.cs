using CommandLine;
using System.Collections;

namespace Yuniql.CLI
{
    //yuniql init
    [Verb("init", HelpText = "Initialize migration structure to target folder")]
    public class InitOption
    {
        //yuniql init -p c:\temp\demo | --path c:\temp\demo
        [Option('p', "path", Required = false, HelpText = "Path to initialize")]
        public string Path { get; set; }

        //yuniql init -d | --debug
        [Option('d', "debug", Required = false, HelpText = "Print debug information including all raw scripts")]
        public bool Debug { get; set; }
    }
}
