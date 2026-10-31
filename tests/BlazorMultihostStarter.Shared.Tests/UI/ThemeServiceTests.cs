using BlazorMultihostStarter.Shared.UI.Theme;

namespace BlazorMultihostStarter.Shared.Tests.UI;

public sealed class ThemeServiceTests
{
    [Fact]
    public void Default_theme_is_light()
    {
        var service = new ThemeService();

        Assert.Equal(ThemeMode.Light, service.CurrentTheme);
        Assert.False(service.IsDarkMode);
    }

    [Fact]
    public void SetTheme_changes_theme_and_fires_event()
    {
        var service = new ThemeService();
        var changedFired = false;
        service.OnChanged += () => changedFired = true;

        service.SetTheme(ThemeMode.Dark);

        Assert.True(changedFired);
        Assert.Equal(ThemeMode.Dark, service.CurrentTheme);
        Assert.True(service.IsDarkMode);
    }

    [Fact]
    public void ToggleTheme_switches_between_modes()
    {
        var service = new ThemeService();

        service.ToggleTheme();
        Assert.Equal(ThemeMode.Dark, service.CurrentTheme);
        Assert.True(service.IsDarkMode);

        service.ToggleTheme();
        Assert.Equal(ThemeMode.Light, service.CurrentTheme);
        Assert.False(service.IsDarkMode);
    }
}
