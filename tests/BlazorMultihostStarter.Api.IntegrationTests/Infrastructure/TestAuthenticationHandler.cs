using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BlazorMultihostStarter.Api.IntegrationTests.Infrastructure;

public sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "TasksTest";
    public const string TenantId = "11111111-1111-4111-8111-111111111111";
    public const string ApiClientId = "22222222-2222-4222-8222-222222222222";
    public const string WebClientId = "33333333-3333-4333-8333-333333333333";
    public const string MauiClientId = "44444444-4444-4444-8444-444444444444";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var objectId = Request.Headers["X-Test-ObjectId"].FirstOrDefault();
        if (!Guid.TryParse(objectId, out var subjectId))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var scope = Request.Headers["X-Test-Scope"].FirstOrDefault() ?? "Tasks.ReadWrite";
        var clientId = Request.Headers["X-Test-ClientId"].FirstOrDefault() ?? WebClientId;
        var claims = new[]
        {
            new Claim("tid", TenantId),
            new Claim("oid", subjectId.ToString("D")),
            new Claim("azp", clientId),
            new Claim("scp", scope)
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
