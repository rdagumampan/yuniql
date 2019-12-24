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
            var platformLowerCased = platform.ToLower();
            _traceService.Debug($"{this.GetType().Name}/platformLowerCased: {platformLowerCased}");

            if (string.IsNullOrEmpty(platformLowerCased) || platformLowerCased.Equals("sqlserver"))
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
                _traceService.Debug($"{this.GetType().Name}/defaultPluginsBasePath: {defaultPluginsBasePath}");

                var defaultAssemblyBasePath = Path.Combine(Environment.CurrentDirectory, ".plugins", platformLowerCased);
                _traceService.Debug($"{this.GetType().Name}/defaultAssemblyBasePath: {defaultAssemblyBasePath}");

                var environmentVariableAssemblyBasePath = _environmentService.GetEnvironmentVariable("YUNIQL_PLUGINS");
                _traceService.Debug($"{this.GetType().Name}/environmentVariableAssemblyBasePath: {environmentVariableAssemblyBasePath}");

                var assemblyBasePath = string.IsNullOrEmpty(environmentVariableAssemblyBasePath) ? defaultAssemblyBasePath : environmentVariableAssemblyBasePath;
                _traceService.Debug($"{this.GetType().Name}/assemblyBasePath: {assemblyBasePath}");

                //TODO: Use DirectoryService and FileService to find case-insensitive filename
                var directoryService = new DirectoryService();
                var fileService = new FileService();

                var directories = directoryService.GetDirectories(defaultPluginsBasePath, "*", SearchOption.AllDirectories).ToList();
                directories.ForEach(d =>
                {
                    _traceService.Debug($"{this.GetType().Name}/Found plugin dir: {d}");

                    var files = directoryService.GetFiles(d, "*.*").ToList();
                    files.ForEach(f =>
                    {
                        _traceService.Debug($"{this.GetType().Name}/Found plugin file: {f}. ProductVersion: {FileVersionInfo.GetVersionInfo(f).ProductVersion}, FileVersion: {FileVersionInfo.GetVersionInfo(f).FileVersion}");
                    });
                });

                var assemblyFilePath = directoryService.GetFiles(assemblyBasePath, "*.dll")
                    .ToList()
                    .First(f => new FileInfo(f).Name.ToLower() == $"yuniql.{platformLowerCased}.dll");
                _traceService.Debug($"{this.GetType().Name}/assemblyFilePath: {assemblyFilePath}");

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
                            _traceService.Debug($"{this.GetType().Name}/loadedAssembly: {a.FullName}");
                        });
                    assemblyContext.Resolving += AssemblyContext_Resolving;
                    assemblyContext.Unloading += AssemblyContext_Unloading;

                    var sqlDataService = assembly.GetTypes()
                        .Where(t => t.Name.ToLower().Contains($"{platformLowerCased.ToLower()}dataservice"))
                        .Select(t => Activator.CreateInstance(t, _traceService))
                        .Cast<IDataService>()
                        .First();

                    var bulkImportService = assembly.GetTypes()
                        .Where(t => t.Name.ToLower().Contains($"{platformLowerCased.ToLower()}bulkimportservice"))
                        .Select(t => Activator.CreateInstance(t, _traceService))
                        .Cast<IBulkImportService>()
                        .First();

                    var tokenReplacementService = new TokenReplacementService(_traceService);

                    var migrationService = new MigrationService(sqlDataService, bulkImportService, tokenReplacementService, directoryService, fileService, _traceService);
                    return migrationService;
                }
                else
                {
                    throw new NotSupportedException($"The target database platform {platformLowerCased} is not supported. " +
                        $"See WIKI for supported database platforms and how to configure plugins for non-sqlserver databases.");
                }
            }
        }

        private void AssemblyContext_Unloading(System.Runtime.Loader.AssemblyLoadContext obj)
        {
            _traceService.Debug($"{this.GetType().Name}/unloading: {obj.Name}");
        }

        private System.Reflection.Assembly AssemblyContext_Resolving(System.Runtime.Loader.AssemblyLoadContext assemblyContext, System.Reflection.AssemblyName failedAssembly)
        {
            var assemblyLoadContext = assemblyContext as PluginAssemblyLoadContext;
            _traceService.Debug($"{this.GetType().Name}/retryResolving: {failedAssembly.FullName}");
            var assemblyFilePath = Path.Combine(assemblyLoadContext.PluginPath, failedAssembly.Name + ".dll");

            _traceService.Debug($"{this.GetType().Name}/failedAssemblyFileExists: {File.Exists(assemblyFilePath)}");
            _traceService.Debug($"{this.GetType().Name}/attempting to reload {failedAssembly.FullName} from {assemblyFilePath}");
            assemblyContext.LoadFromAssemblyPath(assemblyFilePath);
            //assemblyContext.LoadFromAssemblyName(new System.Reflection.AssemblyName(failedAssembly.Name));

            return null;
        }
    }
}

