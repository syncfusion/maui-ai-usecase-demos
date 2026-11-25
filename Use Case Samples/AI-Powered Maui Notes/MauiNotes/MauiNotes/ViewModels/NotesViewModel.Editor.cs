using Syncfusion.Maui.RichTextEditor;
using MauiNotesConstants = MauiNotes.MauiNotesConstants;

namespace MauiNotes.ViewModels;

public partial class NotesViewModel
{
    public void TrackEditorFocus(bool isFocused) => IsEditorFocused = isFocused;

    public void UpdateFormatState(RichTextEditorFormatChangedEventArgs args)
    {
        IsBoldActive = args.NewFormat.IsBold;
        IsItalicActive = args.NewFormat.IsItalic;
        IsUnderlineActive = args.NewFormat.IsUnderline;
        IsStrikethroughActive = args.NewFormat.IsStrikethrough;
        Color? highlightColor = args.NewFormat.HighlightedColor;
        Color? editorBackgroundColor = editorAdapter.IsAttached
            ? editorAdapter.EditorBackgroundColor
            : null;
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

    private void ClearProgrammaticContentFlagAfterDispatch()
    {
        MainThread.BeginInvokeOnMainThread(() => isApplyingProgrammaticContent = false);
    }

    private void ExecuteToolbarAction(string? action)
    {
        if (action is null || IsEditorReadOnly || !editorAdapter.IsAttached)
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
            editorAdapter.ApplyTextColor(Color.FromArgb(SelectedTextColor));
            IsTextPaletteOpen = false;
            return;
        }

        if (action.StartsWith("HighlightColor:", StringComparison.Ordinal))
        {
            string highlightColor = action[(action.IndexOf(':') + 1)..];
            bool removeHighlight = IsHighlightActive && string.Equals(
                SelectedHighlightColor, highlightColor, StringComparison.OrdinalIgnoreCase);
            SelectedHighlightColor = highlightColor;
            editorAdapter.ApplyHighlightColor(removeHighlight
                ? editorAdapter.EditorBackgroundColor
                : Color.FromArgb(SelectedHighlightColor));
            IsHighlightActive = !removeHighlight;
            IsHighlightPaletteOpen = false;
            return;
        }

        switch (action)
        {
            case "Undo": editorAdapter.Undo(); break;
            case "Redo": editorAdapter.Redo(); break;
            case "Bold":
                IsBoldActive = !IsBoldActive;
                editorAdapter.ToggleBold();
                break;
            case "Italic":
                IsItalicActive = !IsItalicActive;
                editorAdapter.ToggleItalic();
                break;
            case "Underline":
                IsUnderlineActive = !IsUnderlineActive;
                editorAdapter.ToggleUnderline();
                break;
            case "Strikethrough":
                IsStrikethroughActive = !IsStrikethroughActive;
                editorAdapter.ToggleStrikethrough();
                break;
            case "Title": editorAdapter.ApplyParagraphFormat(RichTextEditorParagraphFormat.Heading1); break;
            case "Heading": editorAdapter.ApplyParagraphFormat(RichTextEditorParagraphFormat.Heading2); break;
            case "Subheading": editorAdapter.ApplyParagraphFormat(RichTextEditorParagraphFormat.Heading3); break;
            case "Body": editorAdapter.ApplyParagraphFormat(RichTextEditorParagraphFormat.Paragraph); break;
            case "Monostyled":
                editorAdapter.ApplyFontFamily(IsMonostyledActive
                    ? "OpenSans"
                    : MauiNotesConstants.MonospaceFontFamily);
                break;
            case "BulletedList": editorAdapter.ToggleBulletList(); break;
            case "NumberedList": editorAdapter.ToggleNumberedList(); break;
            case "Table": editorAdapter.InsertTable(2, 3); break;
            case "Highlight":
                bool applyHighlight = !IsHighlightActive;
                editorAdapter.ApplyHighlightColor(applyHighlight
                    ? Color.FromArgb(SelectedHighlightColor)
                    : editorAdapter.EditorBackgroundColor);
                IsHighlightActive = applyHighlight;
                break;
        }
    }
}
