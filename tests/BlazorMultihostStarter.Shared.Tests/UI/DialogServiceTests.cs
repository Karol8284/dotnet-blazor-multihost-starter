using BlazorMultihostStarter.Shared.UI.Dialogs;
using Microsoft.AspNetCore.Components;

namespace BlazorMultihostStarter.Shared.Tests.UI;

public sealed class DialogServiceTests
{
    private sealed class DummyComponent : ComponentBase { }

    [Fact]
    public void ShowAsync_sets_current_dialog_and_fires_OnChanged()
    {
        var service = new DialogService();
        var changedFired = false;
        service.OnChanged += () => changedFired = true;

        var task = service.ShowAsync("Test Dialog", typeof(DummyComponent));

        Assert.True(changedFired);
        Assert.NotNull(service.CurrentDialog);
        Assert.Equal("Test Dialog", service.CurrentDialog.Title);
        Assert.Equal(typeof(DummyComponent), service.CurrentDialog.ComponentType);
        Assert.False(task.IsCompleted);
    }

    [Fact]
    public async Task Close_completes_task_with_result()
    {
        var service = new DialogService();
        var task = service.ShowAsync("Test", typeof(DummyComponent));

        service.Close("my-result");

        var result = await task;
        Assert.Equal("my-result", result);
        Assert.Null(service.CurrentDialog);
    }

    [Fact]
    public async Task Cancel_completes_task_with_null()
    {
        var service = new DialogService();
        var task = service.ShowAsync("Test", typeof(DummyComponent));

        service.Cancel();

        var result = await task;
        Assert.Null(result);
        Assert.Null(service.CurrentDialog);
    }

    [Fact]
    public async Task ConfirmAsync_with_true_completes_true()
    {
        var service = new DialogService();
        var confirmTask = service.ConfirmAsync("Confirm Title", "Are you sure?");

        Assert.NotNull(service.CurrentDialog);
        Assert.Equal("Confirm Title", service.CurrentDialog.Title);

        service.Close(true);

        var result = await confirmTask;
        Assert.True(result);
    }

    [Fact]
    public async Task ConfirmAsync_with_cancel_completes_false()
    {
        var service = new DialogService();
        var confirmTask = service.ConfirmAsync("Confirm Title", "Are you sure?");

        service.Close(false);

        var result = await confirmTask;
        Assert.False(result);
    }
}
