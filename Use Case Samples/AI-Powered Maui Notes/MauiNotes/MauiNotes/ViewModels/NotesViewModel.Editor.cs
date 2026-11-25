using MauiNotes.Services.Editor;
using Syncfusion.Maui.RichTextEditor;
using MauiNotesConstants = MauiNotes.MauiNotesConstants;

namespace MauiNotes.ViewModels;

public partial class NotesViewModel
{
    public void TrackEditorFocus(bool isFocused)
    {
        IsEditorFocused = isFocused;
    }

    public void UpdateFormatState(RichTextEditorFormatChangedEventArgs args)
    {
        IsBoldActive = args.NewFormat.IsBold;
        IsItalicActive = args.NewFormat.IsItalic;
        IsUnderlineActive = args.NewFormat.IsUnderline;
        IsStrikethroughActive = args.NewFormat.IsStrikethrough;
        Color? highlightColor = args.NewFormat.HighlightedColor;
        Color? editorBackgroundColor = editorController.EditorBackgroundColor;
        IsHighlightActive = highlightColor is { Alpha: > 0 } &&
            (editorBackgroundColor is null || !highlightColor.Equals(editorBackgroundColor));
        IsMonostyledActive = string.Equals(
            args.NewFormat.FontFamily,
            MauiNotesConstants.MonospaceFontFamilyMatcher,
            StringComparison.OrdinalIgnoreCase);
        IsBulletListActive = args.NewFormat.IsBulletList;
        IsNumberedListActive = args.NewFormat.IsNumberedList;
        CurrentParagraphFormat = args.NewFormat.ParagraphFormat.ToString();
    }

    /// <summary>
    /// Defers clearing <see cref="isApplyingProgrammaticContent"/> until after
    /// the current UI dispatch cycle. Setting Value via binding does not
    /// update the native editor synchronously, so the RTE's own Value
    /// notification for a programmatic content swap arrives on a later dispatcher
    /// callback - the guard must still be set when that callback runs.
    /// </summary>
    private void ClearProgrammaticContentFlagAfterDispatch()
    {
        MainThread.BeginInvokeOnMainThread(() => isApplyingProgrammaticContent = false);
    }

    private void ExecuteToolbarAction(string? action)
    {
        if (action is null || IsEditorReadOnly)
            return;

        if (action == "Format")
        {
            IsFormatPopoverOpen = !IsFormatPopoverOpen;
            IsTextPaletteOpen = false;
            IsHighlightPaletteOpen = false;
            return;
        }

        if (action == "TextPalette")
        {
            IsTextPaletteOpen = !IsTextPaletteOpen;
            IsHighlightPaletteOpen = false;
            return;
        }

        if (action == "HighlightPalette")
        {
            IsHighlightPaletteOpen = !IsHighlightPaletteOpen;
            IsTextPaletteOpen = false;
            return;
        }

        if (action.StartsWith("TextColor:", StringComparison.Ordinal))
        {
            SelectedTextColor = action[(action.IndexOf(':') + 1)..];
            editorController.ApplyTextColor(Color.FromArgb(SelectedTextColor));
            IsTextPaletteOpen = false;
            return;
        }

        if (action.StartsWith("HighlightColor:", StringComparison.Ordinal))
        {
            string highlightColor = action[(action.IndexOf(':') + 1)..];
            bool removeHighlight = IsHighlightActive && string.Equals(
                SelectedHighlightColor, highlightColor, StringComparison.OrdinalIgnoreCase);
            SelectedHighlightColor = highlightColor;
            Color? background = editorController.EditorBackgroundColor ?? Colors.White;
            editorController.ApplyHighlightColor(removeHighlight
                ? background
                : Color.FromArgb(SelectedHighlightColor));
            IsHighlightActive = !removeHighlight;
            IsHighlightPaletteOpen = false;
            return;
        }

        switch (action)
        {
            case "Undo": editorController.Undo(); break;
            case "Redo": editorController.Redo(); break;
            case "Bold":
                IsBoldActive = !IsBoldActive;
                editorController.ToggleBold();
                break;
            case "Italic":
                IsItalicActive = !IsItalicActive;
                editorController.ToggleItalic();
                break;
            case "Underline":
                IsUnderlineActive = !IsUnderlineActive;
                editorController.ToggleUnderline();
                break;
            case "Strikethrough":
                IsStrikethroughActive = !IsStrikethroughActive;
                editorController.ToggleStrikethrough();
                break;
            case "Title": editorController.ApplyParagraphFormat(RichTextEditorParagraphFormat.Heading1); break;
            case "Heading": editorController.ApplyParagraphFormat(RichTextEditorParagraphFormat.Heading2); break;
            case "Subheading": editorController.ApplyParagraphFormat(RichTextEditorParagraphFormat.Heading3); break;
            case "Body": editorController.ApplyParagraphFormat(RichTextEditorParagraphFormat.Paragraph); break;
            case "Monostyled":
                editorController.ApplyFontFamily(IsMonostyledActive
                    ? editorController.DefaultFontFamily
                    : MauiNotesConstants.MonospaceFontFamily);
                break;
            case "BulletedList": editorController.ToggleBulletList(); break;
            case "NumberedList": editorController.ToggleNumberedList(); break;
            case "Table": editorController.InsertTable(2, 3); break;
            case "Highlight":
                bool applyHighlight = !IsHighlightActive;
                Color? editorBackground = editorController.EditorBackgroundColor ?? Colors.White;
                editorController.ApplyHighlightColor(applyHighlight
                    ? Color.FromArgb(SelectedHighlightColor)
                    : editorBackground);
                IsHighlightActive = applyHighlight;
                break;
        }
    }
}
