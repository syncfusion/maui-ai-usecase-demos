using Android.OS;
using Android.Views;
using Microsoft.Maui.ApplicationModel;

namespace TravelPlanner
{
    /// <summary>
    /// Raises <see cref="IKeyboardService.KeyboardVisibilityChanged"/> when the
    /// Android soft keyboard shows or hides. Uses IME insets on API 30+ and falls
    /// back to a height-based heuristic on older devices.
    /// </summary>
    class KeyboardService_Android : IKeyboardService
    {
        private readonly Android.App.Activity activity;
        private readonly ViewTreeObserver.IOnGlobalLayoutListener listener;
        private bool wasVisible;

        public event EventHandler<bool>? KeyboardVisibilityChanged;

        public KeyboardService_Android(Android.App.Activity activity)
        {
            this.activity = activity;
            listener = new LayoutListener(OnGlobalLayout);
            activity.Window?.DecorView?.RootView?.ViewTreeObserver?.AddOnGlobalLayoutListener(listener);
        }

        private void OnGlobalLayout()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var root = activity.Window?.DecorView?.RootView;
                if (root == null)
                    return;

                bool visible = IsKeyboardVisible(root);

                if (visible == wasVisible)
                    return;

                wasVisible = visible;
                KeyboardVisibilityChanged?.Invoke(this, visible);
            });
        }

        private static bool IsKeyboardVisible(View root)
        {
            // API 30+: dedicated IME inset avoids false positives from system bars.
            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                var insets = root.RootWindowInsets;
                if (insets != null)
                {
                    var ime = insets.GetInsets(WindowInsets.Type.Ime());
                    if (ime.Bottom > 0)
                        return true;
                }
            }

            // Fallback for older Android: compare the visible root height to the
            // screen height. If a large portion (~25%+) is covered, treat it as
            // the soft keyboard.
            var screenHeight = root.Resources?.DisplayMetrics?.HeightPixels ?? 0;
            var rect = new Android.Graphics.Rect();
            root.GetWindowVisibleDisplayFrame(rect);
            int visibleHeight = rect.Height();
            if (screenHeight <= 0)
                return false;

            int covered = screenHeight - visibleHeight;
            return covered > screenHeight * 0.25;
        }

        public void Dispose()
        {
            activity.Window?.DecorView?.RootView?.ViewTreeObserver?.RemoveOnGlobalLayoutListener(listener);
        }

        private sealed class LayoutListener : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
        {
            private readonly Action callback;
            public LayoutListener(Action callback) => this.callback = callback;

            public void OnGlobalLayout() => callback();
        }
    }
}
