using BlazorMultihostStarter.Shared.UI.Dashboard;
using BlazorMultihostStarter.Shared.UI.Dialogs;
using BlazorMultihostStarter.Shared.UI.Navigation;
using BlazorMultihostStarter.Shared.UI.Toasts;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorMultihostStarter.Shared.Tests.UI;

public sealed class UIComponentTests
{
    private sealed class SampleDialogContent : ComponentBase
    {
        [Parameter] public string Text { get; set; } = string.Empty;

        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
        {
            builder.AddContent(0, Text);
        }
    }

    [Fact]
    public void ToastContainer_renders_toast_items()
    {
        using var context = new BunitContext();
        var toastService = new ToastService();
        context.Services.AddSingleton<IToastService>(toastService);

        var cut = context.Render<ToastContainer>();
        Assert.DoesNotContain("toast-card", cut.Markup);

        toastService.ShowSuccess("Operation succeeded", "Success Title");

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("toast-card", cut.Markup);
            Assert.Contains("Operation succeeded", cut.Markup);
            Assert.Contains("Success Title", cut.Markup);
        });
    }

    [Fact]
    public void DialogContainer_renders_dialog_and_handles_close()
    {
        using var context = new BunitContext();
        var dialogService = new DialogService();
        context.Services.AddSingleton<IDialogService>(dialogService);

        var cut = context.Render<DialogContainer>();
        Assert.DoesNotContain("modal-backdrop", cut.Markup);

        _ = dialogService.ShowAsync("My Modal", typeof(SampleDialogContent), new Dictionary<string, object?>
        {
            ["Text"] = "Hello inside modal"
        });

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("modal-backdrop", cut.Markup);
            Assert.Contains("My Modal", cut.Markup);
            Assert.Contains("Hello inside modal", cut.Markup);
        });

        dialogService.Close();

        cut.WaitForAssertion(() =>
        {
            Assert.DoesNotContain("modal-backdrop", cut.Markup);
        });
    }

    [Fact]
    public void MetricCard_renders_label_and_value()
    {
        using var context = new BunitContext();
        var cut = context.Render<MetricCard>(parameters => parameters
            .Add(p => p.Label, "Memory Usage")
            .Add(p => p.Value, "128 MB")
            .Add(p => p.Description, "Heap memory")
            .Add(p => p.Icon, "M"));

        Assert.Contains("Memory Usage", cut.Markup);
        Assert.Contains("128 MB", cut.Markup);
        Assert.Contains("Heap memory", cut.Markup);
        Assert.Contains("M", cut.Markup);
    }

    [Fact]
    public void HomeDashboard_renders_cards_and_interacts()
    {
        using var context = new BunitContext();
        var toastService = new ToastService();
        var dialogService = new DialogService();
        context.Services.AddSingleton<IToastService>(toastService);
        context.Services.AddSingleton<IDialogService>(dialogService);

        var cut = context.Render<HomeDashboard>();

        Assert.Contains("Platform Dashboard", cut.Markup);
        Assert.Contains("Server Mode", cut.Markup);
        Assert.Contains("Client-Local", cut.Markup);
        Assert.Contains("Offline Hybrid", cut.Markup);

        // Click Trigger Toast button
        var toastButton = cut.Find("button.btn-primary");
        toastButton.Click();

        Assert.Single(toastService.Toasts);
        Assert.Equal(ToastType.Success, toastService.Toasts[0].Type);
    }

    [Fact]
    public void BottomNav_renders_navigation_links()
    {
        using var context = new BunitContext();
        var cut = context.Render<BottomNav>();

        Assert.Contains("bottom-nav", cut.Markup);
        Assert.Contains("Home", cut.Markup);
        Assert.Contains("Server", cut.Markup);
        Assert.Contains("Local", cut.Markup);
        Assert.Contains("Offline", cut.Markup);
    }
}
