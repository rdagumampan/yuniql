using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Yuniql.PlatformTests.CLI
{
    public class CommandLineExecutionService
    {
        private readonly string _executionProcessFile;

        public CommandLineExecutionService(string executionProcessFile)
        {
            _executionProcessFile = executionProcessFile;
        }
        public string Run(string command, string workspace, string connectionString, string platform, string arguments)
        {
            string processArguments = $"{command} -p \"{workspace}\" -c \"{connectionString}\" --platform \"{platform}\" {arguments}";
            return RunInternal(processArguments);
        }

        public string Run(string command, string workspace, string arguments)
        {
            string processArguments = $"{command} -p \"{workspace}\" {arguments}";
            return RunInternal(processArguments);
        }

        private string RunInternal(string arguments)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _executionProcessFile,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();

            var reader = process.StandardOutput;
            string output = reader.ReadToEnd();
            process.WaitForExit();

            Console.WriteLine(output);
            return output;
        }
    }
}
