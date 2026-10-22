using BlazorMultihostStarter.Contracts.Tasks;

namespace BlazorMultihostStarter.Client.Tasks;

public sealed class LocalTasksClient(
    ITasksLocalStore store,
    TimeProvider timeProvider) : ILocalTasksClient
{
    private const string StorageKey = "tasks.local.v1";

    public Task<IReadOnlyList<LocalTaskRecord>> ListAsync(
        CancellationToken cancellationToken = default) =>
        store.LoadAsync(StorageKey, cancellationToken);

    public async Task<TaskResponse> CreateAsync(
        string? title,
        CancellationToken cancellationToken = default)
    {
        var normalizedTitle = ValidateTitle(title);
        var tasks = (await store.LoadAsync(StorageKey, cancellationToken)).ToList();
        var task = new TaskResponse(
            Guid.NewGuid(),
            normalizedTitle,
            false,
            timeProvider.GetUtcNow(),
            null,
            0);
        tasks.Add(new LocalTaskRecord(task, 0, LocalTaskSyncState.LocalOnly));
        await store.SaveAsync(StorageKey, tasks, cancellationToken);
        return task;
    }

    public async Task<TaskResponse> CompleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var tasks = (await store.LoadAsync(StorageKey, cancellationToken)).ToList();
        var index = tasks.FindIndex(item => item.Task.Id == id);
        if (index < 0)
        {
            throw new HttpRequestException("The task was not found.", null, System.Net.HttpStatusCode.NotFound);
        }

        var task = tasks[index].Task;
        if (!task.IsCompleted)
        {
            task = task with
            {
                IsCompleted = true,
                CompletedAtUtc = timeProvider.GetUtcNow()
            };
            tasks[index] = tasks[index] with { Task = task };
            await store.SaveAsync(StorageKey, tasks, cancellationToken);
        }

        return task;
    }

    public async Task<TaskResponse> RenameAsync(
        Guid id,
        string? title,
        CancellationToken cancellationToken = default)
    {
        var normalizedTitle = ValidateTitle(title);
        var tasks = (await store.LoadAsync(StorageKey, cancellationToken)).ToList();
        var index = tasks.FindIndex(item => item.Task.Id == id);
        if (index < 0)
        {
            throw new HttpRequestException("The task was not found.", null, System.Net.HttpStatusCode.NotFound);
        }

        var task = tasks[index].Task with { Title = normalizedTitle };
        tasks[index] = tasks[index] with { Task = task };
        await store.SaveAsync(StorageKey, tasks, cancellationToken);
        return task;
    }

    internal static string ValidateTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new TaskApiValidationException("Enter a task title.");
        }

        var normalizedTitle = title.Trim();
        if (normalizedTitle.Length > 120)
        {
            throw new TaskApiValidationException("A task title cannot exceed 120 characters.");
        }

        return normalizedTitle;
    }
}
