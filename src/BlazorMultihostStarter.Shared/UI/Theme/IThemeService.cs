namespace BlazorMultihostStarter.Shared.UI.Theme;

public interface IThemeService
{
    event Action? OnChanged;

    ThemeMode CurrentTheme { get; }

    bool IsDarkMode { get; }

    void SetTheme(ThemeMode mode);

    void ToggleTheme();
}
