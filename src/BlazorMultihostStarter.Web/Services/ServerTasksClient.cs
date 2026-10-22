using System.Security.Claims;
using BlazorMultihostStarter.Application.Modules.Tasks;
using BlazorMultihostStarter.Application.Modules.Tasks.CompleteTask;
using BlazorMultihostStarter.Application.Modules.Tasks.CreateTask;
using BlazorMultihostStarter.Application.Modules.Tasks.ListTasks;
using BlazorMultihostStarter.Application.Modules.Tasks.SyncTask;
using BlazorMultihostStarter.Client.Authentication;
using BlazorMultihostStarter.Client.Tasks;
using BlazorMultihostStarter.Contracts.Tasks;
using BlazorMultihostStarter.Web.Security;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorMultihostStarter.Web.Services;

public sealed class ServerTasksClient(
    AuthenticationStateProvider authenticationStateProvider,
    ListTasksHandler listTasks,
    CreateTaskHandler createTask,
    CompleteTaskHandler completeTask,
    SyncTaskHandler syncTask) : ITasksClient
{
    public async Task<TaskListResponse> ListAsync(
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var ownerId = await GetOwnerIdAsync(cancellationToken);
        var result = await listTasks.HandleAsync(ownerId, page, pageSize, cancellationToken);
        return new TaskListResponse(
            result.Items.Select(ToResponse).ToArray(),
            result.Page,
            result.PageSize,
            result.HasMore);
    }

    public async Task<TaskResponse> CreateAsync(
        string? title,
        CancellationToken cancellationToken = default)
    {
        var result = await createTask.HandleAsync(
            title,
            await GetOwnerIdAsync(cancellationToken),
            cancellationToken);
        return ToResponse(result);
    }

    public async Task<TaskResponse> CompleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await completeTask.HandleAsync(
            id,
            await GetOwnerIdAsync(cancellationToken),
            cancellationToken);
        return result is null
            ? throw new HttpRequestException("The task was not found.", null, System.Net.HttpStatusCode.NotFound)
            : ToResponse(result);
    }

    public async Task<TaskSyncResult> SyncAsync(
        SyncTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var outcome = await syncTask.HandleAsync(
            request.Id,
            new SyncTaskCommand(request.Id, request.Title, request.IsCompleted, request.BaseVersion),
            await GetOwnerIdAsync(cancellationToken),
            cancellationToken);

        return outcome switch
        {
            TaskSyncOutcome.Succeeded succeeded => new TaskSyncResult.Applied(ToResponse(succeeded.Task)),
            TaskSyncOutcome.Conflict conflict => new TaskSyncResult.Conflict(ToResponse(conflict.ServerTask)),
            TaskSyncOutcome.Missing => new TaskSyncResult.Missing(),
            _ => throw new InvalidOperationException("The task sync outcome is unknown.")
        };
    }

    private async Task<string> GetOwnerIdAsync(CancellationToken cancellationToken)
    {
        var state = await authenticationStateProvider.GetAuthenticationStateAsync();
        if (state.User.Identity?.IsAuthenticated != true)
        {
            throw new AuthenticationRequiredException();
        }

        return EntraTaskOwner.GetRequiredOwnerId(state.User);
    }

    private static TaskResponse ToResponse(TaskDetails task) =>
        new(task.Id, task.Title, task.IsCompleted, task.CreatedAtUtc, task.CompletedAtUtc, task.Version);
}
