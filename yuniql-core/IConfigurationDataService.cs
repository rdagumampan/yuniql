using System.Collections.Generic;
using System.Data;
using Yuniql.Extensibility;

namespace Yuniql.Core
{
    /// <summary>
    /// Interface for implementing service responsible for accessing target database configuration and executing sql statement batches.
    /// </summary>
    public interface IConfigurationDataService
    {
        /// <summary>
        /// Returns true when database already exists in the target host.
        /// </summary>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        /// <returns>Returns true when database already exists in the target host.</returns>
        bool IsDatabaseExists(int? commandTimeout = null);

        /// <summary>
        /// Creates the database
        /// </summary>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        void CreateDatabase(int? commandTimeout = null);

        /// <summary>
        /// Returns true when migration version tracking table is already created.
        /// </summary>
        /// <param name="schemaName">Schema name for schema versions table. When empty, uses the default schema in the target data platform. </param>
        /// <param name="tableName">Table name for schema versions table. When empty, uses __yuniqldbversion.</param>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        /// <returns>Returns true when version tracking table is already created.</returns>
        bool IsDatabaseConfigured(string schemaName, string tableName, int? commandTimeout = null);

        /// <summary>
        /// Creates schema in target databases.
        /// </summary>
        /// <param name="schemaName">Schema name for schema versions table. When empty, uses the default schema in the target data platform. </param>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        void CreateSchema(string schemaName, int? commandTimeout = null);

        /// <summary>
        /// Creates migration version tracking table in the target database.
        /// </summary>
        /// <param name="schemaName">Schema name for schema versions table. When empty, uses the default schema in the target data platform. </param>
        /// <param name="tableName">Table name for schema versions table. When empty, uses __yuniqldbversion.</param>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        void ConfigureDatabase(string schemaName, string tableName, int? commandTimeout = null);

        /// <summary>
        /// Updates migration version tracking table in the target database..
        /// </summary>
        /// <returns>True if target database was updated, otherwise returns false</returns>
        bool UpdateDatabaseConfiguration();
       
        /// <summary>
        /// Returns the latest version applied in the target database.
        /// </summary>
        /// <param name="schemaName">Schema name for schema versions table. When empty, uses the default schema in the target data platform. </param>
        /// <param name="tableName">Table name for schema versions table. When empty, uses __yuniqldbversion.</param>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        /// <returns>Returns the latest version applied in the target database.</returns>
        string GetCurrentVersion(string schemaName, string tableName, int? commandTimeout = null);

        /// <summary>
        /// Returns all versions applied in the target database.
        /// </summary>
        /// <param name="schemaName">Schema name for schema versions table. When empty, uses the default schema in the target data platform. </param>
        /// <param name="tableName">Table name for schema versions table. When empty, uses __yuniqldbversion.</param>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        /// <returns>All versions applied in the target database.</returns>
        public List<DbVersion> GetAllAppliedVersions(string schemaName, string tableName, int? commandTimeout = null);

        /// <summary>
        /// Returns all versions applied in the target database.
        /// </summary>
        /// <param name="schemaName">Schema name for schema versions table. When empty, uses the default schema in the target data platform. </param>
        /// <param name="tableName">Table name for schema versions table. When empty, uses __yuniqldbversion.</param>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        /// <returns>All versions applied in the target database.</returns>
        List<DbVersion> GetAllVersions(string schemaName, string tableName, int? commandTimeout = null);


        /// <summary>
        /// Creates new entry to version tracking table after all versions were successfully executed.
        /// </summary>
        /// <param name="connection">Connection to target database. Connection will be open automatically.</param>
        /// <param name="transaction">An active transaction.</param>
        /// <param name="version">Migration version.</param>
        /// <param name="schemaName">Schema name for schema versions table. When empty, uses the default schema in the target data platform. </param>
        /// <param name="tableName">Table name for schema versions table. When empty, uses __yuniqldbversion.</param>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        /// <param name="appliedByTool">The source that initiates the migration. This can be yuniql-cli, yuniql-aspnetcore or yuniql-azdevops.</param>
        /// <param name="appliedByToolVersion">The version of the source that initiates the migration.</param>
        /// <param name="failedScriptPath">The failed script path.</param>
        /// <param name="failedScriptError">The failed script error.</param>
        void InsertVersion(
            IDbConnection connection,
            IDbTransaction transaction,
            string version,
            string schemaName,
            string tableName,
            int? commandTimeout = null,
            string appliedByTool = null,
            string appliedByToolVersion = null,
            string failedScriptPath = null,
            string failedScriptError = null);

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
           ITraceService traceService = null);
    }
}