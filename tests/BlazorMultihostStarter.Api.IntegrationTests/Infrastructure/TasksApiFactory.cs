using BlazorMultihostStarter.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using BlazorMultihostStarter.Web.Security;
using Microsoft.Data.Sqlite;

namespace BlazorMultihostStarter.Api.IntegrationTests.Infrastructure;

public sealed class TasksApiFactory : WebApplicationFactory<Program>
{
    private readonly bool _useInteractiveServer;
    private readonly bool _usePostgreSql = !string.IsNullOrWhiteSpace(
        Environment.GetEnvironmentVariable("TEST_POSTGRES_CONNECTION"));
    private readonly SqliteConnection _sqliteConnection = new("Data Source=:memory:");

    public TasksApiFactory(bool useInteractiveServer = false)
    {
        _useInteractiveServer = useInteractiveServer;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connectionString = _usePostgreSql
            ? Environment.GetEnvironmentVariable("TEST_POSTGRES_CONNECTION")!
            : "Data Source=:memory:";

        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Entra:TenantId"] = TestAuthenticationHandler.TenantId,
                ["Authentication:Entra:ApiClientId"] = TestAuthenticationHandler.ApiClientId,
                ["Authentication:Entra:WebClientId"] = TestAuthenticationHandler.WebClientId,
                ["Authentication:Entra:MauiClientId"] = TestAuthenticationHandler.MauiClientId,
                ["Authentication:Entra:ApiScopeName"] = "Tasks.ReadWrite",
                ["Authentication:Entra:ServerClientId"] = "55555555-5555-4555-8555-555555555555",
                ["Authentication:Entra:ServerClientSecret"] = "test-only-placeholder",
                ["Web:Interactivity"] = _useInteractiveServer ? "Server" : "WebAssembly"
            });
        });
        builder.ConfigureTestServices(services =>
        {
            services.Configure<EntraApiOptions>(options =>
            {
                options.TenantId = TestAuthenticationHandler.TenantId;
                options.ApiClientId = TestAuthenticationHandler.ApiClientId;
                options.WebClientId = TestAuthenticationHandler.WebClientId;
                options.MauiClientId = TestAuthenticationHandler.MauiClientId;
                options.ApiScopeName = "Tasks.ReadWrite";
            });

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

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthenticationHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthenticationHandler.SchemeName;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                TestAuthenticationHandler.SchemeName,
                _ => { });

            services.PostConfigure<AuthorizationOptions>(options =>
            {
                var originalPolicy = options.GetPolicy(EntraApiAuthentication.TasksUserPolicy)
                    ?? throw new InvalidOperationException("The Tasks API policy is not registered.");
                var testPolicy = new AuthorizationPolicyBuilder(TestAuthenticationHandler.SchemeName)
                    .RequireAuthenticatedUser()
                    .AddRequirements(originalPolicy.Requirements.ToArray())
                    .Build();
                options.AddPolicy(EntraApiAuthentication.TasksUserPolicy, testPolicy);
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
