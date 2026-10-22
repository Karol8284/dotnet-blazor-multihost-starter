namespace BlazorMultihostStarter.Domain.Modules.Tasks;

public sealed class TaskItem
{
    private TaskItem()
    {
    }

    public Guid Id { get; private set; }

    public string OwnerId { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public int Version { get; private set; } = 1;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public bool IsCompleted { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public static TaskItem Create(string? title, string? ownerId, DateTimeOffset createdAtUtc)
    {
        return Create(Guid.NewGuid(), title, ownerId, createdAtUtc);
    }

    public static TaskItem Create(
        Guid id,
        string? title,
        string? ownerId,
        DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new TaskDomainException(nameof(Id), "A valid task ID is required.");
        }

        if (string.IsNullOrWhiteSpace(ownerId) || ownerId.Length > 80)
        {
            throw new TaskDomainException(nameof(OwnerId), "A valid task owner is required.");
        }

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
            Id = id,
            OwnerId = ownerId.Trim(),
            Title = normalizedTitle,
            CreatedAtUtc = createdAtUtc
        };
    }

    public void Rename(string? title)
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

        if (string.Equals(Title, normalizedTitle, StringComparison.Ordinal))
        {
            return;
        }

        Title = normalizedTitle;
        Version++;
    }

    public void Complete(DateTimeOffset completedAtUtc)
    {
        if (IsCompleted)
        {
            return;
        }

        IsCompleted = true;
        CompletedAtUtc = completedAtUtc;
        Version++;
    }
}
