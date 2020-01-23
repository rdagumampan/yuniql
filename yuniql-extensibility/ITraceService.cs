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
        /// Writes informational messages.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="payload">JSON structured information to support the trace entry.</param>
        void Info(string message, object payload = null);

        /// <summary>
        /// Writes error messages.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="payload">JSON structured information to support the trace entry.</param>
        void Error(string message, object payload = null);

        /// <summary>
        /// Writes debug messages.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="payload">JSON structured information to support the trace entry.</param>
        void Debug(string message, object payload = null);
    }
}
