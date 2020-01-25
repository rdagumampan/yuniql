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
            //docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssw0rd!" -p 1400:1433 -d mcr.microsoft.com/mssql/server:2017-latest
            var configuration = new YuniqlConfiguration
            {
                WorkspacePath = Path.Combine(Environment.CurrentDirectory, "_db"),
                ConnectionString = "Server=localhost,1400;Database=yuniqldb;User Id=SA;Password=P@ssw0rd!",
                AutoCreateDatabase = true,
                Tokens = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("VwColumnPrefix1","Vw1"),
                    new KeyValuePair<string, string>("VwColumnPrefix2","Vw2"),
                    new KeyValuePair<string, string>("VwColumnPrefix3","Vw3"),
                    new KeyValuePair<string, string>("VwColumnPrefix4","Vw4")
                }
            };

            CreateHostBuilder(args)
                .UseYuniql(configuration)
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
