using System;
using System.IO;

using Yuniql.Core;

namespace console_sample
{
    class Program
    {
        static void Main(string[] args)
        {
            //1. deploy new sql server on docker
            //$ docker run -dit -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssw0rd!" -p 1400:1433 -d mcr.microsoft.com/mssql/server:2017-latest

            //2. create custom trace message sinks, this can be your own logger framework
            var traceService = new ConsoleTraceService { IsDebugEnabled = true };

            //3. configure your migration run
            var configuration = Configuration.Instance;
            configuration.Platform = SUPPORTED_DATABASES.SQLSERVER;
            configuration.Workspace = Path.Combine(Environment.CurrentDirectory, "_db");
            configuration.ConnectionString = "Server=localhost,1400;Database=helloyuniql;User Id=SA;Password=P@ssw0rd!";
            configuration.IsAutoCreateDatabase = true;
            configuration.IsDebug = true;

            //4. run migrations
            var migrationServiceFactory = new MigrationServiceFactory(traceService);
            var migrationService = migrationServiceFactory.Create();
            migrationService.Run();

            //5. alternatively, you can validate app for schema version compatibility
            var requiredDbVersion = "v0.00";
            var currentDbVersion = migrationService.GetCurrentVersion();
            if (currentDbVersion != requiredDbVersion)
            {
                throw new ApplicationException($"Startup failed. " +
                    $"Application requires database version {requiredDbVersion} but current version is {currentDbVersion}." +
                    $"Deploy the latest compatible schema version of database and run again.");
            }
        }
    }
}
