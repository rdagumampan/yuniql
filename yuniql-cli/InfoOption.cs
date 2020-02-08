using CommandLine;

namespace Yuniql.CLI
{
    //yuniql info
    [Verb("info", HelpText = "Shows all the migrations applied to target database.")]
    public class InfoOption : BasePlatformOption
    {
    }
}
