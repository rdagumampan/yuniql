using System.Collections.Generic;
using CommandLine;

namespace Yuniql.CLI
{
    // yuniql archive
    [Verb ("archive", HelpText = "Flatten out all of the existing branches and move into v0.00 again. [Future development]")]
    public class ArchiveOption : BasePlatformOption
    {
        [Option('k', "token", Required = false, HelpText = "Replace tokens using the passed key-value pairs.", Separator = ',')]
        public IEnumerable<string> Tokens { get; set; } = new List<string>();
        
        [Option("schema", Required = false, HelpText = "Schema name for schema versions table.")]
        public string Schema { get; set; }

        //yuniql <command> --table "__yuniqlschemaversions" 
        [Option("table", Required = false, HelpText = "Table name for schema versions table.")]
        public string Table { get; set; }
    }
}