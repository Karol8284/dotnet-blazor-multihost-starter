namespace BlazorMultihostStarter.Web.Security;

public static class ProductionConfiguration
{
    public static void Validate(
        IConfiguration configuration,
        string connectionString)
    {
        var allowedHosts = configuration["AllowedHosts"]?
            .Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (allowedHosts is null
            || allowedHosts.Length == 0
            || allowedHosts.Any(host => host.Contains('*')))
        {
            throw new InvalidOperationException(
                "Set AllowedHosts to the explicit host names used by this deployment.");
        }

        if (connectionString.Contains("starter_dev_password", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Replace the local development PostgreSQL password outside Development.");
        }
    }
}
