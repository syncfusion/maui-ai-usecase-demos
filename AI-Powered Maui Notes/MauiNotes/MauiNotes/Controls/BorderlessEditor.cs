#if ANDROID
using Android.Content.Res;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
#endif

namespace EssentialMAUIUIKit.Controls
{
    public class BorderlessEditor : Editor
    {
        static BorderlessEditor()
        {
#if ANDROID
            Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("BorderlessEditor", (handler, view) =>
            {
                if (view is BorderlessEditor)
                    handler.PlatformView.BackgroundTintList = ColorStateList.ValueOf(
                        Colors.Transparent.ToPlatform());
            });
#elif WINDOWS
            Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("BorderlessEditor", (handler, view) =>
            {
                if (view is BorderlessEditor)
                {
                    handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
                    handler.PlatformView.Resources["TextControlBorderThemeThicknessFocused"] =
                        new Microsoft.UI.Xaml.Thickness(0);
                }
            });
#elif MACCATALYST || IOS
            Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("BorderlessEditor", (handler, view) =>
            {
                if (view is BorderlessEditor)
                {
                    handler.PlatformView.Layer.BorderWidth = 0;
                    handler.PlatformView.Layer.BorderColor = UIKit.UIColor.Clear.CGColor;
                }
            });
#endif
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BorderlessEditor"/> class.
        /// </summary>
        public BorderlessEditor()
        {
            this.TextChanged += BorderlessEditor_TextChanged;
        }

        #region Methods

        /// <summary>
        /// Invoked when editor text is changed.
        /// </summary>
        /// <param name="sender">The editor</param>
        /// <param name="e">Text changed event args</param>
        private void BorderlessEditor_TextChanged(object? sender, TextChangedEventArgs e)
        {
            this.InvalidateMeasure();
        }

        #endregion
    }
}
