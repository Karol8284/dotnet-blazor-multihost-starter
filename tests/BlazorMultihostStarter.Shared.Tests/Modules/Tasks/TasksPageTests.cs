using System.Net;
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

        var cut = context.Render<TasksPage>();

        cut.WaitForAssertion(() => Assert.Contains("HTTP 500", cut.Markup));
        Assert.Contains("PostgreSQL connection settings", cut.Markup);
    }

    [Fact]
    public void Creates_task_through_shared_api_client()
    {
        using var context = new BunitContext();
        var apiClient = new FakeTasksApiClient();
        context.Services.AddSingleton<ITasksApiClient>(apiClient);

        var cut = context.Render<TasksPage>();
        cut.Find("#new-task-title").Input("Share this task");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Assert.Contains("Share this task", cut.Markup));
        Assert.Single(apiClient.Tasks);
        Assert.Equal("Share this task", apiClient.Tasks[0].Title);
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

        public Task<IReadOnlyList<TaskResponse>> ListAsync(CancellationToken cancellationToken = default)
        {
            if (failureStatus is { } statusCode)
            {
                throw new HttpRequestException("API request failed.", null, statusCode);
            }

            if (failRequests)
            {
                throw new HttpRequestException("API unavailable.");
            }

            return Task.FromResult<IReadOnlyList<TaskResponse>>(Tasks.ToArray());
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
}
