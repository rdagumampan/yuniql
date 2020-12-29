using System;
using Yuniql.Core;
using Yuniql.Extensibility;
using Yuniql.MySql;
using Yuniql.PostgreSql;
using Yuniql.SqlServer;

namespace Yuniql.PlatformTests
{
    public class MigrationServiceFactory : IMigrationServiceFactory
    {
        private readonly ITraceService _traceService;

        public MigrationServiceFactory(
            ITraceService traceService)
        {
            this._traceService = traceService;
        }

        public IMigrationService Create(string platform)
        {
            switch (platform.ToLower())
            {
                case SUPPORTED_DATABASES.SQLSERVER:
                    {
                        var dataService = new SqlServerDataService(_traceService);
                        var bulkImportService = new SqlServerBulkImportService(_traceService);
                        return dataService.IsTransactionalDdlSupported
                            ? CreateTransactionalMigrationService(dataService, bulkImportService)
                            : CreateNonTransactionalMigrationService(dataService, bulkImportService);
                    }
                case SUPPORTED_DATABASES.POSTGRESQL:
                    {
                        var dataService = new PostgreSqlDataService(_traceService);
                        var bulkImportService = new PostgreSqlBulkImportService(_traceService);
                        return dataService.IsTransactionalDdlSupported
                            ? CreateTransactionalMigrationService(dataService, bulkImportService)
                            : CreateNonTransactionalMigrationService(dataService, bulkImportService);
                    }
                case SUPPORTED_DATABASES.MYSQL:
                    {
                        var dataService = new MySqlDataService(_traceService);
                        var bulkImportService = new MySqlBulkImportService(_traceService);
                        return dataService.IsTransactionalDdlSupported
                            ? CreateTransactionalMigrationService(dataService, bulkImportService)
                            : CreateNonTransactionalMigrationService(dataService, bulkImportService);
                    }
                case SUPPORTED_DATABASES.MARIADB:
                    {
                        var dataService = new MySqlDataService(_traceService);
                        var bulkImportService = new MySqlBulkImportService(_traceService);
                        return dataService.IsTransactionalDdlSupported
                            ? CreateTransactionalMigrationService(dataService, bulkImportService)
                            : CreateNonTransactionalMigrationService(dataService, bulkImportService);
                    }
                default:
                    throw new NotSupportedException($"The target database platform {platform} is not supported or plugins location was not correctly configured. " +
                        $"See WIKI for supported database platforms and usage guide.");
            }
        }

        private IMigrationService CreateTransactionalMigrationService(IDataService dataService, IBulkImportService bulkImportService)
        {
            var localVersionService = new LocalVersionService(_traceService);
            var tokenReplacementService = new TokenReplacementService(_traceService);
            var directoryService = new DirectoryService();
            var fileService = new FileService();
            var metadataService = new MetadataService(dataService, _traceService, tokenReplacementService);
            var environmentService = new EnvironmentService();
            var configurationService = new ConfigurationService(environmentService, localVersionService, _traceService);

            var migrationService = new MigrationServiceTransactional(
                localVersionService,
                dataService,
                bulkImportService,
                metadataService,
                tokenReplacementService,
                directoryService,
                fileService,
                _traceService,
                configurationService);
            return migrationService;
        }

        private IMigrationService CreateNonTransactionalMigrationService(IDataService dataService, IBulkImportService bulkImportService)
        {
            var localVersionService = new LocalVersionService(_traceService);
            var tokenReplacementService = new TokenReplacementService(_traceService);
            var directoryService = new DirectoryService();
            var fileService = new FileService();
            var metadataService = new MetadataService(dataService, _traceService, tokenReplacementService);
            var environmentService = new EnvironmentService();
            var configurationService = new ConfigurationService(environmentService, localVersionService, _traceService);

            var migrationService = new MigrationServiceNonTransactional(
                localVersionService,
                dataService,
                bulkImportService,
                metadataService,
                tokenReplacementService,
                directoryService,
                fileService,
                _traceService,
                configurationService);
            return migrationService;
        }

    }
}

