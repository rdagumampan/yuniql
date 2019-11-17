using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
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
            else if (platform.Equals("pgsql"))
            {
                var type = typeof(IDataService);
                var assembly = Assembly.LoadFrom(@"C:\play\yuniql\yuniql-plugins\postgresql\src\bin\Release\netcoreapp3.0\win-x64\publish\Yuniql.PostgreSql.dll");

                var dataService = assembly.GetTypes()
                    .Where(t=> t.Name.Contains("PostgreSqlDataService"))
                    .Select(t => Activator.CreateInstance(t, _traceService))
                    .Cast<IDataService>()
                    .First();

                var csvImportService = assembly.GetTypes()
                    .Where(t => t.Name.Contains("PostgreSqlCsvImportService"))
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

