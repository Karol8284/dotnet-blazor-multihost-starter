namespace BlazorMultihostStarter.Domain.Modules.Tasks;

public sealed class TaskDomainException(string field, string message) : Exception(message)
{
    public string Field { get; } = field;
}
