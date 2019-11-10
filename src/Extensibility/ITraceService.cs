namespace ArdiLabs.Yuniql.Extensibility
{
    public interface ITraceService
    {
        bool IsDebugEnabled { get; set; }

        void Info(string message, object payload = null);

        void Error(string message, object payload = null);

        void Debug(string message, object payload = null);
    }
}
