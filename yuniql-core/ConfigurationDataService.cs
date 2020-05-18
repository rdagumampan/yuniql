using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Yuniql.Extensibility;

namespace Yuniql.Core
{
    /// <summary>
    /// Service responsible for accessing target database configuration and executing sql statement batches.
    /// This facility is used by MigrationService and must be not be used directly. See <see cref="MigrationService"./>
    /// </summary>
    public class ConfigurationDataService : IConfigurationDataService
    {
        private readonly IDataService _dataService;
        private readonly ITraceService _traceService;

        /// <summary>
        /// Creates new instance of ConfigurationDataService
        /// </summary>
        /// <param name="dataService">An instance of implementation of <see cref="IDataService"/>. 
        /// Each database platform implements IDataService.</param>
        /// <param name="traceService">Trace service provider where trace messages will be written.</param>
        public ConfigurationDataService(
            IDataService dataService,
            ITraceService traceService)
        {
            this._dataService = dataService;
            this._traceService = traceService;
        }

        /// <summary>
        /// Returns true when database already exists in the target host.
        /// </summary>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        /// <returns>Returns true when database already exists in the target host.</returns>
        public bool IsDatabaseExists(int? commandTimeout = null)
        {
            var sqlStatement = string.Format(_dataService.GetSqlForCheckIfDatabaseExists(), _dataService.GetConnectionInfo().Database);
            using (var connection = _dataService.CreateMasterConnection())
            {
                return connection.QuerySingleBool(
                    commandText: sqlStatement,
                    commandTimeout: commandTimeout,
                    transaction: null,
                    traceService: _traceService);
            }
        }

        /// <summary>
        /// Creates the database
        /// </summary>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        public void CreateDatabase(int? commandTimeout = null)
        {
            var sqlStatement = string.Format(_dataService.GetSqlForCreateDatabase(), _dataService.GetConnectionInfo().Database);
            using (var connection = _dataService.CreateMasterConnection())
            {
                connection.ExecuteNonQuery(
                    commandText: sqlStatement,
                    commandTimeout: commandTimeout,
                    transaction: null,
                    traceService: _traceService);
            }
        }

        /// <summary>
        /// Returns true when migration version tracking table is already created.
        /// </summary>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        /// <returns>Returns true when version tracking table is already created.</returns>
        public bool IsDatabaseConfigured(int? commandTimeout = null)
        {
            var sqlStatement = string.Format(_dataService.GetSqlForCheckIfDatabaseConfigured(), _dataService.GetConnectionInfo().Database);
            using (var connection = _dataService.CreateConnection())
            {
                return connection.QuerySingleBool(
                    commandText: sqlStatement,
                    commandTimeout: commandTimeout,
                    transaction: null,
                    traceService: _traceService);
            }
        }

        /// <summary>
        /// Creates migration version tracking table in the target database.
        /// </summary>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        public void ConfigureDatabase(int? commandTimeout = null)
        {
            var sqlStatement = _dataService.GetSqlForConfigureDatabase();
            using (var connection = _dataService.CreateConnection())
            {
                connection.ExecuteNonQuery(
                    commandText: sqlStatement,
                    commandTimeout: commandTimeout,
                    transaction: null,
                    traceService: _traceService);
            }
        }

        /// <summary>
        /// Updates migration version tracking table in the target database..
        /// </summary>
        /// <returns>
        /// True if target database was updated, otherwise returns false
        /// </returns>
        public bool UpdateDatabaseConfiguration()
        {
            using (var connection = _dataService.CreateConnection())
            {
                connection.KeepOpen();
                return _dataService.UpdateDatabaseConfiguration(connection, _traceService);
            }
        }

        /// <summary>
        /// Returns the latest version applied in the target database.
        /// </summary>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        /// <returns>Returns the latest version applied in the target database.</returns>
        public string GetCurrentVersion(int? commandTimeout = null)
        {
            var sqlStatement = _dataService.GetSqlForGetCurrentVersion();
            using (var connection = _dataService.CreateConnection())
            {
                return connection.QuerySingleString(
                    commandText: sqlStatement,
                    commandTimeout: commandTimeout,
                    transaction: null,
                    traceService: _traceService);
            }
        }

        /// <summary>
        /// Returns all versions applied in the target database.
        /// </summary>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        /// <returns>All versions applied in the target database.</returns>
        public List<DbVersion> GetAllAppliedVersions(int? commandTimeout = null)
        {
            return this.GetAllVersions(commandTimeout).Where(x=>x.StatusId == StatusId.Succeeded).ToList();
        }

        /// <summary>
        /// Returns all versions in the target database.
        /// </summary>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        /// <returns>All versions in the target database.</returns>
        public List<DbVersion> GetAllVersions(int? commandTimeout = null)
        {
            var sqlStatement = _dataService.GetSqlForGetAllVersions();

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
                    DbVersion dbVersion;

                    if (_dataService.IsAtomicDDLSupported)
                    {
                        dbVersion = new DbVersion
                        {
                            SequenceId = reader.GetInt16(0),
                            Version = reader.GetString(1),
                            AppliedOnUtc = reader.GetDateTime(2),
                            AppliedByUser = reader.GetString(3)
                        };
                    }
                    else
                    {
                        dbVersion = new DbVersion
                        {
                            SequenceId = reader.GetInt16(0),
                            Version = reader.GetString(1),
                            AppliedOnUtc = reader.GetDateTime(2),
                            AppliedByUser = reader.GetString(3),
                            StatusId = (StatusId) reader.GetInt32(6),
                            FailedScriptPath = reader.GetValue(7) as string,
                            FailedScriptError = reader.GetValue(8) as string
                        };
                    }

                    result.Add(dbVersion);
                }
            }

            return result;
        }

        /// <summary>
        /// Creates new entry to version tracking table after all versions were successfully executed.
        /// </summary>
        /// <param name="connection">Connection to target database. Connection will be open automatically.</param>
        /// <param name="transaction">An active transaction.</param>
        /// <param name="version">Migration version.</param>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        /// <param name="appliedByTool">The applied by tool.</param>
        /// <param name="appliedByToolVersion">The applied by tool version.</param>
        /// <param name="failedScriptPath">The failed script path.</param>
        /// <param name="failedScriptError">The failed script error.</param>
        public void InsertVersion(
            IDbConnection connection,
            IDbTransaction transaction,
            string version,
            int? commandTimeout = null,
            string appliedByTool = null,
            string appliedByToolVersion = null,
            string failedScriptPath = null,
            string failedScriptError = null
            )
        {
            var toolName = string.IsNullOrEmpty(appliedByTool) ? "yuniql-nuget" : appliedByTool;
            var toolVersion = string.IsNullOrEmpty(appliedByToolVersion) ? this.GetType().Assembly.GetName().Version.ToString() : appliedByToolVersion;
            StatusId statusId = string.IsNullOrEmpty(failedScriptPath) ? StatusId.Succeeded : StatusId.Failed;

            IDbParameters dbParameters = null;
            string sqlStatement;

            //in case database supports non transactional flow
            if (_dataService is INonTransactionalFlow nonTransactionalDataService)
            {
                sqlStatement = string.Format(nonTransactionalDataService.GetSqlForUpsertVersion(), version, toolName, $"v{toolVersion}", (int) statusId);

                //Using of db parameters is important and a good practice, otherwise the errors containing SQL specific characters like "'" would need to be escaped in more complicated manner
                dbParameters = _dataService.CreateDbParameters();
                dbParameters.AddParameter("failedScriptPath", failedScriptPath);
                dbParameters.AddParameter("failedScriptError", failedScriptError);
            }
            else
            {
                if (statusId == StatusId.Failed)
                {
                    throw new NotSupportedException(@$"The non-transactional flow is not supported by this platform.");
                }

                sqlStatement = string.Format(_dataService.GetSqlForInsertVersion(), version, toolName, $"v{toolVersion}");
            }
            
            if (null != _traceService)
                _traceService.Debug($"Executing statement: {Environment.NewLine}{sqlStatement}");

            var command = connection
                .KeepOpen()
                .CreateCommand(
                commandText: sqlStatement,
                commandTimeout: commandTimeout,
                transaction: transaction
                );

            dbParameters?.CopyToDataParameterCollection(command.Parameters);

            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes sql statement to target database.
        /// </summary>
        /// <param name="connection">Connection to target database. Connection will be open automatically.</param>
        /// <param name="commandText">The sql statement.</param>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        /// <param name="transaction">An active transaction.</param>
        /// <param name="traceService">Trace service provider where trace messages will be written to.</param>
        /// <returns></returns>
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
