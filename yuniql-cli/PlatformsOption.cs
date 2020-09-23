using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Yuniql.CLI
{
    //yuniql info
    [Verb("platforms", HelpText = "Shows all the migrations applied to target database.")]
    public class PlatformsOption : BasePlatformOption
    {
    }
}
