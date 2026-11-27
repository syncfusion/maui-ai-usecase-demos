namespace MauiNotes.Services.Editor;

public sealed record EditorFormatState(
    bool IsBold,
    bool IsItalic,
    bool IsUnderline,
    bool IsStrikethrough,
    bool IsHighlight,
    bool IsMonostyled,
    bool IsBulletList,
    bool IsNumberedList,
    string ParagraphFormat);

public enum EditorParagraphFormat
{
    Heading1,
    Heading2,
    Heading3,
    Paragraph
}
