using CommandLine;

namespace Yuniql.CLI
{
    public class BaseConfigOption : BasePlatformOption
    {
        //yuniql <command> -o json
        [Option('o', "data-type", Required = false, HelpText = "The choosen Output format")]
        public string DataType { get; set; }

        //yuniql <command> -a true | --auto-create-db true
        [Option('a', "auto-create-db", Required = false, HelpText = "Create database automatically.")]
        public bool? IsAutoCreateDatabase { get; set; } 
    }
}


