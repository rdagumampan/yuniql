using CommandLine;

namespace ArdiLabs.Yuniql.CLI
{
    public class BaseOption
    {
        //yuniql verify -d | --debug
        [Option(longName: "platform", Required = false, HelpText = "Target database platform", Default = "sqlserver")]
        public string Platform { get; set; }
    }
}
