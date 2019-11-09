using CommandLine;

namespace ArdiLabs.Yuniql
{
    public class Program
    {
        //https://github.com/commandlineparser/commandline
        //https://github.com/dotnet/command-line-api

        public static void Main(string[] args)
        {
            var commandLineService = new CommandLineService();

            Parser.Default.ParseArguments<
                InitOption,
                RunOption,
                NextVersionOption,
                InfoOption,
                VerifyOption,
                EraseOption,
                BaselineOption,
                RebaseOption>(args)
              .MapResult(
                (InitOption opts) =>
                {
                    TraceSettings.Instance.IsDebugEnabled = opts.Debug;
                    return commandLineService.RunInitOption(opts);
                },
                (RunOption opts) =>
                {
                    TraceSettings.Instance.IsDebugEnabled = opts.Debug;
                    return commandLineService.RunMigration(opts);
                },
                (NextVersionOption opts) =>
                {
                    TraceSettings.Instance.IsDebugEnabled = opts.Debug;
                    return commandLineService.IncrementVersion(opts);
                },
                (InfoOption opts) =>
                {
                    TraceSettings.Instance.IsDebugEnabled = opts.Debug;
                    return commandLineService.RunInfoOption(opts);
                },
                (VerifyOption opts) =>
                {
                    TraceSettings.Instance.IsDebugEnabled = opts.Debug;
                    return commandLineService.RunVerify(opts);
                },
                (EraseOption opts) =>
                {
                    TraceSettings.Instance.IsDebugEnabled = opts.Debug;
                    return commandLineService.RunEraseOption(opts);
                },
                (BaselineOption opts) =>
                {
                    TraceSettings.Instance.IsDebugEnabled = opts.Debug;
                    return commandLineService.RunBaselineOption(opts);
                },
                (RebaseOption opts) =>
                {
                    TraceSettings.Instance.IsDebugEnabled = opts.Debug;
                    return commandLineService.RunRebaseOption(opts);
                },
                errs => 1);
        }       
    }
}
