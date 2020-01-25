using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Yuniql.AspNetCore;
using Yuniql.PostgreSql;

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
            //docker run -e POSTGRES_USER=sa -e POSTGRES_PASSWORD=P@ssw0rd! -e POSTGRES_DB=yuniqldb -p 5432:5432 postgres
            var traceService = new ConsoleTraceService { IsDebugEnabled = true };
            var configuration =
            app.UseYuniql(
                new PostgreSqlDataService(traceService), 
                new PostgreSqlBulkImportService(traceService), 
                traceService, new YuniqlConfiguration
                {
                    WorkspacePath = Path.Combine(Environment.CurrentDirectory, "_db"),
                    ConnectionString = "Host=localhost;Port=5432;Username=sa;Password=P@ssw0rd!;Database=yuniqldb",
                    AutoCreateDatabase = true
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
