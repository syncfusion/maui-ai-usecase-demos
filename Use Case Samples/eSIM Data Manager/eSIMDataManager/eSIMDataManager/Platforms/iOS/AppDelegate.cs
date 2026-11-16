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
            // Draw the content behind the status bar so no black bar shows above the page.
            application.Windows[0].RootViewController?.View?.Frame = UIScreen.MainScreen.Bounds;
            application.StatusBarStyle = UIStatusBarStyle.Default;
            application.StatusBarHidden = false;
            return base.FinishedLaunching(application, launchOptions);
        }
    }
}
