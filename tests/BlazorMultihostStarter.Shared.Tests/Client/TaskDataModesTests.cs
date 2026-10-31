using BlazorMultihostStarter.Client.Tasks;
using BlazorMultihostStarter.Contracts.Tasks;

namespace BlazorMultihostStarter.Shared.Tests.Client;

public sealed class TaskDataModesTests
{
    [Fact]
    public async Task Client_local_tasks_are_persisted_without_a_server_sync_state()
    {
        var store = new FakeTasksLocalStore();
        var client = new LocalTasksClient(store, new FixedTimeProvider(DateTimeOffset.UtcNow));

        var created = await client.CreateAsync("  Keep on device  ");
        await client.RenameAsync(created.Id, "Renamed locally");
        await client.CompleteAsync(created.Id);
        var record = Assert.Single(await client.ListAsync());

        Assert.Equal("Renamed locally", record.Task.Title);
        Assert.True(record.Task.IsCompleted);
        Assert.Equal(LocalTaskSyncState.LocalOnly, record.SyncState);
        Assert.Equal(0, record.BaseVersion);
    }

    [Fact]
    public async Task Hybrid_tasks_keep_local_changes_until_the_server_applies_them()
    {
        var store = new FakeTasksLocalStore();
        var server = new FakeTasksClient();
        var client = new HybridTasksClient(
            store,
            server,
            new FixedTimeProvider(DateTimeOffset.UtcNow));
        var created = await client.CreateAsync("Created offline");
        server.SyncResult = request => new TaskSyncResult.Applied(
            new TaskResponse(request.Id, request.Title, false, DateTimeOffset.UtcNow, null, 1));

        var report = await client.SyncPendingAsync();
        var record = Assert.Single(await client.ListAsync());

        Assert.Equal(1, report.AppliedCount);
        Assert.Equal(LocalTaskSyncState.Synced, record.SyncState);
        Assert.Equal(1, record.BaseVersion);
        Assert.Equal(created.Id, record.Task.Id);
    }

    [Fact]
    public async Task Hybrid_conflicts_require_an_explicit_choice_before_retry()
    {
        var store = new FakeTasksLocalStore();
        var server = new FakeTasksClient();
        var client = new HybridTasksClient(
            store,
            server,
            new FixedTimeProvider(DateTimeOffset.UtcNow));
        var localTask = await client.CreateAsync("Local title");
        var serverTask = new TaskResponse(
            localTask.Id,
            "Server title",
            false,
            localTask.CreatedAtUtc,
            null,
            2);
        server.SyncResult = _ => new TaskSyncResult.Conflict(serverTask);

        var conflictReport = await client.SyncPendingAsync();
        var conflict = Assert.Single(await client.ListAsync());
        Assert.Equal(1, conflictReport.ConflictCount);
        Assert.Equal(LocalTaskSyncState.Conflict, conflict.SyncState);
        Assert.Equal("Local title", conflict.Task.Title);
        Assert.Equal("Server title", conflict.ServerTask?.Title);

        await client.ResolveConflictAsync(localTask.Id, keepLocal: true);
        var pending = Assert.Single(await client.ListAsync());
        Assert.Equal(LocalTaskSyncState.Pending, pending.SyncState);
        Assert.Equal(2, pending.BaseVersion);
        Assert.Equal("Local title", pending.Task.Title);

        server.SyncResult = request => new TaskSyncResult.Applied(
            new TaskResponse(request.Id, request.Title, false, localTask.CreatedAtUtc, null, 3));
        var retryReport = await client.SyncPendingAsync();
        var synced = Assert.Single(await client.ListAsync());

        Assert.Equal(1, retryReport.AppliedCount);
        Assert.Equal(LocalTaskSyncState.Synced, synced.SyncState);
        Assert.Equal(3, synced.BaseVersion);
    }

    [Fact]
    public async Task Hybrid_conflicts_can_be_resolved_by_keeping_server()
    {
        var store = new FakeTasksLocalStore();
        var server = new FakeTasksClient();
        var client = new HybridTasksClient(
            store,
            server,
            new FixedTimeProvider(DateTimeOffset.UtcNow));
        var localTask = await client.CreateAsync("Local title");
        var serverTask = new TaskResponse(
            localTask.Id,
            "Server title",
            false,
            localTask.CreatedAtUtc,
            null,
            2);
        server.SyncResult = _ => new TaskSyncResult.Conflict(serverTask);

        var conflictReport = await client.SyncPendingAsync();
        var conflict = Assert.Single(await client.ListAsync());
        Assert.Equal(1, conflictReport.ConflictCount);
        Assert.Equal(LocalTaskSyncState.Conflict, conflict.SyncState);
        Assert.Equal("Local title", conflict.Task.Title);
        Assert.Equal("Server title", conflict.ServerTask?.Title);

        await client.ResolveConflictAsync(localTask.Id, keepLocal: false);
        var resolved = Assert.Single(await client.ListAsync());
        Assert.Equal(LocalTaskSyncState.Synced, resolved.SyncState);
        Assert.Equal(2, resolved.BaseVersion);
        Assert.Equal("Server title", resolved.Task.Title);
        Assert.Null(resolved.ServerTask);

        var nextReport = await client.SyncPendingAsync();
        Assert.Equal(0, nextReport.AppliedCount);
        Assert.Equal(0, nextReport.ConflictCount);
        Assert.Equal(0, nextReport.MissingCount);
    }

    private sealed class FakeTasksLocalStore : ITasksLocalStore
    {
        private readonly Dictionary<string, List<LocalTaskRecord>> _tasks = [];

        public Task<IReadOnlyList<LocalTaskRecord>> LoadAsync(
            string key,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<LocalTaskRecord>>(
                _tasks.TryGetValue(key, out var records) ? records.ToArray() : []);

        public Task SaveAsync(
            string key,
            IReadOnlyList<LocalTaskRecord> tasks,
            CancellationToken cancellationToken = default)
        {
            _tasks[key] = tasks.ToList();
            return Task.CompletedTask;
        }
    }

    private sealed class FakeTasksClient : ITasksClient
    {
        public Func<SyncTaskRequest, TaskSyncResult> SyncResult { get; set; } =
            _ => throw new InvalidOperationException("The fake server has no sync response configured.");

        public Task<TaskListResponse> ListAsync(
            int page = 1,
            int pageSize = 25,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new TaskListResponse([], page, pageSize, false));

        public Task<TaskResponse> CreateAsync(
            string? title,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<TaskResponse> CompleteAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<TaskSyncResult> SyncAsync(
            SyncTaskRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(SyncResult(request));
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
