namespace Yuniql.Extensions
{
    public sealed class TraceSettings
    {
        private static readonly TraceSettings instance = new TraceSettings();
        static TraceSettings()
        {
        }
        private TraceSettings()
        {
        }

        public bool IsDebugEnabled { get; set; }

        public static TraceSettings Instance
        {
            get
            {
                return instance;
            }
        }
    }
}
