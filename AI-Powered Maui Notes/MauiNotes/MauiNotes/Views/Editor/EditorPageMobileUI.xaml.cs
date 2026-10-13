using MauiNotes.Controls.AI;
using MauiNotes.Controls.Toolbars;
using MauiNotes.Controls.Popups;
using MauiNotes.ViewModels;
using Syncfusion.Maui.RichTextEditor;

namespace MauiNotes.Views.Editor;

public partial class EditorPageMobileUI : ContentPage
{
    // Mirrors the first page's navigation guard. Without this guard a fast
    // double-tap on the back button queues two Shell navigations and the
    // second one runs while the editor's WebView is still disposing, which
    // blocks the UI thread and triggers the Android ANR ("MauiNotes isn't
    // responding") dialog the moment the editor page is opened.
    private bool isNavigatingBack;
    // Defer only toolbar and popup event wiring until the page has appeared.
    private bool isInitialized;
    private bool isPageVisible;
    private bool isEditorHostLoaded;
    private Window? observedWindow;
    private double observedWindowWidth;
    private double observedWindowHeight;
    private bool isPopupRepositionQueued;

    public EditorPageMobileUI()
    {
        InitializeComponent();
        BindingContext = AppShell.SharedNotesViewModel;
        RichTextEditor.Focused += OnRichTextEditorFocused;
        RichTextEditor.Unfocused += OnRichTextEditorUnfocused;
        RichTextEditor.Loaded += OnEditorLoaded;
    }

    private void OnEditorLoaded(object? sender, EventArgs e)
    {
        isEditorHostLoaded = true;
        InitializeInteractions();
    }

    private void InitializeInteractions()
    {
        if (isInitialized || !isPageVisible || !isEditorHostLoaded)
            return;

        EditorToolbar.AiRequested += OnAiRequested;
        EditorToolbar.FormatRequested += OnFormatRequested;
        FormatPopup.TextPaletteRequested += (sender, _) =>
        {
            if (BindingContext is NotesViewModel { IsTextPaletteOpen: true } &&
                sender is View anchor)
            {
                HighlightPalettePopup.Dismiss();
                TextPalettePopup.ShowFor(anchor);
            }
            else
            {
                TextPalettePopup.Dismiss();
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
        EditorToolbar.TableRequested += OnTableRequested;

        isInitialized = true;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        isPageVisible = true;

        if (!ReferenceEquals(observedWindow, Window))
        {
            if (observedWindow is not null)
                observedWindow.PropertyChanged -= OnWindowPropertyChanged;

            observedWindow = Window;
            if (observedWindow is not null)
            {
                observedWindowWidth = observedWindow.Width;
                observedWindowHeight = observedWindow.Height;
                observedWindow.PropertyChanged += OnWindowPropertyChanged;
            }
        }

        // Defer interaction wiring so the editor and page can complete layout.
        if (!isInitialized)
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), InitializeInteractions);
        else
            Dispatcher.Dispatch(RepositionOpenPopups);

        // AI workflows are always started fresh on re-entry; any in-flight
        // session must not survive the navigation.
        if (BindingContext is NotesViewModel viewModel &&
            (viewModel.IsAiPopupOpen || viewModel.IsAiProcessing))
        {
            viewModel.CloseAiAssistantCommand.Execute(null);
        }
    }

    private async void OnAiRequested(object? sender, EventArgs e)
    {
        if (BindingContext is not NotesViewModel viewModel)
            return;

        if (!await viewModel.BeginAiAssistantAsync(RichTextEditor))
            return;

        if (!await PopupPlacement.WaitForAnchorLayoutAsync(PopupPositionAnchor))
        {
            viewModel.CloseAiAssistantCommand.Execute(null);
            return;
        }

        if (!isPageVisible)
            return;

        FormatPopup.Dismiss();
        TextPalettePopup.Dismiss();
        HighlightPalettePopup.Dismiss();
        AiEditingPopup.ShowWritingTools(PopupPositionAnchor);
    }

    private async void OnBackClicked(object? sender, TappedEventArgs e)
    {
        // Guard against double-taps. A second pending navigation while the
        // first one is still resolving produces an ANR dialog because the
        // editor page's OnDisappearing runs heavy cleanup on the UI thread
        // (popup dismissal, AI cancellation, RTE WebView teardown).
        if (isNavigatingBack)
            return;

        isNavigatingBack = true;
        try
        {
            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            // The shell consumes the back navigation, so we always clear the
            // flag - if GoToAsync throws, we still want the user to be able
            // to retry.
            isNavigatingBack = false;
        }
    }

    private void OnTableRequested(object? sender, EventArgs e)
    {
        if (BindingContext is NotesViewModel viewModel &&
            !viewModel.IsEditorReadOnly)
        {
            EditorToolbarActions.InsertTable(RichTextEditor);
        }
    }

    private void OnRichTextEditorFocused(object? sender, EventArgs e)
    {
        if (BindingContext is NotesViewModel viewModel &&
            sender is CustomRichTextEditor.CustomRichTextEditor editor)
        {
            viewModel.TrackEditorFocus(editor, true);
        }
    }

    private void OnRichTextEditorUnfocused(object? sender, EventArgs e)
    {
        if (BindingContext is NotesViewModel viewModel &&
            sender is CustomRichTextEditor.CustomRichTextEditor editor)
        {
            viewModel.TrackEditorFocus(editor, false);
        }
    }

    private async void OnFormatRequested(object? sender, EventArgs e)
    {
        if (BindingContext is not NotesViewModel { IsEditorEditable: true })
            return;

        if (!await PopupPlacement.WaitForAnchorLayoutAsync(PopupPositionAnchor) || !isPageVisible)
            return;

        AiEditingPopup.Dismiss();
        TextPalettePopup.Dismiss();
        HighlightPalettePopup.Dismiss();
        FormatPopup.ToggleFormat(PopupPositionAnchor, centeredOnAnchor: true);
    }

    private void OnWindowPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not (nameof(Window.Width) or nameof(Window.Height)) ||
            observedWindow is null)
            return;

        double width = observedWindow.Width;
        double height = observedWindow.Height;
        if (Math.Abs(width - observedWindowWidth) < 0.5 &&
            Math.Abs(height - observedWindowHeight) < 0.5)
            return;

        observedWindowWidth = width;
        observedWindowHeight = height;
        QueuePopupReposition();
    }

    private void QueuePopupReposition()
    {
        if (!isPageVisible || !isInitialized || isPopupRepositionQueued)
            return;

        isPopupRepositionQueued = true;
        Dispatcher.Dispatch(() =>
        {
            isPopupRepositionQueued = false;
            RepositionOpenPopups();
        });
    }

    private void RepositionOpenPopups()
    {
        if (!isPageVisible)
            return;

        FormatPopup.Reposition(PopupPositionAnchor, centeredOnAnchor: true);
        AiEditingPopup.Reposition();
        TextPalettePopup.Reposition();
        HighlightPalettePopup.Reposition();
    }

    private void OnEditorFormatChanged(object? sender, RichTextEditorFormatChangedEventArgs e)
    {
        if (BindingContext is NotesViewModel viewModel)
            viewModel.UpdateFormatState(e);
    }

    protected override void OnDisappearing()
    {
        isPageVisible = false;
        isNavigatingBack = false;
        if (observedWindow is not null)
        {
            observedWindow.PropertyChanged -= OnWindowPropertyChanged;
            observedWindow = null;
        }

        // Dismiss the AI popup and cancel active work before leaving the page,
        // so pending callbacks cannot target a hidden editor.
        if (BindingContext is NotesViewModel viewModel &&
            (viewModel.IsAiPopupOpen || viewModel.IsAiProcessing))
        {
            viewModel.CloseAiAssistantCommand.Execute(null);
        }

        FormatPopup.Dismiss();
        TextPalettePopup.Dismiss();
        HighlightPalettePopup.Dismiss();
        AiEditingPopup.Dismiss();

        base.OnDisappearing();
    }
}
