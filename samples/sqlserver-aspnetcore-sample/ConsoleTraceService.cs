using System;
using Yuniql.Extensibility;

namespace aspnetcore_sample
{
    //TIP: You can implement custom ITraceService to capture the log and debug information during your migration run.
    //This us usefule if you wish to sink trace logs into your preferred provider ie. serilog, seq, or others.
    public class ConsoleTraceService : ITraceService
    {
        public ConsoleTraceService()
        {
        }

        public bool IsDebugEnabled { get; set; } = false;

        public bool IsTraceSensitiveData { get; set; } = true;

        public string TraceToDirectory { get; set; } = string.Empty;

        public bool IsTraceToFile { get; set; } = false;

        public void Debug(string message, object payload = null)
        {
            if (IsDebugEnabled)
            {
                var traceMessage = $"DBG   {DateTime.UtcNow.ToString("u")}   {message}{Environment.NewLine}";
                Console.Write(traceMessage);
            }
        }

        public void Info(string message, object payload = null)
        {
            var traceMessage = $"INF   {DateTime.UtcNow.ToString("u")}   {message}{Environment.NewLine}";
            Console.Write(traceMessage);
        }

        public void Warn(string message, object payload = null)
        {
            var traceMessage = $"WRN   {DateTime.UtcNow.ToString("u")}   {message}{Environment.NewLine}";
            Console.Write(traceMessage);
        }

        public void Error(string message, object payload = null)
        {
            var traceMessage = $"ERR   {DateTime.UtcNow.ToString("u")}   {message}{Environment.NewLine}";
            Console.Write(traceMessage);
        }

        public void Success(string message, object payload = null)
        {
            var traceMessage = $"INF   {DateTime.UtcNow.ToString("u")}   {message}{Environment.NewLine}";
            Console.Write(traceMessage);
        }
    }
}
