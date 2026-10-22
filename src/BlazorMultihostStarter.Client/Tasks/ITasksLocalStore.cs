namespace BlazorMultihostStarter.Client.Tasks;

public interface ITasksLocalStore
{
    Task<IReadOnlyList<LocalTaskRecord>> LoadAsync(string key, CancellationToken cancellationToken = default);

    Task SaveAsync(
        string key,
        IReadOnlyList<LocalTaskRecord> tasks,
        CancellationToken cancellationToken = default);
}
