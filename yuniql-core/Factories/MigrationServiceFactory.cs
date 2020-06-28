using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Loader;
using Yuniql.Extensibility;

namespace Yuniql.Core.Factories {
    ///<inheritdoc/>
    public class MigrationServiceFactory : IMigrationServiceFactory {

        private readonly Dictionary<string, MigrationServiceSourceAssemblies> _migrationServiceSourceAssemblies = new Dictionary<string, MigrationServiceSourceAssemblies>();

        private readonly ITraceService _traceService;

        ///<inheritdoc/>
        public MigrationServiceFactory(
            ITraceService traceService) {
            this._traceService = traceService;

            // TODO: Hard-coding the migration provider list for convenience but source definitions should be located in an external config file so that Yuniql.Core and 
            // Yuniql.CLI do not need to be re-compiled or updated when a new migration provider is supported.
            /* ex.
                <platforms>
                    <platform name="sqlserver">
                        <dataServiceSourceAssembly>Yuniql.SqlServer,Yuniql.SqlServer.SqlServerDataService</dataServiceSourceAssembly>
                        <bulkImportServieSourceAssembly>Yuniql.SqlServer,Yuniql.SqlServer.SqlServerBulkImportService</bulkImportServieSourceAssembly>
                        <configurationServiceSourceAssembly>Yuniql.Core,Yuniql.Core.ConfigurationDataService</configurationServiceSourceAssembly>
                    </platform>
                    <platform name="mysql">
                        etc...
                    </platform>
                </platforms>
             */

            _migrationServiceSourceAssemblies.Add(SUPPORTED_DATABASES.SQLSERVER, new MigrationServiceSourceAssemblies {
                DataServiceSourceAssembly = "Yuniql.SqlServer,Yuniql.SqlServer.SqlServerDataService",
                BulkImportServiceSourceAssembly = "Yuniql.SqlServer,Yuniql.SqlServer.SqlServerBulkImportService",
                ConfigurationServiceSourceAssembly = "Yuniql.Core,Yuniql.Core.ConfigurationDataService"
            });

             _migrationServiceSourceAssemblies.Add(SUPPORTED_DATABASES.POSTGRESQL, new MigrationServiceSourceAssemblies {
                DataServiceSourceAssembly = "Yuniql.PostgreSql,Yuniql.PostgreSql.PostgreSqlDataService",
                BulkImportServiceSourceAssembly = "Yuniql.PostgreSql,Yuniql.PostgreSql.PostgreSqlBulkImportService",
                ConfigurationServiceSourceAssembly = "Yuniql.Core,Yuniql.Core.ConfigurationDataService"
            });

        }

        ///<inheritdoc/>
        public IMigrationService Create(string platform, string pluginsPath = "") {
            if (!_migrationServiceSourceAssemblies.ContainsKey(platform.ToLower())) {
                throw new NotSupportedException($"The target database platform {platform} is not supported or plugins location was not correctly configured. " +
                        $"See WIKI for supported database platforms and usage guide.");
            }

            var migrationServiceSourceAssembly = _migrationServiceSourceAssemblies[platform.ToLower()];
            
            IDataService dataService = CreateTypeInstance<IDataService>(migrationServiceSourceAssembly.DataServiceSourceAssembly, pluginsPath, new object[] { _traceService });
            IBulkImportService bulkImportService = CreateTypeInstance<IBulkImportService>(migrationServiceSourceAssembly.BulkImportServiceSourceAssembly, pluginsPath, new object[] { _traceService });
            return dataService.IsAtomicDDLSupported
                            ? CreateTransactionalMigrationService(dataService, bulkImportService, pluginsPath, migrationServiceSourceAssembly.ConfigurationServiceSourceAssembly)
                            : CreateNonTransactionalMigrationService(dataService, bulkImportService, pluginsPath, migrationServiceSourceAssembly.ConfigurationServiceSourceAssembly);
        }

        private IMigrationService CreateTransactionalMigrationService(IDataService dataService, 
                                                                      IBulkImportService bulkImportService, 
                                                                      string pluginsPath,
                                                                      string configurationServiceSourceAssembly) 
        {
            var localVersionService = new LocalVersionService(_traceService);
            var tokenReplacementService = new TokenReplacementService(_traceService);
            var directoryService = new DirectoryService();
            var fileService = new FileService();

            var configurationService = CreateTypeInstance<IConfigurationDataService>(configurationServiceSourceAssembly, 
                                                                                     pluginsPath, 
                                                                                     new object[] { dataService, _traceService, tokenReplacementService });

            var migrationService = new MigrationService(
                localVersionService,
                dataService,
                bulkImportService,
                configurationService,
                tokenReplacementService,
                directoryService,
                fileService,
                _traceService);
            return migrationService;
        }

        private IMigrationService CreateNonTransactionalMigrationService(IDataService dataService, 
                                                                         IBulkImportService bulkImportService, 
                                                                         string pluginsPath,
                                                                         string configurationServiceSourceAssembly) {
            var localVersionService = new LocalVersionService(_traceService);
            var tokenReplacementService = new TokenReplacementService(_traceService);
            var directoryService = new DirectoryService();
            var fileService = new FileService();

            var configurationService = CreateTypeInstance<IConfigurationDataService>(configurationServiceSourceAssembly, 
                                                                                    pluginsPath, 
                                                                                    new object[] { dataService, _traceService, tokenReplacementService });

            var migrationService = new NonTransactionalMigrationService(
                localVersionService,
                dataService,
                bulkImportService,
                configurationService,
                tokenReplacementService,
                directoryService,
                fileService,
                _traceService);
            return migrationService;
        }

        private T CreateTypeInstance<T>(string typeInfo, string pluginsDirectory, params object[] ctorArgs) where T: class { 
            var dataServiceTypeInfo = new FullTypeNameEntry(typeInfo);
            var pluginAssemblyFilePath = $@"{pluginsDirectory}\{dataServiceTypeInfo.AssemblyName}.dll";
            var assemblyLoadContext = new PluginAssemblyLoadContext(AssemblyLoadContext.Default, pluginAssemblyFilePath, _traceService);
            var sourceAssembly = assemblyLoadContext.LoadFromAssemblyName(new AssemblyName(dataServiceTypeInfo.AssemblyName));
            return Activator.CreateInstance(sourceAssembly.GetType(dataServiceTypeInfo.TypeName), ctorArgs) as T;
        }
    }

    ///<inheritdoc/>
    internal struct MigrationServiceSourceAssemblies {
        ///<inheritdoc/>
        public string DataServiceSourceAssembly { get; set; }

        ///<inheritdoc/>
        public string BulkImportServiceSourceAssembly { get; set; }

        ///<inheritdoc/>
        public string ConfigurationServiceSourceAssembly { get; set; }

    }
}
