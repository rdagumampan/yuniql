using ArdiLabs.Yuniql.Extensibility;
using System;

namespace ArdiLabs.Yuniql.Core
{
    public class TraceService : ITraceService
    {
        private string _traceSessionId;
        public TraceService()
        {
            _traceSessionId = "yuniql-log-" + DateTime.Now.ToString("MMddyyyyHHmmss");
        }

        public bool IsDebugEnabled { get; set; } = false;

        public void Info(string message, object payload = null)
        {
            Console.WriteLine($"INF   {DateTime.UtcNow.ToString("o")}   {message}");
        }

        public void Error(string message, object payload = null)
        {
            Console.WriteLine($"ERR   {DateTime.UtcNow.ToString("o")}   {message}");
        }

        public void Debug(string message, object payload = null)
        {
            if (IsDebugEnabled)
            {
                Console.WriteLine($"DBG   {DateTime.UtcNow.ToString("o")}   {message}");
            }
        }
    }
}
