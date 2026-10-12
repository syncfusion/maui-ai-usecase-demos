using Syncfusion.Maui.Popup;

namespace MauiNotes.Controls.Popups;

internal static class PopupPlacement
{
    public static void ShowTopCentered(SfPopup popup, View anchor)
    {
        double horizontalOffset = 0;
        if (anchor.Window is Window window && window.Width > 0)
        {
            horizontalOffset = (window.Width - popup.WidthRequest) / 2 - GetAbsoluteX(anchor);
        }

        popup.ShowRelativeToView(
            anchor,
            PopupRelativePosition.AlignBottomLeft,
            horizontalOffset,
            4);
    }

    private static double GetAbsoluteX(VisualElement element)
    {
        double absoluteX = 0;
        Element? current = element;
        while (current is VisualElement visualElement)
        {
            absoluteX += visualElement.X;
            current = visualElement.Parent;
        }

        return absoluteX;
    }
}
