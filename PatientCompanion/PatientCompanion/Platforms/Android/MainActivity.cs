using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidColor = Android.Graphics.Color;
using AndroidToolbar = AndroidX.AppCompat.Widget.Toolbar;
using AndroidView = Android.Views.View;

namespace PatientCompanion
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Window?.SetStatusBarColor(AndroidColor.White);
            Window?.SetNavigationBarColor(AndroidColor.White);
            Window?.DecorView?.SystemUiFlags = SystemUiFlags.LightStatusBar;

            Window?.DecorView?.Post(() =>
            {
                var toolbar = FindToolbar(Window?.DecorView);

                if (toolbar == null)
                    return;

                toolbar.Menu?.Clear();
                toolbar.OverflowIcon = null;
                toolbar.ContentInsetStartWithNavigation = 0;
                toolbar.SetContentInsetsRelative(0, 0);
                toolbar.SetContentInsetsAbsolute(0, 0);
                toolbar.SetPadding(0, toolbar.PaddingTop, 0, toolbar.PaddingBottom);

                for (var index = 0; index < toolbar.ChildCount; index++)
                {
                    var child = toolbar.GetChildAt(index);

                    if (child?.LayoutParameters is ViewGroup.MarginLayoutParams layoutParams)
                    {
                        layoutParams.LeftMargin = 0;
                        layoutParams.RightMargin = 0;
                        child.LayoutParameters = layoutParams;
                    }
                }
            });
        }

        private static AndroidToolbar? FindToolbar(AndroidView? view)
        {
            if (view is AndroidToolbar toolbar)
                return toolbar;

            if (view is not ViewGroup group)
                return null;

            for (var index = 0; index < group.ChildCount; index++)
            {
                var result = FindToolbar(group.GetChildAt(index));
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}
