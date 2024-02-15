using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace MauiXmlSplitter.Shared;

internal class BaLogger(SynchronizationContext synchronizationContext, ConcurrentDictionary<DateTime, LogRecord> logs, LogLevel minimumLogLevel = LogLevel.Warning) : ILogger<XmlSplitterViewModel>
{
    public event Action? OnLogAdded;
    /// <summary>
    /// The verbosity
    /// </summary>
    private readonly LogLevel verbosity = minimumLogLevel;

    /// <summary>
    /// The scope information
    /// </summary>
    private static readonly AsyncLocal<ScopeInfo?> ScopeInformation = new();

    /// <summary>
    /// Writes a log entry.
    /// </summary>
    /// <typeparam name="TState">The type of the object to be written.</typeparam>
    /// <param name="logLevel">Entry will be written on this level.</param>
    /// <param name="eventId">ID of the event.</param>
    /// <param name="state">The entry to be written. Can be also an object.</param>
    /// <param name="exception">The exception related to this entry.</param>
    /// <param name="formatter">Function to create a <see cref="T:System.String" /> message of the <paramref name="state" /> and <paramref name="exception" />.</param>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = formatter(state, exception);
        Debug.WriteLine($"{logLevel}: {message}");
        _ = logs.TryAdd(DateTime.Now, new LogRecord(logLevel, message, exception));
        synchronizationContext.Post(_ => OnLogAdded?.Invoke(), null);
    }

    /// <summary>
    /// Checks if the given <paramref name="logLevel" /> is enabled.
    /// </summary>
    /// <param name="logLevel">Level to be checked.</param>
    /// <returns>
    ///   <c>true</c> if enabled.
    /// </returns>
    public bool IsEnabled(LogLevel logLevel) => logLevel >= verbosity;

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        // Create a new scope object with the state and the parent scope
        var newScope = new ScopeInfo(state, ScopeInformation.Value);

        // Set the current scope to the new one
        ScopeInformation.Value = newScope;

        // Return an IDisposable that will restore the previous scope when disposed
        return newScope;
    }

    /// <summary>
    /// A class that represents the scope information.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    protected class ScopeInfo(object state, ScopeInfo? parent) : IDisposable
    {
        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public object State { get; } = state;
        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public ScopeInfo? Parent { get; } = parent;

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            // Use a StringBuilder to build the string representation of the scope
            var builder = new StringBuilder();

            // Loop through the scope hierarchy and append the state of each scope
            var current = this;
            while (current != null)
            {
                if (builder.Length > 0)
                {
                    // Use a separator between each scope
                    builder.Insert(0, " => ");
                }

                builder.Insert(0, current.State);

                current = current.Parent;
            }

            return builder.ToString();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Restore the parent scope when disposed
            ScopeInformation.Value = Parent;
            GC.SuppressFinalize(this);
        }
    }

}

public record LogRecord(LogLevel LogLevel, string Message, Exception? Exception = null);