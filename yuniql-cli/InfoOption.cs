using CommandLine;

namespace Yuniql.CLI
{
    //yuniql info
    [Verb("info", HelpText = "Shows the current version structure of target database")]
    public class InfoOption : BaseOption
    {
        //yuniql info -c "<connectiong-string>"
        [Option('c', "connection-string", Required = false, HelpText = "Connection string to target sql server instance")]
        public string ConnectionString { get; set; }

        //yuniql run -d | --debug
        [Option('d', "debug", Required = false, HelpText = "Print debug information including all raw scripts")]
        public bool Debug { get; set; }
    }
}
