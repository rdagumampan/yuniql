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
        /// Returns true if the database platform or version supports Atomic or Transactional DDL operations.
        /// MySql version below 8.0 are known to not support atomic DDL. Other providers like SqlServer, Oracle and PostgreSql 
        /// supports rollback of DDL operations should migration failed.
        /// </summary>
        bool IsTransactionalDdlSupported { get; }

        /// <summary>
        /// Returns true if the database platform or version supports multiple databases in the same server instance.
        /// For example, all major RDMS platforms supports this except for Oracle versions older than 12c
        /// </summary>
        bool IsMultiTenancySupported { get; }

        /// <summary>
        /// Returns true if the database platform or version supports Schema within the database.
        /// MySql version below 8.0 are known to not support Schema.
        /// </summary>
        bool IsSchemaSupported { get; }

        /// <summary>
        /// Returns true if the database platform or version supports batch sql statements.
        /// For example, SQL Server uses GO word as default batch terminator while Snowflow uses semicolon (;).
        /// </summary>
        bool IsBatchSqlSupported { get; }

        /// <summary>
        /// Returns true if the database supports single MERGE or UPSERT sql statement
        /// </summary>
        bool IsUpsertSupported { get; }

        /// <summary>
        /// Default schema name for schema versions table. Leave it null if database doesn't support schema.
        /// For example its dbo in SqlServer and public in PostgreSql
        /// </summary>
        string MetaSchemaName { get; }

        /// <summary>
        /// Table name for schema versions table.
        /// When implementing a new platform, its reccommended to use __yuniql_schema_version as default value
        /// </summary>
        string MetaTableName { get; }

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
        /// Get basic connection information to target database.
        /// </summary>
        /// <returns></returns>
        ConnectionInfo GetConnectionInfo();

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
        /// Returns the SQL statement to use for dropping existing database
        /// </summary>
        public List<string> GetSqlForDropDatabase();

        /// <summary>
        /// Returns the SQL statement to use for checking if the target schema already exists
        /// </summary>
        public string GetSqlForCheckIfSchemaExists();

        /// <summary>
        /// Returns the SQL statement to use for creating schema if the target database supports schemas.
        /// </summary>
        /// <returns></returns>
        public string GetSqlForCreateSchema();

        /// <summary>
        /// Returns the SQL statement to use for checking target database has been configured for migration tracking.
        /// </summary>
        public string GetSqlForCheckIfDatabaseConfigured();

        //TODO: Consider dropping this in next release
        /// <summary>
        /// Returns the SQL statement to use for checking target database has been configured for migration tracking in yuniql v1.0.
        /// </summary>
        string GetSqlForCheckIfDatabaseConfiguredv10();

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
        /// Returns the SQL statement to use for updating version in migration tracking table.
        /// </summary>
        public string GetSqlForUpdateVersion();

        /// <summary>
        /// Returns the SQL statement to use for merging new entry into migration tracking table.
        /// </summary>
        public string GetSqlForUpsertVersion();

        /// <summary>
        /// Returns the SQL statement to use for getting the database verison.
        /// </summary>
        public string GetSqlForGetDatabaseVersion();

        //TODO: Consider dropping this in next release
        /// <summary>
        /// Returns true if the version tracking table requires upgrade for this release
        /// </summary>
        /// <returns></returns>
        public string GetSqlForCheckRequireMetaSchemaUpgrade(string currentSchemaVersion);

        //TODO: Consider dropping this in next release
        /// <summary>
        /// Returns sql for upgrade the existing version tracking table
        /// </summary>
        /// <returns></returns>
        public string GetSqlForUpgradeMetaSchema(string requiredSchemaVersion);

        /// <summary>
        /// Try parses error from database specific exception.
        /// </summary>
        /// <param name="exception">The exc.</param>
        /// <param name="result">The parsed error.</param>
        /// <returns>
        /// True, if the parsing was sucessfull otherwise false
        /// </returns>
        bool TryParseErrorFromException(Exception exception, out string result);
    }
}