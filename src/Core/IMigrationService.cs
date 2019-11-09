using System.Collections.Generic;

namespace ArdiLabs.Yuniql
{
    public interface IMigrationService
    {
        string GetCurrentVersion();

        List<DbVersion> GetAllVersions();

        void Run(string workingPath, string targetVersion, bool autoCreateDatabase, List<KeyValuePair<string, string>> tokens = null, bool uncommitted = false);

        void Erase(string workingPath);
    }
}