using System;
using System.Collections.Generic;
using System.Data;

namespace Yuniql.Extensibility
{
    /// <summary>
    /// Implement this interface to support a database platform or provider.
    /// </summary>
    public interface IDataService
    {
        /// <summary>
        /// Initialize the bulk import service. Sets connection string for future operations.
        /// </summary>
        /// <param name="connectionString">Connection string to the target database.</param>
        public void Initialize(string connectionString);
        
        /// <summary>
        /// Creates new connection to target database.
        /// </summary>
        /// <returns></returns>
        public IDbConnection CreateConnection();

        /// <summary>
        /// Creates new connection to master or metadata database. This is used to check if the database exists when --auto-createdb is set to true.
        /// </summary>
        /// <returns></returns>
        public IDbConnection CreateMasterConnection();

        /// <summary>
        /// Creates empty Db parameters.
        /// </summary>
        /// <returns></returns>
        public IDbParameters CreateDbParameters();

        /// <summary>
        /// Get basic connection information to target database.
        /// </summary>
        /// <returns></returns>
        ConnectionInfo GetConnectionInfo();

        /// <summary>
        /// Returns true if the database platform or version supports Atomic or Transactional DDL operations.
        /// MySql version below 8.0 are known to not support atomic DDL. Other providers like SqlServer, Oracle and PostgreSql 
        /// supports rollback of DDL operations should migration failed.
        /// </summary>
        bool IsAtomicDDLSupported { get; }

        /// <summary>
        /// Returns true if the database platform or version supports Schema within the database.
        /// MySql version below 8.0 are known to not support Schema.
        /// </summary>
        bool IsSchemaSupported { get; }

        /// <summary>
        /// Breaks down statement using terminator word supported by target database.
        /// For example, SQL Sevrer uses GO to split statements from single file.
        /// </summary>
        /// <param name="sqlStatement">Raw sql statement as extracted from .sql file.</param>
        /// <returns>List of statements separated by terminator.</returns>
        List<string> BreakStatements(string sqlStatement);

        /// <summary>
        /// Returns the SQL statement to use for checking if the target database already exists
        /// </summary>
        public string GetSqlForCheckIfDatabaseExists();

        /// <summary>
        /// Returns the SQL statement to use for creating new database if --auto-createdb flag is set to true.
        /// </summary>
        public string GetSqlForCreateDatabase();

        /// <summary>
        /// Returns the SQL statement to use for checking target database has been configured for migration tracking.
        /// </summary>
        public string GetSqlForCheckIfDatabaseConfigured();

        /// <summary>
        /// Returns the SQL statement to use for configuring the migration tracking table.
        /// </summary>
        public string GetSqlForConfigureDatabase();

        /// <summary>
        /// Returns the SQL statement to use for getting the latest migration version appplied in the target database.
        /// </summary>
        public string GetSqlForGetCurrentVersion();

        /// <summary>
        /// Returns the SQL statement to use for getting all versions applied in the target database.
        /// </summary>
        public string GetSqlForGetAllVersions();

        /// <summary>
        /// Returns the SQL statement to use for creating new entry into migration tracking table.
        /// </summary>
        public string GetSqlForInsertVersion();

        /// <summary>
        /// Updates the database migration tracking table.
        /// </summary>
        /// <param name="dbConnection">The database connection.</param>
        /// <param name="traceService">The trace service.</param>
        /// <returns>True if target database was updated, otherwise returns false</returns>
        public bool UpdateDatabaseConfiguration(IDbConnection dbConnection, ITraceService traceService = null);

        /// <summary>
        /// Try parses error from database specific exception.
        /// </summary>
        /// <param name="exc">The exc.</param>
        /// <param name="result">The parsed error.</param>
        /// <returns>
        /// True, if the parsing was sucessfull otherwise false
        /// </returns>
        bool TryParseErrorFromException(Exception exc, out string result);
    }
}