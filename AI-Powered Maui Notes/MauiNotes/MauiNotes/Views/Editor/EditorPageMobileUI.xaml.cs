using System.Diagnostics;
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
        Debug.WriteLine("[EditorPageMobileUI] ctor:start; inflating XAML tree.");
        InitializeComponent();
        Debug.WriteLine("[EditorPageMobileUI] ctor:done; XAML inflated; binding to shared NotesViewModel.");
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
        {
            Debug.WriteLine(
                $"[EditorPageMobileUI] OnEditorFormatChanged: fontFamily={e.NewFormat.FontFamily ?? "<null>"}; bold={e.NewFormat.IsBold}; italic={e.NewFormat.IsItalic}; paragraph={e.NewFormat.ParagraphFormat}.");
            viewModel.UpdateFormatState(e);
        }
    }

    protected override void OnAppearing()
    {
        Debug.WriteLine(
            $"[EditorPageMobileUI] OnAppearing:start; isCached={CacheMode}; selectedNoteId={(BindingContext is NotesViewModel vm && vm.SelectedNote is not null ? vm.SelectedNote.Id.ToString() : "<none>")}.");
        base.OnAppearing();
        Debug.WriteLine("[EditorPageMobileUI] OnAppearing:done; base.OnAppearing returned.");
    }

    protected override void OnDisappearing()
    {
        Debug.WriteLine("[EditorPageMobileUI] OnDisappearing:start.");
        if (BindingContext is NotesViewModel viewModel &&
            (viewModel.IsAiPopupOpen || viewModel.IsAiProcessing))
            viewModel.CloseAiAssistantCommand.Execute(null);

        base.OnDisappearing();
        Debug.WriteLine("[EditorPageMobileUI] OnDisappearing:done.");
    }
}