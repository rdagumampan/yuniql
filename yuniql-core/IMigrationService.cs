using Yuniql.Extensibility;
using System.Collections.Generic;

namespace Yuniql.Core
{
    public interface IMigrationService
    {
        void Initialize(string connectionString, int commandTimeout = DefaultConstants.CommandTimeoutSecs);

        string GetCurrentVersion();

        List<DbVersion> GetAllVersions();

        void Run(
            string workingPath, 
            string targetVersion, 
            bool autoCreateDatabase = false, 
            List<KeyValuePair<string, string>> tokens = null, 
            bool verifyOnly = false, 
            string delimiter = DefaultConstants.Delimiter,
            int commandTimeout = DefaultConstants.CommandTimeoutSecs,
            int batchSize = DefaultConstants.BatchSize
        );

        void Erase(
            string workingPath,
            List<KeyValuePair<string, string>> tokens = null,
            int commandTimeout = DefaultConstants.CommandTimeoutSecs
        );
    }
}