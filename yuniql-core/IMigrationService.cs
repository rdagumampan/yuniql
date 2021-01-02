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
        /// Runs migrations by executing alls scripts in the workspace directory. 
        /// When CSV files are present also run bulk import operations to target database table having same file name.
        /// </summary>
        /// <param name="workspace">The directory path to migration project.</param>
        /// <param name="targetVersion">The maximum version to run to. When NULL, runs migration to the latest version found in the workspace path.</param>
        /// <param name="isAutoCreateDatabase">When TRUE, creates the database in the target host.</param>
        /// <param name="tokens">Token kev/value pairs to replace tokens in script files.</param>
        /// <param name="isVerifyOnly">When TRUE, runs the migration in uncommitted mode. No changes are committed to target database. When NULL, runs migration in atomic mode.</param>
        /// <param name="bulkSeparator">Bulk file values separator character in the CSV bulk import files. When NULL, uses comma.</param>
        /// <param name="metaSchemaName">Schema name for schema versions table. When empty, uses the default schema in the target data platform. </param>
        /// <param name="metaTableName">Table name for schema versions table. When empty, uses __yuniqldbversion.</param>
        /// <param name="commandTimeout">Command timeout in seconds. When NULL, it uses default provider command timeout.</param>
        /// <param name="bulkBatchSize">Batch rows to processed when performing bulk import. When NULL, it uses default provider batch size.</param>
        /// <param name="appliedByTool">The source that initiates the migration. This can be yuniql-cli, yuniql-aspnetcore or yuniql-azdevops.</param>
        /// <param name="appliedByToolVersion">The version of the source that initiates the migration.</param>
        /// <param name="environment">Environment code for environment-aware scripts.</param>
        /// <param name="isContinueAfterFailure">The resume from failure.</param>
        /// <param name="transactionMode"></param>
        /// <param name="isRequiredClearedDraft">When TRUE, migration will fail if the _draft folder is not empty. This is for production migration.</param>
        void Run(
            string workspace, 
            string targetVersion = null, 
            bool? isAutoCreateDatabase = null, 
            List<KeyValuePair<string, string>> tokens = null, 
            bool? isVerifyOnly = null, 
            string bulkSeparator = null,
            string metaSchemaName = null, 
            string metaTableName = null,
            int? commandTimeout = null,
            int? bulkBatchSize = null,
            string appliedByTool = null,
            string appliedByToolVersion = null,
            string environment = null,
            bool? isContinueAfterFailure = null,
            string transactionMode = null,
            bool isRequiredClearedDraft = false
        );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="workspace"></param>
        /// <param name="tokens"></param>
        /// <param name="bulkSeparator"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="environment"></param>
        /// <param name="transactionMode"></param>
        /// <param name="isRequiredClearedDraft"></param>
        void RunNonVersionScripts(
            IDbConnection connection,
            IDbTransaction transaction,
            string workspace,
            List<KeyValuePair<string, string>> tokens = null,
            string bulkSeparator = null,
            int? commandTimeout = null,
            string environment = null,
            string transactionMode = null,
            bool isRequiredClearedDraft = false
        );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="versions"></param>
        /// <param name="workspace"></param>
        /// <param name="targetVersion"></param>
        /// <param name="transactionContext"></param>
        /// <param name="tokens"></param>
        /// <param name="bulkSeparator"></param>
        /// <param name="metaSchemaName"></param>
        /// <param name="metaTableName"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="bulkBatchSize"></param>
        /// <param name="appliedByTool"></param>
        /// <param name="appliedByToolVersion"></param>
        /// <param name="environment"></param>
        /// <param name="transactionMode"></param>
        void RunVersionScripts(
            IDbConnection connection,
            IDbTransaction transaction,
            List<string> versions,
            string workspace,
            string targetVersion,
            TransactionContext transactionContext,
            List<KeyValuePair<string, string>> tokens = null,
            string bulkSeparator = null,
            string metaSchemaName = null,
            string metaTableName = null,
            int? commandTimeout = null,
            int? bulkBatchSize = null,
            string appliedByTool = null,
            string appliedByToolVersion = null,
            string environment = null,
            string transactionMode = null
        );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="workspace"></param>
        /// <param name="scriptDirectory"></param>
        /// <param name="bulkSeparator"></param>
        /// <param name="bulkBatchSize"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="environment"></param>
        void RunBulkImport(
            IDbConnection connection,
            IDbTransaction transaction,
            string workspace,
            string scriptDirectory,
            string bulkSeparator = null,
            int? bulkBatchSize = null,
            int? commandTimeout = null,
            string environment = null
        );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="transactionContext"></param>
        /// <param name="version"></param>
        /// <param name="workspace"></param>
        /// <param name="scriptDirectory"></param>
        /// <param name="metaSchemaName"></param>
        /// <param name="metaTableName"></param>
        /// <param name="tokens"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="environment"></param>
        /// <param name="appliedByTool"></param>
        /// <param name="appliedByToolVersion"></param>
        void RunSqlScripts(
            IDbConnection connection,
            IDbTransaction transaction,
            TransactionContext transactionContext,
            string version,
            string workspace,
            string scriptDirectory,
            string metaSchemaName,
            string metaTableName,
            List<KeyValuePair<string, string>> tokens = null,
            int? commandTimeout = null,
            string environment = null,
            string appliedByTool = null,
            string appliedByToolVersion = null
        );

        /// <summary>
        /// Executes erase scripts presentin _erase directory and subdirectories.
        /// </summary>
        void Erase();
    }
}