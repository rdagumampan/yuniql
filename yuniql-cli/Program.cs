using CommandLine;
using System;
using System.Reflection;
using Yuniql.Core;
using Yuniql.Extensibility;

namespace Yuniql.CLI
{
    public class Program
    {
        //https://github.com/commandlineparser/commandline
        //https://github.com/dotnet/command-line-api

        public static int Main(string[] args)
        {
            var traceService = new FileTraceService();

            var directoryService = new DirectoryService(traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(traceService, directoryService, fileService);

            var environmentService = new EnvironmentService();
            var configurationService = new ConfigurationService(environmentService, workspaceService, traceService);

            var dataServiceFactory = new DataServiceFactory(traceService);
            var manifestFactory = new ManifestFactory(traceService);
            var migrationServiceFactory = new MigrationServiceFactory(traceService);
            var commandLineService = new CommandLineService(
                migrationServiceFactory,
                dataServiceFactory,
                manifestFactory,
                workspaceService,
                environmentService,
                traceService,
                configurationService);

            var resultCode = Parser.Default
                .ParseArguments<
                    CheckOption,
                    InitOption,
                    VerifyOption,
                    RunOption,
                    ApplyOption,
                    ListOption,
                    NextVersionOption,
                    EraseOption,
                    DestroyOption,
                    PlatformsOption,
                    BaselineOption,
                    RebaseOption
                    //ArchiveOption,
                >(args).MapResult(
                    (CheckOption opts) => Dispatch(commandLineService.RunCheckOption, opts, traceService),
                    (InitOption opts) => Dispatch(commandLineService.RunInitOption, opts, traceService),
                    (VerifyOption opts) => Dispatch(commandLineService.RunVerifyOption, opts, traceService),
                    (RunOption opts) => Dispatch(commandLineService.RunRunOption, opts, traceService),
                    (ApplyOption opts) => Dispatch(commandLineService.RunApplyOption, opts, traceService),
                    (ListOption opts) => Dispatch(commandLineService.RunListOption, opts, traceService),
                    (NextVersionOption opts) => Dispatch(commandLineService.RunNextVersionOption, opts, traceService),
                    (EraseOption opts) => Dispatch(commandLineService.RunEraseOption, opts, traceService),
                    (DestroyOption opts) => Dispatch(commandLineService.RunDestroyOption, opts, traceService),
                    (PlatformsOption opts) => Dispatch(commandLineService.RunPlatformsOption, opts, traceService),
                    (BaselineOption opts) => Dispatch(commandLineService.RunBaselineOption, opts, traceService),
                    (RebaseOption opts) => Dispatch(commandLineService.RunRebaseOption, opts, traceService),
                    //(ArchiveOption opts) => Dispatch(commandLineService.RunArchiveOption, opts, traceService)
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
            Console.WriteLine($"Visit https://yuniql.io for documentation and working samples{Environment.NewLine}");
            Console.ResetColor();

            traceService.IsDebugEnabled = opts.IsDebug;
            traceService.IsTraceSensitiveData = opts.IsTraceSensitiveData;
            traceService.IsTraceToFile = opts.IsTraceToFile;
            traceService.IsTraceToDirectory = opts.IsTraceToDirectory;
            traceService.TraceDirectory = opts.TraceDirectory;

            if (!traceService.IsTraceToFile)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"WRN   {DateTime.UtcNow.ToString("u")}   Trace logs settings is set to silent (default) and no log files will be produced. To enable log file creation, pass parameter --trace-to-file or see our CLI command reference.");
                Console.ResetColor();
            }

            return command.Invoke(opts);
        }
    }
}
