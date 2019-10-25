using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ArdiLabs.Yuniql
{
    public interface IMigrationService
    {
        List<DbVersion> GetAllDbVersions();
        void Run(string workingPath, string targetVersion, bool autoCreateDatabase, List<KeyValuePair<string, string>> tokens = null, bool uncommitted = false);
    }
}