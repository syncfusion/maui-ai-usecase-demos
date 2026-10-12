using MauiNotes.Controls.AI;
using MauiNotes.Controls.Toolbars;
using MauiNotes.Controls.Popups;
using MauiNotes.ViewModels;
using CustomRichTextEditor;
using Syncfusion.Maui.RichTextEditor;

namespace MauiNotes.Views.Editor;

public partial class EditorPageMobileUI : ContentPage
{
    public EditorPageMobileUI()
    {
        InitializeComponent();
        BindingContext = AppShell.SharedNotesViewModel;
        EditorToolbar.AiRequested += OnAiRequested;
        EditorToolbar.FormatRequested += OnFormatRequested;
        FormatPopup.TextPaletteRequested += (_, _) =>
            TextPalettePopup.ShowFor(EditorToolbar, centerInWindow: true);
        EditorToolbar.TableRequested += (_, _) =>
        {
            if (BindingContext is NotesViewModel viewModel && !viewModel.IsEditorReadOnly)
                EditorToolbarActions.InsertTable(RichTextEditor);
        };
        RichTextEditor.Focused += (_, _) =>
        {
            if (BindingContext is NotesViewModel viewModel)
                viewModel.TrackEditorFocus(RichTextEditor, true);
        };
        RichTextEditor.Unfocused += (_, _) =>
        {
            if (BindingContext is NotesViewModel viewModel)
                viewModel.TrackEditorFocus(RichTextEditor, false);
        };
    }

    private async void OnAiRequested(object? sender, EventArgs e)
    {
        if (BindingContext is not NotesViewModel viewModel)
            return;

        if (!await viewModel.BeginAiAssistantAsync(RichTextEditor))
            return;
        AiEditingPopup.ShowWritingTools(EditorToolbar, centerInWindow: true);
    }

    private async void OnBackClicked(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private void OnFormatRequested(object? sender, EventArgs e)
    {
        if (BindingContext is NotesViewModel { IsEditorEditable: true })
            FormatPopup.ToggleFormat(EditorToolbar);
    }

    private void OnEditorFormatChanged(object? sender, RichTextEditorFormatChangedEventArgs e)
    {
        if (BindingContext is NotesViewModel viewModel)
            viewModel.UpdateFormatState(e);
    }

    protected override void OnDisappearing()
    {
        if (BindingContext is NotesViewModel viewModel &&
            (viewModel.IsAiPopupOpen || viewModel.IsAiProcessing))
            viewModel.CloseAiAssistantCommand.Execute(null);

        base.OnDisappearing();
    }
}