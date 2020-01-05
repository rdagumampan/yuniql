using System.Collections.Generic;
using System.Data;

namespace Yuniql.Extensibility
{
    public interface IDataService
    {
        void ExecuteNonQuery(string connectionString, string sqlStatement, int commandTimeout = 30);

        string QuerySingleString(string connectionString, string sqlStatement, int commandTimeout = 30);

        bool QuerySingleBool(string connectionString, string sqlStatement, int commandTimeout = 30);

        void ExecuteNonQuery(IDbConnection activeConnection, string sqlStatement, IDbTransaction activeTransaction = null, int commandTimeout = 30);

        int ExecuteScalar(IDbConnection activeConnection, string sqlStatement, IDbTransaction activeTransaction = null, int commandTimeout = 30);

        bool QuerySingleBool(IDbConnection activeConnection, string sqlStatement, IDbTransaction activeTransaction = null, int commandTimeout = 30);

        string QuerySingleString(IDbConnection activeConnection, string sqlStatement, IDbTransaction activeTransaction = null, int commandTimeout = 30);

        public void Initialize(string connectionString, int commandTimeout = 30);

        bool IsTargetDatabaseExists();

        void CreateDatabase();

        bool IsTargetDatabaseConfigured();

        void ConfigureDatabase();

        string GetCurrentVersion();

        List<DbVersion> GetAllVersions();

        void UpdateVersion(IDbConnection activeConnection, IDbTransaction activeTransaction, string version, int commandTimeout = 30);
        
        List<string> BreakStatements(string sqlStatement);

        public IDbConnection CreateConnection();

        ConnectionInfo GetConnectionInfo();

        bool IsAtomicDDLSupported { get; }
    }
};