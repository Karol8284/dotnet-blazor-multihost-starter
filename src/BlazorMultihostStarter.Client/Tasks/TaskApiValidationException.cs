namespace BlazorMultihostStarter.Client.Tasks;

public sealed class TaskApiValidationException(string message) : Exception(message)
{
}
