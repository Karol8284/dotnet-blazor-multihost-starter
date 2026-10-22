using BlazorMultihostStarter.Client.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BlazorMultihostStarter.Shared.Tasks;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedTaskModes(this IServiceCollection services)
    {
        services.TryAddScoped<ITasksLocalStore, BrowserTasksLocalStore>();
        services.TryAddScoped<ILocalTasksClient, LocalTasksClient>();
        services.TryAddScoped<IHybridTasksClient, HybridTasksClient>();
        services.TryAddSingleton<TimeProvider>(TimeProvider.System);
        return services;
    }
}
