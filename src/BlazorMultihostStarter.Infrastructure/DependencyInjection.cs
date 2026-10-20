using BlazorMultihostStarter.Infrastructure.Data;
using BlazorMultihostStarter.Infrastructure.Modules.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorMultihostStarter.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        string databaseProvider = "PostgreSQL")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<StarterDbContext>(options =>
        {
            if (databaseProvider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
            {
                options.UseNpgsql(connectionString);
            }
            else if (databaseProvider.Equals("SQLite", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString);
            }
            else
            {
                throw new InvalidOperationException(
                    $"The database provider '{databaseProvider}' is not supported.");
            }
        });

        return services.AddTasksInfrastructure();
    }
}
