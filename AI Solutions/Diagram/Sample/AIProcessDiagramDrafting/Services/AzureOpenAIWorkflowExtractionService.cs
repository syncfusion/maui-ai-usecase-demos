using System.ClientModel;
using System.Text.Json;
using Azure;
using Azure.AI.OpenAI;
using AIProcessDiagramDrafting.Models;
using OpenAI.Chat;

namespace AIProcessDiagramDrafting.Services;

public sealed class AzureOpenAIWorkflowExtractionService : AzureBaseService, IWorkflowExtractionService
{
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

        var client = CreateClient();
        var chatClient = client.GetChatClient(DeploymentName);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(SystemPrompt),
            new UserChatMessage(processDescription)
        };

        var options = new ChatCompletionOptions
        {
            Temperature = 0f,
            ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat(),
            MaxOutputTokenCount = 2048
        };

        ClientResult<ChatCompletion> result;
        try
        {
            result = await chatClient.CompleteChatAsync(messages, options, cancellationToken);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Azure OpenAI request failed: {ex.Message}", ex);
        }
        catch (ClientResultException ex)
        {
            throw new InvalidOperationException($"Azure OpenAI request failed: {ex.Message}", ex);
        }

        var completion = result.Value;
        var content = string.Concat(completion.Content.Select(content => content.Text ?? string.Empty));
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
