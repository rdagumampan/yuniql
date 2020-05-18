using System.Collections.Generic;
using System.Data;
using Yuniql.Extensibility;
using MySql.Data.MySqlClient;
using System.Linq;
using System;

namespace Yuniql.MySql
{
    public class MySqlDataService : IDataService, INonTransactionalFlow
    {
        private string _connectionString;
        private readonly ITraceService _traceService;

        public MySqlDataService(ITraceService traceService)
        {
            this._traceService = traceService;
        }

        public void Initialize(string connectionString)
        {
            this._connectionString = connectionString;
        }

        public bool IsAtomicDDLSupported => false;

        public bool IsSchemaSupported { get; } = false;

        public IDbConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        public IDbConnection CreateMasterConnection()
        {
            var masterConnectionStringBuilder = new MySqlConnectionStringBuilder(_connectionString);
            masterConnectionStringBuilder.Database = "INFORMATION_SCHEMA";

            return new MySqlConnection(masterConnectionStringBuilder.ConnectionString);
        }

        /// <summary>
        /// Creates empty Db parameters.
        /// </summary>
        /// <returns></returns>
        public IDbParameters CreateDbParameters()
        {
            return new MySqlParameters();
        }

        public List<string> BreakStatements(string sqlStatementRaw)
        {
            return new List<string> { sqlStatementRaw };
        }

        public ConnectionInfo GetConnectionInfo()
        {
            var connectionStringBuilder = new MySqlConnectionStringBuilder(_connectionString);
            return new ConnectionInfo { DataSource = connectionStringBuilder.Server, Database = connectionStringBuilder.Database };
        }

        public string GetSqlForCheckIfDatabaseExists()
            => @"SELECT 1 FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{0}';";

        public string GetSqlForCreateDatabase()
            => @"CREATE DATABASE `{0}`;";

        public string GetSqlForCheckIfDatabaseConfigured()
            => @"SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '__yuniqldbversion' LIMIT 1;";

        public string GetSqlForConfigureDatabase()
            => @"
                CREATE TABLE __yuniqldbversion (
	                sequence_id INT AUTO_INCREMENT PRIMARY KEY NOT NULL,
	                version VARCHAR(190) NOT NULL,
	                applied_on_utc TIMESTAMP NOT NULL,
	                applied_by_user VARCHAR(32) NOT NULL,
	                applied_by_tool VARCHAR(32) NULL,
	                applied_by_tool_version VARCHAR(16) NULL,
	                additional_artifacts BLOB NULL,
                    status_id INT NOT NULL DEFAULT 1 COMMENT '1 - Succeeded, 2 - Failed',
                    failed_script_path VARCHAR(4000) NULL,
                    failed_script_error VARCHAR(4000) NULL,
	                CONSTRAINT ix___yuniqldbversion UNIQUE (version)
                ) ENGINE=InnoDB;
            ";

        public string GetSqlForGetCurrentVersion()
            => @"SELECT version FROM __yuniqldbversion WHERE status_id = 1 ORDER BY sequence_id DESC LIMIT 1;";

        public string GetSqlForGetAllVersions()
            => @"SELECT sequence_id, version, applied_on_utc, applied_by_user, applied_by_tool, applied_by_tool_version, status_id, failed_script_path, failed_script_error FROM __yuniqldbversion ORDER BY version ASC;";

        public string GetSqlForInsertVersion()
    => throw new NotSupportedException("Not supported for current target platform");

        public string GetSqlForUpsertVersion()
    => @"INSERT INTO __yuniqldbversion (version, applied_on_utc, applied_by_user, applied_by_tool, applied_by_tool_version, status_id, failed_script_path, failed_script_error) VALUES ('{0}', UTC_TIMESTAMP(), CURRENT_USER(), '{1}', '{2}', '{3}', @failedScriptPath, @failedScriptError)
ON DUPLICATE KEY UPDATE
applied_on_utc = VALUES(applied_on_utc),
applied_by_user = VALUES(applied_by_user),
applied_by_tool = VALUES(applied_by_tool),
applied_by_tool_version = VALUES(applied_by_tool_version),
status_id = VALUES(status_id),
failed_script_path = VALUES(failed_script_path),
failed_script_error = VALUES(failed_script_error);";

        /// <summary>
        /// Updates the database migration tracking table.
        /// </summary>
        /// <param name="dbConnection">The database connection.</param>
        /// <param name="traceService">The trace service.</param>
        /// <returns>
        /// True if target database was updated, otherwise returns false
        /// </returns>
        public bool UpdateDatabaseConfiguration(IDbConnection dbConnection, ITraceService traceService = null)
        {
            DataTable columnsTable = GetVersionTableColumns(dbConnection, traceService);

            var columnsTableRows = columnsTable.Rows.Cast<DataRow>().Select(x => new { ColumnName = x.Field<string>("COLUMN_NAME"), ColumnType = x.Field<string>("COLUMN_TYPE") }).ToDictionary(x => x.ColumnName, StringComparer.OrdinalIgnoreCase);

            bool databaseUpdated = false;

            //Add new columns into old version of table
            if (!columnsTableRows.ContainsKey("status_id"))
            {
                this.ExecuteNonQuery(dbConnection, "ALTER TABLE __yuniqldbversion ADD COLUMN status_id INT NOT NULL DEFAULT 1 COMMENT '1 - Succeeded, 2 - Failed'", traceService);
                databaseUpdated = true;
            }

            if (!columnsTableRows.ContainsKey("failed_script_path"))
            {
                this.ExecuteNonQuery(dbConnection, "ALTER TABLE __yuniqldbversion ADD COLUMN failed_script_path VARCHAR(4000) NULL", traceService);
                databaseUpdated = true;
            }

            if (!columnsTableRows.ContainsKey("failed_script_error"))
            {
                this.ExecuteNonQuery(dbConnection, "ALTER TABLE __yuniqldbversion ADD COLUMN failed_script_error VARCHAR(4000) NULL", traceService);
                databaseUpdated = true;
            }

            return databaseUpdated;
        }

        private DataTable GetVersionTableColumns(IDbConnection dbConnection, ITraceService traceService = null)
        {
            MySqlCommand dbCommand = (MySqlCommand) dbConnection.CreateCommand();
            dbCommand.CommandText = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = '__yuniqldbversion' AND table_schema = DATABASE()";
            
            return this.FillDataTable(dbCommand, traceService);
        }

        private DataTable FillDataTable(MySqlCommand dbCommand, ITraceService traceService = null)
        {
            if (null != traceService)
                traceService.Debug($"Executing statement: {Environment.NewLine}{dbCommand.CommandText}");

            DataTable dataTable = new DataTable();
            using (MySqlDataAdapter dataAdapter = new MySqlDataAdapter(dbCommand))
            {
                dataAdapter.Fill(dataTable);
            }
            return dataTable;
        }

        private int ExecuteNonQuery(IDbConnection dbConnection, string commandText, ITraceService traceService = null)
        {
            if (null != traceService)
                traceService.Debug($"Executing statement: {Environment.NewLine}{commandText}");

            IDbCommand dbCommand = dbConnection.CreateCommand();
            dbCommand.CommandText = commandText;
            return dbCommand.ExecuteNonQuery();
        }

        /// <summary>
        /// Try parses error from database specific exception.
        /// </summary>
        /// <param name="exc">The exc.</param>
        /// <param name="result">The parsed error.</param>
        /// <returns>
        /// True, if the parsing was sucessfull otherwise false
        /// </returns>
        public bool TryParseErrorFromException(Exception exc, out string result)
        {
            if (exc is MySqlException mySqlException)
            {
                result = $"(0x{mySqlException.ErrorCode:X}): Error Code: {mySqlException.Number}. {mySqlException.Message}";
                return true;
            }
            result = null;
            return false;
        }
    }
}