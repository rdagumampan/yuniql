using CommandLine;

namespace Yuniql.CLI
{
    public class BaseConfigOption : BasePlatformOption
    {
            //yuniql <command> -o "<dataT-type>"
            [Option('c', "data-type", Required = false, HelpText = "The choosen Output format")]
            public string DataType { get; set; }
    }
}


