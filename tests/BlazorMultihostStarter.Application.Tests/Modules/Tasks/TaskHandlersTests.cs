using BlazorMultihostStarter.Application.Modules.Tasks;
using BlazorMultihostStarter.Application.Modules.Tasks.CompleteTask;
using BlazorMultihostStarter.Application.Modules.Tasks.CreateTask;
using BlazorMultihostStarter.Application.Modules.Tasks.GetTask;
using BlazorMultihostStarter.Application.Modules.Tasks.ListTasks;
using BlazorMultihostStarter.Domain.Modules.Tasks;

namespace BlazorMultihostStarter.Application.Tests.Modules.Tasks;

public sealed class TaskHandlersTests
{
    [Fact]
    public async Task Create_persists_normalized_task()
    {
        var repository = new FakeTaskRepository();
        var createdAt = new DateTimeOffset(2026, 10, 20, 12, 0, 0, TimeSpan.Zero);
        var handler = new CreateTaskHandler(repository, new FixedTimeProvider(createdAt));

        var result = await handler.HandleAsync("  Learn vertical slices  ", CancellationToken.None);

        Assert.Equal("Learn vertical slices", result.Title);
        Assert.Equal(createdAt, result.CreatedAtUtc);
        Assert.Single(repository.Items);
        Assert.Equal(1, repository.SaveChangesCount);
    }

    [Fact]
    public async Task List_returns_tasks_from_repository()
    {
        var repository = new FakeTaskRepository();
        repository.Items.Add(TaskItem.Create("First", DateTimeOffset.UtcNow));
        var handler = new ListTasksHandler(repository);

        var result = await handler.HandleAsync(CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("First", result[0].Title);
    }

    [Fact]
    public async Task Get_returns_null_when_task_is_missing()
    {
        var repository = new FakeTaskRepository();
        var handler = new GetTaskHandler(repository);

        var result = await handler.HandleAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Get_returns_task_from_repository()
    {
        var task = TaskItem.Create("Read the docs", DateTimeOffset.UtcNow);
        var repository = new FakeTaskRepository();
        repository.Items.Add(task);
        var handler = new GetTaskHandler(repository);

        var result = await handler.HandleAsync(task.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(task.Id, result.Id);
        Assert.Equal(task.Title, result.Title);
    }

    [Fact]
    public async Task Complete_returns_null_when_task_is_missing()
    {
        var repository = new FakeTaskRepository();
        var handler = new CompleteTaskHandler(repository, new FixedTimeProvider(DateTimeOffset.UtcNow));

        var result = await handler.HandleAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
        Assert.Equal(0, repository.SaveChangesCount);
    }

    [Fact]
    public async Task Complete_updates_task_and_saves_changes()
    {
        var task = TaskItem.Create("Read the docs", DateTimeOffset.UtcNow);
        var repository = new FakeTaskRepository();
        repository.Items.Add(task);
        var completedAt = DateTimeOffset.UtcNow.AddMinutes(2);
        var handler = new CompleteTaskHandler(repository, new FixedTimeProvider(completedAt));

        var result = await handler.HandleAsync(task.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.IsCompleted);
        Assert.Equal(completedAt, result.CompletedAtUtc);
        Assert.Equal(1, repository.SaveChangesCount);
    }

    private sealed class FakeTaskRepository : ITaskRepository
    {
        public List<TaskItem> Items { get; } = [];

        public int SaveChangesCount { get; private set; }

        public Task<IReadOnlyList<TaskItem>> ListAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<TaskItem>>(Items.ToArray());
        }

        public Task<TaskItem?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Items.SingleOrDefault(task => task.Id == id));
        }

        public Task AddAsync(TaskItem task, CancellationToken cancellationToken)
        {
            Items.Add(task);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset currentTime) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => currentTime;
    }
}
