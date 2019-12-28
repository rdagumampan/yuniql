using System;
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
            var configuration = new YuniqlConfiguration
            {
                WorkspacePath = Path.Combine(Environment.CurrentDirectory, "_db"),
                ConnectionString = "Server=localhost,1400;Database=yuniqldb;User Id=SA;Password=P@ssw0rd!",
                AutoCreateDatabase = true
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
