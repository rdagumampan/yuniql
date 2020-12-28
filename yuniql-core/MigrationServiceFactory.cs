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

        ///<inheritdoc/>
        public MigrationServiceFactory(
            ITraceService traceService)
        {
            this._traceService = traceService;
        }

        ///<inheritdoc/>
        public IMigrationService Create()
        {
            var dataService = new SqlServerDataService(_traceService);
            var bulkImportService = new SqlServerBulkImportService(_traceService);
            return CreateInternal(dataService, bulkImportService);
        }

        ///<inheritdoc/>
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

            var metadataService = new MetadataService(dataService, _traceService, tokenReplacementService);

            var migrationService = new MigrationServiceTransactional(
                localVersionService,
                dataService,
                bulkImportService,
                metadataService,
                tokenReplacementService,
                directoryService,
                fileService,
                _traceService);
            return migrationService;
        }
    }
}

