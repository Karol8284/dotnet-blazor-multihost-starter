using Microsoft.AspNetCore.Components;

namespace BlazorMultihostStarter.Shared.UI.Dialogs;

public interface IDialogService
{
    event Action? OnChanged;

    DialogReference? CurrentDialog { get; }

    Task<TResult?> ShowAsync<TComponent, TResult>(
        string title,
        IDictionary<string, object?>? parameters = null,
        DialogOptions? options = null) where TComponent : IComponent;

    Task<object?> ShowAsync(
        string title,
        Type componentType,
        IDictionary<string, object?>? parameters = null,
        DialogOptions? options = null);

    Task<bool> ConfirmAsync(
        string title,
        string message,
        string confirmText = "Confirm",
        string cancelText = "Cancel");

    void Close(object? result = null);

    void Cancel();
}
