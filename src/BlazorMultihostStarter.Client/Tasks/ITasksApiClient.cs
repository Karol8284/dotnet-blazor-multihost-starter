using BlazorMultihostStarter.Contracts.Tasks;

namespace BlazorMultihostStarter.Client.Tasks;

public interface ITasksApiClient
{
    Task<TaskListResponse> ListAsync(
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default);

    Task<TaskResponse> CreateAsync(string? title, CancellationToken cancellationToken = default);

    Task<TaskResponse> CompleteAsync(Guid id, CancellationToken cancellationToken = default);
}
