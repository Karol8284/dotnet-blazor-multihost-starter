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
        Assert.Single(service.GetToasts());
        var toast = service.GetToasts()[0];
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

        var toasts = service.GetToasts();
        Assert.Equal(4, toasts.Count);
        Assert.Equal(ToastType.Success, toasts[0].Type);
        Assert.Equal(ToastType.Error, toasts[1].Type);
        Assert.Equal(ToastType.Warning, toasts[2].Type);
        Assert.Equal(ToastType.Info, toasts[3].Type);
    }

    [Fact]
    public void Dismiss_removes_toast_and_fires_event()
    {
        var service = new ToastService();
        service.ShowInfo("Item to dismiss");
        var id = service.GetToasts()[0].Id;

        var changedCount = 0;
        service.OnChanged += () => changedCount++;

        service.Dismiss(id);

        Assert.Empty(service.GetToasts());
        Assert.Equal(1, changedCount);
    }

    [Fact]
    public void Clear_removes_all_toasts()
    {
        var service = new ToastService();
        service.ShowInfo("1");
        service.ShowInfo("2");
        Assert.Equal(2, service.GetToasts().Count);

        service.Clear();

        Assert.Empty(service.GetToasts());
    }

    [Fact]
    public void Show_throws_when_message_is_empty()
    {
        var service = new ToastService();

        Assert.Throws<ArgumentException>(() => service.Show(string.Empty));
        Assert.Throws<ArgumentNullException>(() => service.Show(null!));
    }
}
