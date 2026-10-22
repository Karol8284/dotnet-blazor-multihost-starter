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
                builder.ConfigureAppConfiguration((_, configuration) =>
                {
                    configuration.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Authentication:Entra:TenantId"] = "__TENANT_ID__",
                        ["Authentication:Entra:ApiClientId"] = "__API_CLIENT_ID__",
                        ["Authentication:Entra:WebClientId"] = "__WEB_CLIENT_ID__",
                        ["Authentication:Entra:MauiClientId"] = "__MAUI_CLIENT_ID__",
                        ["Authentication:Entra:ApiScopeName"] = "Tasks.ReadWrite",
                        ["ConnectionStrings:DefaultConnection"] = "",
                        ["Web:Interactivity"] = webInteractivity
                    });
                });
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
