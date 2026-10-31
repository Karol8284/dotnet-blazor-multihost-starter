using BlazorMultihostStarter.Shared.UI.Toasts;

namespace BlazorMultihostStarter.Shared.Tests.UI;

public sealed class ToastServiceTests
{
    [Fact]
    public void Show_adds_toast_and_fires_OnChanged()
    {
        var service = new ToastService();
        var changedFired = false;
        service.OnChanged += () => changedFired = true;

        service.ShowSuccess("Operation succeeded", "Done");

        Assert.True(changedFired);
        Assert.Single(service.Toasts);
        var toast = service.Toasts[0];
        Assert.Equal("Operation succeeded", toast.Message);
        Assert.Equal("Done", toast.Title);
        Assert.Equal(ToastType.Success, toast.Type);
    }

    [Fact]
    public void Helper_methods_set_expected_types()
    {
        var service = new ToastService();

        service.ShowSuccess("Success msg");
        service.ShowError("Error msg");
        service.ShowWarning("Warning msg");
        service.ShowInfo("Info msg");

        Assert.Equal(4, service.Toasts.Count);
        Assert.Equal(ToastType.Success, service.Toasts[0].Type);
        Assert.Equal(ToastType.Error, service.Toasts[1].Type);
        Assert.Equal(ToastType.Warning, service.Toasts[2].Type);
        Assert.Equal(ToastType.Info, service.Toasts[3].Type);
    }

    [Fact]
    public void Dismiss_removes_toast_and_fires_event()
    {
        var service = new ToastService();
        service.ShowInfo("Item to dismiss");
        var id = service.Toasts[0].Id;

        var changedCount = 0;
        service.OnChanged += () => changedCount++;

        service.Dismiss(id);

        Assert.Empty(service.Toasts);
        Assert.Equal(1, changedCount);
    }

    [Fact]
    public void Clear_removes_all_toasts()
    {
        var service = new ToastService();
        service.ShowInfo("1");
        service.ShowInfo("2");
        Assert.Equal(2, service.Toasts.Count);

        service.Clear();

        Assert.Empty(service.Toasts);
    }

    [Fact]
    public void Show_throws_when_message_is_empty()
    {
        var service = new ToastService();

        Assert.Throws<ArgumentException>(() => service.Show(string.Empty));
        Assert.Throws<ArgumentNullException>(() => service.Show(null!));
    }
}
