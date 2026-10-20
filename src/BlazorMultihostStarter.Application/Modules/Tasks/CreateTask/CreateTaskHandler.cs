using BlazorMultihostStarter.Domain.Modules.Tasks;

namespace BlazorMultihostStarter.Application.Modules.Tasks.CreateTask;

public sealed class CreateTaskHandler(ITaskRepository tasks, TimeProvider timeProvider)
{
    public async Task<TaskDetails> HandleAsync(string? title, CancellationToken cancellationToken)
    {
        var task = TaskItem.Create(title, timeProvider.GetUtcNow());

        await tasks.AddAsync(task, cancellationToken);
        await tasks.SaveChangesAsync(cancellationToken);

        return TaskDetails.From(task);
    }
}
