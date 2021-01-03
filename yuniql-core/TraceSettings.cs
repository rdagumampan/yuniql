namespace Yuniql.Core
{
    /// <summary>
    /// Global settings for checking the level of trace message to write.
    /// </summary>
    public sealed class TraceSettings
    {
        private static readonly TraceSettings instance = new TraceSettings();

        static TraceSettings()
        {
        }

        private TraceSettings()
        {
        }

        /// <summary>
        /// When true, trace messages will include debug messages.
        /// </summary>
        public bool IsDebugEnabled { get; set; }

        /// <summary>
        /// Global singleton instance of trace settings
        /// </summary>
        public static TraceSettings Instance
        {
            get
            {
                return instance;
            }
        }
    }
}
