using System.Text.RegularExpressions;
using Markdig;

namespace MauiNotes.Services.AI;

public enum AiResponseContentType
{
    PlainText,
    Html
}

public sealed record AiFormattedResponse(string Content, AiResponseContentType ContentType);

public sealed class AiResponseFormatter
{
    private static readonly MarkdownPipeline MarkdownPipeline =
        new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

    private static readonly Regex HtmlTagPattern = new(
        @"</?[a-z][^>]*>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex MarkdownPattern = new(
        @"(?m)^\s{0,3}(?:#{1,6}\s|[-*+]\s+|\d+[.)]\s+|>\s?|```|~~~)|" +
        @"(?:\*\*|__|~~|`|\[[^\]]+\]\([^)]+\)|" +
        @"(?<!\*)\*(?!\s).+?(?<!\s)\*(?!\*)|(?<![\w_])_(?!\s).+?(?<!\s)_(?![\w_]))",
        RegexOptions.Compiled);

    private readonly AiHtmlSanitizer sanitizer;

    public AiResponseFormatter(AiHtmlSanitizer sanitizer)
    {
        this.sanitizer = sanitizer;
    }

    public AiFormattedResponse Format(string response)
    {
        ArgumentNullException.ThrowIfNull(response);
        string content = StripOuterMarkdownFence(NormalizeEscapedHtml(response)).Trim();
        if (content.Length == 0)
            return new AiFormattedResponse(string.Empty, AiResponseContentType.PlainText);

        if (HtmlTagPattern.IsMatch(content))
            return new AiFormattedResponse(sanitizer.Sanitize(content), AiResponseContentType.Html);

        if (MarkdownPattern.IsMatch(content))
        {
            string html = Markdown.ToHtml(content, MarkdownPipeline);
            return new AiFormattedResponse(sanitizer.Sanitize(html), AiResponseContentType.Html);
        }

        return new AiFormattedResponse(content, AiResponseContentType.PlainText);
    }

    public static string NormalizeEscapedHtml(string content)
    {
        if (string.IsNullOrEmpty(content) ||
            content.IndexOf("u003c", StringComparison.OrdinalIgnoreCase) < 0)
        {
            return content;
        }

        const string tagPattern =
            "(?:h[1-6]|p|div|ul|ol|li|table|thead|tbody|tr|th|td|strong|b|em|i|u|span|a|br|blockquote|pre|code|del|s)";
        MatchCollection escapedTags = Regex.Matches(
            content,
            @"\\+u003[cC]/?" + tagPattern + @"\b",
            RegexOptions.IgnoreCase);
        bool hasPairedEscapedTags = Regex.IsMatch(
            content,
            @"\\+u003[cC](?<tag>" + tagPattern + @")\b" +
            @"[^>]*(?:>|\\+u003[eE]).*?\\+u003[cC]/\k<tag>\s*(?:>|\\+u003[eE])",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        bool startsWithEscapedTag = Regex.IsMatch(
            content,
            @"^\s*\\+u003[cC]/?" + tagPattern + @"\b",
            RegexOptions.IgnoreCase);
        if (escapedTags.Count < 2 || !hasPairedEscapedTags || !startsWithEscapedTag)
            return content;

        string normalized = Regex.Replace(content, @"\\+u003[cC]", "<", RegexOptions.IgnoreCase);
        return Regex.Replace(normalized, @"\\+u003[eE]", ">", RegexOptions.IgnoreCase);
    }

    private static string StripOuterMarkdownFence(string content)
    {
        string trimmed = content.Trim();
        if (!trimmed.StartsWith("```", StringComparison.Ordinal) || trimmed.Length < 7)
            return content;

        int firstNewline = trimmed.IndexOf('\n');
        int lastFence = trimmed.LastIndexOf("```", StringComparison.Ordinal);
        if (firstNewline < 0 || lastFence <= firstNewline)
            return content;

        return trimmed[(firstNewline + 1)..lastFence].TrimEnd();
    }
}
