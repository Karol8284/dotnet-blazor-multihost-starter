using System.Security.Claims;
using Microsoft.AspNetCore.RateLimiting;
using BlazorMultihostStarter.Application.Modules.Tasks;
using BlazorMultihostStarter.Application.Modules.Tasks.CompleteTask;
using BlazorMultihostStarter.Application.Modules.Tasks.CreateTask;
using BlazorMultihostStarter.Application.Modules.Tasks.GetTask;
using BlazorMultihostStarter.Application.Modules.Tasks.ListTasks;
using BlazorMultihostStarter.Contracts.Tasks;
using BlazorMultihostStarter.Domain.Modules.Tasks;
using BlazorMultihostStarter.Web.Security;

namespace BlazorMultihostStarter.Web.Modules.Tasks;

public static class TasksEndpoints
{
    public static IEndpointRouteBuilder MapTasksModule(this IEndpointRouteBuilder endpoints)
    {
        var tasks = endpoints.MapGroup("/api/tasks")
            .WithTags("Tasks")
            .RequireAuthorization(EntraApiAuthentication.TasksUserPolicy)
            .RequireRateLimiting("tasks-api");

        tasks.MapGet("/{id:guid}", async (
            Guid id,
            ClaimsPrincipal user,
            GetTaskHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(
                id,
                EntraTaskOwner.GetRequiredOwnerId(user),
                cancellationToken);
            return result is null
                ? Results.NotFound()
                : Results.Ok(ToResponse(result));
        })
        .WithName("GetTask");

        tasks.MapGet("/", async (
            int? page,
            int? pageSize,
            ClaimsPrincipal user,
            ListTasksHandler handler,
            CancellationToken cancellationToken) =>
        {
            var requestedPage = page ?? 1;
            var requestedPageSize = pageSize ?? ListTasksHandler.DefaultPageSize;
            if (requestedPage < 1)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["page"] = ["Page must be positive."]
                });
            }

            if (requestedPageSize is < 1 or > ListTasksHandler.MaxPageSize)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["pageSize"] = [$"Page size must be between 1 and {ListTasksHandler.MaxPageSize}."]
                });
            }

            try
            {
                var result = await handler.HandleAsync(
                    EntraTaskOwner.GetRequiredOwnerId(user),
                    requestedPage,
                    requestedPageSize,
                    cancellationToken);
                return Results.Ok(new TaskListResponse(
                    result.Items.Select(ToResponse).ToArray(),
                    result.Page,
                    result.PageSize,
                    result.HasMore));
            }
            catch (ArgumentOutOfRangeException exception)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [exception.ParamName ?? "page"] = [exception.Message]
                });
            }
        })
        .WithName("ListTasks");

        tasks.MapPost("/", async (
            CreateTaskRequest request,
            ClaimsPrincipal user,
            CreateTaskHandler handler,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await handler.HandleAsync(
                    request.Title,
                    EntraTaskOwner.GetRequiredOwnerId(user),
                    cancellationToken);
                return Results.Created($"/api/tasks/{result.Id}", ToResponse(result));
            }
            catch (TaskDomainException exception)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [exception.Field] = [exception.Message]
                });
            }
        })
        .WithName("CreateTask");

        tasks.MapPost("/{id:guid}/complete", async (
            Guid id,
            ClaimsPrincipal user,
            CompleteTaskHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(
                id,
                EntraTaskOwner.GetRequiredOwnerId(user),
                cancellationToken);
            return result is null
                ? Results.NotFound()
                : Results.Ok(ToResponse(result));
        })
        .WithName("CompleteTask");

        return endpoints;
    }

    private static TaskResponse ToResponse(TaskDetails task) =>
        new(task.Id, task.Title, task.IsCompleted, task.CreatedAtUtc, task.CompletedAtUtc);
}
