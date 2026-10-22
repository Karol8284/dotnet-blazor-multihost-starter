using BlazorMultihostStarter.Web.Security;
using Microsoft.Extensions.Configuration;

namespace BlazorMultihostStarter.Api.IntegrationTests.Configuration;

public sealed class ProductionConfigurationTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("*")]
    [InlineData("api.example.com;*.example.com")]
    public void Rejects_empty_or_wildcard_production_hosts(string? allowedHosts)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AllowedHosts"] = allowedHosts
            })
            .Build();

        Assert.Throws<InvalidOperationException>(
            () => ProductionConfiguration.Validate(
                configuration,
                "Host=db.example.com;Database=starter;Username=app;Password=not-a-sample"));
    }

    [Fact]
    public void Rejects_the_local_development_database_password()
    {
        var configuration = CreateConfiguration("api.example.com");

        Assert.Throws<InvalidOperationException>(
            () => ProductionConfiguration.Validate(
                configuration,
                "Host=db.example.com;Database=starter;Username=app;Password=starter_dev_password"));
    }

    [Fact]
    public void Accepts_explicit_hosts_and_non_sample_database_credentials()
    {
        var configuration = CreateConfiguration("api.example.com;app.example.com");

        ProductionConfiguration.Validate(
            configuration,
            "Host=db.example.com;Database=starter;Username=app;Password=managed-outside-source");
    }

    private static IConfiguration CreateConfiguration(string allowedHosts)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AllowedHosts"] = allowedHosts
            })
            .Build();
    }
}
