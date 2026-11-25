#if ANDROID
using Android.Content.Res;
using Microsoft.Maui.Platform;
#endif

namespace MauiNotes.Controls;

/// <summary>
/// An <see cref="Entry"/> with the native platform border or underline removed.
/// Handler mapping lives with the control rather than in MauiProgram.
/// </summary>
public class BorderlessEntry : Entry
{
    static BorderlessEntry()
    {
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("BorderlessEntry", (handler, view) =>
        {
            if (view is not BorderlessEntry)
                return;

#if WINDOWS
            handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            handler.PlatformView.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Microsoft.UI.Colors.Transparent);
            handler.PlatformView.Resources["TextControlBorderThemeThicknessFocused"] = new Microsoft.UI.Xaml.Thickness(0);
#elif ANDROID
            handler.PlatformView.Background = null;
            handler.PlatformView.BackgroundTintList = ColorStateList.ValueOf(Colors.Transparent.ToPlatform());
#elif IOS || MACCATALYST
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
        });
    }
}
