using BlazorMultihostStarter.Shared.UI.Commands;

namespace BlazorMultihostStarter.Shared.Tests.UI;

public sealed class CommandPaletteServiceTests
{
    [Fact]
    public void Open_Close_Toggle_changes_IsOpen_state()
    {
        var service = new CommandPaletteService();
        var changeCount = 0;
        service.OnChanged += () => changeCount++;

        Assert.False(service.IsOpen);

        service.Open();
        Assert.True(service.IsOpen);
        Assert.Equal(1, changeCount);

        service.Close();
        Assert.False(service.IsOpen);
        Assert.Equal(2, changeCount);

        service.Toggle();
        Assert.True(service.IsOpen);
        Assert.Equal(3, changeCount);
    }

    [Fact]
    public void Register_and_Unregister_commands()
    {
        var service = new CommandPaletteService();
        var executed = false;
        var cmd = new PaletteCommand("cmd-1", "Test Command", "General", "Ctrl+T", () => executed = true);

        service.Register(cmd);
        Assert.Single(service.GetCommands());
        Assert.Equal("Test Command", service.GetCommands()[0].Title);

        cmd.Action.Invoke();
        Assert.True(executed);

        service.Unregister("cmd-1");
        Assert.Empty(service.GetCommands());
    }
}
