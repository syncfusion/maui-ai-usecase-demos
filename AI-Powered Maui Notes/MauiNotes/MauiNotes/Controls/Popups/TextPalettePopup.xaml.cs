using Syncfusion.Maui.Popup;

namespace MauiNotes.Controls.Popups;

public partial class TextPalettePopup : ContentView
{
    public TextPalettePopup()
    {
        InitializeComponent();
    }

    public void ShowFor(View anchor, bool centerInWindow = false)
    {
        if (PopupView.Handler is not null)
        {
            double horizontalOffset = centerInWindow && anchor.Window is Window window && window.Width > 0
                ? (window.Width - PopupView.WidthRequest) / 2 - anchor.X
                : 205;
            PopupView.ShowRelativeToView(anchor, PopupRelativePosition.AlignBottomLeft, horizontalOffset, 4);
        }
    }
}
