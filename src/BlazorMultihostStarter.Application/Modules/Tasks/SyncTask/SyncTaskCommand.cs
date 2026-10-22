namespace BlazorMultihostStarter.Application.Modules.Tasks.SyncTask;

public sealed record SyncTaskCommand(
    Guid Id,
    string Title,
    bool IsCompleted,
    int BaseVersion);
