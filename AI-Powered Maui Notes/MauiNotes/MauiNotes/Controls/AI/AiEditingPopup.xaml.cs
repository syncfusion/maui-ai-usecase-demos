using MauiNotes.Controls.Popups;
using MauiNotes.Models.AI;
using MauiNotes.ViewModels;
using Syncfusion.Maui.Popup;
using System.ComponentModel;
using System.Diagnostics;

namespace MauiNotes.Controls.AI;

public partial class AiEditingPopup : ContentView
{
    private NotesViewModel? viewModel;
    private View? editorAnchor;
    private bool centerPopupInWindow;

    public AiEditingPopup()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        if (viewModel is not null)
            viewModel.PropertyChanged -= OnViewModelPropertyChanged;

        base.OnBindingContextChanged();

        viewModel = BindingContext as NotesViewModel;
        if (viewModel is not null)
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    public void ShowWritingTools(View anchor, bool centerInWindow = false)
    {
        editorAnchor = anchor;
        centerPopupInWindow = centerInWindow;
        SetPopupWidths(anchor, centerInWindow);
        WritingToolsPopup.StaysOpen = false;
        PopupPlacement.ShowTopCentered(WritingToolsPopup, anchor);
    }

    public void ShowReview(View anchor, bool centerInWindow = false)
    {
        editorAnchor = anchor;
        SetPopupWidths(anchor, centerInWindow);
        AiReviewPopup.StaysOpen = false;
        PopupPlacement.ShowTopCentered(AiReviewPopup, anchor);
    }

    private void SetPopupWidths(View anchor, bool centerInWindow)
    {
        double windowWidth = anchor.Window is Window window && window.Width > 0
            ? window.Width
            : 352;
        double availableWidth = centerInWindow
            ? Math.Max(1, windowWidth - 24)
            : windowWidth - 24;
        WritingToolsPopup.WidthRequest = centerInWindow
            ? Math.Min(300, availableWidth)
            : Math.Min(300, Math.Max(220, availableWidth));
        WritingToolsPopup.MaximumHeightRequest = anchor.Window is Window popupWindow && popupWindow.Height > 0
            ? centerInWindow
                ? Math.Min(500, Math.Max(1, popupWindow.Height - 32))
                : Math.Min(500, Math.Max(240, popupWindow.Height - 32))
            : 500;
        AiReviewPopup.WidthRequest = centerInWindow
            ? Math.Min(368, availableWidth)
            : Math.Max(200, Math.Min(368, availableWidth));
    }

    private void OnCustomInstructionCompleted(object? sender, EventArgs e)
    {
        if (viewModel?.CanRunCustomAi == true)
            viewModel.AiCustomCommand.Execute(null);
    }

    private void OnAiActionItemTapped(object? sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        if (viewModel?.IsAiActionEnabled == true && e.DataItem is AiActionListItem item)
        {
            Debug.WriteLine($"[AI] Popup action selected: {item.Action}; processing={viewModel.IsAiProcessing}.");
            viewModel.AiActionCommand.Execute(item.Action.ToString());
        }
    }

    private void OnWritingToolsClosed(object? sender, EventArgs e)
    {
        Debug.WriteLine(
            $"[AI] Writing-tools popup closed; popupOpen={viewModel?.IsAiPopupOpen}; processing={viewModel?.IsAiProcessing}.");
        if (viewModel?.IsAiPopupOpen == true)
            viewModel.CloseAiAssistantCommand.Execute(null);
    }

    private void OnWritingToolsOpened(object? sender, EventArgs e)
    {
        Debug.WriteLine(
            $"[AI] Writing-tools popup opened; snapshotState={viewModel?.AiSelectionSnapshotDebugState ?? "unavailable"}.");
    }

    private void OnAiReviewOpened(object? sender, EventArgs e)
    {
        viewModel?.SetAiReviewPopupOpen(true);
        Debug.WriteLine(
            $"[AI] Review popup opened; processing={viewModel?.IsAiProcessing}; " +
            $"snapshotState={viewModel?.AiSelectionSnapshotDebugState ?? "unavailable"}.");
    }

    private void OnAiReviewClosed(object? sender, EventArgs e)
    {
        viewModel?.SetAiReviewPopupOpen(false);
        Debug.WriteLine(
            $"[AI] Review popup dismissed; processing={viewModel?.IsAiProcessing}; " +
            $"hasResult={viewModel?.HasAiReviewResult}; " +
            $"noteContentLength={viewModel?.SelectedNote?.Content?.Length ?? 0}. " +
            "Popup dismissal does not cancel processing or revert editor content.");
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (viewModel is null)
            return;

        if (e.PropertyName == nameof(NotesViewModel.IsAiPopupOpen) &&
            !viewModel.IsAiPopupOpen)
        {
            Debug.WriteLine("[AI] ViewModel closed writing-tools popup.");
            WritingToolsPopup.Dismiss();
        }
        else if (e.PropertyName == nameof(NotesViewModel.IsAiProcessing))
        {
            Debug.WriteLine($"[AI] Popup observed processing state change: {viewModel.IsAiProcessing}.");
            if (viewModel.IsAiProcessing)
            {
                if (editorAnchor is not null)
                    ShowReview(editorAnchor, centerPopupInWindow);
            }
            else if (!viewModel.HasAiReviewResult)
            {
                AiReviewPopup.Dismiss();
            }
        }
        else if (e.PropertyName == nameof(NotesViewModel.HasAiReviewResult) &&
                 !viewModel.HasAiReviewResult)
        {
            AiReviewPopup.Dismiss();
        }
        else if (e.PropertyName == nameof(NotesViewModel.HasAiReviewResult) &&
                 viewModel.HasAiReviewResult)
        {
            Debug.WriteLine(
                $"[AI] AI result became available; reviewPopupVisible={AiReviewPopup.IsOpen}; " +
                "not reopening a popup the user already dismissed.");
        }
    }
}
