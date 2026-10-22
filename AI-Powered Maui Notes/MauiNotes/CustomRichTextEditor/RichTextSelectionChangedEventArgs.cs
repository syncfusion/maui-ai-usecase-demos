namespace CustomRichTextEditor;

/// <summary>
/// Provides data for the
/// <see cref="global::CustomRichTextEditor.CustomRichTextEditor.SelectionChanged"/> event.
/// </summary>
public sealed class RichTextSelectionChangedEventArgs : EventArgs
{
    /// <summary>Gets the selected plain text, or an empty string.</summary>
    public string SelectedText { get; init; } = string.Empty;

    /// <summary>Gets a value indicating whether the selection is non-empty.</summary>
    public bool HasSelection { get; init; }

    /// <summary>Gets the model-level selection start position (internal use).</summary>
    internal int From { get; init; }

    /// <summary>Gets the model-level selection end position (internal use).</summary>
    internal int To { get; init; }

    internal static RichTextSelectionChangedEventArgs Create(
        string selectedText, int from, int to) =>
        new()
        {
            SelectedText = selectedText,
            HasSelection = to > from,
            From = from,
            To = to
        };
}
