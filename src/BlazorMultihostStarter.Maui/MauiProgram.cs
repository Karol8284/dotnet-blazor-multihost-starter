using BlazorMultihostStarter.Client;
using BlazorMultihostStarter.Client.Authentication;
using BlazorMultihostStarter.Client.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Maui.Storage;
using BlazorMultihostStarter.Shared.Tasks;
using BlazorMultihostStarter.Shared.UI;

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

        if (settingsOptions.Entra.IsValid)
        {
            builder.Services.AddSingleton(services =>
                new MauiEntraAuthenticationSession(
                    settingsOptions.Entra,
                    services.GetRequiredService<ILogger<MauiEntraAuthenticationSession>>()));
            builder.Services.AddSingleton<IUserAuthenticationSession>(services =>
                services.GetRequiredService<MauiEntraAuthenticationSession>());
            builder.Services.AddSingleton<IApiAccessTokenProvider>(services =>
                services.GetRequiredService<MauiEntraAuthenticationSession>());
            builder.Services.AddTasksApiClient(new Uri(settingsOptions.ApiBaseAddress));
        }
        else
        {
            builder.Services.AddSingleton<IUserAuthenticationSession>(
                new UnconfiguredAuthenticationSession(
                    "Configure valid Entra client IDs to sign in and synchronize server-backed tasks."));
            builder.Services.AddSingleton<ITasksClient>(
                new UnconfiguredTasksClient(
                    "The Tasks API is unavailable until valid Entra client IDs are configured."));
        }

        builder.Services.AddSharedUiServices();
        builder.Services.AddSharedTaskModes();

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
