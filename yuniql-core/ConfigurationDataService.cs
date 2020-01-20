using System;
using System.Collections.Generic;
using System.Data;
using Yuniql.Extensibility;

namespace Yuniql.Core
{
    public class ConfigurationDataService : IConfigurationDataService
    {
        private readonly IDataService _dataService;
        private readonly ITraceService _traceService;

        public ConfigurationDataService(
            IDataService dataService,
            ITraceService traceService)
        {
            this._dataService = dataService;
            this._traceService = traceService;
        }

        public bool IsDatabaseExists(int? commandTimeout = null)
        {
            var sqlStatement = string.Format(_dataService.GetCheckIfDatabaseExistsSql(), _dataService.GetConnectionInfo().Database);
            using (var connection = _dataService.CreateMasterConnection())
            {
                return connection.QuerySingleBool(
                    commandText: sqlStatement,
                    commandTimeout: commandTimeout,
                    transaction: null,
                    traceService: _traceService);
            }
        }

        public void CreateDatabase(int? commandTimeout = null)
        {
            var sqlStatement = string.Format(_dataService.GetCreateDatabaseSql(), _dataService.GetConnectionInfo().Database);
            using (var connection = _dataService.CreateMasterConnection())
            {
                connection.ExecuteNonQuery(
                    commandText: sqlStatement,
                    commandTimeout: commandTimeout,
                    transaction: null,
                    traceService: _traceService);
            }
        }

        public bool IsDatabaseConfigured(int? commandTimeout = null)
        {
            var sqlStatement = _dataService.GetCheckIfDatabaseConfiguredSql();
            using (var connection = _dataService.CreateConnection())
            {
                return connection.QuerySingleBool(
                    commandText: sqlStatement,
                    commandTimeout: commandTimeout,
                    transaction: null,
                    traceService: _traceService);
            }
        }

        public void ConfigureDatabase(int? commandTimeout = null)
        {
            var sqlStatement = _dataService.GetConfigureDatabaseSql();
            using (var connection = _dataService.CreateConnection())
            {
                connection.ExecuteNonQuery(
                    commandText: sqlStatement,
                    commandTimeout: commandTimeout,
                    transaction: null,
                    traceService: _traceService);
            }
        }

        public string GetCurrentVersion(int? commandTimeout = null)
        {
            var sqlStatement = _dataService.GetGetCurrentVersionSql();
            using (var connection = _dataService.CreateConnection())
            {
                return connection.QuerySingleString(
                    commandText: sqlStatement,
                    commandTimeout: commandTimeout,
                    transaction: null,
                    traceService: _traceService);
            }
        }

        public List<DbVersion> GetAllVersions(int? commandTimeout = null)
        {
            var sqlStatement = _dataService.GetGetAllVersionsSql();

            if (null != _traceService)
                _traceService.Debug($"Executing statement: {Environment.NewLine}{sqlStatement}");

            var result = new List<DbVersion>();
            using (var connection = _dataService.CreateConnection().KeepOpen())
            {
                var command = connection.CreateCommand(
                    commandText: sqlStatement,
                    commandTimeout: commandTimeout,
                    transaction: null);

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var dbVersion = new DbVersion
                    {
                        Id = reader.GetInt16(0),
                        Version = reader.GetString(1),
                        DateInsertedUtc = reader.GetDateTime(2),
                        LastUserId = reader.GetString(3)
                    };
                    result.Add(dbVersion);
                }
            }

            return result;
        }

        public void UpdateVersion(
            IDbConnection connection,
            IDbTransaction transaction,
            string version,
            int? commandTimeout = null)
        {
            var sqlStatement = string.Format(_dataService.GetUpdateVersionSql(), version);

            if (null != _traceService)
                _traceService.Debug($"Executing statement: {Environment.NewLine}{sqlStatement}");

            var command = connection
                .KeepOpen()
                .CreateCommand(
                commandText: sqlStatement,
                commandTimeout: commandTimeout,
                transaction: transaction);
            command.ExecuteNonQuery();
        }

        public int ExecuteSql(
            IDbConnection connection,
            string commandText,
            int? commandTimeout = null,
            IDbTransaction transaction = null,
            ITraceService traceService = null)
        {
            return connection.ExecuteNonQuery(
                commandText: commandText,
                transaction: transaction,
                commandTimeout: commandTimeout,
                traceService: _traceService);

        }
    }
}
