namespace BlazorMultihostStarter.Shared.UI.Commands;

public interface ICommandPaletteService
{
    event Action? OnChanged;

    bool IsOpen { get; }

    void Open();

    void Close();

    void Toggle();

    void Register(PaletteCommand command);

    void Unregister(string id);

    IReadOnlyList<PaletteCommand> GetCommands();
}
