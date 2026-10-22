namespace BlazorMultihostStarter.Application.Modules.Tasks.ListTasks;

public sealed class ListTasksHandler(ITaskRepository tasks)
{
    public const int DefaultPageSize = 25;
    public const int MaxPageSize = 100;

    public async Task<TaskPage> HandleAsync(
        string ownerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Page must be positive.");
        }

        if (pageSize is < 1 or > MaxPageSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                $"Page size must be between 1 and {MaxPageSize}.");
        }

        var skip = ((long)page - 1) * pageSize;
        if (skip > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Page is outside the supported range.");
        }

        var items = await tasks.ListAsync(
            ownerId,
            (int)skip,
            pageSize + 1,
            cancellationToken);
        var hasMore = items.Count > pageSize;
        var pageItems = items
            .Take(pageSize)
            .Select(TaskDetails.From)
            .ToArray();

        return new TaskPage(pageItems, page, pageSize, hasMore);
    }
}
