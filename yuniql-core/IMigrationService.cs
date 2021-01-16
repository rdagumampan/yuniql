using Yuniql.Extensibility;
using System.Collections.Generic;
using System.Data;

namespace Yuniql.Core
{
    /// <summary>
    /// Runs migrations by executing alls scripts in the workspace directory. 
    /// </summary>
    public interface IMigrationService
    {
        /// <summary>
        /// Returns true if the version of target database is equal or greater than local versions
        /// </summary>
        /// <param name="version"></param>
        /// <param name="metaSchemaName"></param>
        /// <param name="metaTableName"></param>
        /// <returns></returns>
        bool IsTargetDatabaseLatest(string version, string metaSchemaName = null, string metaTableName = null);

        /// <summary>
        /// Returns the current migration version applied in target database.
        /// </summary>
        string GetCurrentVersion(string metaSchemaName = null, string metaTableName = null);

        /// <summary>
        /// Returns all migration versions applied in the target database
        /// </summary>
        List<DbVersion> GetAllVersions(string metaSchemaName = null, string metaTableName = null);

        /// <summary>
        /// Runs migrations by executing alls scripts in the workspace directory. 
        /// When CSV files are present also run bulk import operations to target database table having same file name.
        /// </summary>
        void Run();

        /// <summary>
        /// Executes erase scripts presentin _erase directory and subdirectories.
        /// </summary>
        void Erase();
    }
}