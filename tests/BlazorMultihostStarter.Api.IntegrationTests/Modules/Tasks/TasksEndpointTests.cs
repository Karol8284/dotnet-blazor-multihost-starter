using System.Net;
using System.Net.Http.Json;
using BlazorMultihostStarter.Api.IntegrationTests.Infrastructure;
using BlazorMultihostStarter.Contracts;
using BlazorMultihostStarter.Contracts.Tasks;

namespace BlazorMultihostStarter.Api.IntegrationTests.Modules.Tasks;

public sealed class TasksEndpointTests
{
    private const string UserA = "aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa";
    private const string UserB = "bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb";

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
        using var client = CreateAuthenticatedClient(factory, UserA);

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

        var tasks = await client.GetFromJsonAsync<TaskListResponse>("/api/tasks");
        var listed = Assert.Single(tasks!.Items);
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
        using var client = CreateAuthenticatedClient(factory, UserA);

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
        using var client = CreateAuthenticatedClient(factory, UserA);

        using var response = await client.PostAsync(
            $"/api/tasks/{Guid.NewGuid()}/complete",
            content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Anonymous_requests_are_rejected()
    {
        await using var factory = new TasksApiFactory();
        await factory.InitializeDatabaseAsync();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/tasks");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Requests_without_the_required_scope_are_forbidden()
    {
        await using var factory = new TasksApiFactory();
        await factory.InitializeDatabaseAsync();
        using var client = CreateAuthenticatedClient(factory, UserA, "User.Read");

        using var response = await client.GetAsync("/api/tasks");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Requests_from_unregistered_client_applications_are_forbidden()
    {
        await using var factory = new TasksApiFactory();
        await factory.InitializeDatabaseAsync();
        using var client = CreateAuthenticatedClient(
            factory,
            UserA,
            clientId: Guid.NewGuid().ToString("D"));

        using var response = await client.GetAsync("/api/tasks");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Tasks_are_isolated_by_tenant_and_object_id()
    {
        await using var factory = new TasksApiFactory();
        await factory.InitializeDatabaseAsync();
        using var userA = CreateAuthenticatedClient(factory, UserA);
        using var userB = CreateAuthenticatedClient(factory, UserB);

        using var createResponse = await userA.PostAsJsonAsync(
            "/api/tasks",
            new
            {
                title = "Private to user A",
                ownerId = $"{TestAuthenticationHandler.TenantId}:{UserB}"
            });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.NotNull(created);

        var userBTasks = await userB.GetFromJsonAsync<TaskListResponse>("/api/tasks");
        Assert.Empty(userBTasks!.Items);

        using var readAsUserB = await userB.GetAsync($"/api/tasks/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, readAsUserB.StatusCode);

        using var completeAsUserB = await userB.PostAsync(
            $"/api/tasks/{created.Id}/complete",
            content: null);
        Assert.Equal(HttpStatusCode.NotFound, completeAsUserB.StatusCode);

        var userATask = await userA.GetFromJsonAsync<TaskResponse>($"/api/tasks/{created.Id}");
        Assert.NotNull(userATask);
        Assert.False(userATask.IsCompleted);
    }

    [Fact]
    public async Task Rate_limit_is_partitioned_per_authenticated_owner()
    {
        await using var factory = new TasksApiFactory();
        await factory.InitializeDatabaseAsync();
        using var userA = CreateAuthenticatedClient(factory, UserA);
        using var userB = CreateAuthenticatedClient(factory, UserB);

        for (var request = 0; request < 60; request++)
        {
            using var response = await userA.GetAsync("/api/tasks");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        using var throttledResponse = await userA.GetAsync("/api/tasks");
        using var userBResponse = await userB.GetAsync("/api/tasks");

        Assert.Equal(HttpStatusCode.TooManyRequests, throttledResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, userBResponse.StatusCode);
    }

    [Fact]
    public async Task List_tasks_is_paginated_and_page_size_is_bounded()
    {
        await using var factory = new TasksApiFactory();
        await factory.InitializeDatabaseAsync();
        using var client = CreateAuthenticatedClient(factory, UserA);

        foreach (var title in new[] { "First", "Second", "Third" })
        {
            using var response = await client.PostAsJsonAsync(
                "/api/tasks",
                new CreateTaskRequest(title));
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        var firstPage = await client.GetFromJsonAsync<TaskListResponse>(
            "/api/tasks?page=1&pageSize=2");
        Assert.NotNull(firstPage);
        Assert.Equal(2, firstPage.Items.Count);
        Assert.True(firstPage.HasMore);

        var secondPage = await client.GetFromJsonAsync<TaskListResponse>(
            "/api/tasks?page=2&pageSize=2");
        Assert.NotNull(secondPage);
        Assert.Single(secondPage.Items);
        Assert.False(secondPage.HasMore);

        using var invalidPage = await client.GetAsync("/api/tasks?page=0&pageSize=101");
        Assert.Equal(HttpStatusCode.BadRequest, invalidPage.StatusCode);
    }

    private static HttpClient CreateAuthenticatedClient(
        TasksApiFactory factory,
        string objectId,
        string scope = "Tasks.ReadWrite",
        string? clientId = null)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-ObjectId", objectId);
        client.DefaultRequestHeaders.Add("X-Test-Scope", scope);
        if (clientId is not null)
        {
            client.DefaultRequestHeaders.Add("X-Test-ClientId", clientId);
        }

        return client;
    }
}
