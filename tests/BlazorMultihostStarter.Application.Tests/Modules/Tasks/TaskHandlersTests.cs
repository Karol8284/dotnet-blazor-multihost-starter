using BlazorMultihostStarter.Application.Modules.Tasks;
using BlazorMultihostStarter.Application.Modules.Tasks.CompleteTask;
using BlazorMultihostStarter.Application.Modules.Tasks.CreateTask;
using BlazorMultihostStarter.Application.Modules.Tasks.GetTask;
using BlazorMultihostStarter.Application.Modules.Tasks.ListTasks;
using BlazorMultihostStarter.Domain.Modules.Tasks;

namespace BlazorMultihostStarter.Application.Tests.Modules.Tasks;

public sealed class TaskHandlersTests
{
    private const string OwnerId = "tenant-a:user-1";

    [Fact]
    public async Task Create_persists_normalized_task()
    {
        var repository = new FakeTaskRepository();
        var createdAt = new DateTimeOffset(2026, 10, 20, 12, 0, 0, TimeSpan.Zero);
        var handler = new CreateTaskHandler(repository, new FixedTimeProvider(createdAt));

        var result = await handler.HandleAsync("  Learn vertical slices  ", OwnerId, CancellationToken.None);

        Assert.Equal("Learn vertical slices", result.Title);
        Assert.Equal(createdAt, result.CreatedAtUtc);
        Assert.Single(repository.Items);
        Assert.Equal(1, repository.SaveChangesCount);
    }

    [Fact]
    public async Task List_returns_tasks_from_repository()
    {
        var repository = new FakeTaskRepository();
        repository.Items.Add(TaskItem.Create("First", OwnerId, DateTimeOffset.UtcNow));
        var handler = new ListTasksHandler(repository);

        var result = await handler.HandleAsync(OwnerId, page: 1, pageSize: 25, CancellationToken.None);

        var firstTask = Assert.Single(result.Items);
        Assert.Equal("First", firstTask.Title);
        Assert.Equal(1, result.Page);
        Assert.Equal(25, result.PageSize);
        Assert.False(result.HasMore);
    }

    [Fact]
    public async Task List_does_not_return_another_owners_tasks()
    {
        var repository = new FakeTaskRepository();
        repository.Items.Add(TaskItem.Create("Mine", OwnerId, DateTimeOffset.UtcNow));
        repository.Items.Add(TaskItem.Create("Not mine", "tenant-a:user-2", DateTimeOffset.UtcNow));
        var handler = new ListTasksHandler(repository);

        var result = await handler.HandleAsync(OwnerId, page: 1, pageSize: 25, CancellationToken.None);

        var task = Assert.Single(result.Items);
        Assert.Equal("Mine", task.Title);
    }

    [Fact]
    public async Task List_indicates_when_another_page_is_available()
    {
        var repository = new FakeTaskRepository();
        for (var index = 0; index < 3; index++)
        {
            repository.Items.Add(TaskItem.Create($"Task {index}", OwnerId, DateTimeOffset.UtcNow.AddSeconds(index)));
        }

        var handler = new ListTasksHandler(repository);

        var result = await handler.HandleAsync(OwnerId, page: 1, pageSize: 2, CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.True(result.HasMore);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task List_rejects_invalid_pagination(int page, int pageSize)
    {
        var handler = new ListTasksHandler(new FakeTaskRepository());

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => handler.HandleAsync(OwnerId, page, pageSize, CancellationToken.None));
    }

    [Fact]
    public async Task Get_returns_null_when_task_is_missing()
    {
        var repository = new FakeTaskRepository();
        var handler = new GetTaskHandler(repository);

        var result = await handler.HandleAsync(Guid.NewGuid(), OwnerId, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Get_returns_task_from_repository()
    {
        var task = TaskItem.Create("Read the docs", OwnerId, DateTimeOffset.UtcNow);
        var repository = new FakeTaskRepository();
        repository.Items.Add(task);
        var handler = new GetTaskHandler(repository);

        var result = await handler.HandleAsync(task.Id, OwnerId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(task.Id, result.Id);
        Assert.Equal(task.Title, result.Title);
    }

    [Fact]
    public async Task Complete_returns_null_when_task_is_missing()
    {
        var repository = new FakeTaskRepository();
        var handler = new CompleteTaskHandler(repository, new FixedTimeProvider(DateTimeOffset.UtcNow));

        var result = await handler.HandleAsync(Guid.NewGuid(), OwnerId, CancellationToken.None);

        Assert.Null(result);
        Assert.Equal(0, repository.SaveChangesCount);
    }

    [Fact]
    public async Task Complete_updates_task_and_saves_changes()
    {
        var task = TaskItem.Create("Read the docs", OwnerId, DateTimeOffset.UtcNow);
        var repository = new FakeTaskRepository();
        repository.Items.Add(task);
        var completedAt = DateTimeOffset.UtcNow.AddMinutes(2);
        var handler = new CompleteTaskHandler(repository, new FixedTimeProvider(completedAt));

        var result = await handler.HandleAsync(task.Id, OwnerId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.IsCompleted);
        Assert.Equal(completedAt, result.CompletedAtUtc);
        Assert.Equal(1, repository.SaveChangesCount);
    }

    [Fact]
    public async Task Complete_does_not_mutate_another_owners_task()
    {
        var task = TaskItem.Create("Other user's task", "tenant-a:user-2", DateTimeOffset.UtcNow);
        var repository = new FakeTaskRepository();
        repository.Items.Add(task);
        var handler = new CompleteTaskHandler(repository, new FixedTimeProvider(DateTimeOffset.UtcNow));

        var result = await handler.HandleAsync(task.Id, OwnerId, CancellationToken.None);

        Assert.Null(result);
        Assert.False(task.IsCompleted);
        Assert.Equal(0, repository.SaveChangesCount);
    }

    private sealed class FakeTaskRepository : ITaskRepository
    {
        public List<TaskItem> Items { get; } = [];

        public int SaveChangesCount { get; private set; }

        public Task<IReadOnlyList<TaskItem>> ListAsync(
            string ownerId,
            int skip,
            int take,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<TaskItem>>(
                Items.Where(task => task.OwnerId == ownerId)
                    .Skip(skip)
                    .Take(take)
                    .ToArray());
        }

        public Task<TaskItem?> FindByIdAsync(Guid id, string ownerId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Items.SingleOrDefault(
                task => task.Id == id && task.OwnerId == ownerId));
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
