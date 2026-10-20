using BlazorMultihostStarter.Application.Modules.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorMultihostStarter.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        return services.AddTasksModule();
    }
}
