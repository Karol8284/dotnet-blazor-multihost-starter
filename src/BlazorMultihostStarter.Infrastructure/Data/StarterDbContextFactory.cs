using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BlazorMultihostStarter.Infrastructure.Data;

public sealed class StarterDbContextFactory : IDesignTimeDbContextFactory<StarterDbContext>
{
    public StarterDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=blazor_multihost_starter;Username=starter;Password=starter_dev_password";

        var options = new DbContextOptionsBuilder<StarterDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new StarterDbContext(options);
    }
}
