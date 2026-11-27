using MauiNotes.ViewModels;

namespace MauiNotes.Views.Notes;

public partial class NotesPageDesktopUI : ContentPage
{
    public NotesPageDesktopUI(NotesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is NotesViewModel viewModel)
            await viewModel.InitializationTask;
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
