using CustomRichTextEditor;
using Syncfusion.Maui.RichTextEditor;
using EditorControl = CustomRichTextEditor.CustomRichTextEditor;

namespace MauiNotes.Services.Editor;

/// <summary>
/// View-owned editor surface. The view model never holds the control.
/// </summary>
public interface IRichTextEditorAdapter
{
    bool IsAttached { get; }

    Color EditorBackgroundColor { get; }

    void Attach(EditorControl editor);

    void Detach(EditorControl editor);

    void Undo();

    void Redo();

    void ToggleBold();

    void ToggleItalic();

    void ToggleUnderline();

    void ToggleStrikethrough();

    void ToggleBulletList();

    void ToggleNumberedList();

    void ApplyParagraphFormat(RichTextEditorParagraphFormat format);

    void ApplyFontFamily(string fontFamily);

    void ApplyTextColor(Color color);

    void ApplyHighlightColor(Color color);

    void InsertTable(int rows, int columns);

    Task<RichTextSelectionSnapshot?> GetStoredSelectionAsync();

    Task<RichTextSelectionSnapshot?> CaptureSelectionAsync();

    Task<RichTextSelectionSnapshot?> CaptureDocumentAsync();

    Task<string?> GetCurrentHtmlAsync();

    Task<RichTextReplaceResult> ReplaceSelectionAsync(
        RichTextSelectionSnapshot snapshot,
        string html,
        RichTextContentType contentType);

    Task<RichTextReplaceResult> ReplaceDocumentAsync(
        string html,
        RichTextContentType contentType);

    Task<RichTextReplaceResult> RestoreReplacementAsync(RichTextReplaceReceipt receipt);


}
