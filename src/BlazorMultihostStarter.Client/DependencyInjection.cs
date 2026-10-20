using BlazorMultihostStarter.Client.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorMultihostStarter.Client;

public static class DependencyInjection
{
    public static IServiceCollection AddTasksApiClient(
        this IServiceCollection services,
        Uri apiBaseAddress)
    {
        ArgumentNullException.ThrowIfNull(apiBaseAddress);
        if (!apiBaseAddress.IsAbsoluteUri)
        {
            throw new ArgumentException("The API base address must be absolute.", nameof(apiBaseAddress));
        }

        services.AddScoped(_ => new HttpClient { BaseAddress = apiBaseAddress });
        services.AddScoped<ITasksApiClient, TasksApiClient>();

        return services;
    }
}
