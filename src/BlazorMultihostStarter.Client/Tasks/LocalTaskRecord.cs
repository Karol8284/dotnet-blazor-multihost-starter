using BlazorMultihostStarter.Contracts.Tasks;

namespace BlazorMultihostStarter.Client.Tasks;

public sealed record LocalTaskRecord(
    TaskResponse Task,
    int BaseVersion,
    LocalTaskSyncState SyncState,
    TaskResponse? ServerTask = null);
