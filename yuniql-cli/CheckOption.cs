using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Yuniql.CLI
{
    //yuniql info
    [Verb("check", HelpText = "Check and test connectivity to target database server.")]
    public class CheckOption : BasePlatformOption
    {

    }
}
