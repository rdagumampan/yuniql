using CommandLine;

namespace Yuniql.CLI
{
    //yuniql info
    [Verb("info", HelpText = "Shows all the migrations applied to target database.")]
    public class InfoOption : BasePlatformOption
    {
        [Option("table", Required = false, HelpText = "Table name for schema versions table.")]
        public string Table { get; set; }

        [Option("schema", Required = false, HelpText = "Schema name for schema versions table.")]
        public string Schema { get; set; }

    }
}
