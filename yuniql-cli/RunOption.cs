using CommandLine;
using System.Collections.Generic;
using System.ComponentModel;

namespace Yuniql.CLI
{
    //yuniql run
    [Verb("run", HelpText = "Runs migration up to latest available version or up to specific version.")]
    public class RunOption : BaseRunPlatformOption
    {
        //yuniql <command> --continue-after-failure
        [Option("continue-after-failure", Required = false, HelpText = "Skip failed script and continue with migration (Only for platforms which doesn't fully support transactions).", Default = false)]
        public bool ContinueAfterFailure { get; set; }
    }
}
