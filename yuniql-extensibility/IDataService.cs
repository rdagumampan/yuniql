using System.Collections.Generic;
using System.Data;

namespace Yuniql.Extensibility
{

    public interface IDataService
    {
        void ExecuteNonQuery(string connectionString, string sqlStatement, int commandTimeout = DefaultConstants.CommandTimeoutSecs);

        string QuerySingleString(string connectionString, string sqlStatement, int commandTimeout = DefaultConstants.CommandTimeoutSecs);

        bool QuerySingleBool(string connectionString, string sqlStatement, int commandTimeout = DefaultConstants.CommandTimeoutSecs);

        void ExecuteNonQuery(IDbConnection connection, string sqlStatement, IDbTransaction transaction = null, int commandTimeout = DefaultConstants.CommandTimeoutSecs);

        int ExecuteScalar(IDbConnection connection, string sqlStatement, IDbTransaction transaction = null, int commandTimeout = DefaultConstants.CommandTimeoutSecs);

        bool QuerySingleBool(IDbConnection connection, string sqlStatement, IDbTransaction transaction = null, int commandTimeout = DefaultConstants.CommandTimeoutSecs);

        string QuerySingleString(IDbConnection connection, string sqlStatement, IDbTransaction transaction = null, int commandTimeout = DefaultConstants.CommandTimeoutSecs);

        public void Initialize(string connectionString, int commandTimeout = DefaultConstants.CommandTimeoutSecs);

        bool IsTargetDatabaseExists();

        void CreateDatabase();

        bool IsTargetDatabaseConfigured();

        void ConfigureDatabase();

        string GetCurrentVersion();

        List<DbVersion> GetAllVersions();

        void UpdateVersion(IDbConnection connection, IDbTransaction transaction, string version, int commandTimeout = DefaultConstants.CommandTimeoutSecs);
        
        List<string> BreakStatements(string sqlStatement);

        public IDbConnection CreateConnection();

        ConnectionInfo GetConnectionInfo();

        bool IsAtomicDDLSupported { get; }
    }
};