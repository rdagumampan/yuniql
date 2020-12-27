using CommandLine;

namespace Yuniql.CLI
{
    //yuniql run
    [Verb("run", HelpText = "Runs migration up to latest available version or up to specific version.")]
    public class RunOption : BaseRunPlatformOption
    {
        //yuniql <command> --continue-after-failure
        [Option("continue-after-failure", Required = false, HelpText = "Skip failed script and continue with migration (Only for platforms which doesn't fully support transactions).", Default = false)]
        public bool ContinueAfterFailure { get; set; }

        //yuniql <command> --require-cleared-draft
        [Option("require-cleared-draft", Required = false, HelpText = "When set, migration will fail if the _draft directory is not empty. This option ideal when targeting staging/production environment.", Default = false)]
        public bool RequiredClearedDraft { get; set; }
    }
}
