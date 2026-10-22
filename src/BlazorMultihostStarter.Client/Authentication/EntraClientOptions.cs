namespace BlazorMultihostStarter.Client.Authentication;

public sealed class EntraClientOptions
{
    public const string SectionName = "Entra";

    public string TenantId { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string ApiClientId { get; set; } = string.Empty;

    public string ApiScopeName { get; set; } = "Tasks.ReadWrite";

    public string Authority => $"https://login.microsoftonline.com/{TenantId}";

    public string ApiScope => $"api://{ApiClientId}/{ApiScopeName}";

    public bool IsValid =>
        IsConfiguredGuid(TenantId)
        && IsConfiguredGuid(ClientId)
        && IsConfiguredGuid(ApiClientId)
        && !string.IsNullOrWhiteSpace(ApiScopeName)
        && !ApiScopeName.Contains(' ');

    public void Validate()
    {
        if (!IsValid)
        {
            throw new InvalidOperationException(
                $"Configure valid TenantId, ClientId, ApiClientId, and ApiScopeName values in the {SectionName} section.");
        }
    }

    private static bool IsConfiguredGuid(string value) =>
        Guid.TryParse(value, out var parsed) && parsed != Guid.Empty;
}
