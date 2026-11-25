using System.Net;
using System.Text.RegularExpressions;

namespace MauiNotes.Services.AI;

public static partial class HtmlSanitizer
{
    public static string Sanitize(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        string sanitized = ScriptPattern().Replace(html, string.Empty);
        sanitized = StylePattern().Replace(sanitized, string.Empty);
        sanitized = EventHandlerPattern().Replace(sanitized, string.Empty);
        sanitized = JavascriptUrlPattern().Replace(sanitized, "href=\"#\"");
        return sanitized.Trim();
    }

    [GeneratedRegex(@"<script\b[^>]*>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ScriptPattern();

    [GeneratedRegex(@"<style\b[^>]*>.*?</style>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex StylePattern();

    [GeneratedRegex(@"\s+on[a-z]+\s*=\s*(['""]).*?\1", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex EventHandlerPattern();

    [GeneratedRegex(@"(href|src)\s*=\s*(['""])\s*javascript:[^'""]*\2", RegexOptions.IgnoreCase)]
    private static partial Regex JavascriptUrlPattern();

    public static string ToPlainText(string html)
    {
        string withBreaks = Regex.Replace(
            html,
            @"</(?:p|h[1-6]|li|tr|div)\s*>|<br\s*/?>",
            "\n",
            RegexOptions.IgnoreCase);
        string withoutTags = Regex.Replace(withBreaks, "<[^>]+>", " ");
        return WebUtility.HtmlDecode(withoutTags).Trim();
    }
}
