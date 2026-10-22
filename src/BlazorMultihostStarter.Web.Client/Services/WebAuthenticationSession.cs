using BlazorMultihostStarter.Client.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace BlazorMultihostStarter.Web.Client.Services;

public sealed class WebAuthenticationSession(
    IAccessTokenProvider accessTokenProvider,
    AuthenticationStateProvider authenticationStateProvider,
    NavigationManager navigationManager,
    EntraClientOptions options) : IUserAuthenticationSession
{
    public async Task<bool> IsSignedInAsync(CancellationToken cancellationToken = default)
    {
        var state = await authenticationStateProvider.GetAuthenticationStateAsync();
        return state.User.Identity?.IsAuthenticated == true;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (!await IsSignedInAsync(cancellationToken))
        {
            throw new AuthenticationRequiredException();
        }

        try
        {
            var result = await accessTokenProvider.RequestAccessToken(
                new AccessTokenRequestOptions
                {
                    Scopes = [options.ApiScope]
                });

            if (result.TryGetToken(out var accessToken))
            {
                return accessToken.Value;
            }
        }
        catch (AccessTokenNotAvailableException)
        {
            throw new AuthenticationRequiredException();
        }

        throw new AuthenticationRequiredException();
    }

    public Task SignInAsync(CancellationToken cancellationToken = default)
    {
        navigationManager.NavigateTo("authentication/login?returnUrl=/", forceLoad: true);
        return Task.CompletedTask;
    }

    public Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        navigationManager.NavigateTo("authentication/logout?returnUrl=/", forceLoad: true);
        return Task.CompletedTask;
    }
}
