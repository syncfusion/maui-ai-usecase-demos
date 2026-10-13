using Syncfusion.Maui.Popup;

namespace MauiNotes.Controls.Popups;

public partial class HighlightPalettePopup : ContentView
{
    public HighlightPalettePopup()
    {
        InitializeComponent();
    }

    public void ShowFor(View anchor)
    {
        if (PopupView.Handler is not null)
            PopupView.ShowRelativeToView(anchor, PopupRelativePosition.AlignBottomLeft, 205, 4);
    }
}
