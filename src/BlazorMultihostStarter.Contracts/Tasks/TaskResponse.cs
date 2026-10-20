namespace BlazorMultihostStarter.Contracts.Tasks;

public sealed record TaskResponse(
    Guid Id,
    string Title,
    bool IsCompleted,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? CompletedAtUtc);
