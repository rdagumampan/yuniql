using CommandLine;

namespace Yuniql.CLI
{
    //yuniql info
    [Verb("info", HelpText = "Shows the current version structure of target database")]
    public class InfoOption : BasePlatformOption
    {
    }
}
