using CommandLine;
using System.Collections.Generic;

namespace Yuniql.CLI
{
    //yuniql verify
    [Verb("verify", HelpText = "Runs migration steps but never commit any changes")]
    public class VerifyOption : BaseRunPlatformOption
    {
    }
}
