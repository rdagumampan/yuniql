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
                throw new System.NotSupportedException($"The target database platform {platform} is not yet supported. See WIKI for supported database platforms.");
            }
        }
    }

    public class PluginAssemblyLoadContext : AssemblyLoadContext
    {
        private List<Assembly> loadedAssemblies;
        private Dictionary<string, Assembly> sharedAssemblies;

        private string path;

        public PluginAssemblyLoadContext(string path, params Type[] sharedTypes)
        {
            this.path = path;

            this.loadedAssemblies = new List<Assembly>();
            this.sharedAssemblies = new Dictionary<string, Assembly>();

            foreach (Type sharedType in sharedTypes)
                sharedAssemblies[Path.GetFileName(sharedType.Assembly.Location)] = sharedType.Assembly;
        }

        public void Initialize()
        {
            foreach (string dll in Directory.EnumerateFiles(path, "*.dll"))
            {
                if (sharedAssemblies.ContainsKey(Path.GetFileName(dll)))
                    continue;

                loadedAssemblies.Add(this.LoadFromAssemblyPath(dll));
            }
        }

        public IEnumerable<T> GetImplementations<T>()
        {
            return loadedAssemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(T).IsAssignableFrom(t))
                .Select(t => Activator.CreateInstance(t))
                .Cast<T>();
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            string filename = $"{assemblyName.Name}.dll";
            if (sharedAssemblies.ContainsKey(filename))
                return sharedAssemblies[filename];

            return Assembly.Load(assemblyName);
        }
    }
}

