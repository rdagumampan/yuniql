using Yuniql.CLI;
using Yuniql.Core;
using CommandLine;
using System;
using Yuniql.Extensibility;

namespace Yuniql
{
    public class Program
    {
        //https://github.com/commandlineparser/commandline
        //https://github.com/dotnet/command-line-api

        public static int Main(string[] args)
        {
            var environmentService = new EnvironmentService();
            var traceService = new FileTraceService();
            var localVersionService = new LocalVersionService(traceService);
            //var migrationServiceFactory = new CLI.MigrationServiceFactory(traceService);
            var migrationServiceFactory = new Core.Factories.MigrationServiceFactory(traceService);
            var commandLineService = new CommandLineService(migrationServiceFactory, localVersionService, environmentService, traceService);

            var resultCode = Parser.Default.ParseArguments<
                InitOption,
                RunOption,
                NextVersionOption,
                InfoOption,
                VerifyOption,
                EraseOption,
                BaselineOption,
                RebaseOption>(args)
              .MapResult(
                (InitOption opts) => Dispatch(commandLineService.RunInitOption, opts, traceService),
                (RunOption opts) => Dispatch(commandLineService.RunMigration, opts, traceService),
                (NextVersionOption opts) => Dispatch(commandLineService.IncrementVersion, opts, traceService),
                (InfoOption opts) => Dispatch(commandLineService.RunInfoOption, opts, traceService),
                (VerifyOption opts) => Dispatch(commandLineService.RunVerify, opts, traceService),
                (EraseOption opts) => Dispatch(commandLineService.RunEraseOption, opts, traceService),
                (BaselineOption opts) => Dispatch(commandLineService.RunBaselineOption, opts, traceService),
                (RebaseOption opts) => Dispatch(commandLineService.RunRebaseOption, opts, traceService),
                (ArchiveOption opts) => Dispatch(commandLineService.RunArchiveOption, opts, traceService),
                errs => 1);

            return resultCode;
        }

        private static int Dispatch<T>(Func<T, int> command, T opts, ITraceService traceService) where T : BaseOption {
            traceService.IsDebugEnabled = opts.Debug;
            return command.Invoke(opts);
        }
    }
}
