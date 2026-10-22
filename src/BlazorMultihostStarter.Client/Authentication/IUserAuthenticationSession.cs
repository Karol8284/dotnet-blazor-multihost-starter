namespace BlazorMultihostStarter.Client.Authentication;

public interface IUserAuthenticationSession
{
    Task<bool> IsSignedInAsync(CancellationToken cancellationToken = default);

    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);

    Task SignInAsync(CancellationToken cancellationToken = default);

    Task SignOutAsync(CancellationToken cancellationToken = default);
}
