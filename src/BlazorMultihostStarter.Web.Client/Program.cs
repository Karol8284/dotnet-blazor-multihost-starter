using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using BlazorMultihostStarter.Client.Authentication;
using BlazorMultihostStarter.Client;
using BlazorMultihostStarter.Web.Client.Services;
using BlazorMultihostStarter.Shared.Tasks;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var entraOptions = builder.Configuration.GetSection(EntraClientOptions.SectionName)
    .Get<EntraClientOptions>() ?? new EntraClientOptions();
entraOptions.Validate();

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
builder.Services.AddSharedTaskModes();

await builder.Build().RunAsync();
