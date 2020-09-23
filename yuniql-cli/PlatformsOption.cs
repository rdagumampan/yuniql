using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Yuniql.CLI
{
    //yuniql info
    [Verb("platforms", HelpText = "Shows suported platforms and sample usage .")]
    public class PlatformsOption : BasePlatformOption
    {
    }
}
