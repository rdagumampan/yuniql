using Yuniql.Extensibility;
using System.Collections.Generic;

namespace Yuniql.Core
{
    public interface IMigrationService
    {
        void Initialize(string connectionString, int commandTimeout);

        string GetCurrentVersion();

        List<DbVersion> GetAllVersions();

        void Run(
            string workingPath, 
            string targetVersion, 
            bool autoCreateDatabase, 
            List<KeyValuePair<string, string>> tokens = null, 
            bool verifyOnly = false, 
            string delimiter = ",",
            int commandTimeout = 30
            );

        void Erase(
            string workingPath,
            List<KeyValuePair<string, string>> tokens = null,
            int commandTimeout = 30
            );
    }
}