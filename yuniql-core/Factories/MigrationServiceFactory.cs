using System;
using System.Collections.Generic;
using Yuniql.Extensibility;

namespace Yuniql.Core.Factories {
    ///<inheritdoc/>
    public class MigrationServiceFactory : IMigrationServiceFactory {

        private readonly Dictionary<string, MigrationServiceSourceAssemblies> _migrationServiceSourceAssemblies = new Dictionary<string, MigrationServiceSourceAssemblies>();
        private readonly ITraceService _traceService;
        
        ///<inheritdoc/>
        public MigrationServiceFactory(
            ITraceService traceService)
        {
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
        }

        ///<inheritdoc/>
        public IMigrationService Create(string platform)
        {
            // No more need for a switch statement here, and also no need for references to supported platform assemblies (current and future) 
            // within YuniqlCore (which will result in circular references anyway).
            // After a new supported platform is implemented, only the config file needs to be updated and the new supported binaries deployed.

            if (!_migrationServiceSourceAssemblies.ContainsKey(platform.ToLower())) { 
                throw new NotSupportedException($"The target database platform {platform} is not supported or plugins location was not correctly configured. " +
                        $"See WIKI for supported database platforms and usage guide.");
            }

            var migrationServiceSourceAssembly = _migrationServiceSourceAssemblies[platform.ToLower()];
            var dataServiceTypeInfo = new FullTypeNameEntry(migrationServiceSourceAssembly.DataServiceSourceAssembly);
            var bulkImportServiceTypeInfo = new FullTypeNameEntry(migrationServiceSourceAssembly.BulkImportServiceSourceAssembly);
            var configurationServiceTypeInfo = new FullTypeNameEntry(migrationServiceSourceAssembly.ConfigurationServiceSourceAssembly);

            IDataService dataService = AssemblyTypeLoader.CreateInstance<IDataService>(dataServiceTypeInfo.AssemblyName, dataServiceTypeInfo.TypeName, new object[] { _traceService });
            IBulkImportService bulkImportService = AssemblyTypeLoader.CreateInstance<IBulkImportService>(bulkImportServiceTypeInfo.AssemblyName, bulkImportServiceTypeInfo.TypeName, new object[] { _traceService });
            
            return dataService.IsAtomicDDLSupported
                            ? CreateTransactionalMigrationService(dataService, bulkImportService, configurationServiceTypeInfo)
                            : CreateNonTransactionalMigrationService(dataService, bulkImportService, configurationServiceTypeInfo);
        }

        private IMigrationService CreateTransactionalMigrationService(IDataService dataService, IBulkImportService bulkImportService, FullTypeNameEntry configurationServiceTypeInfo)
        {
            var localVersionService = new LocalVersionService(_traceService);
            var tokenReplacementService = new TokenReplacementService(_traceService);
            var directoryService = new DirectoryService();
            var fileService = new FileService();

            IConfigurationDataService configurationService = AssemblyTypeLoader.CreateInstance<IConfigurationDataService>(configurationServiceTypeInfo.AssemblyName, configurationServiceTypeInfo.TypeName, new object[] {dataService,  _traceService, tokenReplacementService });

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

        private IMigrationService CreateNonTransactionalMigrationService(IDataService dataService, IBulkImportService bulkImportService, FullTypeNameEntry configurationServiceTypeInfo)
        {
            var localVersionService = new LocalVersionService(_traceService);
            var tokenReplacementService = new TokenReplacementService(_traceService);
            var directoryService = new DirectoryService();
            var fileService = new FileService();

            IConfigurationDataService configurationService = AssemblyTypeLoader.CreateInstance<IConfigurationDataService>(configurationServiceTypeInfo.AssemblyName, configurationServiceTypeInfo.TypeName, new object[] {dataService,  _traceService, tokenReplacementService });
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
    }

    ///<inheritdoc/>
    public struct MigrationServiceSourceAssemblies { 
        ///<inheritdoc/>
        public string DataServiceSourceAssembly { get; set; }
        
        ///<inheritdoc/>
        public string BulkImportServiceSourceAssembly { get; set; }

        ///<inheritdoc/>
        public string ConfigurationServiceSourceAssembly { get; set; }
    
    }
}
