using MauiNotes.ViewModels;
using System.ComponentModel;

namespace MauiNotes.Controls.Popups;

public partial class FormatPopup : ContentView
{
    private readonly FormatOption[] formatOptions =
    [
        new("Title", "Title", string.Empty, 9),
        new("Heading", "Heading", string.Empty, 9),
        new("Subheading", "Subheading", string.Empty, 9),
        new("Body", "Body", string.Empty, 9),
        new("Monostyled", "Monostyled", string.Empty, 9, NotesViewModel.MonospacedFontFamily),
        new("BulletedList", "Bulleted List", "\u2022", 12),
        new("NumberedList", "Numbered List", "1.", 9)
    ];
    private NotesViewModel? observedViewModel;

    public event EventHandler? TextPaletteRequested;
    public IReadOnlyList<object> FormatOptions => formatOptions;

    public FormatPopup()
    {
        InitializeComponent();
        BindingContextChanged += OnBindingContextChanged;
        OnBindingContextChanged(this, EventArgs.Empty);
    }

    public void ToggleFormat(View anchor)
    {
        if (BindingContext is not NotesViewModel viewModel)
            return;

        bool shouldOpen = !viewModel.IsFormatPopoverOpen;
        viewModel.ToolbarActionCommand.Execute("Format");
        if (shouldOpen)
        {
            viewModel.IsTextPaletteOpen = false;
            viewModel.IsHighlightPaletteOpen = false;
            PopupPlacement.ShowTopCentered(PopupView, anchor);
        }
        else
        {
            PopupView.Dismiss();
        }
    }

    private void OnTextPaletteClicked(object? sender, EventArgs e)
    {
        TextPaletteRequested?.Invoke(this, EventArgs.Empty);
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
