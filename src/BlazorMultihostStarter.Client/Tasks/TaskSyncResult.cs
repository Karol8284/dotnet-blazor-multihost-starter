using BlazorMultihostStarter.Contracts.Tasks;

namespace BlazorMultihostStarter.Client.Tasks;

public abstract record TaskSyncResult
{
    private TaskSyncResult()
    {
    }

    public sealed record Applied(TaskResponse Task) : TaskSyncResult;

    public sealed record Conflict(TaskResponse ServerTask) : TaskSyncResult;

    public sealed record Missing : TaskSyncResult;
}
