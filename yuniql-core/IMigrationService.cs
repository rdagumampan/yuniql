using Yuniql.Extensibility;
using System.Collections.Generic;

namespace Yuniql.Core
{
    public interface IMigrationService
    {

        void Initialize(string connectionString);

        string GetCurrentVersion();

        List<DbVersion> GetAllVersions();

        void Run(string workingPath, string targetVersion, bool autoCreateDatabase, List<KeyValuePair<string, string>> tokens = null, bool verifyOnly = false, string delimeter = ",");

        void Erase(string workingPath);
    }
}