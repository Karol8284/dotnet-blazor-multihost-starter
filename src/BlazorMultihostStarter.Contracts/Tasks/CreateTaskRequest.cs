namespace BlazorMultihostStarter.Contracts.Tasks;

/// <summary>Represents a request to create a task.</summary>
public sealed record CreateTaskRequest(string? Title);
