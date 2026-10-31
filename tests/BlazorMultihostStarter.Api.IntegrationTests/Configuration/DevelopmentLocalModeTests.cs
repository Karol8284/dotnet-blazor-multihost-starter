using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace BlazorMultihostStarter.Api.IntegrationTests.Configuration;

public sealed class DevelopmentLocalModeTests
{
    [Theory]
    [InlineData("WebAssembly")]
    [InlineData("Server")]
    public async Task Local_UI_starts_without_Entra_or_database_while_API_fails_closed(
        string webInteractivity)
    {
        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.UseSetting("Web:Interactivity", webInteractivity);
                builder.UseSetting("Authentication:Entra:TenantId", string.Empty);
                builder.UseSetting("Authentication:Entra:ApiClientId", string.Empty);
                builder.UseSetting("Authentication:Entra:WebClientId", string.Empty);
                builder.UseSetting("Authentication:Entra:MauiClientId", string.Empty);
                builder.UseSetting("ConnectionStrings:DefaultConnection", string.Empty);
            });
        using var client = factory.CreateClient();

        using var localModeResponse = await client.GetAsync("/tasks/local");
        using var apiResponse = await client.GetAsync("/api/tasks");
        var problem = await apiResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, localModeResponse.StatusCode);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, apiResponse.StatusCode);
        Assert.Contains("Tasks API is not configured", problem, StringComparison.Ordinal);
    }
}
