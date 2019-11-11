using ArdiLabs.Yuniql.Extensibility;
using ArdiLabs.Yuniql.SqlServer;

namespace ArdiLabs.Yuniql.Core
{
    public class MigrationServiceFactory : IMigrationServiceFactory
    {
        private readonly ITraceService _traceService;

        public MigrationServiceFactory(ITraceService traceService)
        {
            this._traceService = traceService;
        }

        public IMigrationService Create(string platform)
        {
            if (platform.Equals("sqlserver"))
            {
                var dataService = new SqlServerDataService(_traceService);
                var csvImportService = new SqlServerCsvImportService(_traceService);

                var migrationService = new MigrationService(dataService, csvImportService, _traceService);
                return migrationService;
            }
            else
            {
                throw new System.NotSupportedException($"The target database platform {platform} is not yet supported. See WIKI for supported database platforms.");
            }
        }
    }
}
