using BlazorMultihostStarter.Shared.UI.Commands;
using BlazorMultihostStarter.Shared.UI.Dialogs;
using BlazorMultihostStarter.Shared.UI.Theme;
using BlazorMultihostStarter.Shared.UI.Toasts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BlazorMultihostStarter.Shared.UI;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedUiServices(this IServiceCollection services)
    {
        services.TryAddScoped<IToastService, ToastService>();
        services.TryAddScoped<IDialogService, DialogService>();
        services.TryAddScoped<IThemeService, ThemeService>();
        services.TryAddScoped<ICommandPaletteService, CommandPaletteService>();
        return services;
    }
}
