namespace BlazorMultihostStarter.Contracts.Tasks;

/// <summary>Represents a local task snapshot sent to the server for synchronization.</summary>
public sealed record SyncTaskRequest(
    Guid Id,
    string Title,
    bool IsCompleted,
    int BaseVersion);
