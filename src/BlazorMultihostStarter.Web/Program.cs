using BlazorMultihostStarter.Web.Components;
using BlazorMultihostStarter.Application;
using BlazorMultihostStarter.Infrastructure;
using BlazorMultihostStarter.Web.Security;
using BlazorMultihostStarter.Web.Modules.Tasks;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddOpenApi();
builder.Services.AddEntraApiAuthentication(builder.Configuration);
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

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Configure the ConnectionStrings:DefaultConnection setting before starting the Web host.");
var databaseProvider = builder.Configuration["Database:Provider"] ?? "PostgreSQL";

if (!builder.Environment.IsDevelopment())
{
    ProductionConfiguration.Validate(builder.Configuration, connectionString);
}

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString, databaseProvider);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.MapOpenApi();
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
app.MapTasksModule();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(BlazorMultihostStarter.Shared._Imports).Assembly,
        typeof(BlazorMultihostStarter.Web.Client._Imports).Assembly);

app.Run();

public partial class Program
{
}
