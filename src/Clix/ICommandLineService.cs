namespace ArdiLabs.Yuniql.CLI
{
    public interface ICommandLineService
    {
        object RunInitOption(InitOption opts);

        object IncrementVersion(NextVersionOption opts);

        object RunMigration(RunOption opts);

        object RunVerify(VerifyOption opts);

        object RunInfoOption(InfoOption opts);

        object RunEraseOption(EraseOption opts);

        object RunBaselineOption(BaselineOption opts);

        object RunRebaseOption(RebaseOption opts);

    }
}