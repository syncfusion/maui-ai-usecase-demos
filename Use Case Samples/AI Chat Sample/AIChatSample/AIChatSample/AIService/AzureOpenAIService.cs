using System.Text.Json;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;


namespace AIChatSample;

/// <summary>
/// Real Azure OpenAI implementation of <see cref="IAIService"/>.
///
/// Endpoints, keys, and deployment names are read from environment
/// variables (which may be supplied through user-secrets in dev) — never
/// from source.
/// </summary>
public sealed class AzureOpenAIService : IAIService
{

    private readonly ChatClient _chatClient;
    private readonly TimeSpan _requestTimeout;

    private static readonly JsonSerializerOptions AgentDraftJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    /// <summary>
    /// Test-friendly constructor that accepts credentials explicitly. The
    /// production host (<c>MauiProgram</c>) reads the values from the
    /// <c>AZURE_OPENAI_ENDPOINT</c> / <c>_KEY</c> / <c>_DEPLOYMENT</c>
    /// environment variables so secrets never live in source.
    /// </summary>
    /// <param name="endpoint">Azure OpenAI endpoint URL.</param>
    /// <param name="apiKey">Azure OpenAI API key.</param>
    /// <param name="deployment">Chat-completions deployment name.</param>
    /// <param name="timeoutSeconds">Hard request timeout in seconds; defaults to 100.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when any of <paramref name="endpoint"/>, <paramref name="apiKey"/>,
    /// or <paramref name="deployment"/> is null or whitespace.
    /// </exception>
    public AzureOpenAIService(string? endpoint, string? apiKey, string? deployment, int timeoutSeconds = 100)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new InvalidOperationException(
                "AZURE_OPENAI_ENDPOINT is not set. Configure it via environment variables or user-secrets before using AzureOpenAIService.");
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "AZURE_OPENAI_KEY is not set. Configure it via environment variables or user-secrets before using AzureOpenAIService.");
        }

        if (string.IsNullOrWhiteSpace(deployment))
        {
            throw new InvalidOperationException(
                "AZURE_OPENAI_DEPLOYMENT is not set. Configure it via environment variables or user-secrets before using AzureOpenAIService.");
        }

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri) ||
            endpointUri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException(
                $"AZURE_OPENAI_ENDPOINT must be an https:// URL (got '{endpoint}'). Sending the API key over plain http is not supported.");
        }

        var client = new AzureOpenAIClient(endpointUri, new AzureKeyCredential(apiKey));
        _chatClient = client.GetChatClient(deployment);
        _requestTimeout = TimeSpan.FromSeconds(Math.Max(1, timeoutSeconds));
    }

    public async Task<ContextSuggestionResponse?> GetResponseAsync(
        string prompt,
        string? systemContext = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return new ContextSuggestionResponse { Answer = string.Empty };
        }

        try
        {
            // Combine the caller's cancellation with our hard timeout so the
            // UI never blocks indefinitely.
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(_requestTimeout);

            // The agent's Behaviour/Instructions is supplied as the
            // system prompt so per-agent instructions actually reach
            // the model (otherwise only the user message is sent and
            // every agent behaves identically).
            var messages = new List<ChatMessage>();
            if (!string.IsNullOrWhiteSpace(systemContext))
            {
                messages.Add(new SystemChatMessage(systemContext));
            }
            messages.Add(new UserChatMessage(prompt));

            ChatCompletion completion = await _chatClient
                .CompleteChatAsync(messages, cancellationToken: cts.Token)
                .ConfigureAwait(false);

            if (ct.IsCancellationRequested)
            {
                return null;
            }

            if (completion?.Content is null || completion.Content.Count == 0)
            {
                return new ContextSuggestionResponse { Answer = string.Empty };
            }

            string result = completion.Content[0].Text ?? string.Empty;
            return ParseResponse(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return null;
        }
        catch (OperationCanceledException)
        {
            return new ContextSuggestionResponse
            {
                Answer = $"Request timed out after {_requestTimeout.TotalSeconds:n0} seconds. Check your internet connection and Azure endpoint configuration."
            };
        }
        catch (Exception ex)
        {
            return new ContextSuggestionResponse
            {
                Answer = $"Error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Asks the model to return a strict JSON object with the new agent's
    /// name, description, and long-form instructions. The model may still
    /// wrap the JSON in markdown fences; <see cref="ParseAgentDraft"/>
    /// is forgiving about that.
    /// </summary>
    public async Task<AgentDraft?> DraftAgentAsync(string prompt, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(prompt)) return null;

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(_requestTimeout);

            const string systemPrompt =
                "You generate JSON describing a new specialist AI agent. " +
                "Respond with a single JSON object and NOTHING else (no prose, no markdown fences). " +
                "Schema: { \"name\": string (≤60 chars, Title Case), \"description\": string (one sentence, ≤180 chars), " +
                "\"instructions\": string (system-prompt style, 1–3 short paragraphs in second person) }.";

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(prompt),
            };

            ChatCompletion completion = await _chatClient
                .CompleteChatAsync(messages, cancellationToken: cts.Token)
                .ConfigureAwait(false);

            if (ct.IsCancellationRequested) return null;
            var text = completion?.Content?.FirstOrDefault()?.Text;
            return ParseAgentDraft(text);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return null;
        }
        catch
        {
            // Azure path failure is silent here — the caller can fall back
            // to a local mock-style draft.
            return null;
        }
    }

    internal static AgentDraft? ParseAgentDraft(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var json = ExtractFirstJsonObject(raw);
        if (string.IsNullOrEmpty(json)) return null;
        try
        {
            var draft = JsonSerializer.Deserialize<AgentDraft>(json, AgentDraftJsonOptions);
            if (draft is null) return null;
            if (string.IsNullOrWhiteSpace(draft.Name) &&
                string.IsNullOrWhiteSpace(draft.Description) &&
                string.IsNullOrWhiteSpace(draft.Instructions))
            {
                return null;
            }
            return draft;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Tries to extract a JSON object from the model's reply. The model is
    /// asked to return JSON only, but it sometimes wraps the JSON in
    /// markdown fences or extra text. Falls back to the raw text so the
    /// UI always has something to show.
    /// </summary>
    internal static ContextSuggestionResponse ParseResponse(string result)
    {
        if (string.IsNullOrWhiteSpace(result))
        {
            return new ContextSuggestionResponse { Answer = string.Empty };
        }

        string json = ExtractFirstJsonObject(result);
        if (string.IsNullOrEmpty(json))
        {
            return new ContextSuggestionResponse { Answer = result };
        }

        try
        {
            return JsonSerializer.Deserialize<ContextSuggestionResponse>(json) ?? new ContextSuggestionResponse { Answer = result };
        }
        catch (JsonException)
        {
            return new ContextSuggestionResponse { Answer = result };
        }
    }

    private static string ExtractFirstJsonObject(string text)
    {
        int firstBrace = text.IndexOf('{');
        int lastBrace = text.LastIndexOf('}');
        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            return text.Substring(firstBrace, lastBrace - firstBrace + 1);
        }

        var stripped = text
            .Replace("```json", "", StringComparison.OrdinalIgnoreCase)
            .Replace("```", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim();

        firstBrace = stripped.IndexOf('{');
        lastBrace = stripped.LastIndexOf('}');
        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            return stripped.Substring(firstBrace, lastBrace - firstBrace + 1);
        }

        return string.Empty;
    }

}
