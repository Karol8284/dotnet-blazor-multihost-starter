using BlazorMultihostStarter.Client.Authentication;

namespace BlazorMultihostStarter;

public sealed class MauiEntraOptions
{
    public string ApiBaseAddress { get; set; } = string.Empty;

    public EntraClientOptions Entra { get; set; } = new();

    public void Validate(bool isDevelopment)
    {
        Entra.Validate();

        if (!Guid.TryParse(MauiEntraRegistration.ClientId, out var configuredClientId)
            || configuredClientId == Guid.Empty
            || !string.Equals(Entra.ClientId, MauiEntraRegistration.ClientId, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Set the MAUI client ID consistently in appsettings.json and MauiEntraOptions.cs before building.");
        }

        if (!Uri.TryCreate(ApiBaseAddress, UriKind.Absolute, out var apiBaseAddress))
        {
            throw new InvalidOperationException("Configure ApiBaseAddress as an absolute URL.");
        }

        if (!isDevelopment && !string.Equals(apiBaseAddress.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Release MAUI builds must use an HTTPS API address.");
        }
    }
}

public static class MauiEntraRegistration
{
    public const string ClientId = "00000000-0000-0000-0000-000000000000";

    public const string BundleIdentifier = "com.example.blazormultihoststarter";

    public const string AndroidRedirectScheme = "msal" + ClientId;

    public static string RedirectUri
    {
        get
        {
#if WINDOWS
            return $"ms-appx-web://microsoft.aad.brokerplugin/{ClientId}";
#elif IOS || MACCATALYST
            return $"msauth.{BundleIdentifier}://auth";
#else
            return $"{AndroidRedirectScheme}://auth";
#endif
        }
    }
}

public static class MauiAuthenticationPlatform
{
    public static object? ParentWindow { get; set; }
}
