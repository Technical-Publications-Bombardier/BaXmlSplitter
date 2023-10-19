using System.Reflection.Metadata;

namespace BaXmlSplitter
{
    internal enum Severity
    {
        Hint,
        Warning,
        Error,
        Fatal
    };

    internal class LogMessage
    {
        public string Message { get; set; }
        public Severity Severity { get; set; }

        public LogMessage(string message, Severity severity = Severity.Hint)
        {
            Message = message;
            Severity = severity;
        }
    }
}