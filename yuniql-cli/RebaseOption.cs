using CommandLine;

namespace Yuniql.CLI
{
    //yuniql baseline
    [Verb("rebase", HelpText = "Consolidateds all existing versions, archive them and create a new v0.00 baseline")]
    public class RebaseOption : BasePlatformOption
    {
        ////yuniql rebase -c "<connectiong-string>"
        //[Option('c', "connection-string", Required = false, HelpText = "Connection string to target sql server instance")]
        //public string ConnectionString { get; set; }

        ////yuniql rebase --command-timeout""
        //[Option("command-timeout", Required = false, HelpText = "The time in seconds to wait for the command to execute.", Default = "30")]
        //public string CommandTimeout { get; set; } = ",";

        ////yuniql rebase -d | --debug
        //[Option('d', "debug", Required = false, HelpText = "Print debug information including all raw scripts")]
        //public bool Debug { get; set; }
    }
}
