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

            // Keep the system bars on the app background so no black bar shows above the page.
            var window = Window;
            if (window is null)
                return;

            window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#F6F8FF"));
            window.SetNavigationBarColor(Android.Graphics.Color.ParseColor("#F6F8FF"));
            window.DecorView.SystemUiVisibility = (StatusBarVisibility)(SystemUiFlags.LightStatusBar | SystemUiFlags.LightNavigationBar);
        }
    }
}
