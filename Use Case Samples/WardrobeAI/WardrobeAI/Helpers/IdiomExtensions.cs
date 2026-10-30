using Microsoft.Maui.Devices;

namespace WardrobeAI.Helpers;

/// <summary>
/// Runtime idiom helpers used by view models and navigation code to
/// branch between phone/tablet and desktop experiences.
///
/// <para>MAUI's <see cref="DeviceIdiom"/> values:
/// <list type="bullet">
///   <item><c>Phone</c> — iOS/Android handsets, narrow windows on
///     desktop below the desktop breakpoint.</item>
///   <item><c>Tablet</c> — iPad, Android tablets, Mac Catalyst on iPad.</item>
///   <item><c>Desktop</c> — Windows (WinUI), macOS Mac Catalyst in
///     native-mode, and any window that opts into the desktop idiom.</item>
/// </list>
///
/// The split-screen Dashboard + Shopping page is gated on
/// <see cref="IsDesktop"/>; everything else predates this distinction
/// and uses <c>OnIdiom Phone=</c> / <c>Default=</c> in XAML.</para>
/// </summary>
public static class IdiomExtensions
{
    /// <summary>
    /// True when the running app is reporting the desktop idiom
    /// (Windows desktop, Mac Catalyst native, etc.).
    /// </summary>
    public static bool IsDesktop => DeviceInfo.Idiom == DeviceIdiom.Desktop;
}