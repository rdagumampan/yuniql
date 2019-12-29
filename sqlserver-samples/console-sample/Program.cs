using System;
using System.Collections.Generic;
using System.IO;
using Yuniql.Core;

namespace console_sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var traceService = new ConsoleTraceService { IsDebugEnabled = true };
            var configuration = new YuniqlConfiguration
            {
                //require configuration
                WorkspacePath = Path.Combine(Environment.CurrentDirectory, "_db"),
                ConnectionString = "Server=localhost,1400;Database=yuniqldb;User Id=SA;Password=P@ssw0rd!",
                AutoCreateDatabase = true,

                //optional configuration for more advanced features
                TargetVersion = "v1.00",
                Tokens = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("VwColumnPrefix1","Vw1"),
                    new KeyValuePair<string, string>("VwColumnPrefix2","Vw2"),
                    new KeyValuePair<string, string>("VwColumnPrefix3","Vw3"),
                    new KeyValuePair<string, string>("VwColumnPrefix4","Vw4")
                },
                Delimiter = ",",
                DebugTraceMode = true
            };

            var migrationServiceFactory = new MigrationServiceFactory(traceService);
            var migrationService = migrationServiceFactory.Create(configuration.Platform);
            migrationService.Initialize(configuration.ConnectionString);
            migrationService.Run(
                configuration.WorkspacePath,
                configuration.TargetVersion,
                configuration.AutoCreateDatabase,
                configuration.Tokens,
                configuration.VerifyOnly,
                configuration.Delimiter);
        }
    }
}
