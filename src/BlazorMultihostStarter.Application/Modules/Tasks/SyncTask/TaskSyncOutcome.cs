namespace BlazorMultihostStarter.Application.Modules.Tasks.SyncTask;

public abstract record TaskSyncOutcome
{
    private TaskSyncOutcome()
    {
    }

    public sealed record Succeeded(TaskDetails Task, bool Created = false) : TaskSyncOutcome;

    public sealed record Conflict(TaskDetails ServerTask) : TaskSyncOutcome;

    public sealed record Missing : TaskSyncOutcome;
}
