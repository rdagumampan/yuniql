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

        ///<inheritdoc/>
        public FileTraceService()
        {
            _traceSessionId = DateTime.Now.ToString("MMddyyyy-HHmmss");
        }

        ///<inheritdoc/>
        public bool IsDebugEnabled { get; set; }

        ///<inheritdoc/>
        public bool IsTraceSensitiveData { get; set; }

        ///<inheritdoc/>
        public bool IsTraceToFile { get; set; }

        ///<inheritdoc/>
        public bool IsTraceToDirectory { get; set; }

        private string _traceDirectory;
        ///<inheritdoc/>
        public string TraceDirectory 
        {
            get
            {
                return _traceDirectory;
            }
            set
            {
                //when user specified location but it does not exist
                if (value != null && !Directory.Exists(value))
                {
                    Warn($"The provided trace directory {value} does not exist. " +
                        $"Generated logs will be saved in the current directory {Environment.CurrentDirectory}.");
                } else if (Directory.Exists(value))
                {
                    //an existing trace directory will be used
                    _traceDirectory = value;
                }
            }
        }

        ///<inheritdoc/>
        public void Debug(string message, object payload = null)
        {
            if (IsDebugEnabled)
            {
                var traceMessage = $"DBG   {DateTime.UtcNow.ToString("u")}   {message}{Environment.NewLine}";

                if (IsTraceToFile)
                {
                    var traceFile = GetTraceSessionFilePath();
                    File.AppendAllText(traceFile, traceMessage);
                }

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(traceMessage);
                Console.ResetColor();
            }
        }

        ///<inheritdoc/>
        public void Info(string message, object payload = null)
        {
            var traceMessage = $"INF   {DateTime.UtcNow.ToString("u")}   {message}{Environment.NewLine}";

            if (IsTraceToFile)
            {
                var traceFile = GetTraceSessionFilePath();
                File.AppendAllText(traceFile, traceMessage);
            }

            Console.Write(traceMessage);
        }


        ///<inheritdoc/>
        public void Warn(string message, object payload = null)
        {
            var traceMessage = $"WRN   {DateTime.UtcNow.ToString("u")}   {message}{Environment.NewLine}";

            if (IsTraceToFile)
            {
                var traceFile = GetTraceSessionFilePath();
                File.AppendAllText(traceFile, traceMessage);
            }

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(traceMessage);
            Console.ResetColor();
        }

        ///<inheritdoc/>
        public void Error(string message, object payload = null)
        {
            var traceMessage = $"ERR   {DateTime.UtcNow.ToString("u")}   {message}{Environment.NewLine}";

            if (IsTraceToFile)
            {
                var traceFile = GetTraceSessionFilePath();
                File.AppendAllText(traceFile, traceMessage);
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(traceMessage);
            Console.ResetColor();
        }

        ///<inheritdoc/>
        public void Success(string message, object payload = null)
        {
            var traceMessage = $"INF   {DateTime.UtcNow.ToString("u")}   {message}{Environment.NewLine}";

            if (IsTraceToFile)
            {
                var traceFile = GetTraceSessionFilePath();
                File.AppendAllText(traceFile, traceMessage);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(traceMessage);
            Console.ResetColor();
        }

        private string GetTraceSessionFilePath()
        {
            var traceFilePath = string.Empty;
            if (IsTraceToDirectory && TraceDirectory != null)
            {
                traceFilePath = Path.Combine(TraceDirectory, $"yuniql-log-{_traceSessionId}.txt");
            }
            else
            {
                traceFilePath = Path.Combine(Environment.CurrentDirectory, $"yuniql-log-{_traceSessionId}.txt");
            }

            if (!File.Exists(traceFilePath))
            {
                Console.WriteLine($"INF   {DateTime.UtcNow.ToString("u")}   Trace logs for the current session is located at {traceFilePath}.");
            }

            return traceFilePath;
        }

    }
}
