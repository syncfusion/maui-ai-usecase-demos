using Microsoft.Maui.Graphics;

namespace MauiNotes.Services.Editor;

public interface IRichTextEditorController
{
    Task<EditorSelectionSnapshot?> GetStoredSelectionAsync();
    Task<EditorSelectionSnapshot?> CaptureSelectionAsync();
    Task<EditorSelectionSnapshot?> CaptureDocumentAsync();
    Task<EditorReplaceResult> ReplaceAsync(
        EditorSelectionSnapshot snapshot,
        string content,
        EditorContentType contentType);
    Task<EditorReplaceResult> RestoreAsync(EditorReplacementReceipt receipt);
    Task<string?> GetCurrentHtmlAsync();
    Color EditorBackgroundColor { get; }
    string DefaultFontFamily { get; }
    void Undo();
    void Redo();
    void ToggleBold();
    void ToggleItalic();
    void ToggleUnderline();
    void ToggleStrikethrough();
    void ToggleBulletList();
    void ToggleNumberedList();
    void ApplyTextColor(Color color);
    void ApplyHighlightColor(Color color);
    void ApplyFontFamily(string fontFamily);
    void ApplyParagraphFormat(EditorParagraphFormat format);
    void InsertTable(int rows, int columns);
}
