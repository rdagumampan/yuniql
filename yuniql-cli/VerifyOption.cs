using CommandLine;
using System.Collections.Generic;

namespace Yuniql.CLI
{
    //yuniql verify
    [Verb("verify", HelpText = "Runs migration like the run command but all changes will not be committed.")]
    public class VerifyOption : BaseRunPlatformOption
    {
    }
}
