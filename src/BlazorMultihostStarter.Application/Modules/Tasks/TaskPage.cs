namespace BlazorMultihostStarter.Application.Modules.Tasks;

public sealed record TaskPage(
    IReadOnlyList<TaskDetails> Items,
    int Page,
    int PageSize,
    bool HasMore);
