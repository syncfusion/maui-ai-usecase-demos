using Syncfusion.Maui.Popup;
using MauiNotes.ViewModels;

namespace MauiNotes.Controls.Popups;

public partial class TextPalettePopup : ContentView
{
    private View? anchor;
    private bool centerInWindow;

    public TextPalettePopup()
    {
        InitializeComponent();
    }

    public void ShowFor(View anchor, bool centerInWindow = false)
    {
        if (PopupView.Handler is null)
            return;

        this.anchor = anchor;
        this.centerInWindow = centerInWindow;
        PositionPopup();
    }

    public void Reposition()
    {
        if (PopupView.IsOpen)
            PositionPopup();
    }

    public void Dismiss() => PopupView.Dismiss();

    private void PositionPopup()
    {
        if (anchor is null)
            return;

        if (centerInWindow)
        {
            PopupPlacement.ShowTopCentered(PopupView, anchor);
            return;
        }

        PopupPlacement.ShowToRightOf(PopupView, anchor);
    }

    private void OnPopupClosed(object? sender, EventArgs e)
    {
        if (BindingContext is NotesViewModel viewModel)
            viewModel.IsTextPaletteOpen = false;
    }
}
