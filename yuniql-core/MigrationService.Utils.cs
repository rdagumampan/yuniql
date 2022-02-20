using Yuniql.Extensibility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;

namespace Yuniql.Core
{
    /// <inheritdoc />
    public partial class MigrationService : IMigrationService
    {
        /// <inheritdoc />
        public void Initialize()
        {
            _configurationService.Initialize();
            _configurationService.Validate();

            //override transaction model when target platform doesnt support full transactional ddl
            var configuration = _configurationService.GetConfiguration();
            if (!_dataService.IsTransactionalDdlSupported)
            {
                configuration.TransactionMode = TRANSACTION_MODE.NONE;
                _traceService.Warn($"Target platform does not support for full transactional DDL operations. All operations will be executed with transaction mode = statement. " +
                    $"When transaction mode is set to statement, each batch of sql statement does not participate in a shared transaction context. In the event of failure, the rollback attempt is limited to the individual batch of statement.");
            }

            _dataService.Initialize(configuration.ConnectionString);
            _bulkImportService.Initialize(configuration.ConnectionString);
        }

        /// <inheritdoc />
        public string GetCurrentVersion(string metaSchemaName = null, string metaTableName = null)
        {
            var configuration = _configurationService.GetConfiguration();
            if (!configuration.IsInitialized)
                Initialize();

            return _metadataService.GetCurrentVersion(metaSchemaName, metaTableName);
        }

        /// <inheritdoc />
        public List<DbVersion> GetAllVersions(string metaSchemaName = null, string metaTableName = null)
        {
            var configuration = _configurationService.GetConfiguration();
            if (!configuration.IsInitialized)
                Initialize();

            return _metadataService.GetAllVersions(metaSchemaName, metaTableName);
        }

        /// <inheritdoc />
        public bool IsTargetDatabaseLatest(string targetVersion, string metaSchemaName = null, string metaTableName = null)
        {
            var appliedVersions = _metadataService.GetAllAppliedVersions(metaSchemaName, metaTableName);
            if (!appliedVersions.Any())
                return false;

            var targetDatabaseIsLatest = appliedVersions.Exists(v => v.Version == targetVersion);
            return targetDatabaseIsLatest;
        }
    }
}
