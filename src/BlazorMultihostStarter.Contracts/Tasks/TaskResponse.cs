namespace BlazorMultihostStarter.Contracts.Tasks;

/// <summary>Represents a task returned by the API or a local data-mode client.</summary>
public sealed record TaskResponse(
    Guid Id,
    string Title,
    bool IsCompleted,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    int Version = 1);
