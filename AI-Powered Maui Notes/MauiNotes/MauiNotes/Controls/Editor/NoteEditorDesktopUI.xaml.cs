using MauiNotes.Controls.Popups;
using MauiNotes.Controls.Toolbars;
using MauiNotes.ViewModels;
using CustomRichTextEditor;
using Syncfusion.Maui.RichTextEditor;

namespace MauiNotes.Controls.Editor;

public partial class NoteEditorDesktopUI : ContentView
{
    private bool _isSearchMode;
    private bool? _isCompactSearch;
    private double _requiredToolbarWidth;
    private double _searchPreferredWidth;

    // Editor section - Connect toolbar and rich-text editor interactions.
    public NoteEditorDesktopUI()
    {
        InitializeComponent();
        _searchPreferredWidth = SearchBorder.WidthRequest;
        EditorToolbar.AiRequested += OnAiRequested;
        EditorToolbar.FormatRequested += OnFormatRequested;
        FormatPopup.TextPaletteRequested += (_, _) => TextPalettePopup.ShowFor(EditorToolbar);
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

    private void OnToolbarSizeChanged(object? sender, EventArgs e)
    {
        UpdateResponsiveSearchLayout();
    }

    private void UpdateResponsiveSearchLayout()
    {
        if (_isSearchMode || ToolbarGrid.Width <= 0)
            return;

        if (_requiredToolbarWidth <= 0)
        {
            double editorToolbarWidth = EditorToolbar.Measure(double.PositiveInfinity, double.PositiveInfinity).Width;
            _requiredToolbarWidth = NewNoteBorder.WidthRequest + editorToolbarWidth +
                                    _searchPreferredWidth + ToolbarGrid.Padding.HorizontalThickness;
        }

        bool isCompact = ToolbarGrid.Width < _requiredToolbarWidth;
        if (_isCompactSearch == isCompact &&
            SearchBorder.IsVisible == !isCompact &&
            CompactSearchContainer.IsVisible == isCompact)
        {
            return;
        }

        _isCompactSearch = isCompact;
        SearchBorder.IsVisible = !isCompact;
        CompactSearchContainer.IsVisible = isCompact;
    }

    private void OnCompactSearchClicked(object? sender, EventArgs e)
    {
        if (_isSearchMode)
            return;

        _isSearchMode = true;
        NewNoteBorder.IsVisible = false;
        EditorToolbar.IsVisible = false;
        CompactSearchContainer.IsVisible = false;
        CloseSearchButton.IsVisible = true;
        Grid.SetColumn(SearchBorder, 0);
        Grid.SetColumnSpan(SearchBorder, 3);
        SearchBorder.HorizontalOptions = LayoutOptions.Fill;
        SearchBorder.WidthRequest = -1;
        SearchBorder.IsVisible = true;

        Dispatcher.Dispatch(() => SearchEntry.Focus());
    }

    private void OnCompactSearchTapped(object? sender, TappedEventArgs e)
    {
        OnCompactSearchClicked(sender, e);
    }

    private void OnSearchGlyphTapped(object? sender, TappedEventArgs e)
    {
        OnSearchGlyphClicked(sender, e);
    }

    private void OnSearchGlyphClicked(object? sender, EventArgs e)
    {
        if (_isSearchMode)
        {
            OnCloseSearchClicked(sender, e);
            return;
        }

        SearchEntry.Focus();
    }

    private void OnCloseSearchClicked(object? sender, EventArgs e)
    {
        if (!_isSearchMode)
            return;

        _isSearchMode = false;
        SearchEntry.Unfocus();
        CloseSearchButton.IsVisible = false;
        Grid.SetColumn(SearchBorder, 2);
        Grid.SetColumnSpan(SearchBorder, 1);
        SearchBorder.HorizontalOptions = LayoutOptions.End;
        SearchBorder.WidthRequest = _searchPreferredWidth;
        NewNoteBorder.IsVisible = true;
        EditorToolbar.IsVisible = true;
        UpdateResponsiveSearchLayout();
    }

    // Toolbar section - Open AI writing tools for the selected note.
    private async void OnAiRequested(object? sender, EventArgs e)
    {
        if (BindingContext is not NotesViewModel viewModel)
            return;

        if (!await viewModel.BeginAiAssistantAsync(RichTextEditor))
            return;
        AiEditingPopup.ShowWritingTools(sender as View ?? EditorToolbar.AiButton);
    }

    private void OnFormatRequested(object? sender, EventArgs e)
    {
        if (BindingContext is NotesViewModel { IsEditorEditable: true })
            FormatPopup.ToggleFormat(EditorToolbar);
    }

    // Editor section - Keep toolbar formatting state synchronized with the RTE.
    private void OnEditorFormatChanged(object? sender, RichTextEditorFormatChangedEventArgs e)
    {
        if (BindingContext is NotesViewModel viewModel)
            viewModel.UpdateFormatState(e);
    }
}
