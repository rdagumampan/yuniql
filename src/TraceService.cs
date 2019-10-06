using System;

namespace ArdiLabs.Yuniql
{
    public class TraceService {
        public static void Info(string message, object payload = null) {
            Console.WriteLine($"INFO   {DateTime.UtcNow.ToString("o")}   {message}");
        }
        
        public static void Debug(string message, object payload = null)
        {
            Console.WriteLine($"INFO   {DateTime.UtcNow.ToString("o")}   {message}");
        }
    }
}
