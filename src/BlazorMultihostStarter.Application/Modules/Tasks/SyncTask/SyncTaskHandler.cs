using BlazorMultihostStarter.Domain.Modules.Tasks;

namespace BlazorMultihostStarter.Application.Modules.Tasks.SyncTask;

public sealed class SyncTaskHandler(ITaskRepository tasks, TimeProvider timeProvider)
{
    public async Task<TaskSyncOutcome> HandleAsync(
        Guid routeId,
        SyncTaskCommand request,
        string ownerId,
        CancellationToken cancellationToken)
    {
        if (routeId == Guid.Empty || request.Id != routeId)
        {
            throw new TaskDomainException(nameof(request.Id), "The task ID must match the route ID.");
        }

        if (request.BaseVersion < 0)
        {
            throw new TaskDomainException(nameof(request.BaseVersion), "The base version cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new TaskDomainException(nameof(request.Title), "Enter a task title.");
        }

        if (request.Title.Trim().Length > 120)
        {
            throw new TaskDomainException(nameof(request.Title), "A task title cannot exceed 120 characters.");
        }

        var task = await tasks.FindByIdAsync(routeId, ownerId, cancellationToken);
        if (task is null)
        {
            if (await tasks.FindAnyByIdAsync(routeId, cancellationToken) is not null)
            {
                return new TaskSyncOutcome.Missing();
            }

            if (request.BaseVersion != 0)
            {
                return new TaskSyncOutcome.Missing();
            }

            task = TaskItem.Create(routeId, request.Title, ownerId, timeProvider.GetUtcNow());
            if (request.IsCompleted)
            {
                task.Complete(timeProvider.GetUtcNow());
            }

            await tasks.AddAsync(task, cancellationToken);
            try
            {
                await tasks.SaveChangesAsync(cancellationToken);
            }
            catch (TaskConcurrencyException)
            {
                var currentTask = await tasks.FindCurrentByIdAsync(
                    routeId,
                    ownerId,
                    cancellationToken);
                return currentTask is null
                    ? new TaskSyncOutcome.Missing()
                    : new TaskSyncOutcome.Conflict(TaskDetails.From(currentTask));
            }

            return new TaskSyncOutcome.Succeeded(TaskDetails.From(task), Created: true);
        }

        if (request.BaseVersion != task.Version)
        {
            if (MatchesSnapshot(task, request))
            {
                return new TaskSyncOutcome.Succeeded(TaskDetails.From(task));
            }

            return new TaskSyncOutcome.Conflict(TaskDetails.From(task));
        }

        if (task.IsCompleted && !request.IsCompleted)
        {
            return new TaskSyncOutcome.Conflict(TaskDetails.From(task));
        }

        task.Rename(request.Title);
        if (request.IsCompleted)
        {
            task.Complete(timeProvider.GetUtcNow());
        }

        try
        {
            await tasks.SaveChangesAsync(cancellationToken);
        }
        catch (TaskConcurrencyException)
        {
            var currentTask = await tasks.FindCurrentByIdAsync(
                routeId,
                ownerId,
                cancellationToken);
            return currentTask is null
                ? new TaskSyncOutcome.Missing()
                : new TaskSyncOutcome.Conflict(TaskDetails.From(currentTask));
        }

        return new TaskSyncOutcome.Succeeded(TaskDetails.From(task));
    }

    private static bool MatchesSnapshot(TaskItem task, SyncTaskCommand request) =>
        string.Equals(task.Title, request.Title.Trim(), StringComparison.Ordinal)
        && task.IsCompleted == request.IsCompleted;
}
