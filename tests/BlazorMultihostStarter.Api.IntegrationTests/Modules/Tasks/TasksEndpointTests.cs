using System.Net;
using System.Net.Http.Json;
using BlazorMultihostStarter.Api.IntegrationTests.Infrastructure;
using BlazorMultihostStarter.Contracts;
using BlazorMultihostStarter.Contracts.Tasks;

namespace BlazorMultihostStarter.Api.IntegrationTests.Modules.Tasks;

public sealed class TasksEndpointTests
{
    [Fact]
    public async Task Web_host_serves_the_webassembly_shell()
    {
        await using var factory = new TasksApiFactory();
        await factory.InitializeDatabaseAsync();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("_framework/blazor.web.js", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Create_list_and_complete_share_persisted_task()
    {
        await using var factory = new TasksApiFactory();
        await factory.InitializeDatabaseAsync();
        using var client = factory.CreateClient();

        using var createResponse = await client.PostAsJsonAsync(
            "/api/tasks",
            new CreateTaskRequest("  Share across hosts  "));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.NotNull(created);
        Assert.Equal("Share across hosts", created.Title);

        using var getResponse = await client.GetAsync($"/api/tasks/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetched = await getResponse.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);

        var tasks = await client.GetFromJsonAsync<TaskResponse[]>("/api/tasks");
        var listed = Assert.Single(tasks!);
        Assert.Equal(created.Id, listed.Id);
        Assert.False(listed.IsCompleted);

        using var completeResponse = await client.PostAsync(
            $"/api/tasks/{created.Id}/complete",
            content: null);

        Assert.Equal(HttpStatusCode.OK, completeResponse.StatusCode);
        var completed = await completeResponse.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.NotNull(completed);
        Assert.True(completed.IsCompleted);
        Assert.NotNull(completed.CompletedAtUtc);
    }

    [Fact]
    public async Task Create_rejects_blank_title_with_validation_problem()
    {
        await using var factory = new TasksApiFactory();
        await factory.InitializeDatabaseAsync();
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/api/tasks",
            new CreateTaskRequest("   "));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemResponse>();
        Assert.NotNull(problem);
        Assert.Contains(nameof(TaskResponse.Title), problem.Errors.Keys);
    }

    [Fact]
    public async Task Complete_returns_not_found_for_unknown_task()
    {
        await using var factory = new TasksApiFactory();
        await factory.InitializeDatabaseAsync();
        using var client = factory.CreateClient();

        using var response = await client.PostAsync(
            $"/api/tasks/{Guid.NewGuid()}/complete",
            content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
