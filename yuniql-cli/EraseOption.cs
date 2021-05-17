using CommandLine;
using System.Collections.Generic;

namespace Yuniql.CLI
{
    //yuniql erase
    [Verb("erase", HelpText = "Discovers all scripts in the _erase directory and executes them in single transaction.")]
    public class EraseOption : BasePlatformOption
    {
        //yuniql erase -k "Token1=TokenValue1" -k "Token2=TokenValue2" -k "Token3=TokenValue3" | --token "..." --token "..." --token "..."
        //yuniql erase -k "Token1=TokenValue1,Token2=TokenValue2,Token3=TokenValue3" | --token "...,...,..."
        [Option('k', "token", Required = false, HelpText = "Replace tokens using the passed key-value pairs.", Separator = ',')]
        public IEnumerable<string> Tokens { get; set; } = new List<string>();

        //yuniql erase --force 
        [Option('f', "force", Required = true, HelpText = "Force execution of erase commands.")]
        public bool Force { get; set; }

        //yuniql <command> --environment "DEV" | --environment "PROD"
        [Option("environment", Required = false, HelpText = "Environment code for environment-aware scripts.")]
        public string Environment { get; set; }

        //yuniql <command> --meta-schema "yuniql" 
        [Option("meta-schema", Required = false, HelpText = "Schema name for schema versions table.")]
        public string MetaSchemaName { get; set; }

        //yuniql <command> --meta-table "__yuniqlschemaversions" 
        [Option("meta-table", Required = false, HelpText = "Table name for schema versions table.")]
        public string MetaTableName { get; set; }
    }

    //yuniql erase
    [Verb("destroy", HelpText = "Drops previously deployed database.")]
    public class DestroyOption : BasePlatformOption
    {
        //yuniql erase --force 
        [Option('f', "force", Required = true, HelpText = "Force execution of erase commands.", Default = false)]
        public bool Force { get; set; }
    }
}
