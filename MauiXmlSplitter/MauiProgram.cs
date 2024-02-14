using System.Collections.Concurrent;
using System.Reflection;
using BlazorBootstrap;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using Fluxor;
using MauiXmlSplitter.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
// ReSharper disable once RedundantUsingDirective
using Microsoft.ApplicationInsights;

namespace MauiXmlSplitter;

/// <summary>
/// Xml Splitter program
/// </summary>
public static class MauiProgram
{
#if WINDOWS
    private static Exception _lastFirstChanceException;
#endif
#if !DEBUG
    public static event UnhandledExceptionEventHandler? UnhandledException;
#endif

    /// <summary>
    /// Creates the MAUI application.
    /// </summary>
    /// <returns></returns>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        var config = new ConfigurationBuilder()
            .AddJsonStream(
                Assembly.GetExecutingAssembly().GetManifestResourceStream("MauiXmlSplitter.appsettings.json")!)
            .Build();
        builder.Services.AddFluxor(options => options.ScanAssemblies(typeof(MauiProgram).Assembly));
        builder.Configuration.AddConfiguration(config);
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont(@".\Resources\Fonts\Gabarito\Gabarito-VariableFont_wght.ttf",
                    "Gabarito-VariableFont_wght");
                fonts.AddFont(@".\Resources\Fonts\MonoidNerdFont\MonoidNerdFont-Bold.ttf", "MonoidNerdFont-Bold");
                fonts.AddFont(@".\Resources\Fonts\MonoidNerdFont\MonoidNerdFont-Italic.ttf", "MonoidNerdFont-Italic");
                fonts.AddFont(@".\Resources\Fonts\MonoidNerdFont\MonoidNerdFont-Regular.ttf", "MonoidNerdFont-Regular");
                fonts.AddFont(@".\Resources\Fonts\MonoidNerdFont\MonoidNerdFont-Retina.ttf", "MonoidNerdFont-Retina");
            });
        builder.Services.AddSingleton(new MainPage());
        builder.Services.AddSingleton<ConcurrentDictionary<DateTime, LogRecord>>();
        builder.Services.AddSingleton<ModalService>();
        builder.Services.AddSingleton<ILogger<XmlSplitterViewModel>>(services =>
        {
            var logs = services.GetRequiredService<ConcurrentDictionary<DateTime, LogRecord>>();
            return new BaLogger(logs, LogLevel.Trace);
        });
        builder.Services.AddSingleton<XmlSplitterViewModel>();
        builder.Services.AddSingleton(FolderPicker.Default);
        builder.Services.AddBlazorContextMenu();
        builder.Services.AddBlazorBootstrap();
        builder.Services.AddMauiBlazorWebView();
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
        builder.Configuration.Bind("DetailedErrors", "true");

#else
        builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);
        builder.Services.AddLogging(logging => logging.AddApplicationInsights(
            config => config.ConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"],
            options => { }));
        builder.Logging.AddFilter<ApplicationInsightsLoggerProvider>("XmlSplitter", LogLevel.Trace);
        builder.Services.AddApplicationInsightsTelemetryWorkerService();
        AppDomain.CurrentDomain.UnhandledException += (sender, args) => UnhandledException?.Invoke(sender, args);
        TaskScheduler.UnobservedTaskException += (sender, args) =>
            UnhandledException?.Invoke(sender!, new UnhandledExceptionEventArgs(args.Exception, false));
#if IOS || MACCATALYST
            ObjCRuntime.Runtime.MarshalManagedException += (_, args) => args.ExceptionMode =
 ObjCRuntime.MarshalManagedExceptionMode.UnwindNativeCode;
#elif ANDROID
        AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
        {
            args.Handled = true; // Suppress the exception from being propagated to the managed thread
            UnhandledException?.Invoke(sender!, new UnhandledExceptionEventArgs(args.Exception, true));
        };
#elif WINDOWS
            AppDomain.CurrentDomain.FirstChanceException += (_, args) => _lastFirstChanceException = args.Exception;
            Microsoft.UI.Xaml.Application.Current.UnhandledException += (sender, args) => {
                var exception = args.Exception;
                if (exception.StackTrace is null)
                {
                    exception = _lastFirstChanceException;
                }
                UnhandledException?.Invoke(sender, new UnhandledExceptionEventArgs(exception, true));
            };
#endif
#endif
        var host = builder.Build();
        var logger = host.Services.GetRequiredService<ILogger<XmlSplitterViewModel>>();
        logger.LogDebug("MauiProgram.CreateMauiApp()");
        return host;
    }
}