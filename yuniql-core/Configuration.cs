using System.Collections.Generic;

namespace Yuniql.Core
{
    public class Configuration
    {
        /// <summary>
        /// The directory where yuniql-based migration project is placed.
        /// This is a required property.
        /// </summary>
        public string WorkspacePath { get; set; } = string.Empty;

        /// <summary>
        /// Target database platform. Value can be `sqlserver`,`postgresql`, or `mysql`. Default is `sqlserver`.
        /// </summary>
        public string Platform { get; set; } = "sqlserver";

        /// <summary>
        /// The connection string to the target database server.
        /// This is a required property.
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// When true, automatically creates database in target database server.
        /// </summary>
        public bool AutoCreateDatabase { get; set; } = false;

        /// <summary>
        /// Runs migration only up to the version specified in this property. Defaul is Null and will run upto latest.
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
        /// </summary>
        public bool VerifyOnly { get; set; } = false;

        /// <summary>
        /// Bulk file values separator to use when parsing CSV bulk import files. Default is comma ",".
        /// </summary>
        public string BulkSeparator { get; set; } = ",";

        /// <summary>
        /// The size of each batch when performing bulk load. Default is 100 rows.
        /// This may not be used in non-sqlserver platforms.
        /// </summary>
        public int BulkBatchSize { get; set; } = 0;

        /// <summary>
        /// The time it taks to wait for one commend to execute before it expires and throws error.
        /// Use this prorty to adjust time out when you expect a long running migration execution.
        /// </summary>
        public int CommandTimeout { get; set; } = 30;

        /// <summary>
        /// Enrich trace messages with raw sql statements and more verbose diagnostic messages.
        /// Use this when you are investigating some failed migrations.
        /// </summary>
        public bool DebugTraceMode { get; set; } = false;

        /// <summary>
        /// Describes the source of migration applied to target database.
        /// This is defaulted to yuniql-aspnetcore and its readonly property
        /// </summary>
        public string AppliedByTool { get { return "yuniql-aspnetcore"; } }

        /// <summary>
        /// Describes the version of source of migration applied to target database.
        /// </summary>
        public string AppliedByToolVersion { get { return typeof(Configuration).Assembly.GetName().Version.ToString(); } }

        /// <summary>
        /// Environment to target when running migration with environment-aware scripts.
        /// See https://github.com/rdagumampan/yuniql/wiki/environment-aware-scripts
        /// </summary>
        public string Environment { get; set; } = null;

        /// <summary>
        /// Schema name for schema versions table.
        /// </summary>
        public string MetaSchemaName { get; set; } = null;

        /// <summary>
        /// Table name for schema versions table.
        /// </summary>
        public string MetaTableName { get; set; } = null;

        /// <summary>
        /// When true, forces to skip the the last failed script file and run from next available script in the failed version
        /// </summary>
        public bool? ContinueAfterFailure { get; set; } = null;

        /// <summary>
        /// Transaction mode to use in the migration. 
        /// When full, uses single transaction for entire migration run. 
        /// When partial, each version is executed in one transaction.
        /// When none, no explicit transaction is created for migration run.
        /// </summary>
        public string TransactionMode { get; set; } = TRANSACTION_MODE.FULL;
    }

}
