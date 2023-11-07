using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using Microsoft.Extensions.Logging;

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
            builder.Services.AddSingleton<XmlSplitterViewModel>();
            builder.Services.AddSingleton(FolderPicker.Default);
            builder.Services.AddMauiBlazorWebView();
#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#else
            builder.Logging.AddConsole();
#endif

            builder.Services.AddSingleton(new MainPage());
            return builder.Build();
        }
    }
}
