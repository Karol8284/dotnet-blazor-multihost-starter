using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BlazorMultihostStarter.Client.Authentication;
using BlazorMultihostStarter.Contracts;
using BlazorMultihostStarter.Contracts.Tasks;

namespace BlazorMultihostStarter.Client.Tasks;

public sealed class TasksApiClient(
    HttpClient httpClient,
    IUserAuthenticationSession authenticationSession) : ITasksApiClient
{
    public async Task<TaskListResponse> ListAsync(
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/tasks?page={page}&pageSize={pageSize}");
        using var response = await SendAuthorizedAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<TaskListResponse>(cancellationToken)
            ?? throw new HttpRequestException("The API returned an empty task page response.");
    }

    public async Task<TaskResponse> CreateAsync(
        string? title,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/tasks")
        {
            Content = JsonContent.Create(new CreateTaskRequest(title))
        };
        using var response = await SendAuthorizedAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<TaskResponse>(cancellationToken)
            ?? throw new HttpRequestException("The API returned an empty task response.");
    }

    public async Task<TaskResponse> CompleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"api/tasks/{id}/complete");
        using var response = await SendAuthorizedAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<TaskResponse>(cancellationToken)
            ?? throw new HttpRequestException("The API returned an empty task response.");
    }

    private async Task<HttpResponseMessage> SendAuthorizedAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var accessToken = await authenticationSession.GetAccessTokenAsync(cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return await httpClient.SendAsync(request, cancellationToken);
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
