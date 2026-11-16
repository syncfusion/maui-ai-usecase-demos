using Syncfusion.Maui.Popup;
using System.Diagnostics;

namespace MauiNotes.Controls.Popups;

internal static class PopupPlacement
{
    public const double DefaultVerticalGap = 4;
    private const double MobileVerticalAdjustment = 50;

    public static async Task<bool> WaitForAnchorLayoutAsync(View anchor)
    {
        for (int attempt = 0; attempt < 16; attempt++)
        {
            if (anchor.IsLoaded &&
                anchor.Handler is not null &&
                anchor.Window?.Page is not null &&
                anchor.Width > 0 &&
                anchor.Height > 0)
            {
                return true;
            }

            await Task.Delay(16);
        }

        Debug.WriteLine(
            $"[PopupPosition] Trigger layout was not ready: trigger={anchor.AutomationId}, " +
            $"measured={anchor.Width:F1}x{anchor.Height:F1}, loaded={anchor.IsLoaded}.");
        return false;
    }

    public static void ShowBelow(
        SfPopup popup,
        View anchor,
        bool centeredOnAnchor = false,
        double verticalGap = DefaultVerticalGap)
    {
        if (!TryGetBoundsInWindow(anchor, out Rect anchorBounds))
        {
            Debug.WriteLine(
                $"[PopupPosition] Unable to position {popup.GetType().Name}: " +
                $"trigger={anchor.AutomationId}, measured={anchor.Width:F1}x{anchor.Height:F1}, " +
                $"loaded={anchor.IsLoaded}.");
            return;
        }

        double popupWidth = popup.WidthRequest > 0 ? popup.WidthRequest : popup.Width;
        double popupX = centeredOnAnchor
            ? anchorBounds.Left + (anchorBounds.Width - popupWidth) / 2
            : anchorBounds.Left;
        double popupY = anchorBounds.Bottom + verticalGap;

#if ANDROID || IOS || MACCATALYST
        if (anchor.Window is Window mobileWindow && mobileWindow.Width > 0 && popupWidth <= mobileWindow.Width)
            popupX = Math.Clamp(popupX, 0, mobileWindow.Width - popupWidth);

        popupY -= MobileVerticalAdjustment;
        Debug.WriteLine(
            $"[PopupPosition] trigger={anchor.AutomationId} " +
            $"buttonPosition=({anchorBounds.Left:F1},{anchorBounds.Top:F1}) " +
            $"buttonSize=({anchorBounds.Width:F1}x{anchorBounds.Height:F1}) " +
            $"popupAnchorBottom={anchorBounds.Bottom:F1} gap={verticalGap:F1} " +
            $"verticalAdjustment={MobileVerticalAdjustment:F1} " +
            $"popupPosition=({popupX:F1},{popupY:F1}) " +
            $"popupSize=({popupWidth:F1}x{popup.HeightRequest:F1}).");

        popup.Show(popupX, popupY);
#else
        if (anchor.Window is Window window && window.Width > 0 && popupWidth <= window.Width)
            popupX = Math.Clamp(popupX, 0, window.Width - popupWidth);

        Debug.WriteLine(
            $"[PopupPosition] trigger={anchor.AutomationId} " +
            $"buttonPosition=({anchorBounds.Left:F1},{anchorBounds.Top:F1}) " +
            $"buttonSize=({anchorBounds.Width:F1}x{anchorBounds.Height:F1}) " +
            $"popupPosition=({popupX:F1},{popupY:F1}) popupSize=({popupWidth:F1}x{popup.HeightRequest:F1}).");

        popup.Show(popupX, popupY);
#endif
    }

    public static void ShowTopCentered(SfPopup popup, View anchor, double verticalGap = DefaultVerticalGap)
    {
        if (anchor is null)
            return;

        double horizontalOffset = 0;
        if (anchor.Window is Window window && window.Width > 0)
            horizontalOffset = (window.Width - popup.WidthRequest) / 2 - GetAbsoluteX(anchor);

        popup.ShowRelativeToView(
            anchor,
            PopupRelativePosition.AlignTopLeft,
            horizontalOffset,
            verticalGap);
    }

    public static void ShowToRightOf(SfPopup popup, View anchor, double gap = DefaultVerticalGap)
    {
        if (anchor.Width <= 0 || anchor.Height <= 0)
            return;

        popup.ShowRelativeToView(
            anchor,
            PopupRelativePosition.AlignToRightOf,
            gap,
            0);
    }

    private static bool TryGetBoundsInWindow(View anchor, out Rect bounds)
    {
        bounds = Rect.Zero;
        if (!anchor.IsLoaded ||
            anchor.Width <= 0 ||
            anchor.Height <= 0 ||
            anchor.Window?.Page is not VisualElement windowPage)
        {
            return false;
        }

        double x = 0;
        double y = 0;
        Element? current = anchor;
        while (current is not null)
        {
            if (current is VisualElement element)
            {
                if (ReferenceEquals(element, windowPage))
                {
                    bounds = new Rect(x, y, anchor.Width, anchor.Height);
                    return true;
                }

                x += element.X + element.TranslationX;
                y += element.Y + element.TranslationY;
            }

            current = current.Parent;
        }

        return false;
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
