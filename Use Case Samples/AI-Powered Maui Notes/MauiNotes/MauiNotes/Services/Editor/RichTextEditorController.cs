using CustomRichTextEditor;
using Microsoft.Maui.Graphics;
using Syncfusion.Maui.RichTextEditor;

namespace MauiNotes.Services.Editor;

public sealed class RichTextEditorController : IRichTextEditorController
{
    private readonly CustomRichTextEditor.CustomRichTextEditor editor;

    public RichTextEditorController(CustomRichTextEditor.CustomRichTextEditor editor)
    {
        this.editor = editor;
    }

    public Color EditorBackgroundColor => editor.EditorBackgroundColor;
    public string DefaultFontFamily => editor.DefaultFontFamily;

    public async Task<EditorSelectionSnapshot?> GetStoredSelectionAsync() =>
        Wrap(await editor.GetStoredSelectionAsync());

    public async Task<EditorSelectionSnapshot?> CaptureSelectionAsync() =>
        Wrap(await editor.CaptureSelectionAsync());

    public async Task<EditorSelectionSnapshot?> CaptureDocumentAsync() =>
        Wrap(await editor.CaptureDocumentAsync());

    public async Task<EditorReplaceResult> ReplaceAsync(
        EditorSelectionSnapshot snapshot,
        string content,
        EditorContentType contentType)
    {
        RichTextReplaceResult result = await editor.ReplaceSelectionAsync(
            snapshot.NativeSnapshot,
            content,
            contentType == EditorContentType.Html
                ? RichTextContentType.Html
                : RichTextContentType.PlainText);
        return Wrap(result);
    }

    public async Task<EditorReplaceResult> RestoreAsync(EditorReplacementReceipt receipt) =>
        Wrap(await editor.RestoreReplacementAsync(receipt.NativeReceipt));

    public Task<string?> GetCurrentHtmlAsync() => editor.GetCurrentHtmlAsync();

    public EditorFormatState GetFormatState(RichTextEditorFormatChangedEventArgs args)
    {
        Color? highlightColor = args.NewFormat.HighlightedColor;
        bool isHighlightActive = highlightColor is { Alpha: > 0 } &&
            !highlightColor.Equals(editor.EditorBackgroundColor);
        return new EditorFormatState(
            args.NewFormat.IsBold,
            args.NewFormat.IsItalic,
            args.NewFormat.IsUnderline,
            args.NewFormat.IsStrikethrough,
            isHighlightActive,
            string.Equals(
                args.NewFormat.FontFamily,
                MauiNotes.MauiNotesConstants.MonospaceFontFamilyMatcher,
                StringComparison.OrdinalIgnoreCase),
            args.NewFormat.IsBulletList,
            args.NewFormat.IsNumberedList,
            args.NewFormat.ParagraphFormat.ToString());
    }

    public void Undo() => editor.Undo();
    public void Redo() => editor.Redo();
    public void ToggleBold() => editor.ToggleBold();
    public void ToggleItalic() => editor.ToggleItalic();
    public void ToggleUnderline() => editor.ToggleUnderline();
    public void ToggleStrikethrough() => editor.ToggleStrikethrough();
    public void ToggleBulletList() => editor.ToggleBulletList();
    public void ToggleNumberedList() => editor.ToggleNumberedList();
    public void ApplyTextColor(Color color) => editor.ApplyTextColor(color);
    public void ApplyHighlightColor(Color color) => editor.ApplyHighlightColor(color);
    public void ApplyFontFamily(string fontFamily) => editor.ApplyFontFamily(fontFamily);
    public void InsertTable(int rows, int columns) => editor.InsertTable(rows, columns);

    public void ApplyParagraphFormat(EditorParagraphFormat format) =>
        editor.ApplyParagraphFormat(format switch
        {
            EditorParagraphFormat.Heading1 => RichTextEditorParagraphFormat.Heading1,
            EditorParagraphFormat.Heading2 => RichTextEditorParagraphFormat.Heading2,
            EditorParagraphFormat.Heading3 => RichTextEditorParagraphFormat.Heading3,
            EditorParagraphFormat.Paragraph => RichTextEditorParagraphFormat.Paragraph,
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        });

    private static EditorSelectionSnapshot? Wrap(RichTextSelectionSnapshot? snapshot) =>
        snapshot is null ? null : new EditorSelectionSnapshot(snapshot);

    private static EditorReplaceResult Wrap(RichTextReplaceResult result) =>
        new(
            result.Succeeded,
            result.IsStaleSelection,
            result.ErrorMessage,
            result.UndoReceipt is null ? null : new EditorReplacementReceipt(result.UndoReceipt));
}
