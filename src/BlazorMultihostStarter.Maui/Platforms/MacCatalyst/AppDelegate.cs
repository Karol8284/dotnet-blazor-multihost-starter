using Foundation;
using Microsoft.Identity.Client;
using UIKit;

namespace BlazorMultihostStarter;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        MauiAuthenticationPlatform.ParentWindow = new UIViewController();
        return base.FinishedLaunching(application, launchOptions);
    }

    public override bool OpenUrl(UIApplication application, NSUrl url, NSDictionary options)
    {
        return AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(url);
    }
}
