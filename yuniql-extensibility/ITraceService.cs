namespace Yuniql.Extensibility
{
    /// <summary>
    /// Implement this interface to write trace information to desired sink or log store. 
    /// </summary>
    public interface ITraceService
    {
        /// <summary>
        /// When true, debug messages are always written in Trace logs.
        /// </summary>
        bool IsDebugEnabled { get; set; }

        /// <summary>
        /// When true, sensitive data is not getting redacted.
        /// </summary>
        bool IsTraceSensitiveData { get; set; }

        /// <summary>
        /// This parameter allows users to define the directory where the log files will be created.
        /// </summary>
        bool IsTraceToDirectory { get; set; }

        /// <summary>
        /// When false, the log file creation is disabled.
        /// </summary>
        bool IsTraceToFile { get; set; }

        /// <summary>
        /// The directory where log files created will be placed.
        /// </summary>
        string TraceDirectory { get; set; }

        /// <summary>
        /// Writes debug messages.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="payload">JSON structured information to support the trace entry.</param>
        void Debug(string message, object payload = null);

        /// <summary>
        /// Writes informational messages.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="payload">JSON structured information to support the trace entry.</param>
        void Info(string message, object payload = null);

        /// <summary>
        /// Writes warning messages.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="payload">JSON structured information to support the trace entry.</param>
        void Warn(string message, object payload = null);

        /// <summary>
        /// Writes error messages.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="payload">JSON structured information to support the trace entry.</param>
        void Error(string message, object payload = null);


        /// <summary>
        /// Writes success informational messages.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="payload">JSON structured information to support the trace entry.</param>
        void Success(string message, object payload = null);

    }
}
