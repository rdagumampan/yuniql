using CommandLine;

namespace Yuniql.CLI
{
    //yuniql baseline
    [Verb("baseline", HelpText = "Scripts selected database objects to form your v0.00 schema")]
    public class BaselineOption : BasePlatformOption
    {
        ////yuniql baseline -c "<connectiong-string>"
        //[Option('c', "connection-string", Required = true, HelpText = "Connection string to target sql server instance")]
        //public string ConnectionString { get; set; }

        ////yuniql baseline -d | --debug
        //[Option('d', "debug", Required = false, HelpText = "Print debug information including all raw scripts")]
        //public bool Debug { get; set; }
    }
}
