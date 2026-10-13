using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;

namespace MauiNotes.ViewModels;

public partial class NotesViewModel
{
    private void UpdateNoteTitleFromEditor(string html)
    {
        if (SelectedNote is null)
            return;

        string title = ExtractFirstLineText(html);
        SelectedNote.Title = string.IsNullOrWhiteSpace(title) ? "New Note" : title;
        SelectedNote.PreviewText = ExtractPreviewText(html);
    }

    /// <summary>
    /// Matches a line-break marker that the editor may insert *inside* the
    /// still-open &lt;h1&gt; block while the user is typing (before it presses
    /// Enter/commits a new paragraph). SfRichTextEditor represents a soft or
    /// hard line break this way, so anything from this marker onwards must
    /// be treated as body content, never as part of the title.
    /// </summary>
    private static readonly Regex LineBreakMarker = new(
        @"<br\s*/?>|<div\b[^>]*>|\r\n|\r|\n",
        RegexOptions.IgnoreCase | RegexOptions.Singleline);

    /// <summary>
    /// Finds the first &lt;h1&gt; heading block in the content, wherever it
    /// appears (it may be preceded by an editor-injected wrapper such as a
    /// &lt;div&gt;). Returns null when no heading exists.
    /// </summary>
    private static Match FindTitleHeading(string html) =>
        Regex.Match(html, @"<h1\b[^>]*>(?<text>.*?)</h1>", RegexOptions.IgnoreCase | RegexOptions.Singleline);

    /// <summary>
    /// Splits the inner HTML of the title heading at the first line-break
    /// marker so that text typed after a line break (but before the editor
    /// has closed the &lt;h1&gt; tag and started a real &lt;p&gt;) is never
    /// included in the title.
    /// </summary>
    private static (string TitlePart, string RestPart) SplitHeadingContent(string headingInnerHtml)
    {
        Match lineBreak = LineBreakMarker.Match(headingInnerHtml);
        if (!lineBreak.Success)
            return (headingInnerHtml, string.Empty);

        string titlePart = headingInnerHtml[..lineBreak.Index];
        string restPart = headingInnerHtml[(lineBreak.Index + lineBreak.Length)..];
        return (titlePart, restPart);
    }

    /// <summary>
    /// Extracts the note title strictly from the first line of the editor
    /// content. The first line is expected to be wrapped in an &lt;h1&gt; tag;
    /// if present, its text (up to the first line break) is used verbatim as
    /// the title. Content from the second line onwards - whether it is a
    /// separate block after the first &lt;h1&gt;, or a line break still inside
    /// the open &lt;h1&gt; while the user is typing - is treated as the note
    /// body and is never inspected for title purposes.
    /// </summary>
    private static string ExtractFirstLineText(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        Match heading = FindTitleHeading(html);

        string rawFirstLine;
        if (heading.Success)
        {
            (rawFirstLine, _) = SplitHeadingContent(heading.Groups["text"].Value);
        }
        else
        {
            // Fallback for content that does not yet start with <h1> (e.g.
            // legacy notes): take only the very first block-level element,
            // excluding wrapper containers such as <div> which may span the
            // whole document and would otherwise swallow the body too.
            Match firstBlock = Regex.Match(
                html,
                @"<(?<tag>h[1-6]|p|li)\b[^>]*>(?<text>.*?)</\k<tag>>",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            rawFirstLine = firstBlock.Success
                ? SplitHeadingContent(firstBlock.Groups["text"].Value).TitlePart
                : html.Split(new[] { '\r', '\n' }, StringSplitOptions.None)[0];
        }

        return CleanPlainText(rawFirstLine);
    }

    /// <summary>
    /// Extracts a plain-text preview of the note body: any text that follows
    /// the title, whether that is a line break still inside the open
    /// &lt;h1&gt; block, or the first block-level element that comes after a
    /// closed &lt;h1&gt; title block. Returns an empty string when there is
    /// no body content yet (the UI shows a placeholder such as "No
    /// additional text" in that case).
    /// </summary>
    private static string ExtractPreviewText(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        Match heading = FindTitleHeading(html);
        if (!heading.Success)
            return CleanPlainText(html);

        (_, string restOfHeading) = SplitHeadingContent(heading.Groups["text"].Value);
        if (!string.IsNullOrWhiteSpace(restOfHeading))
            return CleanPlainText(restOfHeading);

        string remainder = html[(heading.Index + heading.Length)..];

        Match firstBlock = Regex.Match(
            remainder,
            @"<(?<tag>h[1-6]|p|li)\b[^>]*>(?<text>.*?)</\k<tag>>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        string rawText = firstBlock.Success ? firstBlock.Groups["text"].Value : remainder;
        return CleanPlainText(rawText);
    }
    private static string CleanPlainText(string rawHtml)
    {
        string plainText = Regex.Replace(rawHtml, "<[^>]+>", " ");
        plainText = WebUtility.HtmlDecode(plainText);
        return Regex.Replace(plainText, @"\s+", " ").Trim();
    }

    internal static string NormalizeEscapedHtml(string content)
    {
        if (string.IsNullOrEmpty(content) ||
            content.IndexOf("u003c", StringComparison.OrdinalIgnoreCase) < 0)
        {
            return content;
        }

        MatchCollection escapedTags = Regex.Matches(
            content,
            @"\\+u003[cC]/?(?:h[1-6]|p|div|ul|ol|li|table|thead|tbody|tr|th|td)\b",
            RegexOptions.IgnoreCase);
        bool hasPairedEscapedTags = Regex.IsMatch(
            content,
            @"\\+u003[cC](?<tag>h[1-6]|p|div|ul|ol|li|table|thead|tbody|tr|th|td)\b" +
            @"[^>]*(?:>|\\+u003[eE]).*?\\+u003[cC]/\k<tag>\s*(?:>|\\+u003[eE])",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        bool startsWithEscapedTag = Regex.IsMatch(
            content,
            @"^\s*\\+u003[cC]/?(?:h[1-6]|p|div|ul|ol|li|table|thead|tbody|tr|th|td)\b",
            RegexOptions.IgnoreCase);
        if (escapedTags.Count < 2 || !hasPairedEscapedTags || !startsWithEscapedTag)
            return content;

        string normalized = Regex.Replace(content, @"\\+u003[cC]", "<", RegexOptions.IgnoreCase);
        return Regex.Replace(normalized, @"\\+u003[eE]", ">", RegexOptions.IgnoreCase);
    }

    private static void LogHtmlBoundary(string stage, string? content, Guid? noteId = null)
    {
        if (content is null)
        {
            Debug.WriteLine($"[HTML] {stage}; noteId={noteId?.ToString() ?? "(none)"}; value=null.");
            return;
        }

        Debug.WriteLine(
            $"[HTML] {stage}; noteId={noteId?.ToString() ?? "(none)"}; length={content.Length}.");
    }
}
