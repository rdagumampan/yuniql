using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Yuniql.Extensibility;
using Yuniql.SqlServer;

namespace Yuniql.Core
{
    public class MigrationServiceFactory : IMigrationServiceFactory
    {
        private readonly IEnvironmentService _environmentService;
        private readonly ITraceService _traceService;

        public MigrationServiceFactory(
            IEnvironmentService environmentService,
            ITraceService traceService)
        {
            this._environmentService = environmentService;
            this._traceService = traceService;
        }

        public IMigrationService Create(string platform)
        {
            if (string.IsNullOrEmpty(platform) || platform.Equals("sqlserver"))
            {
                var sqlDataService = new SqlServerDataService(_traceService);
                var bulkImportService = new SqlServerBulkImportService(_traceService);

                var migrationService = new MigrationService(sqlDataService, bulkImportService, _traceService);
                return migrationService;
            }
            {
                //extracts plugins and creates required services
                var assemblyFile = Path.Combine(Environment.CurrentDirectory, ".plugins", platform, $"Yuniql.{platform}.dll");
                var assemblyBasePath = _environmentService.GetEnvironmentVariable("YUNIQL_PLUGINS");
                if (!string.IsNullOrEmpty(assemblyBasePath))
                {
                    assemblyFile = Path.Combine(assemblyBasePath, $"Yuniql.{platform}.dll");
                }

                if (File.Exists(assemblyFile))
                {
                    var assembly = Assembly.LoadFrom(assemblyFile);
                    var sqlDataService = assembly.GetTypes()
                        .Where(t => t.Name.ToLower().Contains($"{platform}dataservice"))
                        .Select(t => Activator.CreateInstance(t, _traceService))
                        .Cast<IDataService>()
                        .First();

                    var bulkImportService = assembly.GetTypes()
                        .Where(t => t.Name.ToLower().Contains($"{platform}bulkimportservice"))
                        .Select(t => Activator.CreateInstance(t, _traceService))
                        .Cast<IBulkImportService>()
                        .First();

                    var migrationService = new MigrationService(sqlDataService, bulkImportService, _traceService);
                    return migrationService;
                }
                else
                {
                    throw new NotSupportedException($"The target database platform {platform} is not yet supported. See WIKI for supported database platforms and how to configure plugins for non-sqlserver databases.");
                }
            }
        }
    }
}

