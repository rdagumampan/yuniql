using Microsoft.AspNetCore.Builder;
using System.Collections.Generic;
using Yuniql.Core;
using Yuniql.Extensibility;

namespace Yuniql.AspNetCore
{
    public static class YuniqlApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseYuniql(
            this IApplicationBuilder builder,
            YuniqlConfiguration configuration
        )
        {
            var traceService = new FileTraceService { IsDebugEnabled = configuration.DebugTraceMode };
            return builder.UseYuniql(traceService, configuration);
        }

        public static IApplicationBuilder UseYuniql(
            this IApplicationBuilder builder,
            ITraceService traceService,
            YuniqlConfiguration configuration
        )
        {
            var environmentService = new EnvironmentService();
            var migrationServiceFactory = new MigrationServiceFactory(environmentService, traceService);
            var migrationService = migrationServiceFactory.Create(configuration.Platform);
            migrationService.Initialize(configuration.ConnectionString);
            migrationService.Run(
                configuration.WorkspacePath, 
                configuration.TargetVersion, 
                configuration.AutoCreateDatabase, 
                configuration.Tokens, 
                configuration.VerifyOnly, 
                configuration.Delimiter);

            return builder;
        }
    }

    public class YuniqlConfiguration
    {
        public string Platform { get; set; } = "sqlserver";

        public string WorkspacePath { get; set; }

        public string ConnectionString { get; set; }

        public bool AutoCreateDatabase { get; set; } = false;

        public string TargetVersion { get; set; }

        public List<KeyValuePair<string, string>> Tokens { get; set; } = new List<KeyValuePair<string, string>>();

        public bool VerifyOnly { get; set; }

        public string Delimiter { get; set; } = ",";

        public bool DebugTraceMode { get; set; } = false;
    }
}
