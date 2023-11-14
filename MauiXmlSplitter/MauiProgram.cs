using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using MauiXmlSplitter.Shared;
using Microsoft.Extensions.Logging;
// ReSharper disable once RedundantUsingDirective
using Microsoft.ApplicationInsights;
using KristofferStrube.Blazor.Popper;
using System.Collections.Concurrent;
namespace MauiXmlSplitter
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Gabarito/Gabarito-VariableFont_wght.ttf", "Gabarito");
                    fonts.AddFont("MonoidNerdFont/MonoidNerdFont-Regular.ttf");
                    fonts.AddFont("MonoidNerdFont/MonoidNerdFont-Bold.ttf");
                    fonts.AddFont("MonoidNerdFont/MonoidNerdFont-Italic.ttf");
                    fonts.AddFont("MonoidNerdFont/MonoidNerdFont-Retina.ttf");
                });
            builder.Services.AddSingleton(new MainPage());
            builder.Services.AddSingleton<ConcurrentDictionary<DateTime, LogRecord>>();

            builder.Services.AddSingleton<ILogger<XmlSplitterViewModel>>(services =>
            {
                var logs = services.GetRequiredService<ConcurrentDictionary<DateTime, LogRecord>>();
                return new BaLogger(logs, LogLevel.Trace);
            });
            builder.Services.AddSingleton<XmlSplitterViewModel>();
            builder.Services.AddSingleton(FolderPicker.Default);
            builder.Services.AddScoped<Popper>();
            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddLogging(logging => logging.AddApplicationInsights());
#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#else
            //TODO: Add production logging
#endif
            var host = builder.Build();
            var logger = host.Services.GetRequiredService<ILogger<XmlSplitterViewModel>>();
            logger.LogDebug("MauiProgram.CreateMauiApp()");
            return host;
        }
    }
}
