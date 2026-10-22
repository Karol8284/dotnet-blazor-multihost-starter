using BlazorMultihostStarter.Application.Modules.Tasks;
using BlazorMultihostStarter.Domain.Modules.Tasks;
using BlazorMultihostStarter.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazorMultihostStarter.Infrastructure.Modules.Tasks;

public sealed class EfTaskRepository(StarterDbContext dbContext) : ITaskRepository
{
    public async Task<IReadOnlyList<TaskItem>> ListAsync(
        string ownerId,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        return await dbContext.Tasks
            .AsNoTracking()
            .Where(task => task.OwnerId == ownerId)
            .OrderByDescending(task => task.CreatedAtUtc)
            .ThenBy(task => task.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<TaskItem?> FindByIdAsync(
        Guid id,
        string ownerId,
        CancellationToken cancellationToken)
    {
        return dbContext.Tasks.SingleOrDefaultAsync(
            task => task.Id == id && task.OwnerId == ownerId,
            cancellationToken);
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
