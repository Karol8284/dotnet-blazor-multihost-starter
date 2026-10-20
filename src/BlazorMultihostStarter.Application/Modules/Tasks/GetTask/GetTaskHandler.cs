namespace BlazorMultihostStarter.Application.Modules.Tasks.GetTask;

public sealed class GetTaskHandler(ITaskRepository tasks)
{
    public async Task<TaskDetails?> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        var task = await tasks.FindByIdAsync(id, cancellationToken);
        return task is null ? null : TaskDetails.From(task);
    }
}
