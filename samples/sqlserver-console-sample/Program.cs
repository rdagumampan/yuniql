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

            //2. create custom trace message sinks
            var traceService = new ConsoleTraceService { IsDebugEnabled = true };

            //3. configure your migration run
            var configuration = new YuniqlConfiguration
            {
                WorkspacePath = Path.Combine(Environment.CurrentDirectory, "_db"),
                ConnectionString = "Server=localhost,1400;Database=yuniqldb;User Id=SA;Password=P@ssw0rd!",
                AutoCreateDatabase = true
            };

            //4. run migrations
            var migrationServiceFactory = new MigrationServiceFactory(traceService);
            var migrationService = migrationServiceFactory.Create();
            migrationService.Initialize(configuration.ConnectionString);
            migrationService.Run(
                configuration.WorkspacePath,
                targetVersion: configuration.TargetVersion,
                autoCreateDatabase: configuration.AutoCreateDatabase
           );

            //5. validate for schema version compatibility
            var requiredDbVersion = "v0.00";
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
