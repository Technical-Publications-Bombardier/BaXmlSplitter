using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BaXmlSplitter
{
    /// <summary>
    /// Starts the Bombardier XML Splitter
    /// </summary>
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ApplicationConfiguration.Initialize();
            using var configuration = TelemetryConfiguration.CreateDefault();
            var applicationInsights = Remote.ApplicationInsights;

            // Set the connection for Application Insights from remote configuration
            configuration.ConnectionString = applicationInsights?.Properties.ConnectionString;

            // Create a telemetry client to send telemetry data to Application Insights
            var telemetryClient = new TelemetryClient(configuration);

            // Create an in-memory channel to buffer and send telemetry asynchronously
            var channel = new InMemoryChannel();

            // Create a service collection to register dependencies
            var services = new ServiceCollection();

            // Configure the telemetry configuration to use the in-memory channel
            services.Configure<TelemetryConfiguration>(config => config.TelemetryChannel = channel);
            services.AddLogging(builder =>
            {
                builder.AddApplicationInsights(
                    // Add Application Insights as a logger provider
                    configureTelemetryConfiguration: config => config.ConnectionString = applicationInsights?.Properties.ConnectionString,
                    // Use the same connection string as before
                    configureApplicationInsightsLoggerOptions: _ => {
                        /* Default options are fine, hence discard */
                    }
                );
            });

            // Build the service provider from the service collection
            var serviceProvider = services.BuildServiceProvider();

            // Get an instance of ILoggerFactory from the service provider
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            // Create a logger for the XmlSplitter type using the logger factory
            var logger = loggerFactory.CreateLogger<XmlSplitter>();

            Application.ThreadException += (sender, e) =>
            {
                // Track any unhandled exception on the UI thread as ExceptionTelemetry
                telemetryClient.TrackException(e.Exception);
                // Log any unhandled exception on the UI thread using a predefined action
                ProgramLog.UnhandledException(logger, sender, e.Exception);
            };
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                // Check if the exception object is an actual Exception type
                if (e.ExceptionObject is not Exception exception) return;

                // Track any unhandled exception on any thread as ExceptionTelemetry
                telemetryClient.TrackException(exception);

                // Log any unhandled exception on any thread using a predefined action
                ProgramLog.UnhandledException(logger, sender, exception);
            };

            // Create an instance of XmlSplitter and pass the telemetry client and channel as parameters
            using var xmlSplitter = new XmlSplitter(telemetryClient, channel);

            // Run the XmlSplitter as the main form of the application
            Application.Run(xmlSplitter);
        }
    }

    /// <summary>
    /// A reusable log action for unhandled exceptions with a specific log level, event id, and message template.
    /// </summary>
    internal static class ProgramLog
    {
        /// <summary>
        /// The unhandled exception
        /// </summary>
        public static readonly Action<ILogger, object, Exception?> UnhandledException =
            LoggerMessage.Define<object>(LogLevel.Error, new EventId(0, nameof(UnhandledException)), "Unhandled exception {Object}");
    }
}