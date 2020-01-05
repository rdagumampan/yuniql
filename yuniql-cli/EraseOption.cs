using CommandLine;
using System.Collections.Generic;

namespace Yuniql.CLI
{
    //yuniql erase
    [Verb("erase", HelpText = "Discover all scripts in the _erase directory and executes them in single transaction")]
    public class EraseOption : BasePlatformOption
    {
        //yuniql <command> -k "Token1=TokenValue1" -k "Token2=TokenValue2" -k "Token3=TokenValue3" | --token "..." --token "..." --token "..."
        //yuniql <command> -k "Token1=TokenValue1,Token2=TokenValue2,Token3=TokenValue3" | --token "...,...,..."
        [Option('k', "token", Required = false, HelpText = "Replace tokens using the passed key-value pairs", Separator = ',')]
        public IEnumerable<string> Tokens { get; set; } = new List<string>();
    }
}
