using Microsoft.AspNetCore.Components;

namespace BlazorMultihostStarter.Shared.UI.Dialogs;

public sealed class DialogReference
{
    public Guid Id { get; } = Guid.NewGuid();

    public string Title { get; }

    public Type ComponentType { get; }

    public IDictionary<string, object?> Parameters { get; }

    public DialogOptions Options { get; }

    internal TaskCompletionSource<object?> ResultSource { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task<object?> Result => ResultSource.Task;

    public DialogReference(
        string title,
        Type componentType,
        IDictionary<string, object?>? parameters = null,
        DialogOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(componentType);

        Title = title;
        ComponentType = componentType;
        Parameters = parameters ?? new Dictionary<string, object?>();
        Options = options ?? new DialogOptions();
    }

    public void Close(object? result = null)
    {
        ResultSource.TrySetResult(result);
    }

    public void Cancel()
    {
        ResultSource.TrySetResult(null);
    }
}
