namespace ArdiLabs.Yuniql
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

    public sealed class TargetDatabase
    {
        private static readonly TargetDatabase instance = new TargetDatabase();
        static TargetDatabase()
        {
        }
        private TargetDatabase()
        {
        }

        public static TargetDatabase Instance
        {
            get
            {
                return instance;
            }
        }

        public void Initialize()
        {
        }

        public void Complete()
        {
        }
    }
}
