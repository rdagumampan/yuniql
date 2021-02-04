using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Yuniql.CLI
{
    //yuniql info
    [Verb("ping", HelpText = "Test connectivity to target database server.")]
    public class PingOption : BasePlatformOption
    {

    }
}
