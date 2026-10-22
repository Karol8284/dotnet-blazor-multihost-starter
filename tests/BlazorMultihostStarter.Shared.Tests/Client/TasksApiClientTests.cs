using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using BlazorMultihostStarter.Client.Authentication;
using BlazorMultihostStarter.Client.Tasks;
using BlazorMultihostStarter.Contracts.Tasks;

namespace BlazorMultihostStarter.Shared.Tests.Client;

public sealed class TasksApiClientTests
{
    [Fact]
    public async Task Adds_the_host_acquired_access_token_to_API_requests()
    {
        var handler = new RecordingHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.example.test/")
        };
        var client = new TasksApiClient(
            httpClient,
            new FakeAuthenticationSession("access-token-for-api"));

        var tasks = await client.ListAsync();

        Assert.Empty(tasks.Items);
        Assert.Equal("Bearer", handler.Authorization?.Scheme);
        Assert.Equal("access-token-for-api", handler.Authorization?.Parameter);
    }

    [Fact]
    public async Task Does_not_send_a_request_without_an_access_token()
    {
        var handler = new RecordingHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.example.test/")
        };
        var client = new TasksApiClient(
            httpClient,
            new FakeAuthenticationSession(accessToken: null));

        await Assert.ThrowsAsync<AuthenticationRequiredException>(() => client.ListAsync());

        Assert.False(handler.RequestWasSent);
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public AuthenticationHeaderValue? Authorization { get; private set; }

        public bool RequestWasSent { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestWasSent = true;
            Authorization = request.Headers.Authorization;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new TaskListResponse([], 1, 25, false))
            });
        }
    }

    private sealed class FakeAuthenticationSession(string? accessToken)
        : IUserAuthenticationSession
    {
        public Task<bool> IsSignedInAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(accessToken is not null);

        public Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default) =>
            accessToken is null
                ? Task.FromException<string>(new AuthenticationRequiredException())
                : Task.FromResult(accessToken);

        public Task SignInAsync(CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task SignOutAsync(CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
