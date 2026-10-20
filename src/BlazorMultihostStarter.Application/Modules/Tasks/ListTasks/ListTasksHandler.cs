namespace BlazorMultihostStarter.Application.Modules.Tasks.ListTasks;

public sealed class ListTasksHandler(ITaskRepository tasks)
{
    public async Task<IReadOnlyList<TaskDetails>> HandleAsync(CancellationToken cancellationToken)
    {
        var items = await tasks.ListAsync(cancellationToken);
        return items.Select(TaskDetails.From).ToArray();
    }
}
