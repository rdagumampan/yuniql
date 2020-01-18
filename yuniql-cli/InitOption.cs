using CommandLine;
using System.Collections;

namespace Yuniql.CLI
{
    //yuniql init
    [Verb("init", HelpText = "Initialize migration structure to target folder")]
    public class InitOption: BaseOption
    {
    }
}
