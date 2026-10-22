namespace BlazorMultihostStarter.Application.Modules.Tasks;

public sealed class TaskConcurrencyException(Guid taskId, Exception innerException)
    : Exception($"Task {taskId} was changed by another request.", innerException)
{
    public Guid TaskId { get; } = taskId;
}
