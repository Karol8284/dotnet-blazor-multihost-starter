namespace BlazorMultihostStarter.Shared.UI.Dialogs;

public sealed class DialogOptions
{
    public bool CloseOnBackdropClick { get; set; } = true;

    public bool CloseOnEscape { get; set; } = true;

    public string MaxWidth { get; set; } = "32rem";
}
