namespace BlazorMultihostStarter.Shared.UI.Toasts;

public interface IToastService
{
    event Action? OnChanged;

    IReadOnlyList<ToastMessage> GetToasts();

    void Show(string message, ToastType type = ToastType.Info, string? title = null, TimeSpan? duration = null);

    void ShowSuccess(string message, string? title = null);

    void ShowError(string message, string? title = null);

    void ShowWarning(string message, string? title = null);

    void ShowInfo(string message, string? title = null);

    void Dismiss(Guid id);

    void Clear();
}
