using BlazorMultihostStarter.Web.Security;

namespace BlazorMultihostStarter.Api.IntegrationTests.Configuration;

public sealed class EntraApiOptionsTests
{
    [Fact]
    public void Defaults_are_invalid_until_the_tenant_and_client_registrations_are_configured()
    {
        var options = new EntraApiOptions();

        Assert.False(options.IsValid);
    }

    [Fact]
    public void Validates_the_api_and_both_public_client_registrations()
    {
        var options = new EntraApiOptions
        {
            TenantId = "11111111-1111-4111-8111-111111111111",
            ApiClientId = "22222222-2222-4222-8222-222222222222",
            WebClientId = "33333333-3333-4333-8333-333333333333",
            MauiClientId = "44444444-4444-4444-8444-444444444444",
            ApiScopeName = "Tasks.ReadWrite"
        };

        Assert.True(options.IsValid);
        Assert.Equal(
            "https://login.microsoftonline.com/11111111-1111-4111-8111-111111111111/v2.0",
            options.Authority);
        Assert.Equal(
            "api://22222222-2222-4222-8222-222222222222/Tasks.ReadWrite",
            options.ApiScope);
    }

    [Fact]
    public void Rejects_empty_tenant_and_client_identifiers()
    {
        var options = new EntraApiOptions
        {
            TenantId = "00000000-0000-0000-0000-000000000000",
            ApiClientId = "22222222-2222-4222-8222-222222222222",
            WebClientId = "33333333-3333-4333-8333-333333333333",
            MauiClientId = "44444444-4444-4444-8444-444444444444",
            ApiScopeName = "Tasks.ReadWrite"
        };

        Assert.False(options.IsValid);
    }
}
