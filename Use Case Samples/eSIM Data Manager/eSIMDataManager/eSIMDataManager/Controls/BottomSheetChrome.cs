using Microsoft.Maui.Controls.Shapes;

namespace eSIMDataManager.Controls;

/// <summary>
/// Shared chrome for the mobile bottom sheets: the drag handle that sits at the top of the
/// sheet, replacing the coloured accent bar used by the desktop dialogs.
/// </summary>
internal static class BottomSheetChrome
{
    /// <summary>Builds the rounded drag handle shown at the top of a bottom sheet.</summary>
    public static View BuildHandle() => new Border
    {
        WidthRequest = 44,
        HeightRequest = 5,
        HorizontalOptions = LayoutOptions.Center,
        VerticalOptions = LayoutOptions.Center,
        BackgroundColor = Color.FromArgb("#C7D0E0"),
        StrokeThickness = 0,
        StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(3) },
        Margin = new Thickness(0, 10, 0, 4)
    };
}
