using Yuniql.Extensibility;
using System.Collections.Generic;

namespace Yuniql.Core
{
    public interface IMigrationService
    {
        void Initialize(string connectionString, int? commandTimeout);

        string GetCurrentVersion();

        List<DbVersion> GetAllVersions();

        void Run(
            string workingPath, 
            string targetVersion, 
            bool autoCreateDatabase, 
            List<KeyValuePair<string, string>> tokens, 
            bool verifyOnly, 
            string delimiter,
            int? commandTimeout,
            int? batchSize
        );

        void Erase(
            string workingPath,
            List<KeyValuePair<string, string>> tokens,
            int? commandTimeout
        );
    }
}