using System.Collections.Generic;
using System.Data.SqlClient;

namespace ArdiLabs.Yuniql
{
    public interface IMigrationService
    {
        List<DbVersion> GetAllDbVersions(SqlConnectionStringBuilder sqlConnectionString);
        bool IsTargetDatabaseExists(SqlConnectionStringBuilder sqlConnectionString, string targetDatabaseName);
        void Run(string workingPath, string connectionString, string targetVersion, bool autoCreateDatabase);
    }
}