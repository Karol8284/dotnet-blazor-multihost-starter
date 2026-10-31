namespace BlazorMultihostStarter.Shared.UI.Toasts;

public sealed class ToastService : IToastService, IDisposable
{
    private readonly TimeProvider _timeProvider;
    private readonly List<ToastMessage> _toasts = [];
    private readonly Lock _lock = new();

    public event Action? OnChanged;

    public ToastService(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public IReadOnlyList<ToastMessage> Toasts
    {
        get
        {
            lock (_lock)
            {
                return _toasts.ToList();
            }
        }
    }

    public void Show(string message, ToastType type = ToastType.Info, string? title = null, TimeSpan? duration = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        var durationValue = duration ?? TimeSpan.FromSeconds(4);
        var toast = new ToastMessage(
            Guid.NewGuid(),
            message,
            type,
            title,
            _timeProvider.GetUtcNow(),
            durationValue);

        lock (_lock)
        {
            _toasts.Add(toast);
        }

        OnChanged?.Invoke();

        if (durationValue > TimeSpan.Zero)
        {
            _ = AutoDismissAsync(toast.Id, durationValue);
        }
    }

    public void ShowSuccess(string message, string? title = null) =>
        Show(message, ToastType.Success, title);

    public void ShowError(string message, string? title = null) =>
        Show(message, ToastType.Error, title, TimeSpan.FromSeconds(6));

    public void ShowWarning(string message, string? title = null) =>
        Show(message, ToastType.Warning, title, TimeSpan.FromSeconds(5));

    public void ShowInfo(string message, string? title = null) =>
        Show(message, ToastType.Info, title);

    public void Dismiss(Guid id)
    {
        bool removed;
        lock (_lock)
        {
            removed = _toasts.RemoveAll(t => t.Id == id) > 0;
        }

        if (removed)
        {
            OnChanged?.Invoke();
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _toasts.Clear();
        }

        OnChanged?.Invoke();
    }

    private async Task AutoDismissAsync(Guid id, TimeSpan duration)
    {
        try
        {
            await Task.Delay(duration, _timeProvider);
            Dismiss(id);
        }
        catch (Exception)
        {
            // Ignore cancellation or timer exceptions on shutdown
        }
    }

    public void Dispose()
    {
        Clear();
    }
}
