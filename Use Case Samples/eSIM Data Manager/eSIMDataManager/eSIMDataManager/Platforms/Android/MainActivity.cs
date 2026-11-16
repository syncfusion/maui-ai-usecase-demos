using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace eSIMDataManager
{
    [Activity(Theme = "@style/AppTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            ApplyFullscreen();
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);
            if (hasFocus)
                ApplyFullscreen();
        }

        // Removes the top status bar so the page content starts at the very top,
        // and paints the bottom navigation bar with the app background.
        private void ApplyFullscreen()
        {
            var window = Window;
            if (window is null)
                return;

            var background = Android.Graphics.Color.ParseColor("#F6F8FF");
            window.SetStatusBarColor(background);
            window.SetNavigationBarColor(background);
            window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            window.DecorView.SystemUiVisibility = (StatusBarVisibility)(SystemUiFlags.Fullscreen | SystemUiFlags.LightNavigationBar);
        }
    }
}
