namespace BlazorMultihostStarter.Shared.UI.Theme;

public sealed class ThemeService : IThemeService
{
    private ThemeMode _currentTheme = ThemeMode.Light;
    private readonly Lock _lock = new();

    public event Action? OnChanged;

    public ThemeMode CurrentTheme
    {
        get
        {
            lock (_lock)
            {
                return _currentTheme;
            }
        }
    }

    public bool IsDarkMode => CurrentTheme == ThemeMode.Dark;

    public void SetTheme(ThemeMode mode)
    {
        bool changed = false;
        lock (_lock)
        {
            if (_currentTheme != mode)
            {
                _currentTheme = mode;
                changed = true;
            }
        }

        if (changed)
        {
            OnChanged?.Invoke();
        }
    }

    public void ToggleTheme()
    {
        SetTheme(IsDarkMode ? ThemeMode.Light : ThemeMode.Dark);
    }
}
