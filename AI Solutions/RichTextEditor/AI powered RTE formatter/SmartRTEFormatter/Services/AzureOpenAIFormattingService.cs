using System.Text;
using System.Text.Json;

namespace SmartRTEFormatter;

/// <summary>
/// Sends text to an Azure OpenAI chat completions endpoint and converts the response to HTML.
/// </summary>
public sealed class AzureOpenAIFormattingService : IAIFormattingService
{
    public static readonly string BaseEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? string.Empty;
    public static readonly string ApiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? string.Empty;
    public static readonly string DeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? string.Empty;
    private readonly HttpClient httpClient = new();

    private const string SystemPrompt =
    """
    You are an enterprise-grade document formatting assistant.

    Transform any unstructured or semi-structured content into a professional documentation structure.

    Your ONLY job is to move text into the right structural slot. You must NEVER change the wording.

    Rules:

    1. Return ONLY valid JSON.
    2. Do not return HTML.
    3. Do not return Markdown.
    4. Do not return explanations.
    5. Preserve every piece of source information exactly once.
    6. Do not invent facts or missing content.
    7. Create concise headings that describe the content; generated headings are structure, not rewritten source content.
    8. Copy each source sentence, phrase, or item character-for-character exactly as written, including grammar, spelling, punctuation, capitalization, word order, and tense, even if it looks incorrect or informal.
    9. Do not rewrite, paraphrase, summarize, correct grammar, fix spelling, change tense, substitute words, or improve the user's text in any way.
    10. Do not merge two source sentences into one, and do not split one source sentence into two in a way that changes its wording.
    11. Only reorganize the source content into the JSON categories below; the reorganization itself is the only allowed change.
    12. Use paragraphs for prose, explanations, descriptions, standalone statements, commands, code, configuration, quoted text, and table data when no dedicated field exists.
    13. Use bulletItems for unordered facts, features, requirements, highlights, metrics, keywords, and items.
    14. Use numberedItems only when the source expresses an ordered procedure, sequence, or steps.
    15. Use actionItems only for explicit tasks, responsibilities, follow-ups, or work to be completed.
    16. Preserve code, commands, configuration, SQL, JSON, XML, markup, paths, identifiers, and code-like text character-for-character inside paragraphs or list items.
    17. Preserve quoted text verbatim inside paragraphs or bulletItems.
    18. Keep related content together in one section per logical topic.
    19. Never place the same source sentence or item in more than one output array.
    20. Use empty arrays when a category does not apply.

    Return exactly this JSON structure:

    {
      "title":"string",
      "sections":[
        {
          "heading":"string",
          "paragraphs":["string"],
          "bulletItems":["string"],
          "numberedItems":["string"],
          "actionItems":["string"]
        }
      ]
    }
    """;



    /// <summary>
    /// Formats text using the configured Azure OpenAI deployment.
    /// </summary>
    /// <param name="content">The unstructured text to organize.</param>
    /// <returns>The AI-generated document rendered as HTML.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the Azure OpenAI endpoint or API key has not been configured, or when the
    /// service returns no usable formatted content.
    /// </exception>
    /// <exception cref="HttpRequestException">
    /// Thrown when the Azure OpenAI request fails.
    /// </exception>
    /// <exception cref="JsonException">
    /// Thrown when the service response does not contain a valid formatted document.
    /// </exception>
    public async Task<FormattingResult> FormatAsync(string content)
    {
        if (BaseEndpoint.Contains("YOUR_RESOURCE", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(ApiKey) ||
            ApiKey == "YOUR_API_KEY")
        {
            throw new InvalidOperationException(
                "Configure the Azure OpenAI endpoint and API key before formatting content.");
        }

        var requestUri = $"{BaseEndpoint.TrimEnd('/')}/chat/completions";

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            requestUri);

        request.Headers.Add("api-key", ApiKey);

        request.Content = new StringContent(
            JsonSerializer.Serialize(new
            {
                model = DeploymentName,
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = SystemPrompt
                    },
                    new
                    {
                        role = "user",
                        content =
                        $"""
                        Convert the following content into a structured documentation document.

                        Requirements:

                        - Create logical sections.
                        - Copy every source sentence, phrase, and item exactly as written: same wording, grammar, punctuation, capitalization, spelling, word order, and tense.
                        - Do not rewrite, paraphrase, summarize, correct grammar, fix spelling, or improve the user's text in any way.
                        - Do not merge or split source sentences in a way that changes their wording.
                        - Only restructure the content into title, sections, paragraphs, bulletItems, numberedItems, and actionItems; reorganization is the only allowed change.
                        - Use paragraphs for prose, explanations, descriptions, commands, code, configuration, quoted text, and table data.
                        - Use bulletItems for unordered content, features, requirements, highlights, metrics, and keywords.
                        - Use numberedItems only for ordered procedures or sequences.
                        - Use actionItems only for explicit tasks or responsibilities.
                        - Keep related content together, do not duplicate source items, and do not invent missing information.
                        - Use empty arrays when a category does not apply.
                        - Return valid JSON only with exactly the requested schema.

                        Content:

                        {content}
                        """
                    }
                },
                max_completion_tokens = 2500,
                reasoning_effort = "minimal"
            }),
            Encoding.UTF8,
            "application/json");

        using var response = await httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode}: {responseBody}");
        }

        using var responseDocument =
            JsonDocument.Parse(responseBody);

        var structuredContent =
            responseDocument.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

        if (string.IsNullOrWhiteSpace(structuredContent))
        {
            throw new InvalidOperationException(
                "Azure OpenAI returned no formatted content.");
        }

        var cleanedJson = RemoveCodeFence(structuredContent);

        var formattedDocument =
            JsonSerializer.Deserialize<FormattedDocument>(
                cleanedJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (formattedDocument is null)
        {
            throw new JsonException(
                "Azure OpenAI returned an invalid structured document.");
        }

        return new FormattingResult
        {
            HtmlContent = CreateHtml(formattedDocument)
        };
    }

    private static string RemoveCodeFence(string content)
    {
        return content
            .Replace("```json", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("```", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim();
    }

    private static string CreateHtml(
        FormattedDocument document)
    {
        var html = new StringBuilder();

        AppendHeading(html, document.Title, 1);

        foreach (var section in document.Sections)
        {
            AppendHeading(html, section.Heading, 2);

            AppendParagraphs(html, section.Paragraphs);

            if (section.BulletItems.Count > 0)
            {
                AppendSectionList(
                    html,
                    section.BulletItems,
                    "ul");
            }

            if (section.NumberedItems.Count > 0)
            {
                AppendSectionList(
                    html,
                    section.NumberedItems,
                    "ol");
            }

            if (section.ActionItems.Count > 0)
            {
                AppendSectionList(
                    html,
                    section.ActionItems,
                    "ul",
                    "action-items");
            }
        }

        return html.ToString();
    }

    private static void AppendHeading(
        StringBuilder html,
        string text,
        int level)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        html.Append(
            $"<h{level}>{System.Net.WebUtility.HtmlEncode(text)}</h{level}>");
    }

    private static void AppendParagraphs(
        StringBuilder html,
        IEnumerable<string> paragraphs)
    {
        foreach (var paragraph in paragraphs
                     .Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            html.Append(
                $"<p>{System.Net.WebUtility.HtmlEncode(paragraph)}</p>");
        }
    }

    private static void AppendSectionList(
        StringBuilder html,
        IEnumerable<string> items,
        string tag,
        string? cssClass = null)
    {
        var values = items
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        if (values.Count == 0)
            return;

        if (string.IsNullOrWhiteSpace(cssClass))
        {
            html.Append($"<{tag}>");
        }
        else
        {
            html.Append($"<{tag} class=\"{cssClass}\">");
        }

        foreach (var item in values)
        {
            html.Append(
                $"<li>{System.Net.WebUtility.HtmlEncode(item)}</li>");
        }

        html.Append($"</{tag}>");
    }
}