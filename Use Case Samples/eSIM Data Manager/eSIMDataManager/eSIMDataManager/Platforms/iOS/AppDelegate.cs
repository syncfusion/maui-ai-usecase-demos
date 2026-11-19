using System.Linq;
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
            // Removes the top status bar so the page content starts at the very top.
            application.StatusBarHidden = true;
            SetStatusBarHidden(true);
            return base.FinishedLaunching(application, launchOptions);
        }

        public override void OnActivated(UIApplication application)
        {
            base.OnActivated(application);
            SetStatusBarHidden(true);
        }

        private static void SetStatusBarHidden(bool hidden)
        {
            UIApplication.SharedApplication.StatusBarHidden = hidden;
            var window = UIApplication.SharedApplication.ConnectedScenes
                .OfType<UIWindowScene>()
                .SelectMany(scene => scene.Windows)
                .FirstOrDefault();
            window?.RootViewController?.SetNeedsStatusBarAppearanceUpdate();
        }
    }
}
