using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Yuniql.AspNetCore;

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

            //docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssw0rd!" -p 1400:1433 -d mcr.microsoft.com/mssql/server:2017-latest
            var traceService = new ConsoleTraceService { IsDebugEnabled = true };
            app.UseYuniql(traceService, new YuniqlConfiguration
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
