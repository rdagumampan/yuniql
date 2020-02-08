using CommandLine;

namespace Yuniql.CLI
{
    //yuniql baseline
    [Verb("rebase", HelpText = "Consolidateds all existing versions, archive them and create a new v0.00 baseline. [Future development]")]
    public class RebaseOption : BasePlatformOption
    {
    }
}
