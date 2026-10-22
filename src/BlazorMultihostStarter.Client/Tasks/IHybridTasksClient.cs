using BlazorMultihostStarter.Contracts.Tasks;

namespace BlazorMultihostStarter.Client.Tasks;

public interface IHybridTasksClient
{
    Task<IReadOnlyList<LocalTaskRecord>> ListAsync(CancellationToken cancellationToken = default);

    Task<TaskResponse> CreateAsync(string? title, CancellationToken cancellationToken = default);

    Task<TaskResponse> CompleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TaskResponse> RenameAsync(Guid id, string? title, CancellationToken cancellationToken = default);

    Task<HybridSyncReport> SyncPendingAsync(CancellationToken cancellationToken = default);

    Task ResolveConflictAsync(
        Guid id,
        bool keepLocal,
        CancellationToken cancellationToken = default);

    Task RemoveMissingAsync(Guid id, CancellationToken cancellationToken = default);
}
