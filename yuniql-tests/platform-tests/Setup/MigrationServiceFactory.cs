using System;
using Yuniql.Core;
using Yuniql.Extensibility;
using Yuniql.MySql;
using Yuniql.Oracle;
using Yuniql.PostgreSql;
using Yuniql.Redshift;
using Yuniql.Snowflake;
using Yuniql.SqlServer;
using IMigrationServiceFactory = Yuniql.PlatformTests.Interfaces.IMigrationServiceFactory;

namespace Yuniql.PlatformTests.Setup
{
    public class MigrationServiceFactory : IMigrationServiceFactory
    {
        private readonly ITraceService _traceService;

        public MigrationServiceFactory(
            ITraceService traceService)
        {
            _traceService = traceService;
        }

        public IMigrationService Create(string platform)
        {
            switch (platform.ToLower())
            {
                case SUPPORTED_DATABASES.SQLSERVER:
                    {
                        var dataService = new SqlServerDataService(_traceService);
                        var bulkImportService = new SqlServerBulkImportService(_traceService);
                        return CreateInternal(dataService, bulkImportService);
                    }
                case SUPPORTED_DATABASES.POSTGRESQL:
                    {
                        var dataService = new PostgreSqlDataService(_traceService);
                        var bulkImportService = new PostgreSqlBulkImportService(_traceService);
                        return CreateInternal(dataService, bulkImportService);
                    }
                case SUPPORTED_DATABASES.MYSQL:
                    {
                        var dataService = new MySqlDataService(_traceService);
                        var bulkImportService = new MySqlBulkImportService(_traceService);
                        return CreateInternal(dataService, bulkImportService);
                    }
                case SUPPORTED_DATABASES.MARIADB:
                    {
                        var dataService = new MySqlDataService(_traceService);
                        var bulkImportService = new MySqlBulkImportService(_traceService);
                        return CreateInternal(dataService, bulkImportService);
                    }
                case SUPPORTED_DATABASES.SNOWFLAKE:
                    {
                        var dataService = new SnowflakeDataService(_traceService);
                        var bulkImportService = new SnowflakeBulkImportService(_traceService);
                        return CreateInternal(dataService, bulkImportService);
                    }
                case SUPPORTED_DATABASES.REDSHIFT:
                    {
                        var dataService = new RedshiftDataService(_traceService);
                        var bulkImportService = new RedshiftBulkImportService(_traceService);
                        return CreateInternal(dataService, bulkImportService);
                    }
                case SUPPORTED_DATABASES.ORACLE:
                    {
                        var dataService = new OracleDataService(_traceService);
                        var bulkImportService = new OracleBulkImportService(_traceService);
                        return CreateInternal(dataService, bulkImportService);
                    }
                default:
                    throw new NotSupportedException($"The target database platform {platform} is not supported or plugins location was not correctly configured. " +
                        $"See WIKI for supported database platforms and usage guide.");
            }
        }

        private IMigrationService CreateInternal(IDataService dataService, IBulkImportService bulkImportService)
        {
            var directoryService = new DirectoryService(_traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(_traceService, directoryService, fileService);
            var tokenReplacementService = new TokenReplacementService(_traceService);
            var metadataService = new MetadataService(dataService, _traceService, tokenReplacementService);
            var environmentService = new EnvironmentService();
            var configurationService = new ConfigurationService(environmentService, workspaceService, _traceService);

            var migrationService = new MigrationService(
                workspaceService,
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

