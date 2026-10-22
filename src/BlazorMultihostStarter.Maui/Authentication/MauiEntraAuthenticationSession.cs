using BlazorMultihostStarter.Client.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using Microsoft.Maui.Storage;
#if WINDOWS
using Microsoft.Identity.Client.Desktop;
#endif

namespace BlazorMultihostStarter;

public sealed class MauiEntraAuthenticationSession : IUserAuthenticationSession
{
    private readonly EntraClientOptions _options;
    private readonly ILogger<MauiEntraAuthenticationSession> _logger;
    private readonly Lazy<Task<IPublicClientApplication>> _application;

    public MauiEntraAuthenticationSession(
        EntraClientOptions options,
        ILogger<MauiEntraAuthenticationSession> logger)
    {
        options.Validate();
        _options = options;
        _logger = logger;
        _application = new Lazy<Task<IPublicClientApplication>>(CreateApplicationAsync);
    }

    public async Task<bool> IsSignedInAsync(CancellationToken cancellationToken = default)
    {
        return (await TryGetSilentTokenAsync(cancellationToken)) is not null;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        return await TryGetSilentTokenAsync(cancellationToken)
            ?? throw new AuthenticationRequiredException();
    }

    public async Task SignInAsync(CancellationToken cancellationToken = default)
    {
        var application = await _application.Value;
        try
        {
            if (await TryGetSilentTokenAsync(application, cancellationToken) is not null)
            {
                return;
            }

            await application
                .AcquireTokenInteractive([_options.ApiScope])
                .WithPrompt(Prompt.SelectAccount)
                .ExecuteAsync(cancellationToken);
        }
        catch (MsalException exception)
        {
            _logger.LogWarning("Interactive sign-in failed with MSAL error {ErrorCode}.", exception.ErrorCode);
            throw new AuthenticationOperationException(
                "Microsoft Entra sign-in failed. Check the tenant, client registration, and redirect URI settings.",
                exception);
        }
    }

    public async Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        var application = await _application.Value;
        var accounts = await application.GetAccountsAsync();

        try
        {
            foreach (var account in accounts)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await application.RemoveAsync(account);
            }
        }
        catch (MsalException exception)
        {
            _logger.LogWarning("Sign-out failed with MSAL error {ErrorCode}.", exception.ErrorCode);
            throw new AuthenticationOperationException(
                "The Microsoft Entra session could not be cleared.",
                exception);
        }
    }

    private async Task<IPublicClientApplication> CreateApplicationAsync()
    {
        var builder = PublicClientApplicationBuilder.Create(_options.ClientId)
            .WithAuthority(_options.Authority)
            .WithRedirectUri(MauiEntraRegistration.RedirectUri)
            .WithParentActivityOrWindow(() => MauiAuthenticationPlatform.ParentWindow);

#if WINDOWS
        builder = builder.WithBroker(new BrokerOptions(BrokerOptions.OperatingSystems.Windows));
#elif ANDROID || IOS
        builder = builder.WithBroker();
#elif MACCATALYST
        builder = builder.WithUseEmbeddedWebView(false);
#endif

        var application = builder.Build();

#if WINDOWS
        var storage = new StorageCreationPropertiesBuilder(
            "blazor-multihost-msal.cache",
            FileSystem.AppDataDirectory).Build();
        var cache = await MsalCacheHelper.CreateAsync(storage);
        cache.RegisterCache(application.UserTokenCache);
#endif

        return application;
    }

    private async Task<string?> TryGetSilentTokenAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await TryGetSilentTokenAsync(
                await _application.Value,
                cancellationToken);
        }
        catch (MsalUiRequiredException)
        {
            return null;
        }
        catch (MsalException exception)
        {
            _logger.LogWarning(
                "Silent token acquisition failed with MSAL error {ErrorCode}.",
                exception.ErrorCode);
            throw new AuthenticationOperationException(
                "The Microsoft Entra token could not be refreshed. Sign in again.",
                exception);
        }
    }

    private async Task<string?> TryGetSilentTokenAsync(
        IPublicClientApplication application,
        CancellationToken cancellationToken)
    {
        var account = (await application.GetAccountsAsync()).FirstOrDefault();
        var request = account is null
            ? GetOperatingSystemAccountRequest(application)
            : application.AcquireTokenSilent([_options.ApiScope], account);

        if (request is null)
        {
            return null;
        }

        try
        {
            var result = await request.ExecuteAsync(cancellationToken);
            return result.AccessToken;
        }
        catch (MsalUiRequiredException)
        {
            return null;
        }
    }

    private AcquireTokenSilentParameterBuilder? GetOperatingSystemAccountRequest(
        IPublicClientApplication application)
    {
#if WINDOWS
        return application.AcquireTokenSilent(
            [_options.ApiScope],
            PublicClientApplication.OperatingSystemAccount);
#else
        return null;
#endif
    }
}
