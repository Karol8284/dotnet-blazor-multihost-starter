namespace BlazorMultihostStarter.Domain.Modules.Tasks;

public sealed class TaskItem
{
    private TaskItem()
    {
    }

    public Guid Id { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public bool IsCompleted { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public static TaskItem Create(string? title, DateTimeOffset createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new TaskDomainException(nameof(Title), "Enter a task title.");
        }

        var normalizedTitle = title.Trim();
        if (normalizedTitle.Length > 120)
        {
            throw new TaskDomainException(nameof(Title), "A task title cannot exceed 120 characters.");
        }

        return new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = normalizedTitle,
            CreatedAtUtc = createdAtUtc
        };
    }

    public void Complete(DateTimeOffset completedAtUtc)
    {
        if (IsCompleted)
        {
            return;
        }

        IsCompleted = true;
        CompletedAtUtc = completedAtUtc;
    }
}
