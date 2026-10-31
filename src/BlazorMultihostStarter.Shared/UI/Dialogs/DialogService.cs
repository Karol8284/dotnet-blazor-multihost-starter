using Microsoft.AspNetCore.Components;

namespace BlazorMultihostStarter.Shared.UI.Dialogs;

public sealed class DialogService : IDialogService
{
    private DialogReference? _currentDialog;
    private readonly Lock _lock = new();

    public event Action? OnChanged;

    public DialogReference? CurrentDialog
    {
        get
        {
            lock (_lock)
            {
                return _currentDialog;
            }
        }
    }

    public async Task<TResult?> ShowAsync<TComponent, TResult>(
        string title,
        IDictionary<string, object?>? parameters = null,
        DialogOptions? options = null) where TComponent : IComponent
    {
        var rawResult = await ShowAsync(title, typeof(TComponent), parameters, options);
        if (rawResult is TResult typedResult)
        {
            return typedResult;
        }

        return default;
    }

    public Task<object?> ShowAsync(
        string title,
        Type componentType,
        IDictionary<string, object?>? parameters = null,
        DialogOptions? options = null)
    {
        var dialog = new DialogReference(title, componentType, parameters, options);

        lock (_lock)
        {
            // Cancel previous dialog if one was open
            _currentDialog?.Cancel();
            _currentDialog = dialog;
        }

        OnChanged?.Invoke();

        // Clear current dialog when completed
        _ = dialog.Result.ContinueWith(_ =>
        {
            lock (_lock)
            {
                if (_currentDialog == dialog)
                {
                    _currentDialog = null;
                }
            }

            OnChanged?.Invoke();
        }, TaskScheduler.Default);

        return dialog.Result;
    }

    public async Task<bool> ConfirmAsync(
        string title,
        string message,
        string confirmText = "Confirm",
        string cancelText = "Cancel")
    {
        var parameters = new Dictionary<string, object?>
        {
            ["Message"] = message,
            ["ConfirmText"] = confirmText,
            ["CancelText"] = cancelText
        };

        var result = await ShowAsync(title, typeof(ConfirmDialog), parameters, new DialogOptions
        {
            MaxWidth = "28rem",
            CloseOnBackdropClick = true,
            CloseOnEscape = true
        });

        return result is true;
    }

    public void Close(object? result = null)
    {
        DialogReference? dialog;
        lock (_lock)
        {
            dialog = _currentDialog;
            _currentDialog = null;
        }

        dialog?.Close(result);
        OnChanged?.Invoke();
    }

    public void Cancel()
    {
        DialogReference? dialog;
        lock (_lock)
        {
            dialog = _currentDialog;
            _currentDialog = null;
        }

        dialog?.Cancel();
        OnChanged?.Invoke();
    }
}
