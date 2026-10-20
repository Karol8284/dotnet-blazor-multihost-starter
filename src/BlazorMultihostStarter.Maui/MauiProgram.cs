using BlazorMultihostStarter.Client;
using Microsoft.Extensions.Logging;

namespace BlazorMultihostStarter;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

#if ANDROID
        const string apiBaseAddress = "http://10.0.2.2:5080/";
#else
        const string apiBaseAddress = "http://localhost:5080/";
#endif

        builder.Services.AddTasksApiClient(new Uri(apiBaseAddress));

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
