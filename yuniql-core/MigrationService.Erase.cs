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
        public void Erase()
        {
            var configuration = _configurationService.GetConfiguration();
            if (!configuration.IsInitialized)
                Initialize();

            //create a shared open connection to entire migration run
            using (var connection = _dataService.CreateConnection())
            {
                connection.KeepOpen();

                //enclose all executions in a single transaction in case platform supports it
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        //runs all scripts in the _erase folder
                        RunNonVersionDirectories(connection, transaction, configuration.Workspace, Path.Combine(configuration.Workspace, RESERVED_DIRECTORY_NAME.ERASE), tokens: configuration.Tokens, bulkSeparator: DEFAULT_CONSTANTS.BULK_SEPARATOR, commandTimeout: configuration.CommandTimeout, environment: configuration.Environment);
                        _traceService.Info($"Executed script files on {Path.Combine(configuration.Workspace, RESERVED_DIRECTORY_NAME.ERASE)}");

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
