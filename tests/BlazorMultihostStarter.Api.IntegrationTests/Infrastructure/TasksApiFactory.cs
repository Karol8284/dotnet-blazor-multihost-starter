using BlazorMultihostStarter.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Data.Sqlite;

namespace BlazorMultihostStarter.Api.IntegrationTests.Infrastructure;

public sealed class TasksApiFactory : WebApplicationFactory<Program>
{
    private readonly bool _usePostgreSql = !string.IsNullOrWhiteSpace(
        Environment.GetEnvironmentVariable("TEST_POSTGRES_CONNECTION"));
    private readonly SqliteConnection _sqliteConnection = new("Data Source=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connectionString = _usePostgreSql
            ? Environment.GetEnvironmentVariable("TEST_POSTGRES_CONNECTION")!
            : "Data Source=:memory:";

        builder.UseEnvironment("Development");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<StarterDbContext>();
            services.RemoveAll<DbContextOptions<StarterDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<StarterDbContext>>();
            services.AddDbContext<StarterDbContext>(options =>
            {
                if (_usePostgreSql)
                {
                    options.UseNpgsql(connectionString);
                }
                else
                {
                    options.UseSqlite(_sqliteConnection);
                }
            });
        });
    }

    public async Task InitializeDatabaseAsync()
    {
        if (!_usePostgreSql)
        {
            await _sqliteConnection.OpenAsync();
        }

        using var client = CreateClient();
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StarterDbContext>();

        if (_usePostgreSql)
        {
            await dbContext.Database.MigrateAsync();
        }
        else
        {
            await dbContext.Database.EnsureCreatedAsync();
        }

        await dbContext.Tasks.ExecuteDeleteAsync();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && !_usePostgreSql)
        {
            _sqliteConnection.Dispose();
        }
    }
}
