namespace Yuniql.Core
{
    /// <summary>
    /// Global constants used for token replacement in sql statements
    /// </summary>
    public static class CONSTANTS {

        /// <summary>
        /// Token for target database name
        /// </summary>
        public const string YUNIQL_DB_NAME = "YUNIQL_DB_NAME";

        /// <summary>
        /// Token for schema name to store schema version tracking table.
        /// </summary>
        public const string YUNIQL_SCHEMA_NAME = "YUNIQL_SCHEMA_NAME";

        /// <summary>
        /// Token for table name to store schem version tracking data.
        /// </summary>
        public const string YUNIQL_TABLE_NAME = "YUNIQL_TABLE_NAME";
    }
}
