using BlazorMultihostStarter.Shared;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace BlazorMultihostStarter;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        var webView = blazorWebView
            ?? throw new InvalidOperationException("The BlazorWebView was not initialized from MainPage.xaml.");

        webView.RootComponents.Add(new RootComponent
        {
            Selector = "#app",
            ComponentType = typeof(Routes)
        });
    }
}
