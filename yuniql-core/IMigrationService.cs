using Yuniql.Extensibility;
using System.Collections.Generic;

namespace Yuniql.Core
{
    public interface IMigrationService
    {
        void Initialize(string connectionString, int? commandTimeout = null);

        string GetCurrentVersion();

        List<DbVersion> GetAllVersions();

        void Run(
            string workingPath, 
            string targetVersion = null, 
            bool? autoCreateDatabase = null, 
            List<KeyValuePair<string, string>> tokens = null, 
            bool? verifyOnly = null, 
            string delimiter = null,
            int? commandTimeout = null,
            int? batchSize = null
        );

        void Erase(
            string workingPath,
            List<KeyValuePair<string, string>> tokens = null,
            int? commandTimeout = null
        );
    }
}