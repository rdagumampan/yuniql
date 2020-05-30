using Microsoft.AspNetCore.Builder;
using Yuniql.Core;
using Yuniql.Extensibility;

namespace Yuniql.AspNetCore
{
    public static class YuniqApplicationBuilderExtensions
    {
        /// <summary>
        /// Runs database migrations with Yuniql. This uses default trace service FileTraceService.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration">Desired configuration when yuniql runs. Set your workspace location, connection string, target version and other parameters.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseYuniql(
            this IApplicationBuilder builder,
            YuniqlConfiguration configuration
        )
        {
            var traceService = new FileTraceService { IsDebugEnabled = configuration.DebugTraceMode };
            return builder.UseYuniql(traceService, configuration);
        }

        /// <summary>
        /// Runs database migrations with Yuniql. This uses your specific implementation of ITraceService interface.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="traceService">Your custom implementation of ITraceService interface</param>
        /// <param name="configuration">Desired configuration when yuniql runs. Set your workspace location, connection string, target version and other parameters.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseYuniql(
            this IApplicationBuilder builder,
            ITraceService traceService,
            YuniqlConfiguration configuration
        )
        {
            var migrationServiceFactory = new MigrationServiceFactory(traceService);
            var migrationService = migrationServiceFactory.Create();
            migrationService.Initialize(configuration.ConnectionString);
            migrationService.Run(
                configuration.WorkspacePath,
                configuration.TargetVersion,
                autoCreateDatabase: configuration.AutoCreateDatabase,
                tokens: configuration.Tokens,
                verifyOnly: configuration.VerifyOnly,
                bulkSeparator: configuration.BulkSeparator,
                metaSchemaName: configuration.MetaSchemaName,
                metaTableName: configuration.MetaTableName,
                commandTimeout: configuration.CommandTimeout,
                bulkBatchSize: configuration.BulkBatchSize,
                appliedByTool: configuration.ApplyByTool,
                appliedByToolVersion: configuration.ApplyByToolVersion,
                environmentCode: configuration.Environment);

            return builder;
        }

        /// <summary>
        /// Runs database migrations with Yuniql. Use this interface to run migrations targeting non-sqlserver platforms such as PostgreSql and MySql.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="dataService">Implementation of <see cref="IDataService". See Yuniql.PostgreSql and Yuniql.MySql pacages./></param>
        /// <param name="bulkImportService">Implementation of <see cref="IBulkImportService". See Yuniql.PostgreSql and Yuniql.MySql pacages./></param>
        /// <param name="traceService">Your custom implementation of ITraceService interface</param>
        /// <param name="configuration">Desired configuration when yuniql runs. Set your workspace location, connection string, target version and other parameters.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseYuniql(
            this IApplicationBuilder builder,
            IDataService dataService,
            IBulkImportService bulkImportService,
            ITraceService traceService,
            YuniqlConfiguration configuration
        )
        {
            var migrationServiceFactory = new MigrationServiceFactory(traceService);
            var migrationService = migrationServiceFactory.Create(dataService, bulkImportService);
            migrationService.Initialize(configuration.ConnectionString);
            migrationService.Run(
                configuration.WorkspacePath,
                configuration.TargetVersion,
                autoCreateDatabase: configuration.AutoCreateDatabase,
                tokens: configuration.Tokens,
                verifyOnly: configuration.VerifyOnly,
                bulkSeparator: configuration.BulkSeparator,
                metaSchemaName: configuration.MetaSchemaName,
                metaTableName: configuration.MetaTableName,
                commandTimeout: configuration.CommandTimeout,
                bulkBatchSize: configuration.BulkBatchSize,
                appliedByTool: configuration.ApplyByTool,
                appliedByToolVersion: configuration.ApplyByToolVersion,
                environmentCode: configuration.Environment);

            return builder;
        }

    }
}
