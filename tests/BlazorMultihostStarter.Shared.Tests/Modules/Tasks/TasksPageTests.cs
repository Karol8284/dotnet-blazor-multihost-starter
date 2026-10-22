using System.Net;
using BlazorMultihostStarter.Client.Authentication;
using BlazorMultihostStarter.Client.Tasks;
using BlazorMultihostStarter.Contracts.Tasks;
using BlazorMultihostStarter.Shared.Modules.Tasks;
using Bunit;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorMultihostStarter.Shared.Tests.Modules.Tasks;

public sealed class TasksPageTests
{
    [Fact]
    public void Renders_empty_state_when_api_has_no_tasks()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton<ITasksApiClient>(new FakeTasksApiClient());
        context.Services.AddSingleton<IUserAuthenticationSession>(new FakeAuthenticationSession());

        var cut = context.Render<TasksPage>();

        cut.WaitForAssertion(() => Assert.Contains("No tasks yet", cut.Markup));
        Assert.Contains("Your tasks", cut.Markup);
        Assert.Contains("Add a task", cut.Markup);
    }

    [Fact]
    public void Shows_api_unavailable_message_when_initial_load_fails()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton<ITasksApiClient>(new FakeTasksApiClient(failRequests: true));
        context.Services.AddSingleton<IUserAuthenticationSession>(new FakeAuthenticationSession());

        var cut = context.Render<TasksPage>();

        cut.WaitForAssertion(() => Assert.Contains("The API host could not be reached", cut.Markup));
        Assert.Contains("Try again", cut.Markup);
    }

    [Fact]
    public void Explains_server_errors_instead_of_suggesting_the_host_is_offline()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton<ITasksApiClient>(
            new FakeTasksApiClient(failureStatus: HttpStatusCode.InternalServerError));
        context.Services.AddSingleton<IUserAuthenticationSession>(new FakeAuthenticationSession());

        var cut = context.Render<TasksPage>();

        cut.WaitForAssertion(() => Assert.Contains("HTTP 500", cut.Markup));
        Assert.Contains("PostgreSQL connection settings", cut.Markup);
    }

    [Fact]
    public void Explains_rate_limit_responses()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton<ITasksApiClient>(
            new FakeTasksApiClient(failureStatus: HttpStatusCode.TooManyRequests));
        context.Services.AddSingleton<IUserAuthenticationSession>(new FakeAuthenticationSession());

        var cut = context.Render<TasksPage>();

        cut.WaitForAssertion(() => Assert.Contains("Too many requests", cut.Markup));
    }

    [Fact]
    public void Creates_task_through_shared_api_client()
    {
        using var context = new BunitContext();
        var apiClient = new FakeTasksApiClient();
        context.Services.AddSingleton<ITasksApiClient>(apiClient);
        context.Services.AddSingleton<IUserAuthenticationSession>(new FakeAuthenticationSession());

        var cut = context.Render<TasksPage>();
        cut.Find("#new-task-title").Input("Share this task");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Assert.Contains("Share this task", cut.Markup));
        Assert.Single(apiClient.Tasks);
        Assert.Equal("Share this task", apiClient.Tasks[0].Title);
    }

    [Fact]
    public void Requires_sign_in_before_loading_tasks()
    {
        using var context = new BunitContext();
        var apiClient = new FakeTasksApiClient();
        var authentication = new FakeAuthenticationSession(isSignedIn: false);
        context.Services.AddSingleton<ITasksApiClient>(apiClient);
        context.Services.AddSingleton<IUserAuthenticationSession>(authentication);

        var cut = context.Render<TasksPage>();

        cut.WaitForAssertion(() => Assert.Contains("Sign in required", cut.Markup));
        Assert.Equal(0, apiClient.ListRequestCount);

        cut.Find(".sign-in-panel button").Click();

        cut.WaitForAssertion(() => Assert.Contains("No tasks yet", cut.Markup));
        Assert.True(authentication.IsSignedIn);
        Assert.Equal(1, apiClient.ListRequestCount);
    }

    [Fact]
    public void Loads_more_tasks_using_a_later_page()
    {
        using var context = new BunitContext();
        var apiClient = new FakeTasksApiClient();
        for (var index = 0; index < 30; index++)
        {
            apiClient.Tasks.Add(new TaskResponse(
                Guid.NewGuid(),
                $"Task {index + 1}",
                false,
                DateTimeOffset.UtcNow.AddMinutes(-index),
                null));
        }

        context.Services.AddSingleton<ITasksApiClient>(apiClient);
        context.Services.AddSingleton<IUserAuthenticationSession>(new FakeAuthenticationSession());

        var cut = context.Render<TasksPage>();

        cut.WaitForAssertion(() => Assert.Contains("Task 25", cut.Markup));
        Assert.DoesNotContain("Task 26", cut.Markup);
        cut.Find(".load-more-button").Click();
        cut.WaitForAssertion(() => Assert.Contains("Task 26", cut.Markup));
        Assert.Contains("Task 30", cut.Markup);
    }

    [Fact]
    public void Shared_ui_does_not_reference_server_or_platform_projects()
    {
        var references = typeof(TasksPage).Assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .ToArray();

        Assert.DoesNotContain(references, name => name == "BlazorMultihostStarter.Infrastructure");
        Assert.DoesNotContain(references, name => name?.StartsWith("BlazorMultihostStarter.Maui", StringComparison.Ordinal) == true);
        Assert.DoesNotContain(references, name => name?.StartsWith("BlazorMultihostStarter.Web", StringComparison.Ordinal) == true);
    }

    private sealed class FakeTasksApiClient(
        bool failRequests = false,
        HttpStatusCode? failureStatus = null) : ITasksApiClient
    {
        public List<TaskResponse> Tasks { get; } = [];

        public int ListRequestCount { get; private set; }

        public Task<TaskListResponse> ListAsync(
            int page = 1,
            int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            ListRequestCount++;

            if (failureStatus is { } statusCode)
            {
                throw new HttpRequestException("API request failed.", null, statusCode);
            }

            if (failRequests)
            {
                throw new HttpRequestException("API unavailable.");
            }

            var items = Tasks
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToArray();

            return Task.FromResult(new TaskListResponse(
                items,
                page,
                pageSize,
                Tasks.Count > page * pageSize));
        }

        public Task<TaskResponse> CreateAsync(
            string? title,
            CancellationToken cancellationToken = default)
        {
            var task = new TaskResponse(
                Guid.NewGuid(),
                title?.Trim() ?? string.Empty,
                false,
                DateTimeOffset.UtcNow,
                null);
            Tasks.Add(task);
            return Task.FromResult(task);
        }

        public Task<TaskResponse> CompleteAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var index = Tasks.FindIndex(task => task.Id == id);
            if (index < 0)
            {
                throw new HttpRequestException("Task was not found.");
            }

            Tasks[index] = Tasks[index] with
            {
                IsCompleted = true,
                CompletedAtUtc = DateTimeOffset.UtcNow
            };
            return Task.FromResult(Tasks[index]);
        }
    }

    private sealed class FakeAuthenticationSession(bool isSignedIn = true)
        : IUserAuthenticationSession
    {
        public bool IsSignedIn { get; private set; } = isSignedIn;

        public Task<bool> IsSignedInAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(IsSignedIn);

        public Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default) =>
            IsSignedIn
                ? Task.FromResult("test-access-token")
                : Task.FromException<string>(new AuthenticationRequiredException());

        public Task SignInAsync(CancellationToken cancellationToken = default)
        {
            IsSignedIn = true;
            return Task.CompletedTask;
        }

        public Task SignOutAsync(CancellationToken cancellationToken = default)
        {
            IsSignedIn = false;
            return Task.CompletedTask;
        }
    }
}
