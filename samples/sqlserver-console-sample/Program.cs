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
            var baseDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\"));
            var traceService = new ConsoleTraceService { IsDebugEnabled = true };
            var configuration = new YuniqlConfiguration
            {
                WorkspacePath = Path.Combine(baseDirectory, "_db"),
                ConnectionString = "Server=localhost,1400;Database=yuniqldb;User Id=SA;Password=P@ssw0rd!",
                AutoCreateDatabase = true,
                Tokens = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("VwColumnPrefix1","Vw1"),
                    new KeyValuePair<string, string>("VwColumnPrefix2","Vw2"),
                    new KeyValuePair<string, string>("VwColumnPrefix3","Vw3"),
                    new KeyValuePair<string, string>("VwColumnPrefix4","Vw4")
                }
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

            var requiredDbVersion = "v1.01";
            var currentDbVersion = migrationService.GetCurrentVersion();
            if(currentDbVersion != requiredDbVersion)
            {
                throw new ApplicationException($"Startup failed. " +
                    $"Application requires database version {requiredDbVersion} but current version is {currentDbVersion}." +
                    $"Deploy the latest compatible schema version of database and run again.");
            }
        }
    }
}
