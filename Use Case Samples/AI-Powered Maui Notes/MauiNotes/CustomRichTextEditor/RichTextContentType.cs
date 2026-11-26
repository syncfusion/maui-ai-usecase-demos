namespace CustomRichTextEditor;

/// <summary>
/// Specifies the format of content supplied to
/// <see cref="CustomRichTextEditor.ReplaceSelectionAsync"/> and
/// <see cref="CustomRichTextEditor.InsertAtCaretAsync"/>.
/// </summary>
public enum RichTextContentType
{
    /// <summary>Content is plain text. It is inserted as text; no markup is interpreted.</summary>
    PlainText,

    /// <summary>
    /// Content is an HTML fragment. It is parsed against the editor schema,
    /// sanitized, and inserted as editor nodes. Supported tags: p, h1-h6,
    /// b/strong, i/em, u, s/strike, ul/ol/li, a, br, blockquote, code, pre,
    /// img, and table/tr/th/td. Script tags, event-handler attributes, and
    /// javascript: URLs are removed before parsing.
    /// </summary>
    Html
}
