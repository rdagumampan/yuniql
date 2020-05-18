using Yuniql.Extensibility;
using System.Collections.Generic;

namespace Yuniql.Core
{
    /// <summary>
    /// Runs migrations by executing alls scripts in the workspace directory. 
    /// </summary>
    public interface IMigrationService
    {
        /// <summary>
        /// Initializes the current instance of <see cref="MigrationService"./>
        /// </summary>
        /// <param name="connectionString">Connection string to target database server or instance.</param>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        void Initialize(string connectionString, int? commandTimeout = null);

        /// <summary>
        /// Returns the current migration version applied in target database.
        /// </summary>
        string GetCurrentVersion();

        /// <summary>
        /// Returns all migration versions applied in the target database
        /// </summary>
        List<DbVersion> GetAllVersions();

        /// <summary>
        /// Runs migrations by executing alls scripts in the workspace directory.
        /// When CSV files are present also run bulk import operations to target database table having same file name.
        /// </summary>
        /// <param name="workingPath">The directory path to migration project.</param>
        /// <param name="targetVersion">The maximum version to run to. When NULL, runs migration to the latest version found in the workspace path.</param>
        /// <param name="autoCreateDatabase">When TRUE, creates the database in the target host.</param>
        /// <param name="tokens">Token kev/value pairs to replace tokens in script files.</param>
        /// <param name="verifyOnly">When TRUE, runs the migration in uncommitted mode. No changes are committed to target database. When NULL, runs migration in atomic mode.</param>
        /// <param name="delimiter">Delimeter character in the CSV bulk import files. When NULL, uses comma.</param>
        /// <param name="commandTimeout">Command timeout in seconds. When NULL, it uses default provider command timeout.</param>
        /// <param name="batchSize">Batch rows to processed when performing bulk import. When NULL, it uses default provider batch size.</param>
        /// <param name="appliedByTool">The applied by tool.</param>
        /// <param name="appliedByToolVersion">The applied by tool version.</param>
        /// <param name="environmentCode">The environment code.</param>
        /// <param name="resumeFromFailure">The resume from failure.</param>
        void Run(
            string workingPath, 
            string targetVersion = null, 
            bool? autoCreateDatabase = null, 
            List<KeyValuePair<string, string>> tokens = null, 
            bool? verifyOnly = null, 
            string delimiter = null,
            int? commandTimeout = null,
            int? batchSize = null,
            string appliedByTool = null,
            string appliedByToolVersion = null,
            string environmentCode = null,
            NonTransactionalResolvingOption? resumeFromFailure = null
        );

        /// <summary>
        /// Executes erase scripts presentin _erase directory and subdirectories.
        /// </summary>
        /// <param name="workingPath">The directory path to migration project.</param>
        /// <param name="tokens">Token kev/value pairs to replace tokens in script files.</param>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        void Erase(
            string workingPath,
            List<KeyValuePair<string, string>> tokens = null,
            int? commandTimeout = null,
            string environmentCode = null
        );
    }
}