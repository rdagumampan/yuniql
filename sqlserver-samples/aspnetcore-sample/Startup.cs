using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            Console.WriteLine($"AppDomain.CurrentDomain.BaseDirectory: {AppDomain.CurrentDomain.BaseDirectory}");
            Console.WriteLine($"Environment.CurrentDirectory: {Environment.CurrentDirectory}");

            var traceService = new ConsoleTraceService { IsDebugEnabled = true };
            var configuration = new YuniqlConfiguration
            {
                WorkspacePath = Path.Combine(Environment.CurrentDirectory, "_db"),
                ConnectionString = "Server=localhost,1400;Database=yuniqldb;User Id=SA;Password=P@ssw0rd!",
                AutoCreateDatabase = true,
                //TargetVersion ="v1.00",
                //Tokens = new List<KeyValuePair<string, string>> {
                //    new KeyValuePair<string, string>("VwColumnPrefix1","Vw1"),
                //    new KeyValuePair<string, string>("VwColumnPrefix2","Vw2"),
                //    new KeyValuePair<string, string>("VwColumnPrefix3","Vw3"),
                //    new KeyValuePair<string, string>("VwColumnPrefix4","Vw4")
                //},
                //Delimiter = ",",
                //DebugTraceMode = true
            };
            app.UseYuniql(traceService, configuration);

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
