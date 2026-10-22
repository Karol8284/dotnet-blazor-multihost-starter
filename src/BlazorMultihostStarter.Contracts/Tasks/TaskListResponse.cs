namespace BlazorMultihostStarter.Contracts.Tasks;

public sealed record TaskListResponse(
    IReadOnlyList<TaskResponse> Items,
    int Page,
    int PageSize,
    bool HasMore);
