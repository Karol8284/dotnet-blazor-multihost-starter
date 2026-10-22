using BlazorMultihostStarter.Domain.Modules.Tasks;

namespace BlazorMultihostStarter.Application.Modules.Tasks;

public interface ITaskRepository
{
    Task<IReadOnlyList<TaskItem>> ListAsync(
        string ownerId,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<TaskItem?> FindByIdAsync(Guid id, string ownerId, CancellationToken cancellationToken);

    Task<TaskItem?> FindAnyByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<TaskItem?> FindCurrentByIdAsync(
        Guid id,
        string ownerId,
        CancellationToken cancellationToken);

    Task AddAsync(TaskItem task, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
