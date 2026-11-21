namespace BlazorMultihostStarter.Shared.UI.Commands;

public sealed class CommandPaletteService : ICommandPaletteService
{
    private readonly List<PaletteCommand> _commands = [];
    private readonly Lock _lock = new();
    private bool _isOpen;

    public event Action? OnChanged;

    public bool IsOpen
    {
        get
        {
            lock (_lock)
            {
                return _isOpen;
            }
        }
    }

    public void Open()
    {
        bool changed = false;
        lock (_lock)
        {
            if (!_isOpen)
            {
                _isOpen = true;
                changed = true;
            }
        }

        if (changed)
        {
            OnChanged?.Invoke();
        }
    }

    public void Close()
    {
        bool changed = false;
        lock (_lock)
        {
            if (_isOpen)
            {
                _isOpen = false;
                changed = true;
            }
        }

        if (changed)
        {
            OnChanged?.Invoke();
        }
    }

    public void Toggle()
    {
        lock (_lock)
        {
            _isOpen = !_isOpen;
        }

        OnChanged?.Invoke();
    }

    public void Register(PaletteCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        lock (_lock)
        {
            _commands.RemoveAll(c => c.Id == command.Id);
            _commands.Add(command);
        }

        OnChanged?.Invoke();
    }

    public void Unregister(string id)
    {
        bool removed;
        lock (_lock)
        {
            removed = _commands.RemoveAll(c => c.Id == id) > 0;
        }

        if (removed)
        {
            OnChanged?.Invoke();
        }
    }

    public IReadOnlyList<PaletteCommand> GetCommands()
    {
        lock (_lock)
        {
            return _commands.ToList();
        }
    }
}
