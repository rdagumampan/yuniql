using Yuniql.CLI;
using Yuniql.Core;
using CommandLine;

namespace Yuniql
{
    public class Program
    {
        //https://github.com/commandlineparser/commandline
        //https://github.com/dotnet/command-line-api

        public static void Main(string[] args)
        {
            var environmentService = new EnvironmentService();
            var traceService = new TraceService();
            var localVersionService = new LocalVersionService(traceService);
            var migrationServiceFactory = new MigrationServiceFactory(environmentService, traceService);
            var commandLineService = new CommandLineService(migrationServiceFactory, localVersionService, environmentService, traceService);

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
                    traceService.IsDebugEnabled = opts.Debug;
                    return commandLineService.RunInitOption(opts);
                },
                (RunOption opts) =>
                {
                    traceService.IsDebugEnabled = opts.Debug;
                    return commandLineService.RunMigration(opts);
                },
                (NextVersionOption opts) =>
                {
                    traceService.IsDebugEnabled = opts.Debug;
                    return commandLineService.IncrementVersion(opts);
                },
                (InfoOption opts) =>
                {
                    traceService.IsDebugEnabled = opts.Debug;
                    return commandLineService.RunInfoOption(opts);
                },
                (VerifyOption opts) =>
                {
                    traceService.IsDebugEnabled = opts.Debug;
                    return commandLineService.RunVerify(opts);
                },
                (EraseOption opts) =>
                {
                    traceService.IsDebugEnabled = opts.Debug;
                    return commandLineService.RunEraseOption(opts);
                },
                (BaselineOption opts) =>
                {
                    traceService.IsDebugEnabled = opts.Debug;
                    return commandLineService.RunBaselineOption(opts);
                },
                (RebaseOption opts) =>
                {
                    traceService.IsDebugEnabled = opts.Debug;
                    return commandLineService.RunRebaseOption(opts);
                },
                errs => 1);
        }
    }
}
