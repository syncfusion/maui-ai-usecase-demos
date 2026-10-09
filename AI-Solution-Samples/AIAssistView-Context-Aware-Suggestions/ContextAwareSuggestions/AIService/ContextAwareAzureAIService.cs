using System.Text.Json;
using OpenAI.Chat;

namespace ContextAwareSuggestions;

/// <summary>
/// Provides Azure OpenAI integration for generating AI responses with context-aware follow-up suggestions.
/// </summary>
/// <remarks>
/// This service sends user prompts to Azure OpenAI, retrieves AI-generated responses, and returns structured results containing both the answer and contextual suggestions that can be displayed in the Syncfusion AI AssistView.
/// </remarks>
public class ContextAwareAzureAIService : AzureBaseService,IAzureAIService
{
    private const string SystemPrompt =
"""
You are an AI learning assistant.

Return JSON only.

{
    "answer":"response",
    "suggestions":[
        "suggestion1",
        "suggestion2",
        "suggestion3",Da
        "suggestion4"
    ]
}

Suggestions must:

1. Be related to the response.
2. Help users continue learning.
3. Never repeat previous suggestions.
4. Be less than 4 words each.
5. Generate exactly 4 suggestions.
""";

    /// <summary>
    /// Sends the specified prompt to Azure OpenAI and retrieves an AI-generated response along with context-aware suggestions.
    /// </summary>
    /// <param name="prompt">
    /// The user prompt submitted to the AI service.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="ContextSuggestionResponse"/> with the generated answer and contextual suggestions, or an error response if the request fails.
    /// </returns>
    public async Task<ContextSuggestionResponse?>GetResponseAsync(string prompt)
    {
        try
        {
            var client = CreateClient();

            ChatClient chatClient = client.GetChatClient(DeploymentName);

            var messages = new List<ChatMessage>()
                           {
                               new SystemChatMessage(SystemPrompt),
                               new UserChatMessage(prompt)
                           };

            // Add timeout to prevent indefinite hanging
            using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(60));

            ChatCompletion completion = await chatClient.CompleteChatAsync(messages, cancellationToken: cts.Token);

            if (completion == null || completion.Content == null || completion.Content.Count == 0)
            {
                return new ContextSuggestionResponse
                {
                    Answer = string.Empty
                };
            }

            string result = completion.Content[0].Text ?? string.Empty;

            // The model should return JSON only, but sometimes it wraps the JSON
            // in markdown code fences or extra text. Try to extract a JSON
            // object from the response before deserializing.
            string json = string.Empty;

            int firstBrace = result.IndexOf('{');
            int lastBrace = result.LastIndexOf('}');

            if (firstBrace >= 0 && lastBrace > firstBrace)
            {
                json = result.Substring(firstBrace, lastBrace - firstBrace + 1);
            }
            else
            {
                // Fallback: strip common markdown fences and try again
                var stripped = result.Replace("```json", "", StringComparison.OrdinalIgnoreCase).Replace("```", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();

                firstBrace = stripped.IndexOf('{');
                lastBrace = stripped.LastIndexOf('}');

                if (firstBrace >= 0 && lastBrace > firstBrace)
                {
                    json = stripped.Substring(firstBrace, lastBrace - firstBrace + 1);
                }
                else
                {
                    // Nothing parseable found — return the raw text as Answer so UI shows something.
                    return new ContextSuggestionResponse
                    {
                        Answer = result
                    };
                }
            }

            try
            {
                var parsed = JsonSerializer.Deserialize<ContextSuggestionResponse>(json);
                return parsed;
            }
            catch (JsonException)
            {
                // If deserialization fails, log the problem and return the raw text as Answer.
                return new ContextSuggestionResponse
                {
                    Answer = result
                };
            }
        }
        catch (OperationCanceledException)
        {
            return new ContextSuggestionResponse
            {
                Answer = $"Request timed out after 20 seconds. Check your internet connection and Azure endpoint configuration."
            };
        }
        catch (Exception ex)
        {

            // Surface exception message to UI so user sees an error instead of a silent failure
            return new ContextSuggestionResponse
            {
                Answer = $"Error: {ex.Message}"
            };
        }
    }
}