namespace CustomRichTextEditor;

/// <summary>
/// A guarded receipt for restoring the content replaced by one editor
/// transaction. Restoration is rejected if the document has since changed.
/// </summary>
public sealed class RichTextReplaceReceipt
{
    internal RichTextReplaceReceipt(
        RichTextSelectionSnapshot appliedRange,
        string originalHtml)
    {
        AppliedRange = appliedRange;
        OriginalHtml = originalHtml;
    }

    internal RichTextSelectionSnapshot AppliedRange { get; }
    public RichTextSelectionSnapshot AppliedRangeSnapshot => AppliedRange;
    internal string OriginalHtml { get; }
}
