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
                var csvImportService = new SqlServerBulkImportService(_traceService);

                var migrationService = new MigrationService(dataService, csvImportService, _traceService);
                return migrationService;
            }
            {
                //extracts plugins and creates required services
                var assemblyFile = Path.Combine(Environment.CurrentDirectory, ".plugins", platform, $"Yuniql.{platform}.dll");
                if (File.Exists(assemblyFile))
                {
                    var assembly = Assembly.LoadFrom(assemblyFile);
                    var dataService = assembly.GetTypes()
                        .Where(t => t.Name.ToLower().Contains($"{platform}dataservice"))
                        .Select(t => Activator.CreateInstance(t, _traceService))
                        .Cast<IDataService>()
                        .First();

                    var csvImportService = assembly.GetTypes()
                        .Where(t => t.Name.ToLower().Contains($"{platform}bulkimportservice"))
                        .Select(t => Activator.CreateInstance(t, _traceService))
                        .Cast<IBulkImportService>()
                        .First();

                    var migrationService = new MigrationService(dataService, csvImportService, _traceService);
                    return migrationService;
                }
                else
                {
                    throw new NotSupportedException($"The target database platform {platform} is not yet supported. See WIKI for supported database platforms.");
                }
            }
        }
    }
}

