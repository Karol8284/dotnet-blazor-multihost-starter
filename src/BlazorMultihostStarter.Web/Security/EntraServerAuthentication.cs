using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace BlazorMultihostStarter.Web.Security;

public static class EntraServerAuthentication
{
    public const string ServerClientIdConfigurationKey = "Authentication:Entra:ServerClientId";
    public const string ServerClientSecretConfigurationKey = "Authentication:Entra:ServerClientSecret";
    public const string CookieScheme = CookieAuthenticationDefaults.AuthenticationScheme;

    public static bool IsConfigured(IConfiguration configuration, string? tenantId) =>
        Guid.TryParse(tenantId, out var parsedTenantId)
        && parsedTenantId != Guid.Empty
        && Guid.TryParse(configuration[ServerClientIdConfigurationKey], out var parsedClientId)
        && parsedClientId != Guid.Empty
        && !string.IsNullOrWhiteSpace(configuration[ServerClientSecretConfigurationKey]);

    public static IServiceCollection AddEntraInteractiveServerAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        string tenantId)
    {
        var clientId = configuration[ServerClientIdConfigurationKey];
        var clientSecret = configuration[ServerClientSecretConfigurationKey];
        if (!IsConfigured(configuration, tenantId))
        {
            throw new InvalidOperationException(
                "Interactive Server requires Authentication:Entra:ServerClientId and ServerClientSecret. Store the secret outside source control.");
        }

        var parsedClientId = Guid.Parse(clientId!);
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieScheme;
            options.DefaultSignInScheme = CookieScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieScheme, options =>
        {
            options.Cookie.Name = "BlazorMultihostStarter.ServerAuth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        })
        .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            options.Authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";
            options.ClientId = parsedClientId.ToString("D");
            options.ClientSecret = clientSecret;
            options.ResponseType = "code";
            options.UsePkce = true;
            options.MapInboundClaims = false;
            options.GetClaimsFromUserInfoEndpoint = true;
            options.SaveTokens = false;
            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                NameClaimType = "name",
                RoleClaimType = "roles"
            };
        });

        services.AddCascadingAuthenticationState();
        services.AddAuthorization();
        return services;
    }
}
