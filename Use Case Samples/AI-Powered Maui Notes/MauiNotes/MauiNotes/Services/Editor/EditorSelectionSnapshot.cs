using CustomRichTextEditor;

namespace MauiNotes.Services.Editor;

public sealed class EditorSelectionSnapshot
{
    internal EditorSelectionSnapshot(RichTextSelectionSnapshot snapshot)
    {
        NativeSnapshot = snapshot;
    }

    internal RichTextSelectionSnapshot NativeSnapshot { get; }
    public long DocumentRevision => NativeSnapshot.DocumentRevision;
    public string SelectedText => NativeSnapshot.SelectedText;
    public string SelectedHtml => NativeSnapshot.SelectedHtml;
    public bool HasSelection => NativeSnapshot.HasSelection;
}

public sealed class EditorReplacementReceipt
{
    internal EditorReplacementReceipt(RichTextReplaceReceipt receipt)
    {
        NativeReceipt = receipt;
        AppliedRangeSnapshot = new EditorSelectionSnapshot(receipt.AppliedRangeSnapshot);
    }

    internal RichTextReplaceReceipt NativeReceipt { get; }
    public EditorSelectionSnapshot AppliedRangeSnapshot { get; }
}

public sealed record EditorReplaceResult(
    bool Succeeded,
    bool IsStaleSelection,
    string? ErrorMessage,
    EditorReplacementReceipt? UndoReceipt);
