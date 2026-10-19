using Syncfusion.Maui.Popup;
using MauiNotes.ViewModels;

namespace MauiNotes.Controls.Popups;

public partial class HighlightPalettePopup : ContentView
{
    private View? anchor;

    public HighlightPalettePopup()
    {
        InitializeComponent();
    }

    public void ShowFor(View anchor)
    {
        if (PopupView.Handler is null)
            return;

        this.anchor = anchor;
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
        if (anchor is not null)
            PopupPlacement.ShowToRightOf(PopupView, anchor);
    }

    private void OnPopupClosed(object? sender, EventArgs e)
    {
        if (BindingContext is NotesViewModel viewModel)
            viewModel.IsHighlightPaletteOpen = false;
    }
}
