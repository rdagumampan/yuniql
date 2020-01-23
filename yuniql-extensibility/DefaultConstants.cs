namespace Yuniql.Extensibility
{
    /// <summary>
    /// Globa default values.
    /// </summary>
    public static class DefaultConstants {

        /// <summary>
        /// Default command timeout in seconds.
        /// </summary>
        public const int CommandTimeoutSecs = 30;

        /// <summary>
        /// Default batch size of bulk load operations.
        /// </summary>
        public const int BatchSize = 0;
        
        /// <summary>
        /// Default CSV file delimiter.
        /// </summary>
        public const string Delimiter = ",";
    }
};