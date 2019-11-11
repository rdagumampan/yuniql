using System;

namespace Yuniql.Extensions
{
    public class TraceService : ITraceService
    {
        public static void Info(string message, object payload = null)
        {
            Console.WriteLine($"INF   {DateTime.UtcNow.ToString("o")}   {message}");
        }

        public static void Error(string message, object payload = null)
        {
            Console.WriteLine($"ERR   {DateTime.UtcNow.ToString("o")}   {message}");
        }

        public static void Debug(string message, object payload = null)
        {
            Console.WriteLine($"DBG   {DateTime.UtcNow.ToString("o")}   {message}");
        }
    }
}
