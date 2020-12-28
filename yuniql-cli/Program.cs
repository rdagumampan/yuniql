using CommandLine;
using System;
using System.Reflection;
using Yuniql.CLI;
using Yuniql.Core;
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
            var migrationServiceFactory = new CLI.MigrationServiceFactory(traceService);
            var commandLineService = new CommandLineService(migrationServiceFactory,
                                                            localVersionService,
                                                            environmentService,
                                                            traceService);

            var resultCode = Parser.Default
                .ParseArguments<InitOption, RunOption, ListOption, NextVersionOption, VerifyOption, EraseOption, BaselineOption, RebaseOption, PlatformsOption>(args)
                .MapResult((InitOption opts) => Dispatch(commandLineService.RunInitOption, opts, traceService),
                           (RunOption opts) => Dispatch(commandLineService.RunRunOption, opts, traceService),
                           (NextVersionOption opts) => Dispatch(commandLineService.RunNextVersionOption, opts, traceService),
                           (ListOption opts) => Dispatch(commandLineService.RunListOption, opts, traceService),
                           (VerifyOption opts) => Dispatch(commandLineService.RunVerifyOption, opts, traceService),
                           (EraseOption opts) => Dispatch(commandLineService.RunEraseOption, opts, traceService),
                           (BaselineOption opts) => Dispatch(commandLineService.RunBaselineOption, opts, traceService),
                           (RebaseOption opts) => Dispatch(commandLineService.RunRebaseOption, opts, traceService),
                           (ArchiveOption opts) => Dispatch(commandLineService.RunArchiveOption, opts, traceService),
                           (PlatformsOption opts) => Dispatch(commandLineService.RunPlatformsOption, opts, traceService),

                           errs => 1);

            return resultCode;
        }

        private static int Dispatch<T>(Func<T, int> command, T opts, ITraceService traceService)
            where T : BaseOption
        {
            var toolVersion = typeof(CommandLineService).Assembly.GetName().Version;
            var toolPlatform = Environment.OSVersion.Platform == PlatformID.Win32NT ? "windows" : "linux";
            var toolCopyright = (typeof(CommandLineService).Assembly
                .GetCustomAttribute(typeof(AssemblyCopyrightAttribute)) as AssemblyCopyrightAttribute).Copyright;

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"Running yuniql v{toolVersion.Major}.{toolVersion.Minor}.{toolVersion.Build} for {toolPlatform}-x64");
            Console.WriteLine($"{toolCopyright}. Apache License v2.0");
            Console.WriteLine($"Visit https://yuniql.io for documentation & more samples{Environment.NewLine}");
            Console.ResetColor();

            traceService.IsDebugEnabled = opts.Debug;
            return command.Invoke(opts);
        }
    }
}
