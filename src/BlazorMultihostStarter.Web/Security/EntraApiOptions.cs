namespace BlazorMultihostStarter.Web.Security;

public sealed class EntraApiOptions
{
    public const string SectionName = "Authentication:Entra";

    public string TenantId { get; set; } = string.Empty;

    public string ApiClientId { get; set; } = string.Empty;

    public string WebClientId { get; set; } = string.Empty;

    public string MauiClientId { get; set; } = string.Empty;

    public string ApiScopeName { get; set; } = "Tasks.ReadWrite";

    public string Authority => $"https://login.microsoftonline.com/{TenantId}/v2.0";

    public string ApiScope => $"api://{ApiClientId}/{ApiScopeName}";

    public string[] AllowedClientIds => [WebClientId, MauiClientId];

    public bool IsValid =>
        IsConfiguredGuid(TenantId)
        && IsConfiguredGuid(ApiClientId)
        && IsConfiguredGuid(WebClientId)
        && IsConfiguredGuid(MauiClientId)
        && !string.IsNullOrWhiteSpace(ApiScopeName)
        && !ApiScopeName.Contains(' ');

    private static bool IsConfiguredGuid(string value) =>
        Guid.TryParse(value, out var parsed) && parsed != Guid.Empty;
}
