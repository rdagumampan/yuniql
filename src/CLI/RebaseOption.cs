using CommandLine;

namespace ArdiLabs.Yuniql
{
    //yuniql baseline
    [Verb("rebase", HelpText = "Consolidateds all existing versions, archive them and create a new v0.00 baseline")]
    public class RebaseOption
    {
        //yuniql info -c "<connectiong-string>"
        [Option('c', "connection-string", Required = true, HelpText = "Connection string to target sql server instance")]
        public string ConnectionString { get; set; }

        //yuniql run -d | --debug
        [Option('d', "debug", Required = false, HelpText = "Print debug information including all raw scripts")]
        public bool Debug { get; set; }
    }
}
