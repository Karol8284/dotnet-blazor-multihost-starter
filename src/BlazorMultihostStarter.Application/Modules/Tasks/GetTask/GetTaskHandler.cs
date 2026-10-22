namespace BlazorMultihostStarter.Application.Modules.Tasks.GetTask;

public sealed class GetTaskHandler(ITaskRepository tasks)
{
    public async Task<TaskDetails?> HandleAsync(
        Guid id,
        string ownerId,
        CancellationToken cancellationToken)
    {
        var task = await tasks.FindByIdAsync(id, ownerId, cancellationToken);
        return task is null ? null : TaskDetails.From(task);
    }
}
