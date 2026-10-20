using BlazorMultihostStarter.Application.Modules.Tasks.CompleteTask;
using BlazorMultihostStarter.Application.Modules.Tasks.CreateTask;
using BlazorMultihostStarter.Application.Modules.Tasks.GetTask;
using BlazorMultihostStarter.Application.Modules.Tasks.ListTasks;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorMultihostStarter.Application.Modules.Tasks;

public static class TasksModule
{
    public static IServiceCollection AddTasksModule(this IServiceCollection services)
    {
        services.AddScoped<CreateTaskHandler>();
        services.AddScoped<GetTaskHandler>();
        services.AddScoped<ListTasksHandler>();
        services.AddScoped<CompleteTaskHandler>();

        return services;
    }
}
