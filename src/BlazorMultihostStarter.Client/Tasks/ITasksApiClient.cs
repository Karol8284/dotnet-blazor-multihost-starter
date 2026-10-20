using BlazorMultihostStarter.Contracts.Tasks;

namespace BlazorMultihostStarter.Client.Tasks;

public interface ITasksApiClient
{
    Task<IReadOnlyList<TaskResponse>> ListAsync(CancellationToken cancellationToken = default);

    Task<TaskResponse> CreateAsync(string? title, CancellationToken cancellationToken = default);

    Task<TaskResponse> CompleteAsync(Guid id, CancellationToken cancellationToken = default);
}
