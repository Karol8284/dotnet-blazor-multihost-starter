namespace BlazorMultihostStarter.Contracts.Tasks;

/// <summary>Represents one page of tasks and its pagination state.</summary>
public sealed record TaskListResponse(
    IReadOnlyList<TaskResponse> Items,
    int Page,
    int PageSize,
    bool HasMore);
