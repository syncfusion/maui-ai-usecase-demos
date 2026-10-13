using MauiNotes.ViewModels;
using System.ComponentModel;
// Pull the cross-project font constant into scope without forcing every
// reference site to spell the parent namespace.
using MauiNotesConstants = MauiNotes.MauiNotesConstants;

namespace MauiNotes.Controls.Popups;

public partial class FormatPopup : ContentView
{
    private readonly FormatOption[] formatOptions =
    [
        new("Title", "Title", string.Empty, 9),
        new("Heading", "Heading", string.Empty, 9),
        new("Subheading", "Subheading", string.Empty, 9),
        new("Body", "Body", string.Empty, 9),
        // The Monostyled option used to apply the literal "Courier New" font
        // family. On Android that string is not a bundled font asset, which
        // made Microsoft.Maui.FontManager throw a Java.Lang.RuntimeException
        // ("Font asset not found Courier New") every time the user tapped
        // inside the editor - one of the main contributors to the editor
        // page ANR. The CSS-standard "monospace" keyword is honoured by every
        // WebView and never triggers an asset lookup, so the editor and the
        // monostyled state machine both stay in sync.
        new("Monostyled", "Monostyled", string.Empty, 9, MauiNotesConstants.MonospaceFontFamily),
        new("BulletedList", "Bulleted List", "\u2022", 12),
        new("NumberedList", "Numbered List", "1.", 9)
    ];
    private NotesViewModel? observedViewModel;

    public event EventHandler? TextPaletteRequested;
    public event EventHandler? HighlightPaletteRequested;
    public IReadOnlyList<object> FormatOptions => formatOptions;
    public bool IsOpen => PopupView.IsOpen;

    public FormatPopup()
    {
        InitializeComponent();
        BindingContextChanged += OnBindingContextChanged;
        OnBindingContextChanged(this, EventArgs.Empty);
    }

    public void ToggleFormat(View anchor, bool centeredOnAnchor = false)
    {
        if (BindingContext is not NotesViewModel viewModel)
            return;

        bool shouldOpen = !viewModel.IsFormatPopoverOpen;
        viewModel.ToolbarActionCommand.Execute("Format");
        if (shouldOpen)
        {
            viewModel.IsTextPaletteOpen = false;
            viewModel.IsHighlightPaletteOpen = false;
            PopupPlacement.ShowBelow(PopupView, anchor, centeredOnAnchor);
        }
        else
        {
            PopupView.Dismiss();
        }
    }

    public void Reposition(View anchor, bool centeredOnAnchor = false)
    {
        if (PopupView.IsOpen)
            PopupPlacement.ShowBelow(PopupView, anchor, centeredOnAnchor);
    }

    public void Dismiss() => PopupView.Dismiss();

    private void OnFormatOptionTapped(object? sender, TappedEventArgs e)
    {
        if (observedViewModel is not null &&
            sender is Grid { BindingContext: FormatOption option })
        {
            observedViewModel.ToolbarActionCommand.Execute(option.Action);
        }
    }

    private void OnTextPaletteClicked(object? sender, EventArgs e)
    {
        if (BindingContext is NotesViewModel viewModel)
            viewModel.ToolbarActionCommand.Execute("TextPalette");

        TextPaletteRequested?.Invoke(sender, EventArgs.Empty);
    }

    private void OnHighlightPaletteClicked(object? sender, EventArgs e)
    {
        if (BindingContext is NotesViewModel viewModel)
            viewModel.ToolbarActionCommand.Execute("HighlightPalette");

        HighlightPaletteRequested?.Invoke(sender, EventArgs.Empty);
    }

    private void OnFormatPopupClosed(object? sender, EventArgs e)
    {
        if (BindingContext is NotesViewModel viewModel)
        {
            viewModel.IsFormatPopoverOpen = false;
            viewModel.IsTextPaletteOpen = false;
            viewModel.IsHighlightPaletteOpen = false;
        }
    }

    private void OnBindingContextChanged(object? sender, EventArgs e)
    {
        if (observedViewModel is not null)
            observedViewModel.PropertyChanged -= OnViewModelPropertyChanged;

        observedViewModel = BindingContext as NotesViewModel;
        if (observedViewModel is null)
            return;

        observedViewModel.PropertyChanged += OnViewModelPropertyChanged;
        UpdateFormatOptions(observedViewModel);
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not NotesViewModel viewModel
            || e.PropertyName is not (nameof(NotesViewModel.CurrentParagraphFormat)
                or nameof(NotesViewModel.IsBulletListActive)
                or nameof(NotesViewModel.IsNumberedListActive)))
            return;

        UpdateFormatOptions(viewModel);
    }

    private void UpdateFormatOptions(NotesViewModel viewModel)
    {
        foreach (FormatOption option in formatOptions)
        {
            bool isTitle = option.Action == "Title"
                && viewModel.CurrentParagraphFormat == "Heading1";

            option.IsChecked = isTitle;
            option.IsHighlighted = option.Action switch
            {
                "Title" => isTitle,
                "Heading" => viewModel.CurrentParagraphFormat == "Heading2",
                "Subheading" => viewModel.CurrentParagraphFormat == "Heading3",
                "Body" => viewModel.CurrentParagraphFormat == "Paragraph",
                "BulletedList" => viewModel.IsBulletListActive,
                "NumberedList" => viewModel.IsNumberedListActive,
                _ => false
            };
        }
    }
}

internal sealed class FormatOption(
    string action,
    string text,
    string glyph,
    double glyphFontSize,
    string? fontFamily = null) : INotifyPropertyChanged
{
    private bool isHighlighted;
    private bool isChecked;

    public string Action { get; } = action;
    public string Text { get; } = text;
    public string Glyph { get; } = glyph;
    public double GlyphFontSize { get; } = glyphFontSize;
    public string? FontFamily { get; } = fontFamily;

    public bool IsHighlighted
    {
        get => isHighlighted;
        set
        {
            if (isHighlighted == value)
                return;

            isHighlighted = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsHighlighted)));
        }
    }

    public bool IsChecked
    {
        get => isChecked;
        set
        {
            if (isChecked == value)
                return;

            isChecked = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChecked)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
