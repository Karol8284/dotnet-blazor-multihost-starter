using System.Net;
using BlazorMultihostStarter.Api.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace BlazorMultihostStarter.Api.IntegrationTests.Diagnostics;

public sealed class HealthCheckEndpointTests
{
    [Theory]
    [InlineData("/healthz")]
    [InlineData("/health/live")]
    public async Task Liveness_probe_returns_ok_and_healthy(string path)
    {
        await using var factory = new TasksApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(path);
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", content);
    }

    [Fact]
    public async Task Readiness_probe_returns_ok_when_database_is_connected()
    {
        await using var factory = new TasksApiFactory();
        await factory.InitializeDatabaseAsync();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/health/ready");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", content);
    }

    [Fact]
    public async Task Readiness_probe_returns_ok_in_development_when_database_is_unconfigured()
    {
        await using var devFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.UseSetting("Authentication:Entra:TenantId", string.Empty);
                builder.UseSetting("Authentication:Entra:ApiClientId", string.Empty);
                builder.UseSetting("Authentication:Entra:WebClientId", string.Empty);
                builder.UseSetting("Authentication:Entra:MauiClientId", string.Empty);
                builder.UseSetting("ConnectionStrings:DefaultConnection", string.Empty);
                builder.UseSetting("Web:Interactivity", "WebAssembly");
            });

        using var client = devFactory.CreateClient();

        using var response = await client.GetAsync("/health/ready");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", content);
    }

    [Fact]
    public async Task Readiness_probe_returns_service_unavailable_when_database_cannot_connect()
    {
        await using var brokenDbFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.UseSetting("Authentication:Entra:TenantId", "11111111-1111-4111-8111-111111111111");
                builder.UseSetting("Authentication:Entra:ApiClientId", "22222222-2222-4222-8222-222222222222");
                builder.UseSetting("Authentication:Entra:WebClientId", "33333333-3333-4333-8333-333333333333");
                builder.UseSetting("Authentication:Entra:MauiClientId", "44444444-4444-4444-8444-444444444444");
                builder.UseSetting("Authentication:Entra:ApiScopeName", "Tasks.ReadWrite");
                builder.UseSetting(
                    "ConnectionStrings:DefaultConnection",
                    "Host=127.0.0.1;Port=54399;Database=nonexistent;Username=invalid;Password=invalid;Timeout=1;Command Timeout=1");
                builder.UseSetting("Database:Provider", "PostgreSQL");
                builder.UseSetting("Web:Interactivity", "WebAssembly");
            });

        using var client = brokenDbFactory.CreateClient();

        using var response = await client.GetAsync("/health/ready");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("Unhealthy", content);
    }
}
