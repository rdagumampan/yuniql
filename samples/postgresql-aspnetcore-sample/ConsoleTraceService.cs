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

        public void Info(string message, object payload = null)
        {
            var traceMessage = $"INF   {DateTime.UtcNow.ToString("o")}   {message}{Environment.NewLine}";
            Console.Write(traceMessage);
        }

        public void Error(string message, object payload = null)
        {
            var traceMessage = $"ERR   {DateTime.UtcNow.ToString("o")}   {message}{Environment.NewLine}";
            Console.Write(traceMessage);
        }

        public void Debug(string message, object payload = null)
        {
            if (IsDebugEnabled)
            {
                var traceMessage = $"DBG   {DateTime.UtcNow.ToString("o")}   {message}{Environment.NewLine}";
                Console.Write(traceMessage);
            }
        }

        public void Success(string message, object payload = null)
        {
            var traceMessage = $"INF   {DateTime.UtcNow.ToString("u")}   {message}{Environment.NewLine}";
            Console.Write(traceMessage);
        }
    }
}
