using BlazorMultihostStarter.Web.Components;
using BlazorMultihostStarter.Application;
using BlazorMultihostStarter.Infrastructure;
using BlazorMultihostStarter.Web.Modules.Tasks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Configure the ConnectionStrings:DefaultConnection setting before starting the Web host.");
var databaseProvider = builder.Configuration["Database:Provider"] ?? "PostgreSQL";

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
