using Microsoft.Extensions.Logging;

namespace MauiXmlSplitter.Data;

/// <summary>
/// A reusable log action for unhandled exceptions with a specific log level, event id, and message template.
/// </summary>
internal static class DataLog
{
    private const int EventIdOffset = 100;

    /// <summary>
    /// The token acquisition exception
    /// </summary>
    internal static readonly Action<ILogger, Exception?, Exception> TokenAcquisitionProblem =
        LoggerMessage.Define<Exception?>(LogLevel.Error,
            new EventId(EventIdOffset + 0, nameof(TokenAcquisitionProblem)),
            "Unable to acquire HashiCorp Cloud Platform token {Exception}");
}