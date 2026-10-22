using BlazorMultihostStarter.Client.Authentication;

namespace BlazorMultihostStarter.Shared.Tests.Client;

public sealed class EntraClientOptionsTests
{
    [Fact]
    public void Validates_public_client_settings_and_builds_the_API_scope()
    {
        var options = new EntraClientOptions
        {
            TenantId = "11111111-1111-4111-8111-111111111111",
            ClientId = "33333333-3333-4333-8333-333333333333",
            ApiClientId = "22222222-2222-4222-8222-222222222222",
            ApiScopeName = "Tasks.ReadWrite"
        };

        options.Validate();

        Assert.Equal(
            "https://login.microsoftonline.com/11111111-1111-4111-8111-111111111111",
            options.Authority);
        Assert.Equal(
            "api://22222222-2222-4222-8222-222222222222/Tasks.ReadWrite",
            options.ApiScope);
    }

    [Fact]
    public void Rejects_unconfigured_client_identifiers()
    {
        var options = new EntraClientOptions();

        Assert.False(options.IsValid);
        Assert.Throws<InvalidOperationException>(options.Validate);
    }
}
