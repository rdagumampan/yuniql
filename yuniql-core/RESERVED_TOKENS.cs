namespace Yuniql.Core
{
    /// <summary>
    /// Global constants used for token replacement in sql statements
    /// </summary>
    public static class RESERVED_TOKENS
    {

        /// <summary>
        /// Token for database name value when performing queries in version tracking table
        /// </summary>
        public const string YUNIQL_DB_NAME = "YUNIQL_DB_NAME";

        /// <summary>
        /// Token for schema name value when performing queries in version tracking table
        /// </summary>
        public const string YUNIQL_SCHEMA_NAME = "YUNIQL_SCHEMA_NAME";

        /// <summary>
        /// Token for table name value when performing queries in version tracking table
        /// </summary>
        public const string YUNIQL_TABLE_NAME = "YUNIQL_TABLE_NAME";

        /// <summary>
        /// Token for sequence id value when performing queries in version tracking table
        /// </summary>
        public const string YUNIQL_SEQUENCE_ID = "YUNIQL_SEQUENCE_ID";

        /// <summary>
        /// Token for version value when performing queries in version tracking table
        /// </summary>
        public const string YUNIQL_VERSION = "YUNIQL_VERSION";

        /// <summary>
        /// Token for applied utc date value when performing queries in version tracking table
        /// </summary>
        public const string YUNIQL_APPLIED_ON_UTC = "YUNIQL_APPLIED_ON_UTC";

        /// <summary>
        /// Token for applied by user value when performing queries in version tracking table
        /// </summary>
        public const string YUNIQL_APPLIED_BY_USER = "YUNIQL_APPLIED_BY_USER";

        /// <summary>
        /// Token for applied by tool value when performing queries in version tracking table
        /// </summary>
        public const string YUNIQL_APPLIED_BY_TOOL = "YUNIQL_APPLIED_BY_TOOL";

        /// <summary>
        /// Token for applied by tool version value when performing queries in version tracking table
        /// </summary>
        public const string YUNIQL_APPLIED_BY_TOOL_VERSION = "YUNIQL_APPLIED_BY_TOOL_VERSION";

        /// <summary>
        /// Token for status value when performing queries in version tracking table
        /// </summary>
        public const string YUNIQL_STATUS = "YUNIQL_STATUS";

        /// <summary>
        /// Token for duration ms value when performing queries in version tracking table
        /// </summary>
        public const string YUNIQL_DURATION_MS = "YUNIQL_DURATION_MS";

        /// <summary>
        /// Token for directoru checksum when performing queries in version tracking table
        /// </summary>
        public const string YUNIQL_CHECKSUM = "YUNIQL_CHECKSUM";

        /// <summary>
        /// Token for failed script value when performing queries in version tracking table
        /// </summary>
        public const string YUNIQL_FAILED_SCRIPT_PATH = "YUNIQL_FAILED_SCRIPT_PATH";

        /// <summary>
        /// Token for failed script error value when performing queries in version tracking table
        /// </summary>
        public const string YUNIQL_FAILED_SCRIPT_ERROR = "YUNIQL_FAILED_SCRIPT_ERROR";

        /// <summary>
        /// Token for additional artifacts value when performing queries in version tracking table
        /// </summary>
        public const string YUNIQL_ADDITIONAL_ARTIFACTS = "YUNIQL_ADDITIONAL_ARTIFACTS";
    }
}
