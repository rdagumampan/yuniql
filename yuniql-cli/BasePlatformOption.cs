using CommandLine;

namespace Yuniql.CLI
{
    public class BasePlatformOption: BaseOption
    {
        ////yuniql <command> -p c:\temp\demo | --path c:\temp\demo
        //[Option('p', "path", Required = false, HelpText = "Path to run migration from")]
        //public string Path { get; set; }

        //yuniql <command> -d | --debug
        [Option(longName: "platform", Required = false, HelpText = "Target database platform", Default = "sqlserver")]
        public string Platform { get; set; }

        //yuniql <command> --plugins-path "," | --plugins-path "|"
        [Option("plugins-path", Required = false, HelpText = "The location of plugins. The default location is current location of the yuniql assemblies.")]
        public string PluginsPath { get; set; }

        //yuniql <command> -c "<connectiong-string>"
        [Option('c', "connection-string", Required = false, HelpText = "Connection string to target database server instance")]
        public string ConnectionString { get; set; }

        //yuniql <command> --command-timeout""
        [Option("command-timeout", Required = false, HelpText = "The time in seconds to wait for the command to execute.", Default = 30)]
        public int CommandTimeout { get; set; } = 30;

        ////yuniql <command> -d | --debug
        //[Option('d', "debug", Required = false, HelpText = "Print debug information including all raw scripts", Default = false)]
        //public bool Debug { get; set; }
    }

}
