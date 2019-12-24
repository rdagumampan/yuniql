using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Yuniql.Extensibility;
using Yuniql.SqlServer;

namespace Yuniql.Core
{
    //https://docs.microsoft.com/en-us/dotnet/core/dependency-loading/loading-managed
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

        // It is important to mark this method as NoInlining, otherwise the JIT could decide
        // to inline it into the Main method. That could then prevent successful unloading
        // of the plugin because some of the MethodInfo / Type / Plugin.Interface / HostAssemblyLoadContext
        // instances may get lifetime extended beyond the point when the plugin is expected to be
        // unloaded.
        [MethodImpl(MethodImplOptions.NoInlining)]
        public IMigrationService Create(string platform)
        {
            _traceService.Debug($"platform: {platform}");

            if (string.IsNullOrEmpty(platform) || platform.Equals("sqlserver"))
            {
                var sqlDataService = new SqlServerDataService(_traceService);
                var bulkImportService = new SqlServerBulkImportService(_traceService);
                var tokenReplacementService = new TokenReplacementService(_traceService);
                var directoryService = new DirectoryService();
                var fileService = new FileService();

                var migrationService = new MigrationService(sqlDataService, bulkImportService, tokenReplacementService, directoryService, fileService, _traceService);
                return migrationService;
            }
            {
                //extracts plugins and creates required services
                var defaultPluginsBasePath = Path.Combine(Environment.CurrentDirectory, ".plugins");
                _traceService.Debug($"defaultPluginsBasePath: {defaultPluginsBasePath}");

                var defaultAssemblyBasePath = Path.Combine(Environment.CurrentDirectory, ".plugins", platform);
                _traceService.Debug($"defaultAssemblyBasePath: {defaultAssemblyBasePath}");

                var environmentVariableAssemblyBasePath = _environmentService.GetEnvironmentVariable("YUNIQL_PLUGINS");
                _traceService.Debug($"environmentVariableAssemblyBasePath: {environmentVariableAssemblyBasePath}");

                var assemblyBasePath = string.IsNullOrEmpty(environmentVariableAssemblyBasePath) ? defaultAssemblyBasePath : environmentVariableAssemblyBasePath;
                _traceService.Debug($"assemblyBasePath: {assemblyBasePath}");

                var assemblyFilePath = Path.Combine(assemblyBasePath, $"Yuniql.{platform}.dll");
                _traceService.Debug($"assemblyFilePath: {assemblyFilePath}");

                //TODO: Use DirectoryService and FileService to find case-insensitive filename
                var directoryService = new DirectoryService();
                var fileService = new FileService();

                var directories = directoryService.GetDirectories(defaultPluginsBasePath, "*", SearchOption.AllDirectories).ToList();
                directories.ForEach(d =>
                {
                    _traceService.Debug($"Found plugin dir: {d}");

                    var files = directoryService.GetFiles(d, "*.*").ToList();
                    files.ForEach(f =>
                    {
                        _traceService.Debug($"Found plugin file: {f}. ProductVersion: {FileVersionInfo.GetVersionInfo(f).ProductVersion}, FileVersion: {FileVersionInfo.GetVersionInfo(f).FileVersion}");
                    });
                });

                if (File.Exists(assemblyFilePath))
                {
                    // create the unloadable HostAssemblyLoadContext
                    var assemblyContext = new PluginAssemblyLoadContext(assemblyBasePath);

                    //the plugin assembly into the HostAssemblyLoadContext. 
                    //the assemblyPath must be an absolute path.
                    var assembly = assemblyContext.LoadFromAssemblyPath(assemblyFilePath);
                    assemblyContext.Assemblies
                        .ToList()
                        .ForEach(a =>
                        {
                            _traceService.Debug($"loadedAssembly: {a.FullName}");
                        });
                    assemblyContext.Resolving += AssemblyContext_Resolving;
                    assemblyContext.Unloading += AssemblyContext_Unloading;

                    var sqlDataService = assembly.GetTypes()
                        .Where(t => t.Name.ToLower().Contains($"{platform.ToLower()}dataservice"))
                        .Select(t => Activator.CreateInstance(t, _traceService))
                        .Cast<IDataService>()
                        .First();

                    var bulkImportService = assembly.GetTypes()
                        .Where(t => t.Name.ToLower().Contains($"{platform.ToLower()}bulkimportservice"))
                        .Select(t => Activator.CreateInstance(t, _traceService))
                        .Cast<IBulkImportService>()
                        .First();

                    var tokenReplacementService = new TokenReplacementService(_traceService);

                    var migrationService = new MigrationService(sqlDataService, bulkImportService, tokenReplacementService, directoryService, fileService, _traceService);
                    return migrationService;
                }
                else
                {
                    throw new NotSupportedException($"The target database platform {platform} is not supported. " +
                        $"See WIKI for supported database platforms and how to configure plugins for non-sqlserver databases.");
                }
            }
        }

        private void AssemblyContext_Unloading(System.Runtime.Loader.AssemblyLoadContext obj)
        {
            _traceService.Debug($"unloading: {obj.Name}");
        }

        private System.Reflection.Assembly AssemblyContext_Resolving(System.Runtime.Loader.AssemblyLoadContext assemblyContext, System.Reflection.AssemblyName failedAssembly)
        {
            _traceService.Debug($"retryResolving: {failedAssembly.FullName}");

            var defaultAssemblyBasePath = Path.Combine(Environment.CurrentDirectory, ".plugins", "PostgreSql");
            var assemblyFilePath = Path.Combine(defaultAssemblyBasePath, failedAssembly.Name + ".dll");

            _traceService.Debug($"failedAssemblyFileExists: {File.Exists(assemblyFilePath)}");
            _traceService.Debug($"attempting to reload {failedAssembly.FullName} from {assemblyFilePath}");
            assemblyContext.LoadFromAssemblyPath(assemblyFilePath);
            //assemblyContext.LoadFromAssemblyName(new System.Reflection.AssemblyName(failedAssembly.Name));

            return null;
        }
    }
}

