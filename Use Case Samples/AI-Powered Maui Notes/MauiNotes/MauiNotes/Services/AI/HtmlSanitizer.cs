using System.Text.RegularExpressions;

namespace MauiNotes.Services.AI;

/// <summary>
/// Strips script, style, event-handler attributes, and javascript: URLs from
/// model HTML before it is inserted into the WebView-based editor.
/// </summary>
public static partial class HtmlSanitizer
{
    public static string Sanitize(string? html)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;

        string sanitized = ScriptBlock().Replace(html, string.Empty);
        sanitized = StyleBlock().Replace(sanitized, string.Empty);
        sanitized = EventAttribute().Replace(sanitized, string.Empty);
        sanitized = JavaScriptUrl().Replace(sanitized, "$1#");
        return sanitized;
    }

    [GeneratedRegex(@"<\s*script\b[^>]*>.*?<\s*/\s*script\s*>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ScriptBlock();

    [GeneratedRegex(@"<\s*style\b[^>]*>.*?<\s*/\s*style\s*>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex StyleBlock();

    [GeneratedRegex(@"\s+on[a-z]+\s*=\s*(""[^""]*""|'[^']*'|[^\s>]+)", RegexOptions.IgnoreCase)]
    private static partial Regex EventAttribute();

    [GeneratedRegex(@"(href|src)\s*=\s*[""']?\s*javascript:[^""'\s>]*", RegexOptions.IgnoreCase)]
    private static partial Regex JavaScriptUrl();
}
