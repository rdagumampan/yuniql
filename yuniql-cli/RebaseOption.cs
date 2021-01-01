using CommandLine;

namespace Yuniql.CLI
{
    //yuniql baseline
    [Verb("rebase", HelpText = "Consolidates all existing versions, archive them and create a new v0.00 baseline. [Future development]")]
    public class RebaseOption : BasePlatformOption
    {
    }
}
