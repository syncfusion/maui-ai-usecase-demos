using Ganss.Xss;

namespace MauiNotes.Services.AI;

public sealed class AiHtmlSanitizer
{
    private readonly HtmlSanitizer sanitizer = CreateSanitizer();

    public string Sanitize(string html) => sanitizer.Sanitize(html);

    private static HtmlSanitizer CreateSanitizer()
    {
        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedTags.Clear();
        sanitizer.AllowedTags.UnionWith(
        [
            "a", "h1", "h2", "h3", "h4", "h5", "h6", "p", "div", "ul", "ol", "li",
            "table", "thead", "tbody", "tr", "th", "td", "strong", "b", "em", "i", "u", "span",
            "br", "blockquote", "pre", "code", "del", "s", "strike"
        ]);
        sanitizer.AllowedAttributes.Clear();
        sanitizer.AllowedAttributes.UnionWith(["href", "title"]);
        sanitizer.UriAttributes.Clear();
        sanitizer.UriAttributes.Add("href");
        sanitizer.AllowedSchemes.Clear();
        sanitizer.AllowedSchemes.UnionWith(["http", "https", "mailto"]);
        return sanitizer;
    }
}
