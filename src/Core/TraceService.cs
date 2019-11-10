using ArdiLabs.Yuniql.Extensibility;
using System;

namespace ArdiLabs.Yuniql.Core
{
    public class TraceService : ITraceService
    {
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
            Console.WriteLine($"DBG   {DateTime.UtcNow.ToString("o")}   {message}");
        }
    }
}
