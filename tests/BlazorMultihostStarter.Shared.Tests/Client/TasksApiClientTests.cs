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
            new FakeAccessTokenProvider("access-token-for-api"));

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
            new FakeAccessTokenProvider(accessToken: null));

        await Assert.ThrowsAsync<AuthenticationRequiredException>(() => client.ListAsync());

        Assert.False(handler.RequestWasSent);
    }

    [Fact]
    public async Task Returns_server_snapshot_as_a_sync_conflict()
    {
        var serverTask = new TaskResponse(
            Guid.NewGuid(),
            "Server title",
            false,
            DateTimeOffset.UtcNow,
            null,
            2);
        var handler = new RecordingHandler
        {
            Response = new HttpResponseMessage(HttpStatusCode.Conflict)
            {
                Content = JsonContent.Create(new TaskSyncConflictResponse(serverTask))
            }
        };
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.example.test/")
        };
        var client = new TasksApiClient(httpClient, new FakeAccessTokenProvider("token"));

        var result = await client.SyncAsync(
            new SyncTaskRequest(serverTask.Id, "Local title", false, 1));

        var conflict = Assert.IsType<TaskSyncResult.Conflict>(result);
        Assert.Equal(serverTask, conflict.ServerTask);
        Assert.Equal("Bearer", handler.Authorization?.Scheme);
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public AuthenticationHeaderValue? Authorization { get; private set; }

        public bool RequestWasSent { get; private set; }

        public HttpResponseMessage? Response { get; init; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestWasSent = true;
            Authorization = request.Headers.Authorization;
            if (Response is not null)
            {
                return Task.FromResult(Response);
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new TaskListResponse([], 1, 25, false))
            });
        }
    }

    private sealed class FakeAccessTokenProvider(string? accessToken)
        : IApiAccessTokenProvider
    {
        public Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default) =>
            accessToken is null
                ? Task.FromException<string>(new AuthenticationRequiredException())
                : Task.FromResult(accessToken);
    }
}
