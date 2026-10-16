using Microsoft.Maui.Controls;

namespace Todo.Features.Common.Views;

/// <summary>
/// Lightweight snackbar-style toast used to confirm destructive
/// actions ("List deleted successfully", "Note moved", ...).
/// Renders a pill at the bottom of the screen that fades in and
/// out, then auto-dismisses.
/// </summary>
public partial class ToastView : ContentView
{
    public ToastView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Shows the supplied <paramref name="message"/> for
    /// <paramref name="durationMs"/> milliseconds (default 2400 ms).
    /// Fades in and out with a quick scale for the "snackbar pop"
    /// effect.
    /// </summary>
    public async Task ShowAsync(string message, int durationMs = 2400)
    {
        MessageLabel.Text = message;
        Root.IsVisible = true;
        await Task.WhenAll(
            Root.FadeTo(1, 180, Easing.CubicIn),
            Root.ScaleTo(1.0, 180, Easing.CubicOut));
        try
        {
            await Task.Delay(durationMs);
        }
        finally
        {
            await Task.WhenAll(
                Root.FadeTo(0, 220, Easing.CubicIn),
                Root.ScaleTo(0.96, 220, Easing.CubicIn));
            Root.IsVisible = false;
        }
    }
}
