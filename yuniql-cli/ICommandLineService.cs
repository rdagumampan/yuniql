namespace Yuniql.CLI
{
    public interface ICommandLineService
    {
        int RunCheckOption(CheckOption opts);

        int RunInitOption(InitOption opts);

        int RunNextVersionOption(NextVersionOption opts);

        int RunRunOption(RunOption opts);

        int RunVerifyOption(VerifyOption opts);

        int RunListOption(ListOption opts);

        int RunEraseOption(EraseOption opts);

        int RunBaselineOption(BaselineOption opts);

        int RunRebaseOption(RebaseOption opts);

    }
}