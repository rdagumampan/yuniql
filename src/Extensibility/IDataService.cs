using System.Collections.Generic;
using System.Data;

namespace ArdiLabs.Yuniql
{
    public interface IDataService
    {
        void ExecuteNonQuery(string connectionString, string sqlStatement);

        string QuerySingleString(string connectionString, string sqlStatement);

        bool QuerySingleBool(string connectionString, string sqlStatement);

        void ExecuteNonQuery(IDbConnection activeConnection, string sqlStatement, IDbTransaction transaction = null);

        int ExecuteScalar(IDbConnection activeConnection, string sqlStatement, IDbTransaction transaction = null);

        bool QuerySingleBool(IDbConnection activeConnection, string sqlStatement, IDbTransaction transaction = null);

        string QuerySingleString(IDbConnection activeConnection, string sqlStatement, IDbTransaction transaction = null);

        bool IsTargetDatabaseExists();

        void CreateDatabase();

        bool IsTargetDatabaseConfigured();

        void ConfigureDatabase();

        string GetCurrentVersion();

        List<DbVersion> GetAllVersions();

        void UpdateVersion(IDbConnection activeConnection, IDbTransaction transaction, string version);
        
        List<string> BreakStatements(string sqlStatementRaw);

        public IDbConnection CreateConnection();

        ConnectionInfo GetConnectionInfo();
    }
};