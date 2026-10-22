using BlazorMultihostStarter.Contracts.Tasks;

namespace BlazorMultihostStarter.Client.Tasks;

public interface ITasksClient
{
    Task<TaskListResponse> ListAsync(
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default);

    Task<TaskResponse> CreateAsync(string? title, CancellationToken cancellationToken = default);

    Task<TaskResponse> CompleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TaskSyncResult> SyncAsync(
        SyncTaskRequest request,
        CancellationToken cancellationToken = default);
}
