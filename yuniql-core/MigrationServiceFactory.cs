using System;
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
                    var assemblyContext = new HostAssemblyLoadContext(assemblyBasePath);
                    var assembly = assemblyContext.LoadFromAssemblyPath(assemblyFile);

                    //var assembly = Assembly.LoadFrom(assemblyFile);
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

    // This is a collectible (unloadable) AssemblyLoadContext that loads the dependencies
    // of the plugin from the plugin's binary directory.
    class HostAssemblyLoadContext : AssemblyLoadContext
    {
        // Resolver of the locations of the assemblies that are dependencies of the
        // main plugin assembly.
        private AssemblyDependencyResolver _resolver;

        public HostAssemblyLoadContext(string pluginPath) : base(isCollectible: true)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
        }

        // The Load method override causes all the dependencies present in the plugin's binary directory to get loaded
        // into the HostAssemblyLoadContext together with the plugin assembly itself.
        // NOTE: The Interface assembly must not be present in the plugin's binary directory, otherwise we would
        // end up with the assembly being loaded twice. Once in the default context and once in the HostAssemblyLoadContext.
        // The types present on the host and plugin side would then not match even though they would have the same names.
        protected override Assembly Load(AssemblyName name)
        {
            string assemblyPath = _resolver.ResolveAssemblyToPath(name);
            if (assemblyPath != null)
            {
                Console.WriteLine($"Loading assembly {assemblyPath} into the HostAssemblyLoadContext");
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }
    }
}

