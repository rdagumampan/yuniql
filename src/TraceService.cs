using System;

namespace ArdiLabs.Yuniql
{
    public class TraceService {
        public static void Info(string message, object payload = null) {
            Console.WriteLine($"INFO   {DateTime.UtcNow.ToString("o")}   {message}");
        }
        
        public static void Debug(string message, object payload = null)
        {
            Console.WriteLine($"DEBUG   {DateTime.UtcNow.ToString("o")}   {message}");
        }
    }

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
