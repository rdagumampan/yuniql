using CommandLine;

namespace Yuniql.CLI
{
    //yuniql info
    [Verb("list", HelpText = "Shows all the migrations applied to target database.")]
    public class ListOption : BasePlatformOption
    {
        //yuniql <command> --meta-schema "yuniql" 
        [Option("meta-schema", Required = false, HelpText = "Schema name for schema versions table.")]
        public string MetaSchema { get; set; }

        //yuniql <command> --table "__yuniqlschemaversions" 
        [Option("meta-table", Required = false, HelpText = "Table name for schema versions table.")]
        public string MetaTable { get; set; }
    }
}
