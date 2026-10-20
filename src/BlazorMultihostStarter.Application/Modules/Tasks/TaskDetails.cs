using BlazorMultihostStarter.Domain.Modules.Tasks;

namespace BlazorMultihostStarter.Application.Modules.Tasks;

public sealed record TaskDetails(
    Guid Id,
    string Title,
    bool IsCompleted,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? CompletedAtUtc)
{
    public static TaskDetails From(TaskItem task) =>
        new(task.Id, task.Title, task.IsCompleted, task.CreatedAtUtc, task.CompletedAtUtc);
}
