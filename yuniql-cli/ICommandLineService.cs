namespace Yuniql.CLI
{
    public interface ICommandLineService
    {
        int RunInitOption(InitOption opts);

        int IncrementVersion(NextVersionOption opts);

        int RunMigration(RunOption opts);

        int RunVerify(VerifyOption opts);

        int RunInfoOption(InfoOption opts);

        int RunEraseOption(EraseOption opts);

        int RunBaselineOption(BaselineOption opts);

        int RunRebaseOption(RebaseOption opts);

    }
}