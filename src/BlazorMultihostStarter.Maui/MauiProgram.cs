using BlazorMultihostStarter.Client;
using BlazorMultihostStarter.Client.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Maui.Storage;

namespace BlazorMultihostStarter;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        using (var settings = FileSystem.OpenAppPackageFileAsync("appsettings.json").GetAwaiter().GetResult())
        {
            builder.Configuration.AddJsonStream(settings);
        }

        var settingsOptions = builder.Configuration.GetSection("Maui")
            .Get<MauiEntraOptions>() ?? new MauiEntraOptions();

#if DEBUG
        if (string.IsNullOrWhiteSpace(settingsOptions.ApiBaseAddress))
        {
#if ANDROID
            settingsOptions.ApiBaseAddress = "http://10.0.2.2:5080/";
#else
            settingsOptions.ApiBaseAddress = "http://localhost:5080/";
#endif
        }
#endif

#if DEBUG
        settingsOptions.Validate(isDevelopment: true);
#else
        settingsOptions.Validate(isDevelopment: false);
#endif

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddSingleton<IUserAuthenticationSession>(services =>
            new MauiEntraAuthenticationSession(
                settingsOptions.Entra,
                services.GetRequiredService<ILogger<MauiEntraAuthenticationSession>>()));
        builder.Services.AddTasksApiClient(new Uri(settingsOptions.ApiBaseAddress));

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
