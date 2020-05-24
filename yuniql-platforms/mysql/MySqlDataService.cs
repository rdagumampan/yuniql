using System.Collections.Generic;
using System.Data;
using Yuniql.Extensibility;
using MySql.Data.MySqlClient;
using System.Linq;
using System;

namespace Yuniql.MySql
{
    ///<inheritdoc/>
    public class MySqlDataService : IDataService, INonTransactionalFlow
    {
        private string _connectionString;
        private readonly ITraceService _traceService;

        ///<inheritdoc/>
        public MySqlDataService(ITraceService traceService)
        {
            this._traceService = traceService;
        }

        ///<inheritdoc/>
        public void Initialize(string connectionString)
        {
            this._connectionString = connectionString;
        }

        ///<inheritdoc/>
        public bool IsAtomicDDLSupported => false;

        ///<inheritdoc/>
        public bool IsSchemaSupported { get; } = false;

        ///<inheritdoc/>
        public bool IsBatchSqlSupported { get; } = false;

        ///<inheritdoc/>
        public string TableName { get; set; } = "__yuniqldbversion";

        ///<inheritdoc/>
        public string SchemaName { get; set; }

        ///<inheritdoc/>
        public IDbConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        ///<inheritdoc/>
        public IDbConnection CreateMasterConnection()
        {
            var masterConnectionStringBuilder = new MySqlConnectionStringBuilder(_connectionString);
            masterConnectionStringBuilder.Database = "INFORMATION_SCHEMA";

            return new MySqlConnection(masterConnectionStringBuilder.ConnectionString);
        }

        ///<inheritdoc/>
        public List<string> BreakStatements(string sqlStatementRaw)
        {
            return new List<string> { sqlStatementRaw };
        }

        ///<inheritdoc/>
        public ConnectionInfo GetConnectionInfo()
        {
            var connectionStringBuilder = new MySqlConnectionStringBuilder(_connectionString);
            return new ConnectionInfo { DataSource = connectionStringBuilder.Server, Database = connectionStringBuilder.Database };
        }

        ///<inheritdoc/>
        public string GetSqlForCheckIfDatabaseExists()
            => @"SELECT 1 FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '${YUNIQL_DB_NAME}';";

        ///<inheritdoc/>
        public string GetSqlForCreateDatabase()
            => @"CREATE DATABASE `${YUNIQL_DB_NAME}`;";

        ///<inheritdoc/>
        public string GetSqlForCreateSchema()
            => throw new NotSupportedException("Custom schema is not supported in MySql.");

        ///<inheritdoc/>
        public string GetSqlForCheckIfDatabaseConfigured()
            => @"SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '${YUNIQL_DB_NAME}' AND TABLE_NAME = '${YUNIQL_TABLE_NAME}' LIMIT 1;";

        ///<inheritdoc/>
        public string GetSqlForConfigureDatabase()
            => @"
                CREATE TABLE ${YUNIQL_TABLE_NAME} (
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

        ///<inheritdoc/>
        public string GetSqlForGetCurrentVersion()
            => @"SELECT version FROM ${YUNIQL_TABLE_NAME} WHERE status_id = 1 ORDER BY sequence_id DESC LIMIT 1;";

        ///<inheritdoc/>
        public string GetSqlForGetAllVersions()
            => @"SELECT sequence_id, version, applied_on_utc, applied_by_user, applied_by_tool, applied_by_tool_version, additional_artifacts, status_id, failed_script_path, failed_script_error FROM ${YUNIQL_TABLE_NAME} ORDER BY version ASC;";

        ///<inheritdoc/>
        public string GetSqlForInsertVersion()
            => throw new NotSupportedException("Not supported for current target platform");

        ///<inheritdoc/>
        public string GetSqlForUpsertVersion()
            => @"INSERT INTO ${YUNIQL_TABLE_NAME} (version, applied_on_utc, applied_by_user, applied_by_tool, applied_by_tool_version, additional_artifacts, status_id, failed_script_path, failed_script_error) VALUES (@version, UTC_TIMESTAMP(), CURRENT_USER(), @toolName, @toolVersion, @additionalArtifacts, @statusId, @failedScriptPath, @failedScriptError)
                    ON DUPLICATE KEY UPDATE
                    applied_on_utc = VALUES(applied_on_utc),
                    applied_by_user = VALUES(applied_by_user),
                    applied_by_tool = VALUES(applied_by_tool),
                    applied_by_tool_version = VALUES(applied_by_tool_version),
                    status_id = VALUES(status_id),
                    failed_script_path = VALUES(failed_script_path),
                    failed_script_error = VALUES(failed_script_error);
            ";

        ///<inheritdoc/>
        public bool UpdateDatabaseConfiguration(IDbConnection dbConnection, ITraceService traceService = null, string schemaName = null, string tableName = null)
        {
            DataTable columnsTable = GetVersionTableColumns(dbConnection, traceService, tableName);

            var columnsTableRows = columnsTable.Rows.Cast<DataRow>().Select(x => new { ColumnName = x.Field<string>("COLUMN_NAME"), ColumnType = x.Field<string>("COLUMN_TYPE") }).ToDictionary(x => x.ColumnName, StringComparer.OrdinalIgnoreCase);

            bool databaseUpdated = false;

            //Add new columns into old version of table
            if (!columnsTableRows.ContainsKey("status_id"))
            {
                this.ExecuteNonQuery(dbConnection, $"ALTER TABLE {tableName ?? this.TableName} ADD COLUMN status_id INT NOT NULL DEFAULT 1 COMMENT '1 - Succeeded, 2 - Failed'", traceService);
                databaseUpdated = true;
            }

            if (!columnsTableRows.ContainsKey("failed_script_path"))
            {
                this.ExecuteNonQuery(dbConnection, $"ALTER TABLE {tableName ?? this.TableName} ADD COLUMN failed_script_path VARCHAR(4000) NULL", traceService);
                databaseUpdated = true;
            }

            if (!columnsTableRows.ContainsKey("failed_script_error"))
            {
                this.ExecuteNonQuery(dbConnection, $"ALTER TABLE {tableName ?? this.TableName} ADD COLUMN failed_script_error VARCHAR(4000) NULL", traceService);
                databaseUpdated = true;
            }

            return databaseUpdated;
        }

        private DataTable GetVersionTableColumns(IDbConnection dbConnection, ITraceService traceService = null, string tableName = null)
        {
            MySqlCommand dbCommand = (MySqlCommand)dbConnection.CreateCommand();
            dbCommand.CommandText = $"SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = '{tableName ?? this.TableName}' AND table_schema = DATABASE()";

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

        ///<inheritdoc/>
        public bool TryParseErrorFromException(Exception exception, out string result)
        {
            if (exception is MySqlException mySqlException)
            {
                result = $"(0x{mySqlException.ErrorCode:X}): Error Code: {mySqlException.Number}. {mySqlException.Message}";
                return true;
            }
            result = null;
            return false;
        }
    }
}