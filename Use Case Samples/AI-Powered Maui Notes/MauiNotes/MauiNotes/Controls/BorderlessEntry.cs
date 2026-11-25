#if ANDROID
using Android.Content.Res;
#endif
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;

namespace MauiNotes.Controls;
/// <summary>
/// An <see cref="Entry"/> with the native platform border or underline removed.
/// </summary>
public class BorderlessEntry : Entry
{
    static BorderlessEntry()
    {
#if ANDROID
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("BorderlessEntry", (handler, view) =>
        {
            if (view is BorderlessEntry)
            {
                handler.PlatformView.BackgroundTintList = ColorStateList.ValueOf(Colors.Transparent.ToPlatform());
            }
        });
#endif
#if WINDOWS
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("BorderlessEntry", (handler, view) =>
        {
            if (view is BorderlessEntry)
            {
                handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
                handler.PlatformView.Resources["TextControlBorderThemeThicknessFocused"] = new Microsoft.UI.Xaml.Thickness(0);
            }
        });
#endif
#if MACCATALYST
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("BorderlessEntry", (handler, view) =>
        {
            if (view is BorderlessEntry)
            {
                handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
            }
        });
#endif
    }
}
