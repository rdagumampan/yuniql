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
        /// Initializes the current instance of <see cref="MigrationService"/>
        /// </summary>
        /// <param name="connectionString">Connection string to target database server or instance.</param>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        void Initialize(string connectionString, int? commandTimeout = null);

        /// <summary>
        /// Returns the current migration version applied in target database.
        /// </summary>
        string GetCurrentVersion(string schemaName = null, string tableName = null);

        /// <summary>
        /// Returns all migration versions applied in the target database
        /// </summary>
        List<DbVersion> GetAllVersions(string schemaName = null, string tableName = null);

        /// <summary>
        /// Runs migrations by executing alls scripts in the workspace directory. 
        /// When CSV files are present also run bulk import operations to target database table having same file name.
        /// </summary>
        /// <param name="workingPath">The directory path to migration project.</param>
        /// <param name="targetVersion">The maximum version to run to. When NULL, runs migration to the latest version found in the workspace path.</param>
        /// <param name="autoCreateDatabase">When TRUE, creates the database in the target host.</param>
        /// <param name="tokens">Token kev/value pairs to replace tokens in script files.</param>
        /// <param name="verifyOnly">When TRUE, runs the migration in uncommitted mode. No changes are committed to target database. When NULL, runs migration in atomic mode.</param>
        /// <param name="bulkSeparator">Bulk file values separator character in the CSV bulk import files. When NULL, uses comma.</param>
        /// <param name="metaSchemaName">Schema name for schema versions table. When empty, uses the default schema in the target data platform. </param>
        /// <param name="metaTableName">Table name for schema versions table. When empty, uses __yuniqldbversion.</param>
        /// <param name="commandTimeout">Command timeout in seconds. When NULL, it uses default provider command timeout.</param>
        /// <param name="bulkBatchSize">Batch rows to processed when performing bulk import. When NULL, it uses default provider batch size.</param>
        /// <param name="appliedByTool">The source that initiates the migration. This can be yuniql-cli, yuniql-aspnetcore or yuniql-azdevops.</param>
        /// <param name="appliedByToolVersion">The version of the source that initiates the migration.</param>
        /// <param name="environmentCode">Environment code for environment-aware scripts.</param>
        /// <param name="resumeFromFailure">The resume from failure.</param>
        /// <param name="noTransaction">When TRUE, migration will run without using transactions</param>
        /// <param name="requiredClearedDraftFolder">When TRUE, migration will fail if the _draft folder is not empty. This is for production migration</param>
        void Run(
            string workingPath, 
            string targetVersion = null, 
            bool? autoCreateDatabase = null, 
            List<KeyValuePair<string, string>> tokens = null, 
            bool? verifyOnly = null, 
            string bulkSeparator = null,
            string metaSchemaName = null, 
            string metaTableName = null,
            int? commandTimeout = null,
            int? bulkBatchSize = null,
            string appliedByTool = null,
            string appliedByToolVersion = null,
            string environmentCode = null,
            NonTransactionalResolvingOption? resumeFromFailure = null,
            bool noTransaction = false,
            bool requiredClearedDraftFolder = false
        );

        /// <summary>
        /// Executes erase scripts presentin _erase directory and subdirectories.
        /// </summary>
        /// <param name="workingPath">The directory path to migration project.</param>
        /// <param name="tokens">Token kev/value pairs to replace tokens in script files.</param>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        /// <param name="environmentCode">Environment code for environment-aware scripts.</param>
        bool IsTargetDatabaseLatest(string targetVersion, string schemaName = null, string tableName = null);

        /// <summary>
        /// Runs migrations by executing alls scripts in the workspace directory. 
        /// When CSV files are present also run bulk import operations to target database table having same file name.
        /// </summary>
        /// <param name="connection">The <see cref="IDbConnection"/> to use.</param>
        /// <param name="transaction">The <see cref="IDbTransaction"/> to use</param>
        /// <param name="workingPath">The directory path to migration project.</param>
        /// <param name="tokenKeyPairs">Token kev/value pairs to replace tokens in script files.</param>
        /// <param name="bulkSeparator">Bulk file values separator character in the CSV bulk import files. When NULL, uses comma.</param>
        /// <param name="commandTimeout">Command timeout in seconds. When NULL, it uses default provider command timeout.</param>
        /// <param name="environmentCode">Environment code for environment-aware scripts.</param>
        /// <param name="requiredClearedDraftFolder">When TRUE, the migration will fail if the current folder is empty. This option is for production migration</param>
        void RunNonVersionScripts(
            IDbConnection connection,
            IDbTransaction transaction,
            string workingPath,
            List<KeyValuePair<string, string>> tokenKeyPairs = null,
            string bulkSeparator = null,
            int? commandTimeout = null,
            string environmentCode = null,
            bool requiredClearedDraftFolder = false
        );

        void RunVersionScripts(
            IDbConnection connection,
            IDbTransaction transaction,
            List<string> dbVersions,
            string workingPath,
            string targetVersion,
            NonTransactionalContext nonTransactionalContext,
            List<KeyValuePair<string, string>> tokenKeyPairs = null,
            string bulkSeparator = null,
            string metaSchemaName = null,
            string metaTableName = null,
            int? commandTimeout = null,
            int? bulkBatchSize = null,
            string appliedByTool = null,
            string appliedByToolVersion = null,
            string environmentCode = null
        );

        void RunBulkImport(
            IDbConnection connection,
            IDbTransaction transaction,
            string workingPath,
            string scriptDirectory,
            string bulkSeparator = null,
            int? bulkBatchSize = null,
            int? commandTimeout = null,
            string environmentCode = null
        );

        void RunSqlScripts(
            IDbConnection connection,
            IDbTransaction transaction,
            NonTransactionalContext nonTransactionalContext,
            string version,
            string workingPath,
            string scriptDirectory,
            string metaSchemaName,
            string metaTableName,
            List<KeyValuePair<string, string>> tokenKeyPairs = null,
            int? commandTimeout = null,
            string environmentCode = null,
            string appliedByTool = null,
            string appliedByToolVersion = null
        );

        void Erase(
            string workingPath,
            List<KeyValuePair<string, string>> tokens = null,
            int? commandTimeout = null,
            string environmentCode = null
        );
    }
}