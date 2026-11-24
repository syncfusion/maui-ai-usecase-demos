using MauiNotes.Controls.Popups;
using MauiNotes.Controls.Toolbars;
using MauiNotes.ViewModels;
using CustomRichTextEditor;
using Syncfusion.Maui.RichTextEditor;
using System.ComponentModel;

namespace MauiNotes.Controls.Editor;

public partial class NoteEditorDesktopUI : ContentView
{
    private bool _isSearchMode;
    private bool? _isCompactSearch;
    private double _requiredToolbarWidth;
    private double _searchPreferredWidth;
    private NotesViewModel? _observedViewModel;
    private Window? _observedWindow;

    // Editor section - Connect toolbar and rich-text editor interactions.
    public NoteEditorDesktopUI()
    {
        InitializeComponent();
        _searchPreferredWidth = SearchBorder.WidthRequest;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        EditorToolbar.AiRequested += OnAiRequested;
        EditorToolbar.FormatRequested += OnFormatRequested;
        EditorToolbar.AiButton.SizeChanged += OnPopupAnchorSizeChanged;
        EditorToolbar.FormatButton.SizeChanged += OnPopupAnchorSizeChanged;
        FormatPopup.TextPaletteRequested += (sender, _) =>
        {
            if (BindingContext is not NotesViewModel { IsTextPaletteOpen: true })
            {
                TextPalettePopup.Dismiss();
                return;
            }

            if (sender is View anchor)
            {
                HighlightPalettePopup.Dismiss();
                TextPalettePopup.ShowFor(anchor);
            }
        };
        FormatPopup.HighlightPaletteRequested += (sender, _) =>
        {
            if (BindingContext is not NotesViewModel { IsHighlightPaletteOpen: true })
            {
                HighlightPalettePopup.Dismiss();
                return;
            }

            if (sender is View anchor)
            {
                TextPalettePopup.Dismiss();
                HighlightPalettePopup.ShowFor(anchor);
            }
        };
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

    protected override void OnBindingContextChanged()
    {
        if (_observedViewModel is not null)
            _observedViewModel.PropertyChanged -= OnViewModelPropertyChanged;

        base.OnBindingContextChanged();

        _observedViewModel = BindingContext as NotesViewModel;
        if (_observedViewModel is not null)
            _observedViewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NotesViewModel.SelectedNote))
            DismissTransientPopups();
    }

    private void DismissTransientPopups()
    {
        FormatPopup.Dismiss();
        TextPalettePopup.Dismiss();
        HighlightPalettePopup.Dismiss();
        AiEditingPopup.Dismiss();
    }

    private void OnToolbarSizeChanged(object? sender, EventArgs e)
    {
        UpdateResponsiveSearchLayout();
        RepositionOpenPopups();
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        if (!ReferenceEquals(_observedWindow, Window))
        {
            if (_observedWindow is not null)
                _observedWindow.PropertyChanged -= OnWindowPropertyChanged;

            _observedWindow = Window;
            if (_observedWindow is not null)
                _observedWindow.PropertyChanged += OnWindowPropertyChanged;
        }

        Dispatcher.Dispatch(RepositionOpenPopups);
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        if (_observedWindow is not null)
        {
            _observedWindow.PropertyChanged -= OnWindowPropertyChanged;
            _observedWindow = null;
        }
    }

    private void OnWindowPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(Window.X) or nameof(Window.Y) or
            nameof(Window.Width) or nameof(Window.Height))
        {
            Dispatcher.Dispatch(RepositionOpenPopups);
        }
    }

    private void OnPopupAnchorSizeChanged(object? sender, EventArgs e) =>
        RepositionOpenPopups();

    private void RepositionOpenPopups()
    {
        if (EditorToolbar.IsVisible)
        {
            FormatPopup.Reposition(EditorToolbar.FormatButton);
            AiEditingPopup.Reposition();
            TextPalettePopup.Reposition();
            HighlightPalettePopup.Reposition();
        }
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
        DismissTransientPopups();
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

        try
        {
            await viewModel.BeginAiAssistantAsync(RichTextEditor);
            if (!await PopupPlacement.WaitForAnchorLayoutAsync(EditorToolbar.AiButton))
            {
                viewModel.CloseAiAssistantCommand.Execute(null);
                return;
            }

            FormatPopup.Dismiss();
            TextPalettePopup.Dismiss();
            HighlightPalettePopup.Dismiss();
            AiEditingPopup.ShowWritingTools(EditorToolbar.AiButton);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NoteEditorDesktopUI] OnAiRequested failed: {ex}");
        }
    }

    private async void OnFormatRequested(object? sender, EventArgs e)
    {
        if (BindingContext is not NotesViewModel { IsEditorEditable: true })
            return;

        try
        {
            if (!await PopupPlacement.WaitForAnchorLayoutAsync(EditorToolbar.FormatButton))
                return;

            AiEditingPopup.Dismiss();
            TextPalettePopup.Dismiss();
            HighlightPalettePopup.Dismiss();
            FormatPopup.ToggleFormat(EditorToolbar.FormatButton);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NoteEditorDesktopUI] OnFormatRequested failed: {ex}");
        }
    }

    // Editor section - Keep toolbar formatting state synchronized with the RTE.
    private void OnEditorFormatChanged(object? sender, RichTextEditorFormatChangedEventArgs e)
    {
        if (BindingContext is NotesViewModel viewModel)
            viewModel.UpdateFormatState(e);
    }
}
