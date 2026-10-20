using BlazorMultihostStarter.Application.Modules.Tasks;
using BlazorMultihostStarter.Domain.Modules.Tasks;
using BlazorMultihostStarter.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazorMultihostStarter.Infrastructure.Modules.Tasks;

public sealed class EfTaskRepository(StarterDbContext dbContext) : ITaskRepository
{
    public async Task<IReadOnlyList<TaskItem>> ListAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Tasks
            .AsNoTracking()
            .OrderByDescending(task => task.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<TaskItem?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Tasks.SingleOrDefaultAsync(task => task.Id == id, cancellationToken);
    }

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken)
    {
        await dbContext.Tasks.AddAsync(task, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
