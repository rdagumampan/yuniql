using System.Collections.Generic;
using Yuniql.Extensibility;

namespace Yuniql.Core
{
    /// <summary>
    /// Global singleton instance of session configuration
    /// </summary>
    public sealed class Configuration
    {
        private static readonly Configuration instance = new Configuration();

        ///<inheritdoc/>
        static Configuration() {}

        ///<inheritdoc/>
        private Configuration() {}

        /// <summary>
        /// Returns global singleton instance of session configuration
        /// </summary>
        public static Configuration Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Returns true when global configuration has been initiatlized via configurationService.Initialize()
        /// </summary>
        public bool IsInitialized { get; set; } = false;
        
        /// <summary>
        /// The directory where yuniql-based migration project is placed.
        /// This is a required property.
        /// </summary>
        public string Workspace { get; set; } = null;

        /// <summary>
        /// Target database platform. Value can be `sqlserver`,`postgresql`, or `mysql`.
        /// This is a required property.
        /// </summary>
        public string Platform { get; set; } = null;

        /// <summary>
        /// The connection string to the target database server.
        /// This is a required property.
        /// </summary>
        public string ConnectionString { get; set; } = null;

        /// <summary>
        /// When true, automatically creates database in target database server.
        /// This is defaulted to false.
        /// </summary>
        public bool IsAutoCreateDatabase { get; set; } = false;

        /// <summary>
        /// Runs migration only up to the version specified in this property. 
        /// When Null, it will run upto latest unapplied version.
        /// </summary>
        public string TargetVersion { get; set; } = null;

        /// <summary>
        /// The list of token key/value pairs to repair tokens in each script file. 
        /// </summary>
        public List<KeyValuePair<string, string>> Tokens { get; set; } = new List<KeyValuePair<string, string>>();

        /// <summary>
        /// When you run yuniql verify, it checks if all your versions can be executed without errors. 
        /// It runs through all the non-versioned script folders (except _init) and all migration steps that yuninql run takes but without committing the transaction. 
        /// All changes are rolledback after a successful verification run.
        /// This is defaulted to false.
        /// </summary>
        public bool IsVerifyOnly { get; set; } = false;

        /// <summary>
        /// Bulk file values separator to use when parsing CSV bulk import files.
        /// This is defaulted to comma ",".
        /// </summary>
        public string BulkSeparator { get; set; } = DEFAULT_CONSTANTS.BULK_SEPARATOR;

        /// <summary>
        /// The size of each batch when performing bulk load. This may not be used in non-sqlserver platforms.
        /// This is defaulted to 0.
        /// </summary>
        public int BulkBatchSize { get; set; } = DEFAULT_CONSTANTS.BULK_BATCH_SIZE;

        /// <summary>
        /// The time it taks to wait for one commend to execute before it expires and throws error.
        /// Use this prorty to adjust time out when you expect a long running migration execution.
        /// This is defaulted to 30 secs.
        /// </summary>
        public int CommandTimeout { get; set; } = DEFAULT_CONSTANTS.COMMAND_TIMEOUT_SECS;

        /// <summary>
        /// Enrich trace messages with raw sql statements and more verbose diagnostic messages.
        /// Use this when you are investigating some failed migrations.
        /// This is defaulted to false.
        /// </summary>
        public bool IsDebug { get; set; } = false;

        /// <summary>
        /// Describes the source of migration applied to target database.
        /// This is defaulted to yuniql-cli.
        /// </summary>
        public string AppliedByTool { get; set; } = "yuniql-cli";

        /// <summary>
        /// Describes the version of source of migration applied to target database.
        /// This is defaulted to yuniql.core assembly version.
        /// </summary>
        public string AppliedByToolVersion { get; set; } = $"{typeof(IMigrationService).Assembly.GetName().Version.ToString()}";

        /// <summary>
        /// Environment to target when running migration with environment-aware scripts.
        /// See https://github.com/rdagumampan/yuniql/wiki/environment-aware-scripts
        /// </summary>
        public string Environment { get; set; } = null;

        /// <summary>
        /// Schema name for schema versions table.
        /// This is defaulted to the target data platform's default schema.
        /// </summary>
        public string MetaSchemaName { get; set; } = null;

        /// <summary>
        /// Table name for schema versions table.
        /// This is defaulted to __yuniql_schema_version in all target data platforms.
        /// </summary>
        public string MetaTableName { get; set; } = null;

        /// <summary>
        /// When true, forces to skip the the last failed script file and run from next available script in the failed version
        /// </summary>
        public bool? IsContinueAfterFailure { get; set; } = null;

        /// <summary>
        /// Transaction mode to use in the migration. Valid options are session, version and statement.
        /// When session, uses single transaction for entire migration run. 
        /// When version, each version is executed in one transaction.
        /// When statement, no explicit transaction is created for migration run.
        /// This is defaulted to session.
        /// </summary>
        public string TransactionMode { get; set; } = TRANSACTION_MODE.SESSION;

        /// <summary>
        /// When true, migration will fail if the _draft directory is not empty. This option ideal when targeting staging/production environment.
        /// This is defaulted to false.
        /// </summary>
        public bool IsRequiredClearedDraft { get; set; } = false;

        /// <summary>
        /// When true, action would be executed. This is required when executing potentially damaging actions such as yuniql-erase and yuniql-drop.
        /// This is defaulted to false.
        /// </summary>
        public bool IsForced { get; set; } = false;
    }
}
