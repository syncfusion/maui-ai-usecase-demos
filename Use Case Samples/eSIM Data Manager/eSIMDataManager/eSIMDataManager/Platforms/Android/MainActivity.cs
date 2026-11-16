using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace eSIMDataManager
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HideSystemBars();
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);
            if (hasFocus)
                HideSystemBars();
        }

        // Removes the top status bar and the bottom navigation bar so the app fills the screen.
        private void HideSystemBars()
        {
            var window = Window;
            if (window is null)
                return;

            window.DecorView.SystemUiVisibility = StatusBarVisibility.Fullscreen | StatusBarVisibility.HideNavigation;
        }
    }
}
