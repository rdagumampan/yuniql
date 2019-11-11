using ArdiLabs.Yuniql.Extensibility;
using System.Collections.Generic;

namespace ArdiLabs.Yuniql.Core
{
    public interface IMigrationService
    {

        void Initialize(string connectionString);

        string GetCurrentVersion();

        List<DbVersion> GetAllVersions();

        void Run(string workingPath, string targetVersion, bool autoCreateDatabase, List<KeyValuePair<string, string>> tokens = null, bool verifyOnly = false);

        void Erase(string workingPath);
    }
}