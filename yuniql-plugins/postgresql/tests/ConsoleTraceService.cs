using System;
using Yuniql.Extensibility;

namespace Yuniql.PosgreSQL.Tests
{
    public class ConsoleTraceService : ITraceService
    {
        public bool IsDebugEnabled { get; set; }

        public void Debug(string message, object payload = null)
        {
            Console.WriteLine($"DBG {message}");
        }

        public void Error(string message, object payload = null)
        {
            Console.WriteLine($"ERR {message}");
        }

        public void Info(string message, object payload = null)
        {
            Console.WriteLine($"INF {message}");
        }
    }
}
