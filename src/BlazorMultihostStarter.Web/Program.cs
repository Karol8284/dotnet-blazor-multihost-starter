using BlazorMultihostStarter.Web.Components;
using BlazorMultihostStarter.Application;
using BlazorMultihostStarter.Client.Authentication;
using BlazorMultihostStarter.Client.Tasks;
using BlazorMultihostStarter.Infrastructure;
using BlazorMultihostStarter.Shared.Tasks;
using BlazorMultihostStarter.Web.Modules.Tasks;
using BlazorMultihostStarter.Web.Configuration;
using BlazorMultihostStarter.Web.Diagnostics;
using BlazorMultihostStarter.Web.Security;
using BlazorMultihostStarter.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
var webInteractivity = WebInteractivityMode.Parse(builder.Configuration["Web:Interactivity"]);
var entraApiOptions = builder.Configuration.GetSection(EntraApiOptions.SectionName)
    .Get<EntraApiOptions>() ?? new EntraApiOptions();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var apiConfigured = entraApiOptions.IsValid && !string.IsNullOrWhiteSpace(connectionString);
var serverAuthenticationConfigured = EntraServerAuthentication.IsConfigured(
    builder.Configuration,
    entraApiOptions.TenantId);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddOpenApi();
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
    .AddCheck<DatabaseHealthCheck>(DatabaseHealthCheck.Name, tags: ["ready"]);

if (!builder.Environment.IsDevelopment())
{
    if (!apiConfigured)
    {
        throw new InvalidOperationException(
            "Configure valid Entra API identifiers and ConnectionStrings:DefaultConnection before starting the Web host outside Development.");
    }

    ProductionConfiguration.Validate(builder.Configuration, connectionString!);
    if (webInteractivity == WebInteractivity.Server && !serverAuthenticationConfigured)
    {
        throw new InvalidOperationException(
            "Interactive Server requires a configured confidential Entra client.");
    }
}

if (apiConfigured)
{
    builder.Services.AddEntraApiAuthentication(builder.Configuration);
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(
        connectionString!,
        builder.Configuration["Database:Provider"] ?? "PostgreSQL");
}

if (webInteractivity == WebInteractivity.Server)
{
    if (serverAuthenticationConfigured)
    {
        builder.Services.AddEntraInteractiveServerAuthentication(
            builder.Configuration,
            entraApiOptions.TenantId);
        builder.Services.AddScoped<IUserAuthenticationSession, ServerAuthenticationSession>();
    }
    else
    {
        builder.Services.AddScoped<IUserAuthenticationSession>(_ =>
            new UnconfiguredAuthenticationSession(
                "Configure a confidential Entra client to sign in with Interactive Server."));
    }

    if (apiConfigured && serverAuthenticationConfigured)
    {
        builder.Services.AddScoped<ITasksClient, ServerTasksClient>();
    }
    else
    {
        builder.Services.AddScoped<ITasksClient>(_ =>
            new UnconfiguredTasksClient(
                "The Tasks API is unavailable until valid Entra IDs and a database connection are configured."));
    }

    builder.Services.AddSharedTaskModes();
}

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("tasks-api", context =>
    {
        var partitionKey = EntraTaskOwner.TryGetOwnerId(context.User, out var ownerId)
            ? ownerId
            : context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 60,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            });
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    if (apiConfigured)
    {
        app.MapOpenApi();
    }
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
if (apiConfigured)
{
    app.MapTasksModule();
}
else
{
    app.Logger.LogWarning(
        "Tasks API is disabled in Development until valid Entra API identifiers and a database connection string are configured.");
    app.Map("/api", api => api.Run(async context =>
    {
        var problem = Results.Problem(
            statusCode: StatusCodes.Status503ServiceUnavailable,
            title: "Tasks API is not configured",
            detail: "Configure Entra API identifiers and a database connection string to enable server-backed tasks.");
        await problem.ExecuteAsync(context);
    }));
}

if (webInteractivity == WebInteractivity.Server && serverAuthenticationConfigured)
{
    app.MapServerAuthenticationEndpoints();
}

app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(BlazorMultihostStarter.Shared._Imports).Assembly,
        typeof(BlazorMultihostStarter.Web.Client._Imports).Assembly);

app.Run();

public partial class Program
{
}
