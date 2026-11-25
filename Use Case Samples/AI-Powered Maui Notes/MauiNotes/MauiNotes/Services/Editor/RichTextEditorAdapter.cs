using CustomRichTextEditor;
using Syncfusion.Maui.RichTextEditor;

namespace MauiNotes.Services.Editor;

/// <summary>
/// Adapts <see cref="CustomRichTextEditor.CustomRichTextEditor"/> to
/// <see cref="IRichTextEditorAdapter"/>. Views attach the focused instance so the
/// view model never stores a control reference.
/// </summary>
public sealed class RichTextEditorAdapter : IRichTextEditorAdapter
{
    private CustomRichTextEditor.CustomRichTextEditor? editor;

    public Color? EditorBackgroundColor => editor?.EditorBackgroundColor;

    public string DefaultFontFamily => editor?.DefaultFontFamily ?? string.Empty;

    public void Attach(CustomRichTextEditor.CustomRichTextEditor? richTextEditor) =>
        editor = richTextEditor;

    public Task<RichTextSelectionSnapshot?> GetStoredSelectionAsync() =>
        Require().GetStoredSelectionAsync();

    public Task<RichTextSelectionSnapshot?> GetSelectionAsync() =>
        Require().CaptureSelectionAsync();

    public Task<RichTextSelectionSnapshot?> CaptureDocumentAsync() =>
        Require().CaptureDocumentAsync();

    public Task<string?> GetCurrentHtmlAsync() =>
        Require().GetCurrentHtmlAsync();

    public Task<RichTextReplaceResult> RestoreReplacementAsync(RichTextReplaceReceipt receipt) =>
        Require().RestoreReplacementAsync(receipt);

    public Task<RichTextReplaceResult> ReplaceAsync(
        RichTextSelectionSnapshot snapshot,
        string content,
        RichTextContentType contentType) =>
        Require().ReplaceSelectionAsync(snapshot, content, contentType);

    public void Undo() => Require().Undo();

    public void Redo() => Require().Redo();

    public void ToggleBold() => Require().ToggleBold();

    public void ToggleItalic() => Require().ToggleItalic();

    public void ToggleUnderline() => Require().ToggleUnderline();

    public void ToggleStrikethrough() => Require().ToggleStrikethrough();

    public void ApplyTextColor(Color color) => Require().ApplyTextColor(color);

    public void ApplyHighlightColor(Color color) => Require().ApplyHighlightColor(color);

    public void ApplyFontFamily(string fontFamily) => Require().ApplyFontFamily(fontFamily);

    public void ApplyParagraphFormat(RichTextEditorParagraphFormat format) =>
        Require().ApplyParagraphFormat(format);

    public void ToggleBulletList() => Require().ToggleBulletList();

    public void ToggleNumberedList() => Require().ToggleNumberedList();

    public void InsertTable(int rows, int columns) => Require().InsertTable(rows, columns);

    public void LoadContent(string html) => Require().Value = html;

    private CustomRichTextEditor.CustomRichTextEditor Require() =>
        editor ?? throw new InvalidOperationException("No editor is registered.");
}
