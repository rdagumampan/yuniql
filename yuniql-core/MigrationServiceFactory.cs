using System;
using Yuniql.Extensibility;
using Yuniql.SqlServer;

namespace Yuniql.Core
{
    /// <summary>
    /// Factory class of creating instance of <see cref="IMigrationService"/>.
    /// </summary>
    public class MigrationServiceFactory : IMigrationServiceFactory
    {
        private readonly ITraceService _traceService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="traceService"></param>
        public MigrationServiceFactory(
            ITraceService traceService)
        {
            this._traceService = traceService;
        }
        
        /// <summary>
        /// Create instance of <see cref="IMigrationService"/> with default support for SQL Server.
        /// </summary>
        public IMigrationService Create()
        {
            var dataService = new SqlServerDataService(_traceService);
            var bulkImportService = new SqlServerBulkImportService(_traceService);
            return CreateInternal(dataService, bulkImportService);
        }

        /// <summary>
        /// Create instance of <see cref="IMigrationService"/> and uses external data services.
        /// When targeting PostgreSql or MySql, this is where you can pass the implementation of <see cref="IDataService"/> and <see cref="IBulkImportService"/>.
        /// </summary>
        /// <param name="dataService">Platform specific data service providing compatible SQL statements and connection objects.</param>
        /// <param name="bulkImportService">Platform specific service provding support for bulk import of CSV files.</param>
        /// <returns>An instance of <see cref="IMigrationService"/> and uses external data services.</returns>
        public IMigrationService Create(IDataService dataService, IBulkImportService bulkImportService)
        {
            return CreateInternal(dataService, bulkImportService);
        }

        private IMigrationService CreateInternal(IDataService dataService, IBulkImportService bulkImportService)
        {
            var localVersionService = new LocalVersionService(_traceService);
            var tokenReplacementService = new TokenReplacementService(_traceService);
            var directoryService = new DirectoryService();
            var fileService = new FileService();

            var configurationService = new ConfigurationDataService(dataService, _traceService);

            var migrationService = new MigrationService(
                localVersionService,
                dataService,
                bulkImportService,
                configurationService,
                tokenReplacementService,
                directoryService,
                fileService,
                _traceService);
            return migrationService;
        }
    }
}

