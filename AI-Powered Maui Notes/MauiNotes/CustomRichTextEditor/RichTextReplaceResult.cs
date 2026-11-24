namespace CustomRichTextEditor;

/// <summary>
/// Describes the outcome of a range-replacement or caret-insertion operation.
/// </summary>
public sealed class RichTextReplaceResult
{
    /// <summary>Gets a value indicating whether the content was applied.</summary>
    public bool Succeeded { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the operation was rejected because the
    /// document changed after the snapshot was captured.
    /// </summary>
    public bool IsStaleSelection { get; private set; }

    /// <summary>Gets the error message when the operation failed, otherwise null.</summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>Gets the revision of the document after the operation.</summary>
    public long DocumentRevision { get; private set; }

    /// <summary>
    /// Gets a receipt that can restore the replaced range if the document has
    /// not changed since this operation.
    /// </summary>
    public RichTextReplaceReceipt? UndoReceipt { get; private set; }

    /// <summary>Creates a successful result.</summary>
    public static RichTextReplaceResult Success(long revision) =>
        new() { Succeeded = true, DocumentRevision = revision };

    internal static RichTextReplaceResult Success(
        long revision,
        RichTextReplaceReceipt? undoReceipt) =>
        new()
        {
            Succeeded = true,
            DocumentRevision = revision,
            UndoReceipt = undoReceipt
        };

    /// <summary>Creates a stale-selection rejection result.</summary>
    public static RichTextReplaceResult Stale(long revision) =>
        new() { Succeeded = false, IsStaleSelection = true, DocumentRevision = revision };

    /// <summary>Creates a failed result with a message.</summary>
    public static RichTextReplaceResult Failure(string message) =>
        new() { Succeeded = false, ErrorMessage = message };

    /// <inheritdoc/>
    public override string ToString() =>
        Succeeded ? $"Success (rev {DocumentRevision})" : $"Failed: {ErrorMessage}";
}
