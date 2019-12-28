using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Yuniql.Extensibility;
using Yuniql.SqlServer;

namespace Yuniql.Core
{
    //https://docs.microsoft.com/en-us/dotnet/core/dependency-loading/loading-managed
    public class MigrationServiceFactory : IMigrationServiceFactory
    {
        private readonly ITraceService _traceService;

        public MigrationServiceFactory(
            ITraceService traceService)
        {
            this._traceService = traceService;
        }

        // It is important to mark this method as NoInlining, otherwise the JIT could decide
        // to inline it into the Main method. That could then prevent successful unloading
        // of the plugin because some of the MethodInfo / Type / Plugin.Interface / HostAssemblyLoadContext
        // instances may get lifetime extended beyond the point when the plugin is expected to be
        // unloaded.
        [MethodImpl(MethodImplOptions.NoInlining)]
        public IMigrationService Create(string platform, string pluginsPath = null)
        {
            var platformLowerCased = platform.ToLower();
            _traceService.Debug($"platformLowerCased: {platformLowerCased}");

            //sqlserver is built-in with yuniql, works out of the box
            if (string.IsNullOrEmpty(platformLowerCased) || platformLowerCased.Equals("sqlserver"))
            {
                var sqlDataService = new SqlServerDataService(_traceService);
                var bulkImportService = new SqlServerBulkImportService(_traceService);

                var localVersionService = new LocalVersionService(_traceService);
                var tokenReplacementService = new TokenReplacementService(_traceService);
                var directoryService = new DirectoryService();
                var fileService = new FileService();

                var migrationService = new MigrationService(localVersionService, sqlDataService, bulkImportService, tokenReplacementService, directoryService, fileService, _traceService);
                return migrationService;
            }
            //other platforms like postgresql, mysql and others are delivered as plugins
            //by implementing Yuniql.Extensibility interfaces and placing other plugins directory
            else
            {
                //we picked the plugins location based on these order
                //parameter-based -> environment variable -> default from .exe assemply path
                var defaultPluginsBasePath = pluginsPath;
                _traceService.Debug($"pluginsPathParameter: {pluginsPath}");

                if (string.IsNullOrEmpty(defaultPluginsBasePath))
                {
                    var environmentService = new EnvironmentService();
                    defaultPluginsBasePath = environmentService.GetEnvironmentVariable("YUNIQL_PLUGINS");
                    _traceService.Debug($"pluginsPathEnvVariable: {defaultPluginsBasePath}");
                }
                else if (string.IsNullOrEmpty(defaultPluginsBasePath))
                {
                    defaultPluginsBasePath = Path.Combine(Environment.CurrentDirectory, ".plugins");
                    _traceService.Debug($"pluginsPathEnvCurrentDirectory: {defaultPluginsBasePath}");
                }

                //TODO: Use DirectoryService and FileService to find case-insensitive filename
                var directoryService = new DirectoryService();
                var fileService = new FileService();

                //check if the base plugin directory exists
                if (!directoryService.Exists(defaultPluginsBasePath))
                {
                    throw new NotSupportedException($"The target database platform {platformLowerCased} is not supported or plugins location was not correctly configured. " +
                        $"The plugins location is set to : {defaultPluginsBasePath}" +
                        $"See WIKI for supported database platforms and how to configure plugins for non-sqlserver databases.");
                }

                //extracts plugins and creates required services
                var pluginAssemblyBasePath = Path.Combine(defaultPluginsBasePath, platformLowerCased);
                _traceService.Debug($"defaultAssemblyBasePath: {pluginAssemblyBasePath}");

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

                //check if the base plugin directory for selected platform exists
                if (!directoryService.Exists(pluginAssemblyBasePath))
                {
                    throw new NotSupportedException($"The target database platform {platformLowerCased} is not supported or plugins location was not correctly configured. " +
                        $"The plugins location is set to : {pluginAssemblyBasePath}" +
                        $"See WIKI for supported database platforms and how to configure plugins for non-sqlserver databases.");
                }

                var pluginAssemblyFilePath = directoryService.GetFileCaseInsensitive(pluginAssemblyBasePath, $"yuniql.{platformLowerCased}.dll");
                _traceService.Debug($"pluginAssemblyFilePath: {pluginAssemblyFilePath}");

                //check if the plugin assembly file for selected platform exists
                if (fileService.Exists(pluginAssemblyFilePath))
                {
                    // create the unloadable HostAssemblyLoadContext
                    var pluginAssemblyLoadContext = new PluginAssemblyLoadContext(pluginAssemblyBasePath, _traceService);

                    //the plugin assembly into the HostAssemblyLoadContext. 
                    //the assemblyPath must be an absolute path.
                    var assembly = pluginAssemblyLoadContext.LoadFromAssemblyPath(pluginAssemblyFilePath);
                    pluginAssemblyLoadContext.Assemblies
                        .ToList()
                        .ForEach(a =>
                        {
                            _traceService.Debug($"loadedAssembly: {a.FullName}");
                        });
                    pluginAssemblyLoadContext.Resolving += AssemblyContext_Resolving;
                    pluginAssemblyLoadContext.Unloading += AssemblyContext_Unloading;

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

                    var localVersionService = new LocalVersionService(_traceService);
                    var tokenReplacementService = new TokenReplacementService(_traceService);

                    var migrationService = new MigrationService(localVersionService, sqlDataService, bulkImportService, tokenReplacementService, directoryService, fileService, _traceService);
                    return migrationService;
                }
                else
                {
                    throw new NotSupportedException($"The target database platform {platformLowerCased} is not supported or plugins location was not correctly configured. " +
                        $"The plugin assembly file location is set to : {pluginAssemblyFilePath}" +
                        $"See WIKI for supported database platforms and how to configure plugins for non-sqlserver databases.");
                }
            }
        }

        private void AssemblyContext_Unloading(AssemblyLoadContext obj)
        {
            _traceService.Debug($"unloading: {obj.Name}");
        }

        private Assembly AssemblyContext_Resolving(AssemblyLoadContext assemblyContext, AssemblyName failedAssembly)
        {
            var pluginAssemblyLoadContext = assemblyContext as PluginAssemblyLoadContext;
            _traceService.Debug($"retryResolving: {failedAssembly.FullName}");

            var assemblyFilePath = Path.Combine(pluginAssemblyLoadContext.PluginPath, failedAssembly.Name + ".dll");
            _traceService.Debug($"failedAssemblyFileExists: {File.Exists(assemblyFilePath)}");

            using (var file = File.Open(assemblyFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                _traceService.Debug($"attempting to reload {failedAssembly.FullName} via streaming from {assemblyFilePath}");
                pluginAssemblyLoadContext.LoadFromStream(file);
            }

            //assemblyContext.LoadFromAssemblyPath(assemblyFilePath);
            //assemblyContext.LoadFromAssemblyName(new System.Reflection.AssemblyName(failedAssembly.Name));

            return null;
        }
    }
}

