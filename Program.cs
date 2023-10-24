using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;

namespace BaXmlSplitter
{
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
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddApplicationInsights(options => options.ConnectionString = configuration.ConnectionString));
            var applicationInsights = Remote.ApplicationInsights;
            configuration.ConnectionString = applicationInsights?.Properties.ConnectionString;
            var telemetryClient = new TelemetryClient(configuration);
            var logger = loggerFactory.CreateLogger<XmlSplitter>();
            Application.ThreadException += (sender, e) =>
            {
                telemetryClient.TrackException(e.Exception);
                ProgramLog.UnhandledException(logger, sender, e.Exception);
            };
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                if (e.ExceptionObject is not Exception exception) return;
                telemetryClient.TrackException(exception);
                ProgramLog.UnhandledException(logger, sender, exception);
            };
            using var xmlSplitter = new XmlSplitter(telemetryClient);
            Application.Run(xmlSplitter);
        }
    }

    internal static class ProgramLog
    {
        public static readonly Action<ILogger, object, Exception?> UnhandledException =
            LoggerMessage.Define<object>(LogLevel.Error, new EventId(0, nameof(UnhandledException)), "Unhandled exception {Object}");
    }
}