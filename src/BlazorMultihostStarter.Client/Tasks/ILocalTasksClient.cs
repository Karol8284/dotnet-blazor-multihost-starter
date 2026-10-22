using BlazorMultihostStarter.Contracts.Tasks;

namespace BlazorMultihostStarter.Client.Tasks;

public interface ILocalTasksClient
{
    Task<IReadOnlyList<LocalTaskRecord>> ListAsync(CancellationToken cancellationToken = default);

    Task<TaskResponse> CreateAsync(string? title, CancellationToken cancellationToken = default);

    Task<TaskResponse> CompleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TaskResponse> RenameAsync(Guid id, string? title, CancellationToken cancellationToken = default);
}
