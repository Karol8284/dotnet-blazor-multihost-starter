namespace BlazorMultihostStarter.Client.Authentication;

public sealed class UnconfiguredAuthenticationSession(string message) : IUserAuthenticationSession
{
    public Task<bool> IsSignedInAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(false);

    public Task SignInAsync(CancellationToken cancellationToken = default) =>
        Task.FromException(new AuthenticationOperationException(message));

    public Task SignOutAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
