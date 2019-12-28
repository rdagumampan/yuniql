using Microsoft.AspNetCore.Builder;
using Yuniql.Core;
using Yuniql.Extensibility;

namespace Yuniql.AspNetCore
{
    public static class YuniqApplicationBuilderExtensions
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
}
