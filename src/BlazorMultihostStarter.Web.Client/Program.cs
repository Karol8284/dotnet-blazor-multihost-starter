using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using BlazorMultihostStarter.Client.Authentication;
using BlazorMultihostStarter.Client;
using BlazorMultihostStarter.Client.Tasks;
using BlazorMultihostStarter.Web.Client.Services;
using BlazorMultihostStarter.Shared.Tasks;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var entraOptions = builder.Configuration.GetSection(EntraClientOptions.SectionName)
    .Get<EntraClientOptions>() ?? new EntraClientOptions();

if (entraOptions.IsValid)
{
    builder.Services.AddSingleton(entraOptions);
    builder.Services.AddMsalAuthentication(options =>
    {
        options.ProviderOptions.Authentication.Authority = entraOptions.Authority;
        options.ProviderOptions.Authentication.ClientId = entraOptions.ClientId;
        options.ProviderOptions.DefaultAccessTokenScopes.Add(entraOptions.ApiScope);
    });

    builder.Services.AddScoped<WebAuthenticationSession>();
    builder.Services.AddScoped<IUserAuthenticationSession>(services =>
        services.GetRequiredService<WebAuthenticationSession>());
    builder.Services.AddScoped<IApiAccessTokenProvider>(services =>
        services.GetRequiredService<WebAuthenticationSession>());
    builder.Services.AddTasksApiClient(new Uri(builder.HostEnvironment.BaseAddress));
}
else if (builder.HostEnvironment.IsDevelopment())
{
    builder.Services.AddScoped<IUserAuthenticationSession>(_ =>
        new UnconfiguredAuthenticationSession(
            "Configure valid Entra client IDs to sign in and synchronize server-backed tasks."));
    builder.Services.AddScoped<ITasksClient>(_ =>
        new UnconfiguredTasksClient(
            "The Tasks API is unavailable until valid Entra client IDs are configured."));
}
else
{
    entraOptions.Validate();
}

builder.Services.AddSharedTaskModes();

await builder.Build().RunAsync();
