namespace BlazorMultihostStarter.Shared.UI.Toasts;

public sealed record ToastMessage(
    Guid Id,
    string Message,
    ToastType Type,
    string? Title,
    DateTimeOffset CreatedAt,
    TimeSpan Duration);
