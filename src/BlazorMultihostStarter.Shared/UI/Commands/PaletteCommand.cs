namespace BlazorMultihostStarter.Shared.UI.Commands;

public sealed record PaletteCommand(
    string Id,
    string Title,
    string Category,
    string? Shortcut,
    Action Action);
