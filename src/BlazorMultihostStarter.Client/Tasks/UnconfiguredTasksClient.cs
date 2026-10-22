using System.Net;
using BlazorMultihostStarter.Contracts.Tasks;

namespace BlazorMultihostStarter.Client.Tasks;

public sealed class UnconfiguredTasksClient(string message) : ITasksClient
{
    public Task<TaskListResponse> ListAsync(
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default) =>
        Task.FromException<TaskListResponse>(CreateFailure());

    public Task<TaskResponse> CreateAsync(
        string? title,
        CancellationToken cancellationToken = default) =>
        Task.FromException<TaskResponse>(CreateFailure());

    public Task<TaskResponse> CompleteAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        Task.FromException<TaskResponse>(CreateFailure());

    public Task<TaskSyncResult> SyncAsync(
        SyncTaskRequest request,
        CancellationToken cancellationToken = default) =>
        Task.FromException<TaskSyncResult>(CreateFailure());

    private HttpRequestException CreateFailure() =>
        new(message, null, HttpStatusCode.ServiceUnavailable);
}
