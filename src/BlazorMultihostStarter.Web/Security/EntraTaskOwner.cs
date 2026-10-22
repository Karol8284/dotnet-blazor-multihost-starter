using System.Security.Claims;

namespace BlazorMultihostStarter.Web.Security;

public static class EntraTaskOwner
{
    public static string GetRequiredOwnerId(ClaimsPrincipal principal)
    {
        if (!TryGetOwnerId(principal, out var ownerId))
        {
            throw new InvalidOperationException(
                "The Tasks authorization policy requires valid Entra tid and oid claims.");
        }

        return ownerId;
    }

    public static bool TryGetOwnerId(ClaimsPrincipal principal, out string ownerId)
    {
        if (Guid.TryParse(principal.FindFirst("tid")?.Value, out var tenantGuid)
            && Guid.TryParse(principal.FindFirst("oid")?.Value, out var objectGuid))
        {
            ownerId = $"{tenantGuid:D}:{objectGuid:D}";
            return true;
        }

        ownerId = string.Empty;
        return false;
    }
}
