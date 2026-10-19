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
    private View? triggerAnchor;
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

    public bool IsOpen => WritingToolsPopup.IsOpen || AiReviewPopup.IsOpen;

    public void ShowWritingTools(View trigger, bool centerInWindow = false)
    {
        triggerAnchor = trigger;
        centerPopupInWindow = centerInWindow;
        SetPopupWidths(trigger);
        AiReviewPopup.Dismiss();
        WritingToolsPopup.StaysOpen = false;
        if (centerInWindow)
            PopupPlacement.ShowTopCentered(WritingToolsPopup, trigger);
        else
            PopupPlacement.ShowBelow(WritingToolsPopup, trigger, centeredOnAnchor: true);
    }

    public void ShowReview(View trigger, bool centerInWindow = false)
    {
        triggerAnchor = trigger;
        centerPopupInWindow = centerInWindow;
        SetPopupWidths(trigger);
        if (WritingToolsPopup.IsOpen)
            WritingToolsPopup.Dismiss();

        AiReviewPopup.StaysOpen = false;
        if (centerInWindow)
            PopupPlacement.ShowTopCentered(AiReviewPopup, trigger);
        else
            PopupPlacement.ShowBelow(AiReviewPopup, trigger, centeredOnAnchor: true);
    }

    public void Reposition()
    {
        if (!IsOpen || triggerAnchor is null)
            return;

        SetPopupWidths(triggerAnchor);

        if (WritingToolsPopup.IsOpen)
        {
            if (centerPopupInWindow)
                PopupPlacement.ShowTopCentered(WritingToolsPopup, triggerAnchor);
            else
                PopupPlacement.ShowBelow(WritingToolsPopup, triggerAnchor, centeredOnAnchor: true);
        }
        else if (AiReviewPopup.IsOpen)
        {
            if (centerPopupInWindow)
                PopupPlacement.ShowTopCentered(AiReviewPopup, triggerAnchor);
            else
                PopupPlacement.ShowBelow(AiReviewPopup, triggerAnchor, centeredOnAnchor: true);
        }
    }

    public void Dismiss()
    {
        WritingToolsPopup.Dismiss();
        AiReviewPopup.Dismiss();
    }

    private void SetPopupWidths(View anchor)
    {
        double windowWidth = anchor.Window is Window window && window.Width > 0
            ? window.Width
            : 352;
        double availableWidth = Math.Max(1, windowWidth - 24);
        WritingToolsPopup.WidthRequest = Math.Min(300, availableWidth);
        WritingToolsPopup.MaximumHeightRequest = anchor.Window is Window popupWindow && popupWindow.Height > 0
            ? Math.Min(500, Math.Max(1, popupWindow.Height - 32))
            : 500;
        AiReviewPopup.WidthRequest = Math.Min(368, availableWidth);
    }

    private void OnCustomInstructionCompleted(object? sender, EventArgs e)
    {
        if (viewModel?.CanRunCustomAi == true)
            viewModel.AiCustomCommand.Execute(null);
    }

    private void OnAiActionItemTapped(object? sender, TappedEventArgs e)
    {
        if (viewModel?.IsAiActionEnabled == true &&
            sender is Border { BindingContext: AiActionListItem item })
        {
            Debug.WriteLine($"[AI] Popup action selected: {item.Action}; processing={viewModel.IsAiProcessing}.");
            viewModel.AiActionCommand.Execute(item.Action.ToString());
        }
    }

    private void OnWritingToolsClosed(object? sender, EventArgs e)
    {
        Debug.WriteLine(
            $"[AI] Writing-tools popup closed; popupOpen={viewModel?.IsAiPopupOpen}; processing={viewModel?.IsAiProcessing}.");

        if (viewModel?.IsAiProcessing == true)
        {
            ShowReviewAtTrigger();
            return;
        }

        if (viewModel?.IsAiPopupOpen == true)
            viewModel.CloseAiAssistantCommand.Execute(null);
    }

    private void ShowReviewAtTrigger()
    {
        if (viewModel is null ||
            (viewModel.IsAiProcessing != true && !viewModel.HasAiError) ||
            triggerAnchor is null)
            return;

        if (AiReviewPopup.IsOpen)
        {
            Reposition();
            return;
        }

        Debug.WriteLine("[AI] Opening review popup after writing-tools popup closed, using the saved AI trigger.");
        ShowReview(triggerAnchor, centerPopupInWindow);
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
                if (WritingToolsPopup.IsOpen)
                    WritingToolsPopup.Dismiss();
                else
                    ShowReviewAtTrigger();
            }
            else if (!viewModel.HasAiReviewResult && !viewModel.HasAiError)
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
