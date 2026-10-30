using BlazorMultihostStarter.Shared;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace BlazorMultihostStarter;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        blazorWebView.RootComponents.Add(new RootComponent
        {
            Selector = "#app",
            ComponentType = typeof(Routes)
        });
    }
}
