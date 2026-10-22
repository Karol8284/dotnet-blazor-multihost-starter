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
        if (AuthenticationContinuationHelper.IsBrokerResponse(null))
        {
            _ = Task.Factory.StartNew(
                () => AuthenticationContinuationHelper.SetBrokerContinuationEventArgs(url));
            return true;
        }

        return AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(url);
    }
}
