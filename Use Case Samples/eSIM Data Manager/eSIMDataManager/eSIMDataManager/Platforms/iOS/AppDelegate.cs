using Foundation;
using UIKit;

namespace eSIMDataManager
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            // Remove the top navigation bar so the app content starts at the very top.
            application.StatusBarHidden = true;
            return base.FinishedLaunching(application, launchOptions);
        }
    }
}
