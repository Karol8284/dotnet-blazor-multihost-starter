using BlazorMultihostStarter.Domain.Modules.Tasks;

namespace BlazorMultihostStarter.Domain.Tests.Modules.Tasks;

public sealed class TaskItemTests
{
    [Fact]
    public void Create_trims_title_and_sets_initial_state()
    {
        var createdAt = new DateTimeOffset(2026, 10, 20, 12, 0, 0, TimeSpan.Zero);

        var task = TaskItem.Create("  Write the tests  ", createdAt);

        Assert.NotEqual(Guid.Empty, task.Id);
        Assert.Equal("Write the tests", task.Title);
        Assert.Equal(createdAt, task.CreatedAtUtc);
        Assert.False(task.IsCompleted);
        Assert.Null(task.CompletedAtUtc);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_blank_title(string? title)
    {
        var exception = Assert.Throws<TaskDomainException>(
            () => TaskItem.Create(title, DateTimeOffset.UtcNow));

        Assert.Equal(nameof(TaskItem.Title), exception.Field);
    }

    [Fact]
    public void Create_rejects_title_longer_than_limit()
    {
        var title = new string('x', 121);

        var exception = Assert.Throws<TaskDomainException>(
            () => TaskItem.Create(title, DateTimeOffset.UtcNow));

        Assert.Contains("120", exception.Message);
    }

    [Fact]
    public void Complete_is_idempotent_and_preserves_first_completion_time()
    {
        var task = TaskItem.Create("Write the tests", DateTimeOffset.UtcNow);
        var completedAt = DateTimeOffset.UtcNow.AddMinutes(1);

        task.Complete(completedAt);
        task.Complete(completedAt.AddMinutes(1));

        Assert.True(task.IsCompleted);
        Assert.Equal(completedAt, task.CompletedAtUtc);
    }
}
