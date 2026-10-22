namespace BlazorMultihostStarter.Client.Tasks;

public enum LocalTaskSyncState
{
    LocalOnly,
    Pending,
    Synced,
    Conflict,
    Missing
}
