using System.Net.Http;
using System.Text;
using System.Text.Json;
using AIProcessDiagramDrafting.Models;

namespace AIProcessDiagramDrafting.Services;

public sealed class AzureOpenAIWorkflowExtractionService : AzureBaseService, IWorkflowExtractionService
{
    private static readonly HttpClient HttpClient = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const string SystemPrompt = """
        You extract a process workflow from a short business description.
        Return JSON only.
        Do not include markdown, code fences, or explanations.
        The response must match this shape:
        {
          "title": "string",
          "nodes": [{ "id": "string", "type": "start|process|decision|end", "label": "string" }],
          "connectors": [{ "id": "string", "sourceId": "string", "targetId": "string", "label": "string" }],
          "warnings": ["string"]
        }
        Do not generate coordinates.
        Do not add unsupported workflow steps.
        """;

    public async Task<WorkflowResponse> ExtractAsync(string processDescription, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(processDescription);

        var request = new HttpRequestMessage(HttpMethod.Post, GetChatCompletionsUri());
        request.Headers.TryAddWithoutValidation("api-key", ApiKey);

        var payload = new
        {
            model = DeploymentName,
            messages = new object[]
            {
                new { role = "system", content = SystemPrompt },
                new { role = "user", content = processDescription }
            },
            temperature = 0f,
            max_completion_tokens = 2048,
            response_format = new { type = "json_object" }
        };

        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await HttpClient.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Azure OpenAI request failed: {ex.Message}", ex);
        }

        using (response)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Azure OpenAI request failed: HTTP {(int)response.StatusCode} ({response.ReasonPhrase}): {responseBody}");
            }

            using var responseDocument = JsonDocument.Parse(responseBody);
            var content = responseDocument.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;

            var json = ExtractJsonPayload(content);

            try
            {
                return JsonSerializer.Deserialize<WorkflowResponse>(json, JsonOptions)
                    ?? throw new InvalidOperationException("Azure OpenAI returned an empty workflow response.");
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Azure OpenAI returned an invalid workflow response.", ex);
            }
        }
    }

    private static string ExtractJsonPayload(string content)
    {
        var cleaned = CleanJson(content);
        var startIndex = cleaned.IndexOf('{');
        var endIndex = cleaned.LastIndexOf('}');

        if (startIndex >= 0 && endIndex > startIndex)
        {
            return cleaned[startIndex..(endIndex + 1)];
        }

        return cleaned;
    }

    private static string CleanJson(string content)
    {
        content = content.Trim();

        if (content.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            content = content[7..];
        }

        if (content.StartsWith("```", StringComparison.Ordinal))
        {
            content = content[3..];
        }

        if (content.EndsWith("```", StringComparison.Ordinal))
        {
            content = content[..^3];
        }

        return content.Trim();
    }
}
