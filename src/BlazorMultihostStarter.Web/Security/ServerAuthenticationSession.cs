using BlazorMultihostStarter.Client.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorMultihostStarter.Web.Security;

public sealed class ServerAuthenticationSession(
    AuthenticationStateProvider authenticationStateProvider,
    NavigationManager navigationManager) : IUserAuthenticationSession
{
    public async Task<bool> IsSignedInAsync(CancellationToken cancellationToken = default)
    {
        var state = await authenticationStateProvider.GetAuthenticationStateAsync();
        return state.User.Identity?.IsAuthenticated == true;
    }

    public Task SignInAsync(CancellationToken cancellationToken = default)
    {
        navigationManager.NavigateTo("/auth/server/sign-in?returnUrl=/", forceLoad: true);
        return Task.CompletedTask;
    }

    public Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        navigationManager.NavigateTo("/auth/server/sign-out?returnUrl=/", forceLoad: true);
        return Task.CompletedTask;
    }
}
