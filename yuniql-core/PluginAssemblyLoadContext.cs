using System;
using System.Reflection;
using System.Runtime.Loader;
using Yuniql.Extensibility;

namespace Yuniql.Core
{
    //https://github.com/dotnet/samples/tree/master/core/tutorials/Unloading
    // This is a collectible (unloadable) AssemblyLoadContext that loads the dependencies
    // of the plugin from the plugin's binary directory.
    public class PluginAssemblyLoadContext : AssemblyLoadContext
    {
        // Resolver of the locations of the assemblies that are dependencies of the
        // main plugin assembly.
        private AssemblyDependencyResolver _resolver;
        private readonly ITraceService _traceService;
        private AssemblyLoadContext _defaultLoadContext;

        public string PluginPath { get; set; }

        public PluginAssemblyLoadContext(
            AssemblyLoadContext defaultLoadContext,
            string pluginPath,
            ITraceService traceService) : base(isCollectible: true)
        {
            _defaultLoadContext = defaultLoadContext;
            PluginPath = pluginPath;
            this._traceService = traceService;
            this._resolver = new AssemblyDependencyResolver(pluginPath);
        }

        // The Load method override causes all the dependencies present in the plugin's binary directory to get loaded
        // into the HostAssemblyLoadContext together with the plugin assembly itself.
        // NOTE: The Interface assembly must not be present in the plugin's binary directory, otherwise we would
        // end up with the assembly being loaded twice. Once in the default context and once in the HostAssemblyLoadContext.
        //The types present on the host and plugin side would then not match even though they would have the same names.
        protected override Assembly Load(AssemblyName assemblyName)
        {
            if (null == assemblyName.Name) return null;

            try
            {
                var assembly = _defaultLoadContext.LoadFromAssemblyName(assemblyName);
                if (null != assembly)
                {
                    return assembly;
                }
            }
            catch (Exception ex)
            {
                string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
                _traceService.Debug($"Resolving dependency: {assemblyName.Name}, v{assemblyName.Version} from componentAssemblyPath: {PluginPath}");

                if (assemblyPath != null)
                {
                    _traceService.Debug($"Resolved dependency. Loading {assemblyPath} into the PluginAssemblyLoadContext");
                    return LoadFromAssemblyPath(assemblyPath);
                }
                else
                {
                    _traceService.Debug($"Failed resolving dependency: {assemblyName.Name}, v{assemblyName.Version}");
                }
            }

            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            _traceService.Debug($"Resolving unmanaged dependency: {unmanagedDllName} from componentAssemblyPath: {PluginPath}");

            if (libraryPath != null)
            {
                _traceService.Debug($"Resolved unmanaged dependency. Loading {libraryPath} into the PluginAssemblyLoadContext");
                return LoadUnmanagedDllFromPath(libraryPath);
            }

            return IntPtr.Zero;
        }
    }
}

