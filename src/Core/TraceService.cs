using ArdiLabs.Yuniql.Extensibility;
using System;
using System.IO;

namespace ArdiLabs.Yuniql.Core
{
    public class TraceService : ITraceService
    {
        private string _traceSessionId;
        public TraceService()
        {
            _traceSessionId = DateTime.Now.ToString("MMddyyyyHHmmss");
        }

        public bool IsDebugEnabled { get; set; } = false;

        public void Info(string message, object payload = null)
        {
            var traceFile = GetTraceSessionFilePath();
            var traceMessage = $"INF   {DateTime.UtcNow.ToString("o")}   {message}{Environment.NewLine}";

            File.AppendAllText(traceFile, traceMessage);
            Console.WriteLine(traceMessage);
        }

        public void Error(string message, object payload = null)
        {
            var traceFile = GetTraceSessionFilePath();
            var traceMessage = $"ERR   {DateTime.UtcNow.ToString("o")}   {message}{Environment.NewLine}";

            File.AppendAllText(traceFile, traceMessage);
            Console.WriteLine(traceMessage);
        }

        public void Debug(string message, object payload = null)
        {
            if (IsDebugEnabled)
            {
                var traceFile = GetTraceSessionFilePath();
                var traceMessage = $"DBG   {DateTime.UtcNow.ToString("o")}   {message}{Environment.NewLine}";

                File.AppendAllText(traceFile, traceMessage);
                Console.WriteLine(traceMessage);
            }
        }

        private string GetTraceSessionFilePath()
        {
            return Path.Combine(Environment.CurrentDirectory, $"yuniql-log-{_traceSessionId}.txt");
        }

    }
}
