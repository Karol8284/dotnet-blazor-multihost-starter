using BlazorMultihostStarter.Application.Modules.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorMultihostStarter.Infrastructure.Modules.Tasks;

public static class TasksModule
{
    public static IServiceCollection AddTasksInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ITaskRepository, EfTaskRepository>();
        return services;
    }
}
