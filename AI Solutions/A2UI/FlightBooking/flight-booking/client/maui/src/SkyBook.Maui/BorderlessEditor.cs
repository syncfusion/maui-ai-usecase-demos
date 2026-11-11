using Microsoft.Maui.Handlers;

namespace SkyBook.Maui;

/// <summary>
/// Removes the native border / underline from any Editor whose StyleId is "Borderless"
/// (the outer Border in XAML is then the only visible box). Call once from MauiProgram.
/// </summary>
public static class BorderlessEditor
{
#if WINDOWS
    private static void SetRes(Microsoft.UI.Xaml.FrameworkElement el, string key, object value)
    {
        try
        {
            if (el.Resources.ContainsKey(key)) el.Resources.Remove(key);
            el.Resources.Add(key, value);
        }
        catch (Exception) { /* ignore: not all keys can be overridden on every WinUI version */ }
    }
#endif

  public static void Register()
  {
    EditorHandler.Mapper.AppendToMapping("Borderless", (handler, view) =>
    {
      if (view is not Editor { StyleId: "Borderless" }) return;
#if WINDOWS
            var tb = handler.PlatformView;
            tb.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            tb.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            // Keep it borderless when focused / hovered. Some WinUI resource keys throw when set
            // after the control is templated, so each one is applied safely.
            var clear = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            SetRes(tb, "TextControlBorderThemeThicknessFocused", new Microsoft.UI.Xaml.Thickness(0));
            SetRes(tb, "TextControlBackgroundFocused", clear);
            SetRes(tb, "TextControlBackgroundPointerOver", clear);
            SetRes(tb, "TextControlBorderBrushFocused", clear);
            SetRes(tb, "TextControlBorderBrushPointerOver", clear);
            tb.GotFocus += (_, _) => { tb.BorderThickness = new Microsoft.UI.Xaml.Thickness(0); tb.Background = clear; };
#elif ANDROID
      handler.PlatformView.Background = null;                       // removes the underline
      handler.PlatformView.SetPadding(0, handler.PlatformView.PaddingTop, 0, handler.PlatformView.PaddingBottom);
#elif IOS || MACCATALYST
            handler.PlatformView.Layer.BorderWidth = 0;
            handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
#endif
    });
  }
}
