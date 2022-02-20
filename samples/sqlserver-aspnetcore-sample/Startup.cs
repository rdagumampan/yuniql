using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Yuniql.AspNetCore;
using Yuniql.SqlServer;

namespace aspnetcore_sample
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //1. deploy new sql server on docker
            //$ docker run -dit --name yuniql-sqlserver  -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssw0rd!" -p 1400:1433 -d mcr.microsoft.com/mssql/server:2017-latest

            //2. create custom trace message sinks, this can be your own logger framework
            var traceService = new ConsoleTraceService { IsDebugEnabled = true };

            //3. run migrations
            var dataService = new SqlServerDataService(traceService);
            var bulkService = new SqlServerBulkImportService(traceService);
            app.UseYuniql(dataService, bulkService, traceService, new Configuration
            {
                Platform = "sqlserver",
                Workspace = Path.Combine(Environment.CurrentDirectory, "_db"),
                ConnectionString = "Server=localhost,1400;Database=helloyuniql;User Id=SA;Password=P@ssw0rd!",
                IsAutoCreateDatabase = true, IsDebug = true
            });

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
