using CommandLine;
using System;
using System.Collections.Generic;

namespace Yuniql.CLI
{
    //yuniql info
    [Verb("drop", HelpText = "Drop database.")]
    public class DropOption : BasePlatformOption
    {
        //yuniql erase -k "Token1=TokenValue1" -k "Token2=TokenValue2" -k "Token3=TokenValue3" | --token "..." --token "..." --token "..."
        //yuniql erase -k "Token1=TokenValue1,Token2=TokenValue2,Token3=TokenValue3" | --token "...,...,..."
        [Option('k', "token", Required = false, HelpText = "Replace tokens using the passed key-value pairs.", Separator = ',')]
        public IEnumerable<string> Tokens { get; set; } = new List<string>();

        //yuniql erase --force 
        [Option('f', "force", Required = true, HelpText = "Force execution of drop commands.")]
        public bool Force { get; set; }

        //yuniql <command> --environment "DEV" | --environment "PROD"
        [Option("environment", Required = false, HelpText = "Environment code for environment-aware scripts.")]
        public string Environment { get; set; }
    }
}
