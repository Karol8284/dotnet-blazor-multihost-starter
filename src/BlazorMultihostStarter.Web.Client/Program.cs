using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using BlazorMultihostStarter.Client.Authentication;
using BlazorMultihostStarter.Client;
using BlazorMultihostStarter.Web.Client.Services;

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

builder.Services.AddScoped<IUserAuthenticationSession, WebAuthenticationSession>();
builder.Services.AddTasksApiClient(new Uri(builder.HostEnvironment.BaseAddress));

await builder.Build().RunAsync();
