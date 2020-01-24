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

        /// <summary>
        /// When true, debug messages are always written in Trace logs.
        /// </summary>
        public bool IsDebugEnabled { get; set; } = false;

        /// <summary>
        /// Writes informational messages.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="payload">JSON structured information to support the trace entry.</param>
        public void Info(string message, object payload = null)
        {
            var traceFile = GetTraceSessionFilePath();
            var traceMessage = $"INF   {DateTime.UtcNow.ToString("o")}   {message}{Environment.NewLine}";

            File.AppendAllText(traceFile, traceMessage);
            Console.Write(traceMessage);
        }

        /// <summary>
        /// Writes error messages.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="payload">JSON structured information to support the trace entry.</param>
        public void Error(string message, object payload = null)
        {
            var traceFile = GetTraceSessionFilePath();
            var traceMessage = $"ERR   {DateTime.UtcNow.ToString("o")}   {message}{Environment.NewLine}";

            File.AppendAllText(traceFile, traceMessage);
            Console.Write(traceMessage);
        }

        /// <summary>
        /// Writes debug messages.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="payload">JSON structured information to support the trace entry.</param>
        public void Debug(string message, object payload = null)
        {
            if (IsDebugEnabled)
            {
                var traceFile = GetTraceSessionFilePath();
                var traceMessage = $"DBG   {DateTime.UtcNow.ToString("o")}   {message}{Environment.NewLine}";

                File.AppendAllText(traceFile, traceMessage);
                Console.Write(traceMessage);
            }
        }

        private string GetTraceSessionFilePath()
        {
            return Path.Combine(Environment.CurrentDirectory, $"yuniql-migration-{_traceSessionId}.txt");
        }

    }
}
