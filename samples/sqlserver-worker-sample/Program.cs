using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Yuniql.AspNetCore;

namespace worker_sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //1. deploy new sql server on docker
            //$ docker run -dit -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssw0rd!" -p 1400:1433 -d mcr.microsoft.com/mssql/server:2017-latest

            //2. create custom trace message sinks
            var traceService = new ConsoleTraceService { IsDebugEnabled = true };

            CreateHostBuilder(args)
                //3. run migrations
                .UseYuniql(traceService, new Configuration
                {
                    WorkspacePath = Path.Combine(Environment.CurrentDirectory, "_db"),
                    ConnectionString = "Server=localhost,1400;Database=yuniqldb;User Id=SA;Password=P@ssw0rd!",
                    AutoCreateDatabase = true, DebugTraceMode = true
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
