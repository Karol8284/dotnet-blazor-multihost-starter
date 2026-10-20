namespace BlazorMultihostStarter.Contracts;

public sealed record ValidationProblemResponse
{
    public Dictionary<string, string[]> Errors { get; init; } = [];
}
