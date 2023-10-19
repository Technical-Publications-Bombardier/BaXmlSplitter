namespace BaXmlSplitter
{
    internal enum Severity
    {
        Hint,
        Warning,
        Error,
        Fatal
    };

    internal class LogMessage(string message, Severity severity = Severity.Hint)
    {
        public string Message { get; set; } = message;
        public Severity Severity { get; set; } = severity;
    }
}