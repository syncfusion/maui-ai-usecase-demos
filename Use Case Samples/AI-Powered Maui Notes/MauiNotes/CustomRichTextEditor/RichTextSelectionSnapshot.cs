namespace CustomRichTextEditor;

/// <summary>
/// An immutable, editor-owned capture of the selected range at a point in time.
/// Create it with <see cref="CustomRichTextEditor.CaptureSelectionAsync"/>.
/// The underlying editor positions are not part of the public contract.
/// </summary>
/// <remarks>
/// The snapshot is also revision-stamped. If the document changes after the
/// snapshot was taken, <c>ReplaceSelectionAsync</c> will reject it as stale
/// instead of replacing content at invalid positions.
/// </remarks>
public sealed class RichTextSelectionSnapshot
{
    /// <summary>The document revision at capture time. Internal: revision counter source.</summary>
    internal long InternalDocumentRevision { get; init; }

    /// <summary>The captured model-level selection start position.</summary>
    internal int InternalFrom { get; init; }

    /// <summary>The captured model-level selection end position.</summary>
    internal int InternalTo { get; init; }

    internal string InternalDocumentFingerprint { get; init; } = string.Empty;

    /// <summary>Gets the document revision at capture time.</summary>
    public long DocumentRevision => InternalDocumentRevision;

    /// <summary>Gets the selected plain text at capture time.</summary>
    public string SelectedText { get; init; } = string.Empty;

    /// <summary>
    /// Gets the selected content serialized as an HTML fragment at capture time.
    /// This is a standalone fragment contract — it is not the same value as
    /// <see cref="SelectedText"/>.
    /// </summary>
    public string SelectedHtml { get; init; } = string.Empty;

    /// <summary>Gets a value indicating whether a non-empty selection was captured.</summary>
    public bool HasSelection => InternalTo > InternalFrom;

    /// <summary>Internal factory used by the control. Not for application use.</summary>
    internal static RichTextSelectionSnapshot Create(
        long revision,
        int from,
        int to,
        string selectedText,
        string selectedHtml,
        string documentFingerprint) =>
        new()
        {
            InternalDocumentRevision = revision,
            InternalFrom = from,
            InternalTo = to,
            SelectedText = selectedText,
            SelectedHtml = selectedHtml,
            InternalDocumentFingerprint = documentFingerprint
        };

    /// <inheritdoc/>
    public override string ToString() =>
        $"Selection[{InternalFrom}..{InternalTo}] rev {DocumentRevision}: {SelectedText}";
}
