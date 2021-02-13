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
        public void Destroy()
        {
            var configuration = _configurationService.GetConfiguration();
            if (!configuration.IsInitialized)
                Initialize();

            var tokens = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>(RESERVED_TOKENS.YUNIQL_DB_NAME, _dataService.GetConnectionInfo().Database) };
            var sqlStatementListRaw = _dataService.GetSqlForDropDatabase();

            using (var connection = _dataService.CreateMasterConnection())
            {
                connection.KeepOpen();
                sqlStatementListRaw.ForEach(sqlStatementRaw =>
                {
                    var sqlStatement = _tokenReplacementService.Replace(tokens, sqlStatementRaw);
                    connection.ExecuteNonQuery(
                            commandText: sqlStatement,
                            commandTimeout: configuration.CommandTimeout,
                            traceService: _traceService
                        );
                });
            }
        }
    }
}
