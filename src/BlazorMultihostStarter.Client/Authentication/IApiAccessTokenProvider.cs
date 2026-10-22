namespace BlazorMultihostStarter.Client.Authentication;

public interface IApiAccessTokenProvider
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
