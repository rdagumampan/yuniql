using CommandLine;
using System.Collections;

namespace Yuniql.CLI
{
    //yuniql init
    [Verb("init", HelpText = "Initializes target directory with required directory structure.")]
    public class InitOption: BaseOption
    {
    }
}
