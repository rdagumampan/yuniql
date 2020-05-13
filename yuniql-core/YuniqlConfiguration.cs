using System.Collections.Generic;

namespace Yuniql.Core
{
    public class YuniqlConfiguration
    {
        /// <summary>
        /// The directory where yuniql-based migration project is placed.
        /// This is a required property.
        /// </summary>
        public string WorkspacePath { get; set; }

        /// <summary>
        /// Target database platform. Value can be `sqlserver`,`postgresql`, or `mysql`. Default is `sqlserver`.
        /// </summary>
        public string Platform { get; set; } = "sqlserver";

        /// <summary>
        /// The connection string to the target database server.
        /// This is a required property.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// When true, automatically creates database in target database server.
        /// </summary>
        public bool AutoCreateDatabase { get; set; } = false;

        /// <summary>
        /// Runs migration only up to the version specified in this property. Defaul is Null and will run upto latest.
        /// </summary>
        public string TargetVersion { get; set; }

        /// <summary>
        /// The list of token key/value pairs to repair tokens in each script file. 
        /// </summary>
        public List<KeyValuePair<string, string>> Tokens { get; set; } = new List<KeyValuePair<string, string>>();

        /// <summary>
        /// When you run yuniql verify, it checks if all your versions can be executed without errors. 
        /// It runs through all the non-versioned script folders (except _init) and all migration steps that yuninql run takes but without committing the transaction. 
        /// All changes are rolledback after a successful verification run.
        /// </summary>
        public bool VerifyOnly { get; set; }

        /// <summary>
        /// Delimter to use when parsing CSV bulk import files. Default is comma ",".
        /// </summary>
        public string Delimiter { get; set; } = ",";

        /// <summary>
        /// The size of each batch when performing bulk load. Default is 100 rows.
        /// This may not be used in non-sqlserver platforms.
        /// </summary>
        public int BatchSize { get; set; } = 100;

        /// <summary>
        /// The time it taks to wait for one commend to execute before it expires and throws error.
        /// Use this prorty to adjust time out when you expect a long running migration execution.
        /// </summary>
        public int CommandTimeout { get; set; } = 30;

        /// <summary>
        /// Enrich trace messages with raw sql statements and more verbose diagnostic messages.
        /// Use this when you are investigating some failed migrations.
        /// </summary
        public bool DebugTraceMode { get; set; } = false;

        /// <summary>
        /// Environment code for environment-aware scripts.
        /// </summary>
        public string Environment { get; set; }

        /// <summary>
        /// Schema name for schema versions table. When empty, uses the default schema in the target data platform. 
        /// For example, dbo for SqlServer and public for PostgreSql
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// Table name for schema versions table. When empty, uses __yuniqldbversion.
        /// </summary>
        public string Table { get; set; }
    }

}
