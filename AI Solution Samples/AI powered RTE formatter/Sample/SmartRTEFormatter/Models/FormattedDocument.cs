using System.Text.Json.Serialization;

namespace SmartRTEFormatter.Models;

public sealed class FormattedDocument
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("sections")]
    public List<FormattedSection> Sections { get; set; } = new();
}

public sealed class FormattedSection
{
    [JsonPropertyName("heading")]
    public string Heading { get; set; } = string.Empty;

    [JsonPropertyName("paragraphs")]
    public List<string> Paragraphs { get; set; } = new();

    [JsonPropertyName("bulletItems")]
    public List<string> BulletItems { get; set; } = new();

    [JsonPropertyName("numberedItems")]
    public List<string> NumberedItems { get; set; } = new();

    [JsonPropertyName("actionItems")]
    public List<string> ActionItems { get; set; } = new();

    [JsonPropertyName("keywords")]
    public List<string> Keywords { get; set; } = new();

    [JsonPropertyName("notes")]
    public List<string> Notes { get; set; } = new();

    [JsonPropertyName("quotes")]
    public List<string> Quotes { get; set; } = new();

    [JsonPropertyName("codeSnippets")]
    public List<CodeSnippet> CodeSnippets { get; set; } = new();

    [JsonPropertyName("tables")]
    public List<FormattedTable> Tables { get; set; } = new();
}

public sealed class CodeSnippet
{
    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
}

public sealed class FormattedTable
{
    [JsonPropertyName("headers")]
    public List<string> Headers { get; set; } = new();

    [JsonPropertyName("rows")]
    public List<List<string>> Rows { get; set; } = new();
}

public sealed class FormattingResult
{
    public required string HtmlContent { get; init; }

    public required string StructuredJson { get; init; }
}
