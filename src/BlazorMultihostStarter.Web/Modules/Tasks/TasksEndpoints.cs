using BlazorMultihostStarter.Application.Modules.Tasks;
using BlazorMultihostStarter.Application.Modules.Tasks.CompleteTask;
using BlazorMultihostStarter.Application.Modules.Tasks.CreateTask;
using BlazorMultihostStarter.Application.Modules.Tasks.GetTask;
using BlazorMultihostStarter.Application.Modules.Tasks.ListTasks;
using BlazorMultihostStarter.Contracts.Tasks;
using BlazorMultihostStarter.Domain.Modules.Tasks;

namespace BlazorMultihostStarter.Web.Modules.Tasks;

public static class TasksEndpoints
{
    public static IEndpointRouteBuilder MapTasksModule(this IEndpointRouteBuilder endpoints)
    {
        var tasks = endpoints.MapGroup("/api/tasks").WithTags("Tasks");

        tasks.MapGet("/{id:guid}", async (
            Guid id,
            GetTaskHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(id, cancellationToken);
            return result is null
                ? Results.NotFound()
                : Results.Ok(ToResponse(result));
        })
        .WithName("GetTask");

        tasks.MapGet("/", async (ListTasksHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(cancellationToken);
            return Results.Ok(result.Select(ToResponse).ToArray());
        })
        .WithName("ListTasks");

        tasks.MapPost("/", async (
            CreateTaskRequest request,
            CreateTaskHandler handler,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await handler.HandleAsync(request.Title, cancellationToken);
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
            CompleteTaskHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(id, cancellationToken);
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
