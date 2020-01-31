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
            //docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssw0rd!" -p 1400:1433 -d mcr.microsoft.com/mssql/server:2017-latest
            var traceService = new ConsoleTraceService { IsDebugEnabled = true };
            var configuration = new YuniqlConfiguration
            {
                WorkspacePath = Path.Combine(Environment.CurrentDirectory, "_db"),
                ConnectionString = "Server=localhost,1400;Database=yuniqldb;User Id=SA;Password=P@ssw0rd!",
                AutoCreateDatabase = true
            };

            var migrationServiceFactory = new MigrationServiceFactory(traceService);
            var migrationService = migrationServiceFactory.Create();
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
