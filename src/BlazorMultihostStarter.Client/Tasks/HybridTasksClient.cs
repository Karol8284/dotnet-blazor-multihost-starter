using BlazorMultihostStarter.Contracts.Tasks;

namespace BlazorMultihostStarter.Client.Tasks;

public sealed class HybridTasksClient(
    ITasksLocalStore store,
    ITasksClient serverTasks,
    TimeProvider timeProvider) : IHybridTasksClient
{
    private const string StorageKey = "tasks.hybrid.v1";

    public Task<IReadOnlyList<LocalTaskRecord>> ListAsync(
        CancellationToken cancellationToken = default) =>
        store.LoadAsync(StorageKey, cancellationToken);

    public async Task<TaskResponse> CreateAsync(
        string? title,
        CancellationToken cancellationToken = default)
    {
        var normalizedTitle = LocalTasksClient.ValidateTitle(title);
        var tasks = (await store.LoadAsync(StorageKey, cancellationToken)).ToList();
        var task = new TaskResponse(
            Guid.NewGuid(),
            normalizedTitle,
            false,
            timeProvider.GetUtcNow(),
            null,
            0);
        tasks.Add(new LocalTaskRecord(task, 0, LocalTaskSyncState.Pending));
        await store.SaveAsync(StorageKey, tasks, cancellationToken);
        return task;
    }

    public async Task<TaskResponse> CompleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var tasks = (await store.LoadAsync(StorageKey, cancellationToken)).ToList();
        var index = FindEditableTask(tasks, id);
        var task = tasks[index].Task;
        if (!task.IsCompleted)
        {
            task = task with
            {
                IsCompleted = true,
                CompletedAtUtc = timeProvider.GetUtcNow()
            };
            tasks[index] = tasks[index] with
            {
                Task = task,
                SyncState = LocalTaskSyncState.Pending
            };
            await store.SaveAsync(StorageKey, tasks, cancellationToken);
        }

        return task;
    }

    public async Task<TaskResponse> RenameAsync(
        Guid id,
        string? title,
        CancellationToken cancellationToken = default)
    {
        var normalizedTitle = LocalTasksClient.ValidateTitle(title);
        var tasks = (await store.LoadAsync(StorageKey, cancellationToken)).ToList();
        var index = FindEditableTask(tasks, id);
        var task = tasks[index].Task with { Title = normalizedTitle };
        tasks[index] = tasks[index] with
        {
            Task = task,
            SyncState = LocalTaskSyncState.Pending
        };
        await store.SaveAsync(StorageKey, tasks, cancellationToken);
        return task;
    }

    public async Task<HybridSyncReport> SyncPendingAsync(
        CancellationToken cancellationToken = default)
    {
        var tasks = (await store.LoadAsync(StorageKey, cancellationToken)).ToList();
        var appliedCount = 0;
        var conflictCount = 0;
        var missingCount = 0;

        for (var index = 0; index < tasks.Count; index++)
        {
            var record = tasks[index];
            if (record.SyncState != LocalTaskSyncState.Pending)
            {
                continue;
            }

            var result = await serverTasks.SyncAsync(
                new SyncTaskRequest(
                    record.Task.Id,
                    record.Task.Title,
                    record.Task.IsCompleted,
                    record.BaseVersion),
                cancellationToken);

            switch (result)
            {
                case TaskSyncResult.Applied applied:
                    tasks[index] = record with
                    {
                        Task = applied.Task,
                        BaseVersion = applied.Task.Version,
                        SyncState = LocalTaskSyncState.Synced,
                        ServerTask = null
                    };
                    appliedCount++;
                    break;
                case TaskSyncResult.Conflict conflict:
                    tasks[index] = record with
                    {
                        SyncState = LocalTaskSyncState.Conflict,
                        ServerTask = conflict.ServerTask
                    };
                    conflictCount++;
                    break;
                case TaskSyncResult.Missing:
                    tasks[index] = record with
                    {
                        SyncState = LocalTaskSyncState.Missing,
                        ServerTask = null
                    };
                    missingCount++;
                    break;
                default:
                    throw new InvalidOperationException("The task sync result is unknown.");
            }

            await store.SaveAsync(StorageKey, tasks, cancellationToken);
        }

        return new HybridSyncReport(appliedCount, conflictCount, missingCount);
    }

    public async Task ResolveConflictAsync(
        Guid id,
        bool keepLocal,
        CancellationToken cancellationToken = default)
    {
        var tasks = (await store.LoadAsync(StorageKey, cancellationToken)).ToList();
        var index = tasks.FindIndex(item => item.Task.Id == id);
        if (index < 0 || tasks[index].SyncState != LocalTaskSyncState.Conflict)
        {
            throw new InvalidOperationException("The task has no server-version conflict to resolve.");
        }

        var record = tasks[index];
        var serverTask = record.ServerTask
            ?? throw new InvalidOperationException("The conflict is missing its server snapshot.");
        tasks[index] = keepLocal
            ? record with
            {
                BaseVersion = serverTask.Version,
                SyncState = LocalTaskSyncState.Pending,
                ServerTask = null
            }
            : record with
            {
                Task = serverTask,
                BaseVersion = serverTask.Version,
                SyncState = LocalTaskSyncState.Synced,
                ServerTask = null
            };
        await store.SaveAsync(StorageKey, tasks, cancellationToken);
    }

    public async Task RemoveMissingAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var tasks = (await store.LoadAsync(StorageKey, cancellationToken)).ToList();
        var removed = tasks.RemoveAll(
            item => item.Task.Id == id && item.SyncState == LocalTaskSyncState.Missing);
        if (removed == 0)
        {
            throw new InvalidOperationException("The task is not marked as missing on the server.");
        }

        await store.SaveAsync(StorageKey, tasks, cancellationToken);
    }

    private static int FindEditableTask(IReadOnlyList<LocalTaskRecord> tasks, Guid id)
    {
        var index = tasks.ToList().FindIndex(item => item.Task.Id == id);
        if (index < 0)
        {
            throw new HttpRequestException("The task was not found.", null, System.Net.HttpStatusCode.NotFound);
        }

        if (tasks[index].SyncState is LocalTaskSyncState.Conflict or LocalTaskSyncState.Missing)
        {
            throw new InvalidOperationException("Resolve the synchronization issue before editing this task.");
        }

        return index;
    }
}
