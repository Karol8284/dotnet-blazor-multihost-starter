using BlazorMultihostStarter.Client.Tasks;
using Microsoft.JSInterop;

namespace BlazorMultihostStarter.Shared.Tasks;

public sealed class BrowserTasksLocalStore(IJSRuntime javascript) : ITasksLocalStore, IAsyncDisposable
{
    private const string ModulePath = "./_content/BlazorMultihostStarter.Shared/tasksStorage.js";
    private IJSObjectReference? _module;

    public async Task<IReadOnlyList<LocalTaskRecord>> LoadAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        var module = await GetModuleAsync(cancellationToken);
        var records = await module.InvokeAsync<LocalTaskRecord[]?>(
            "read",
            cancellationToken,
            key);
        return records ?? [];
    }

    public async Task SaveAsync(
        string key,
        IReadOnlyList<LocalTaskRecord> tasks,
        CancellationToken cancellationToken = default)
    {
        var module = await GetModuleAsync(cancellationToken);
        await module.InvokeVoidAsync("write", cancellationToken, key, tasks);
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }
    }

    private async ValueTask<IJSObjectReference> GetModuleAsync(CancellationToken cancellationToken)
    {
        return _module ??= await javascript.InvokeAsync<IJSObjectReference>(
            "import",
            cancellationToken,
            ModulePath);
    }
}
