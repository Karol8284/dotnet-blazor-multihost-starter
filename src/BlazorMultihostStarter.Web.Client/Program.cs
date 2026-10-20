using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorMultihostStarter.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddTasksApiClient(new Uri(builder.HostEnvironment.BaseAddress));

await builder.Build().RunAsync();
