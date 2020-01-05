using CommandLine;
using System.Collections.Generic;

namespace Yuniql.CLI
{
    //yuniql run
    [Verb("run", HelpText = "Runs migration up to latest or target version")]
    public class RunOption : BaseRunPlatformOption
    {

    }
}
