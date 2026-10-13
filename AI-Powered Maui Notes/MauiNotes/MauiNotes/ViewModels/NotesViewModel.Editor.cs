using Syncfusion.Maui.RichTextEditor;
using CustomRichTextEditor;

namespace MauiNotes.ViewModels;

public partial class NotesViewModel
{
    public void TrackEditorFocus(CustomRichTextEditor.CustomRichTextEditor editor, bool isFocused)
    {
        if (isFocused)
            ActiveEditor = editor;

        IsEditorFocused = isFocused;
    }
    /// <summary>
    /// Android-safe monospace family alias. The RTE engine's font picker
    /// asks the platform to load the family it is given; platform-specific
    /// names that do not exist as an asset (e.g. "Courier New") throw
    /// <c>java.lang.RuntimeException: Font asset not found</c> on every
    /// touch interaction, which is a primary contributor to the ANR stalls
    /// observed when entering the editor page. We compare and apply
    /// <c>"monospace"</c> instead, which the Android font manager resolves
    /// to a built-in typeface without doing asset I/O on the UI thread.
    /// </summary>
    internal const string MonospacedFontFamily = "monospace";

    public void UpdateFormatState(RichTextEditorFormatChangedEventArgs args)
    {
        IsBoldActive = args.NewFormat.IsBold;
        IsItalicActive = args.NewFormat.IsItalic;
        IsUnderlineActive = args.NewFormat.IsUnderline;
        IsStrikethroughActive = args.NewFormat.IsStrikethrough;
        Color? highlightColor = args.NewFormat.HighlightedColor;
        Color? editorBackgroundColor = ActiveEditor?.EditorBackgroundColor;
        IsHighlightActive = highlightColor is { Alpha: > 0 } &&
            (editorBackgroundColor is null || !highlightColor.Equals(editorBackgroundColor));
        IsMonostyledActive = string.Equals(args.NewFormat.FontFamily, MonospacedFontFamily, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(args.NewFormat.FontFamily, "Courier New", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(args.NewFormat.FontFamily, "Courier", StringComparison.OrdinalIgnoreCase);
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
            ActiveEditor?.ApplyTextColor(Color.FromArgb(SelectedTextColor));
            IsTextPaletteOpen = false;
            return;
        }

        if (action.StartsWith("HighlightColor:", StringComparison.Ordinal))
        {
            CustomRichTextEditor.CustomRichTextEditor? activeEditorForHighlight = ActiveEditor;
            if (activeEditorForHighlight is null)
                return;

            string highlightColor = action[(action.IndexOf(':') + 1)..];
            bool removeHighlight = IsHighlightActive && string.Equals(
                SelectedHighlightColor, highlightColor, StringComparison.OrdinalIgnoreCase);
            SelectedHighlightColor = highlightColor;
            activeEditorForHighlight.ApplyHighlightColor(removeHighlight
                ? activeEditorForHighlight.EditorBackgroundColor
                : Color.FromArgb(SelectedHighlightColor));
            IsHighlightActive = !removeHighlight;
            IsHighlightPaletteOpen = false;
            return;
        }

        CustomRichTextEditor.CustomRichTextEditor? editor = ActiveEditor;
        if (editor is null)
            return;

        switch (action)
        {
            case "Undo": editor.Undo(); break;
            case "Redo": editor.Redo(); break;
            case "Bold":
                IsBoldActive = !IsBoldActive;
                editor.ToggleBold();
                break;
            case "Italic":
                IsItalicActive = !IsItalicActive;
                editor.ToggleItalic();
                break;
            case "Underline":
                IsUnderlineActive = !IsUnderlineActive;
                editor.ToggleUnderline();
                break;
            case "Strikethrough":
                IsStrikethroughActive = !IsStrikethroughActive;
                editor.ToggleStrikethrough();
                break;
            case "Title": editor.ApplyParagraphFormat(RichTextEditorParagraphFormat.Heading1); break;
            case "Heading": editor.ApplyParagraphFormat(RichTextEditorParagraphFormat.Heading2); break;
            case "Subheading": editor.ApplyParagraphFormat(RichTextEditorParagraphFormat.Heading3); break;
            case "Body": editor.ApplyParagraphFormat(RichTextEditorParagraphFormat.Paragraph); break;
            case "Monostyled":
                // Use the Android-resolvable monospace alias rather than a
                // platform-specific name like "Courier New". Syncfusion's RTE
                // engine asks Android to load the family it is given on every
                // interaction, and an unsupported family throws
                // Font asset not found on the main thread, which is a major
                // contributor to the editor page ANR.
                editor.ApplyFontFamily(IsMonostyledActive ? editor.DefaultFontFamily : MonospacedFontFamily);
                break;
            case "BulletedList": editor.ToggleBulletList(); break;
            case "NumberedList": editor.ToggleNumberedList(); break;
            case "Table": editor.InsertTable(2, 3); break;
            case "Highlight":
                bool applyHighlight = !IsHighlightActive;
                editor.ApplyHighlightColor(applyHighlight
                    ? Color.FromArgb(SelectedHighlightColor)
                    : editor.EditorBackgroundColor);
                IsHighlightActive = applyHighlight;
                break;
        }
    }
}
