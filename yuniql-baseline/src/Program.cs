using System;
using CommandLine;

namespace Yuniql.Extensions
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<BaselineOption>(args)
              .MapResult(
                (BaselineOption opts) =>
                {
                    TraceSettings.Instance.IsDebugEnabled = opts.Debug;
                    return RunBaselineOption(opts);
                },
                errs => 1);
        }

        private static object RunBaselineOption(BaselineOption opts)
        {
            try
            {
                //use default app directory when not specified explicitly 
                if (string.IsNullOrEmpty(opts.Path))
                {
                    opts.Path = Environment.CurrentDirectory;
                }

                //use environment variable when not specified explicitly
                if (string.IsNullOrEmpty(opts.ConnectionString))
                {
                    var environmentService = new EnvironmentService();
                    opts.ConnectionString = environmentService.GetEnvironmentVariable("YUNIQLX_BASELINE_CONNECTION_STRING");
                }

                var baselineService = new BaselineService();
                baselineService.Run(opts.ConnectionString, opts.Path);

                TraceService.Info($"Initialized {opts.Path}.");
            }
            catch (Exception ex)
            {
                TraceService.Error($"Failed to execute init function. {Environment.NewLine}{ex.ToString()}");
                throw;
            }

            return 0;
        }
    }

    //yuniql baseline
    [Verb("baseline", HelpText = "Create a baseline scripts of source sql server database. See supported objects on github/yuniql/wiki")]
    public class BaselineOption
    {
        //yuniqlx baseline -c "<connectiong-string>"
        [Option('c', "connection-string", Required = true, HelpText = "Connection string for source sql server instance")]
        public string ConnectionString { get; set; }

        //yuniqlx baseline -p c:\temp\demo | --path c:\temp\demo
        [Option('p', "path", Required = false, HelpText = "Path where to drop the baseline scripts")]
        public string Path { get; set; }

        //yuniqlx baseline -d | --debug
        [Option('d', "debug", Required = false, HelpText = "Print debug information including all raw scripts")]
        public bool Debug { get; set; }
    }
}
