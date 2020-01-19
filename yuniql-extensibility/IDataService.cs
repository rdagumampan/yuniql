using System.Collections.Generic;
using System.Data;

namespace Yuniql.Extensibility
{

    public interface IDataService
    {
        public void Initialize(string connectionString);
        
        public IDbConnection CreateConnection();

        public IDbConnection CreateMasterConnection();

        ConnectionInfo GetConnectionInfo();

        bool IsAtomicDDLSupported { get; }

        List<string> BreakStatements(string sqlStatement);
        
        public string GetCheckIfDatabaseExistsSql();

        public string GetCreateDatabaseSql();

        public string GetCheckIfDatabaseConfiguredSql();

        public string GetConfigureDatabaseSql();

        public string GetGetCurrentVersionSql();

        public string GetGetAllVersionsSql();

        public string GetUpdateVersionSql();
    }
};