using System.Net;
using System.Net.Http.Json;
using BlazorMultihostStarter.Contracts;
using BlazorMultihostStarter.Contracts.Tasks;

namespace BlazorMultihostStarter.Client.Tasks;

public sealed class TasksApiClient(HttpClient httpClient) : ITasksApiClient
{
    public async Task<IReadOnlyList<TaskResponse>> ListAsync(CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync("api/tasks", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<List<TaskResponse>>(cancellationToken)
            ?? throw new HttpRequestException("The API returned an empty task list response.");
    }

    public async Task<TaskResponse> CreateAsync(
        string? title,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/tasks",
            new CreateTaskRequest(title),
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<TaskResponse>(cancellationToken)
            ?? throw new HttpRequestException("The API returned an empty task response.");
    }

    public async Task<TaskResponse> CompleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsync(
            $"api/tasks/{id}/complete",
            content: null,
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<TaskResponse>(cancellationToken)
            ?? throw new HttpRequestException("The API returned an empty task response.");
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemResponse>(
                cancellationToken);
            var message = problem?.Errors.Values
                .SelectMany(errors => errors)
                .FirstOrDefault();

            throw new TaskApiValidationException(message ?? "The task request is invalid.");
        }

        response.EnsureSuccessStatusCode();
    }
}
