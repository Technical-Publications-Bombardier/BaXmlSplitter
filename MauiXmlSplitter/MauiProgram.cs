#if ANDROID
using Android.Runtime;
#endif
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using BlazorBootstrap;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using MauiXmlSplitter.Models;
using MauiXmlSplitter.Resources;
using MauiXmlSplitter.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
// ReSharper disable once RedundantUsingDirective
using Microsoft.ApplicationInsights;

namespace MauiXmlSplitter;

/// <summary>
///     Xml Splitter program
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
    ///     Creates the MAUI application.
    /// </summary>
    /// <returns></returns>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        var config = new ConfigurationBuilder()
            .AddJsonStream(
                Assembly.GetExecutingAssembly().GetManifestResourceStream("MauiXmlSplitter.appsettings.json")!)
            .Build();
        builder.Configuration.AddConfiguration(config);
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit();
        builder.Services.AddSingleton(new MainPage());
        builder.Services.AddLocalization();
        builder.Services.AddTransient<BaXmlDocument>(_ => new BaXmlDocument
        {
            ResolveEntities = false
        });
        builder.Services.AddSingleton<CultureInfo>(_ =>
            new CultureInfo(Preferences.Default.Get(nameof(SettingsViewModel.Culture),
                CultureInfo.CurrentCulture.TwoLetterISOLanguageName)));
        builder.Services.AddSingleton<ModalService>();
        builder.Services.AddSingleton<ConcurrentDictionary<DateTime, LogRecord>>();
        builder.Services.AddSingleton<ILogger<XmlSplitterViewModel>>(services =>
        {
            var logs = services.GetRequiredService<ConcurrentDictionary<DateTime, LogRecord>>();
            return SynchronizationContext.Current != null
                ? new BaLogger(SynchronizationContext.Current, logs, LogLevel.Trace)
                : default!;
        });
        builder.Services.AddSingleton<ConcurrentBag<XmlSplitReportEntry>>();
        builder.Services.AddSingleton<IXmlSplitReport<XmlSplitter>>(services =>
        {
            var report = services.GetRequiredService<ConcurrentBag<XmlSplitReportEntry>>();
            return SynchronizationContext.Current != null
                ? new XmlSplitReport(SynchronizationContext.Current, string.Empty)
                : default!;
        });
        builder.Services.AddSingleton<XmlSplitterViewModel>(services =>
        {
            var locale = services.GetRequiredService<CultureInfo>();
            var logs = services.GetRequiredService<ConcurrentDictionary<DateTime, LogRecord>>();
            var logger = services.GetRequiredService<ILogger<XmlSplitterViewModel>>();
            var report = services.GetRequiredService<IXmlSplitReport<XmlSplitter>>();
            var modalService = services.GetRequiredService<ModalService>();
            return SynchronizationContext.Current != null
                ? new XmlSplitterViewModel(locale, logs, logger, modalService, SynchronizationContext.Current, report)
                : default!;
        });
        builder.Services.AddSingleton<SettingsViewModel>();
        builder.Services.AddSingleton(FolderPicker.Default);
        builder.Services.AddBlazorContextMenu();
        builder.Services.AddBlazorBootstrap();
        builder.Services.AddMauiBlazorWebView();
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
        builder.Configuration.Bind("DetailedErrors", "true");

#else
        builder.Services.AddApplicationInsightsTelemetryWorkerService((a) => a.ConnectionString =
 builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);
        builder.Services.AddLogging(logging => logging.AddApplicationInsights(
            telConfig => telConfig.ConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"],
            options => { }));
        builder.Logging.AddFilter<ApplicationInsightsLoggerProvider>(AppResources.XmlSplitter, LogLevel.Trace);
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
        Microsoft.UI.Xaml.Application.Current.UnhandledException += (sender, args) =>
        {
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
        logger.LogDebug(AppResources.MauiProgramCreateMauiApp);
        return host;
    }
}