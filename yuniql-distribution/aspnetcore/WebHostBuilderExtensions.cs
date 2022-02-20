using Microsoft.AspNetCore.Hosting;
using Yuniql.Core;
using Yuniql.Extensibility;

namespace Yuniql.AspNetCore
{
    public static class WebHostBuilderExtensions
    {
        /// <summary>
        /// Runs database migrations with Yuniql. Use this interface to run migrations targeting non-sqlserver platforms such as PostgreSql and MySql.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="dataService">Implementation of <see cref="IDataService". See Yuniql.PostgreSql and Yuniql.MySql pacages./></param>
        /// <param name="bulkImportService">Implementation of <see cref="IBulkImportService". See Yuniql.PostgreSql and Yuniql.MySql pacages./></param>
        /// <param name="traceService">Your custom implementation of ITraceService interface</param>
        /// <param name="configuration">Desired configuration when yuniql runs. Set your workspace location, connection string, target version and other parameters.</param>
        /// <returns></returns>
        public static IWebHostBuilder UseYuniql(
            this IWebHostBuilder builder,
            IDataService dataService,
            IBulkImportService bulkImportService,
            ITraceService traceService,
            Configuration configuration
        )
        {
            ConfigurationHelper.Initialize(configuration);

            var migrationServiceFactory = new MigrationServiceFactory(traceService);
            var migrationService = migrationServiceFactory.Create(dataService, bulkImportService);
            migrationService.Run();

            return builder;
        }

    }
}
