using CustomRichTextEditor;

namespace MauiNotes.Services.Editor;

/// <summary>
/// View-owned editor surface. The view model talks to this instead of holding
/// <see cref="CustomRichTextEditor.CustomRichTextEditor"/> instances.
/// </summary>
public interface IRichTextEditorAdapter
{
    Color? EditorBackgroundColor { get; }

    string DefaultFontFamily { get; }

    Task<RichTextSelectionSnapshot?> GetStoredSelectionAsync();

    Task<RichTextSelectionSnapshot?> GetSelectionAsync();

    Task<RichTextSelectionSnapshot?> CaptureDocumentAsync();

    Task<string?> GetCurrentHtmlAsync();

    Task<RichTextReplaceResult> ReplaceAsync(
        RichTextSelectionSnapshot snapshot,
        string content,
        RichTextContentType contentType);

    Task<RichTextReplaceResult> RestoreReplacementAsync(RichTextReplaceReceipt receipt);

    void Undo();

    void Redo();

    void ToggleBold();

    void ToggleItalic();

    void ToggleUnderline();

    void ToggleStrikethrough();

    void ApplyTextColor(Color color);

    void ApplyHighlightColor(Color color);

    void ApplyFontFamily(string fontFamily);

    void ApplyParagraphFormat(Syncfusion.Maui.RichTextEditor.RichTextEditorParagraphFormat format);

    void ToggleBulletList();

    void ToggleNumberedList();

    void InsertTable(int rows, int columns);

    /// <summary>
    /// Loads HTML as a programmatic update. The implementation must not treat
    /// the resulting value notification as a user edit.
    /// </summary>
    void LoadContent(string html);
}
