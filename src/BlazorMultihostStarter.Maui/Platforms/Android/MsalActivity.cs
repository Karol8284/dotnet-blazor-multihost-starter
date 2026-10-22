using Android.App;
using Android.Content;
using Microsoft.Identity.Client;

namespace BlazorMultihostStarter;

[Activity(Exported = true)]
[IntentFilter(
    [Intent.ActionView],
    Categories = [Intent.CategoryDefault, Intent.CategoryBrowsable],
    DataScheme = MauiEntraRegistration.AndroidRedirectScheme,
    DataHost = "auth")]
public sealed class MsalActivity : BrowserTabActivity
{
}
