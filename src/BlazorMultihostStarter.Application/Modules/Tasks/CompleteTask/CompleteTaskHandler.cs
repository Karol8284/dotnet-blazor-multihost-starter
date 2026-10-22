namespace BlazorMultihostStarter.Application.Modules.Tasks.CompleteTask;

public sealed class CompleteTaskHandler(ITaskRepository tasks, TimeProvider timeProvider)
{
    public async Task<TaskDetails?> HandleAsync(
        Guid id,
        string ownerId,
        CancellationToken cancellationToken)
    {
        var task = await tasks.FindByIdAsync(id, ownerId, cancellationToken);
        if (task is null)
        {
            return null;
        }

        task.Complete(timeProvider.GetUtcNow());
        await tasks.SaveChangesAsync(cancellationToken);

        return TaskDetails.From(task);
    }
}
