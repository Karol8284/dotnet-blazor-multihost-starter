namespace BlazorMultihostStarter.Contracts.Tasks;

/// <summary>Represents the current server version when a local task snapshot conflicts.</summary>
public sealed record TaskSyncConflictResponse(TaskResponse ServerTask);
