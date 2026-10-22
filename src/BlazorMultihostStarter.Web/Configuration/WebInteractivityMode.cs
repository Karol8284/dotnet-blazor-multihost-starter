using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorMultihostStarter.Web.Configuration;

public enum WebInteractivity
{
    WebAssembly,
    Server
}

public static class WebInteractivityMode
{
    public static WebInteractivity Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || string.Equals(value, nameof(WebInteractivity.WebAssembly), StringComparison.OrdinalIgnoreCase))
        {
            return WebInteractivity.WebAssembly;
        }

        if (string.Equals(value, nameof(WebInteractivity.Server), StringComparison.OrdinalIgnoreCase))
        {
            return WebInteractivity.Server;
        }

        throw new InvalidOperationException(
            "Web:Interactivity must be either 'WebAssembly' or 'Server'.");
    }

    public static IComponentRenderMode GetRenderMode(string? value) =>
        Parse(value) switch
        {
            WebInteractivity.Server => new InteractiveServerRenderMode(prerender: false),
            WebInteractivity.WebAssembly => new InteractiveWebAssemblyRenderMode(prerender: false),
            _ => throw new InvalidOperationException("The Web interactivity mode is not supported.")
        };
}
