using Yuniql.Extensibility;
using System;
using System.IO;

namespace Yuniql.Core
{
    /// <summary>
    /// Writes trace information into a text file in the current workspace directory. 
    /// </summary>
    public class FileTraceService : ITraceService
    {
        private string _traceSessionId;
        public FileTraceService()
        {
            _traceSessionId = DateTime.Now.ToString("MMddyyyy-HHmmss");
        }

        ///<inheritdoc/>
        public bool IsDebugEnabled { get; set; } = false;

        ///<inheritdoc/>
        public bool TraceSensitiveData { get; set; } = false;


        ///<inheritdoc/>
        public void Debug(string message, object payload = null)
        {
            if (IsDebugEnabled)
            {
                var traceFile = GetTraceSessionFilePath();
                var traceMessage = $"DBG   {DateTime.UtcNow.ToString("u")}   {message}{Environment.NewLine}";

                File.AppendAllText(traceFile, traceMessage);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(traceMessage);
                Console.ResetColor();
            }
        }

        ///<inheritdoc/>
        public void Info(string message, object payload = null)
        {
            var traceFile = GetTraceSessionFilePath();
            var traceMessage = $"INF   {DateTime.UtcNow.ToString("u")}   {message}{Environment.NewLine}";

            File.AppendAllText(traceFile, traceMessage);
            Console.Write(traceMessage);
        }


        ///<inheritdoc/>
        public void Warn(string message, object payload = null)
        {
            var traceFile = GetTraceSessionFilePath();
            var traceMessage = $"WRN   {DateTime.UtcNow.ToString("u")}   {message}{Environment.NewLine}";

            File.AppendAllText(traceFile, traceMessage);
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(traceMessage);
            Console.ResetColor();
        }

        ///<inheritdoc/>
        public void Error(string message, object payload = null)
        {
            var traceFile = GetTraceSessionFilePath();
            var traceMessage = $"ERR   {DateTime.UtcNow.ToString("u")}   {message}{Environment.NewLine}";

            File.AppendAllText(traceFile, traceMessage);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(traceMessage);
            Console.ResetColor();
        }

        ///<inheritdoc/>
        public void Success(string message, object payload = null)
        {
            var traceFile = GetTraceSessionFilePath();
            var traceMessage = $"INF   {DateTime.UtcNow.ToString("u")}   {message}{Environment.NewLine}";

            File.AppendAllText(traceFile, traceMessage);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(traceMessage);
            Console.ResetColor();
        }

        private string GetTraceSessionFilePath()
        {
            return Path.Combine(Environment.CurrentDirectory, $"yuniql-log-{_traceSessionId}.txt");
        }

    }
}
