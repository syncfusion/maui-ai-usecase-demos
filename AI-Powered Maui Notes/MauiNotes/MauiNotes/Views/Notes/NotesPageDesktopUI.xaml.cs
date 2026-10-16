using MauiNotes.ViewModels;

namespace MauiNotes.Views.Notes;

public partial class NotesPageDesktopUI : ContentPage
{
    // Page section - Attach the shared notes state.
    public NotesPageDesktopUI()
    {
        InitializeComponent();
        BindingContext = AppShell.SharedNotesViewModel;
    }

    // Lifecycle section - Close transient AI UI when leaving the page.
    protected override void OnDisappearing()
    {
        if (BindingContext is NotesViewModel viewModel &&
            (viewModel.IsAiPopupOpen || viewModel.IsAiProcessing))
        {
            viewModel.CloseAiAssistantCommand.Execute(null);
        }

        base.OnDisappearing();
    }
}
