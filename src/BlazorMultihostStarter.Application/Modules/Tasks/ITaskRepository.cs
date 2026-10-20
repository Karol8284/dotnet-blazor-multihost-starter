using BlazorMultihostStarter.Domain.Modules.Tasks;

namespace BlazorMultihostStarter.Application.Modules.Tasks;

public interface ITaskRepository
{
    Task<IReadOnlyList<TaskItem>> ListAsync(CancellationToken cancellationToken);

    Task<TaskItem?> FindByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(TaskItem task, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
