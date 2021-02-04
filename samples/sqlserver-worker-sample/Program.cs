using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Yuniql.AspNetCore;
using Yuniql.Core;

namespace worker_sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //1. deploy new sql server on docker
            //$ docker run -dit -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssw0rd!" -p 1400:1433 -d mcr.microsoft.com/mssql/server:2017-latest

            //2. create custom trace message sinks, this can be your own logger framework
            var traceService = new ConsoleTraceService { IsDebugEnabled = true };

            CreateHostBuilder(args)
                //3. run migrations
                .UseYuniql(traceService, new Yuniql.AspNetCore.Configuration
                {
                    Platform = SUPPORTED_DATABASES.SQLSERVER,
                    Workspace = Path.Combine(Environment.CurrentDirectory, "_db"),
                    ConnectionString = "Server=localhost,1400;Database=helloyuniql;User Id=SA;Password=P@ssw0rd!",
                    IsAutoCreateDatabase = true, IsDebug = true
                })
                .Build()
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                });
    }
}
