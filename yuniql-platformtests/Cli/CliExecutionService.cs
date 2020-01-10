using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Yuniql.PlatformTests
{
    public class CliExecutionService
    {
        private readonly string _executionProcessFile;

        public CliExecutionService(string executionProcessFile)
        {
            this._executionProcessFile = executionProcessFile;
        }
        public string Run(string command, string workspace, string connectionString, string arguments)
        {
            string processArguments = $"{command} -p \"{workspace}\" -c \"{connectionString}\" {arguments}";
            return Run(processArguments);
        }

        public string ExecuteCli(string command, string workspace, string arguments)
        {
            string processArguments = $"{command} -p \"{workspace}\" {arguments}";
            return Run(processArguments);
        }

        public string Run(string arguments)
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

            return output;
        }
    }
}
