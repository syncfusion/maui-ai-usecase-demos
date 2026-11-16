using System.Text.Json;
using Azure;
using Azure.AI.OpenAI;
using AIProcessDiagramDrafting.Models;

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

        var messages = new List<OpenAI.Chat.ChatMessage>
        {
            new OpenAI.Chat.SystemChatMessage(SystemPrompt),
            new OpenAI.Chat.UserChatMessage(processDescription)
        };

        var options = new OpenAI.Chat.ChatCompletionOptions
        {
            Temperature = 0f,
            ResponseFormat = OpenAI.Chat.ChatResponseFormat.CreateJsonObjectFormat(),
            MaxOutputTokenCount = 2048
        };

        try
        {
            var completion = await chatClient.CompleteChatAsync(messages, options, cancellationToken);
            var content = string.Concat(completion.Value.Content.Select(content => content.Text ?? string.Empty));
            var json = ExtractJsonPayload(content);

            return JsonSerializer.Deserialize<WorkflowResponse>(json, JsonOptions)
                ?? throw new InvalidOperationException("Azure OpenAI returned an empty workflow response.");
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Azure OpenAI request failed: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Azure OpenAI returned an invalid workflow response.", ex);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException($"Azure OpenAI request failed: {ex.Message}", ex);
        }
    }

    private static string ExtractJsonPayload(string content)
    {
        var trimmed = content.Trim();
        var startIndex = trimmed.IndexOf('{');
        var endIndex = trimmed.LastIndexOf('}');

        if (startIndex >= 0 && endIndex > startIndex)
        {
            return trimmed[startIndex..(endIndex + 1)];
        }

        return trimmed;
    }
}
