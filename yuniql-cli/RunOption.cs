using CommandLine;

namespace Yuniql.CLI
{
    //yuniql run
    [Verb("run", HelpText = "Runs migration up to latest available version or up to specific version.")]
    public class RunOption : BaseRunPlatformOption
    {
    }

    //yuniql apply
    [Verb("apply", HelpText = "Alias to run command. Use this when yuniql run creates conflict such as when used in docker run.")]
    public class ApplyOption : BaseRunPlatformOption
    {
    }
}
