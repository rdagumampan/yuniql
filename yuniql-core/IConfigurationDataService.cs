using System.Collections.Generic;
using System.Data;
using Yuniql.Extensibility;

namespace Yuniql.Core
{
    public interface IConfigurationDataService
    {
        void ConfigureDatabase(int? commandTimeout = null);

        void CreateDatabase(int? commandTimeout = null);

        List<DbVersion> GetAllVersions(int? commandTimeout = null);

        string GetCurrentVersion(int? commandTimeout = null);

        bool IsDatabaseConfigured(int? commandTimeout = null);

        bool IsDatabaseExists(int? commandTimeout = null);

        void UpdateVersion(
            IDbConnection connection, 
            IDbTransaction transaction, 
            string version, 
            int? commandTimeout = null);

        public int ExecuteSql(
           IDbConnection connection,
           string commandText,
           int? commandTimeout = null,
           IDbTransaction transaction = null,
           ITraceService traceService = null);
    }
}