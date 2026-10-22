using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace BlazorMultihostStarter.Web.Security;

public static class ServerAuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapServerAuthenticationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/auth/server/sign-in", (string? returnUrl) =>
                Results.Challenge(
                    new AuthenticationProperties { RedirectUri = GetLocalReturnUrl(returnUrl) },
                    [OpenIdConnectDefaults.AuthenticationScheme]))
            .AllowAnonymous();

        endpoints.MapGet("/auth/server/sign-out", async (
                HttpContext context,
                string? returnUrl) =>
            {
                await context.SignOutAsync(EntraServerAuthentication.CookieScheme);
                return Results.SignOut(
                    new AuthenticationProperties { RedirectUri = GetLocalReturnUrl(returnUrl) },
                    [OpenIdConnectDefaults.AuthenticationScheme]);
            })
            .RequireAuthorization();

        return endpoints;
    }

    private static string GetLocalReturnUrl(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl)
        && returnUrl[0] == '/'
        && (returnUrl.Length == 1 || returnUrl[1] != '/')
        && !returnUrl.Contains('\\')
            ? returnUrl
            : "/";
}
