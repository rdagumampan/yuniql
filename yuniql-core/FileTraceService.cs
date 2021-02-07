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

        private IDirectoryService _directoryService;

        private string _traceDirectory;

        public FileTraceService(IDirectoryService directoryService)
        {
            _traceSessionId = DateTime.Now.ToString("MMddyyyy-HHmmss");
            _directoryService = directoryService;
        }

        ///<inheritdoc/>
        public bool IsDebugEnabled { get; set; } = false;

        ///<inheritdoc/>
        public bool IsTraceSensitiveData { get; set; } = false;

        ///<inheritdoc/>
        public bool IsTraceSilent { get; set; } = false;

        ///<inheritdoc/>
        public string TraceDirectory 
        {
            get
            {
                return _traceDirectory;
            }
            set
            {
                if (_directoryService.Exists(value))
                {
                    _traceDirectory = value;
                }
                else if (value!=null)
                {
                    Warn($"The provided trace directory does not exist. " +
                        $"Generated logs will be saved in the current execution directory.");
                }
            }
        }

        ///<inheritdoc/>
        public void Debug(string message, object payload = null)
        {
            if (IsDebugEnabled)
            {
                var traceMessage = $"DBG   {DateTime.UtcNow.ToString("u")}   {message}{Environment.NewLine}";

                if (!IsTraceSilent)
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

            if (!IsTraceSilent)
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

            if (!IsTraceSilent)
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

            if (!IsTraceSilent)
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

            if (!IsTraceSilent)
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

            if (TraceDirectory != null)
            {
                return Path.Combine(TraceDirectory, $"yuniql-log-{_traceSessionId}.txt");
            }
            else
            {
                return Path.Combine(Environment.CurrentDirectory, $"yuniql-log-{_traceSessionId}.txt");
            }
        }

    }
}
