using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BlazorMultihostStarter.Web.Security;

public static class EntraApiAuthentication
{
    public const string TasksUserPolicy = "TasksUser";

    public static IServiceCollection AddEntraApiAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<EntraApiOptions>()
            .Bind(configuration.GetSection(EntraApiOptions.SectionName))
            .Validate(settings => settings.IsValid,
                "Configure valid TenantId, ApiClientId, WebClientId, and MauiClientId values in Authentication:Entra.")
            .ValidateOnStart();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<EntraApiOptions>>((jwtOptions, entraOptions) =>
            {
                var options = entraOptions.Value;
                jwtOptions.Authority = options.Authority;
                jwtOptions.Audience = options.ApiClientId;
                jwtOptions.RequireHttpsMetadata = true;
                jwtOptions.MapInboundClaims = false;
                jwtOptions.IncludeErrorDetails = false;
                jwtOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    NameClaimType = "name",
                    RoleClaimType = "roles"
                };
            });

        services.AddSingleton<IAuthorizationHandler, TasksApiAuthorizationHandler>();
        services.AddAuthorization(authorization =>
        {
            authorization.AddPolicy(TasksUserPolicy, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new TasksApiAuthorizationRequirement());
            });
        });

        return services;
    }

    private static bool HasTenantAndObjectIds(
        ClaimsPrincipal principal,
        EntraApiOptions options)
    {
        return Guid.TryParse(options.TenantId, out var expectedTenantId)
            && Guid.TryParse(principal.FindFirst("tid")?.Value, out var tenantId)
            && tenantId == expectedTenantId
            && Guid.TryParse(principal.FindFirst("oid")?.Value, out _);
    }

    private static bool IsAllowedClient(ClaimsPrincipal principal, EntraApiOptions options)
    {
        var clientId = principal.FindFirst("azp")?.Value;
        return clientId is not null
            && options.AllowedClientIds.Contains(clientId, StringComparer.OrdinalIgnoreCase);
    }

    private static bool HasScope(ClaimsPrincipal principal, EntraApiOptions options)
    {
        var scopes = principal.FindFirst("scp")?.Value;
        return scopes?.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Contains(options.ApiScopeName, StringComparer.Ordinal) == true;
    }

    private sealed class TasksApiAuthorizationRequirement : IAuthorizationRequirement
    {
    }

    private sealed class TasksApiAuthorizationHandler(IOptions<EntraApiOptions> options)
        : AuthorizationHandler<TasksApiAuthorizationRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            TasksApiAuthorizationRequirement requirement)
        {
            if (HasTenantAndObjectIds(context.User, options.Value)
                && IsAllowedClient(context.User, options.Value)
                && HasScope(context.User, options.Value))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
