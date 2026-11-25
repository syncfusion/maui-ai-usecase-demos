using CustomRichTextEditor;
using Syncfusion.Maui.RichTextEditor;
using EditorControl = CustomRichTextEditor.CustomRichTextEditor;

namespace MauiNotes.Services.Editor;

public sealed class RichTextEditorAdapter : IRichTextEditorAdapter
{
    private EditorControl? editor;

    public bool IsAttached => editor is not null;

    public Color EditorBackgroundColor =>
        Require().EditorBackgroundColor;

    public void Attach(EditorControl editor) => this.editor = editor;

    public void Detach(EditorControl editor)
    {
        if (ReferenceEquals(this.editor, editor))
            this.editor = null;
    }

    public void Undo() => Require().Undo();

    public void Redo() => Require().Redo();

    public void ToggleBold() => Require().ToggleBold();

    public void ToggleItalic() => Require().ToggleItalic();

    public void ToggleUnderline() => Require().ToggleUnderline();

    public void ToggleStrikethrough() => Require().ToggleStrikethrough();

    public void ToggleBulletList() => Require().ToggleBulletList();

    public void ToggleNumberedList() => Require().ToggleNumberedList();

    public void ApplyParagraphFormat(RichTextEditorParagraphFormat format) =>
        Require().ApplyParagraphFormat(format);

    public void ApplyFontFamily(string fontFamily) =>
        Require().ApplyFontFamily(fontFamily);

    public void ApplyTextColor(Color color) =>
        Require().ApplyTextColor(color);

    public void ApplyHighlightColor(Color color) =>
        Require().ApplyHighlightColor(color);

    public void InsertTable(int rows, int columns) =>
        Require().InsertTable(rows, columns);

    public Task<RichTextSelectionSnapshot?> GetStoredSelectionAsync() =>
        Require().GetStoredSelectionAsync();

    public Task<RichTextSelectionSnapshot?> CaptureSelectionAsync() =>
        Require().CaptureSelectionAsync();

    public Task<RichTextSelectionSnapshot?> CaptureDocumentAsync() =>
        Require().CaptureDocumentAsync();

    public Task<string?> GetCurrentHtmlAsync() =>
        Require().GetCurrentHtmlAsync();

    public Task<RichTextReplaceResult> ReplaceSelectionAsync(
        RichTextSelectionSnapshot snapshot,
        string html,
        RichTextContentType contentType) =>
        Require().ReplaceSelectionAsync(snapshot, html, contentType);

    public Task<RichTextReplaceResult> ReplaceDocumentAsync(
        string html,
        RichTextContentType contentType) =>
        Require().ReplaceDocumentAsync(html, contentType);

    public Task<RichTextReplaceResult> RestoreReplacementAsync(RichTextReplaceReceipt receipt) =>
        Require().RestoreReplacementAsync(receipt);

    private EditorControl Require() =>
        editor ?? throw new InvalidOperationException("The rich text editor is not attached.");
}
