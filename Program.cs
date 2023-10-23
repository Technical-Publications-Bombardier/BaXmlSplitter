using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System.Text.Json;

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
            try
            {
                var applicationInsights = JsonSerializer.Deserialize<Remote.AzureResource>(Properties.Resources.ApplicationInsights);
                configuration.ConnectionString = applicationInsights?.Properties.ConnectionString;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            var telemetryClient = new TelemetryClient(configuration);
            Application.ThreadException += (sender, e) =>
            {
                telemetryClient.TrackException(e.Exception);
            };
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                if (e.ExceptionObject is Exception exception)
                {
                    telemetryClient.TrackException(exception);
                }
            };
            using var xmlSplitter = new XmlSplitter(telemetryClient);
            Application.Run(xmlSplitter);
        }
    }
}