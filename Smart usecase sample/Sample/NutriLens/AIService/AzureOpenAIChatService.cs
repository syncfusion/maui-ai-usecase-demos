using NutriLens.Models;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace NutriLens.Services;

public sealed class AzureOpenAIChatService : IAzureOpenAIChatService
{
    // Keep secrets outside the app binary and configure them in the runtime environment.
    private const string BaseEndpoint = AzureOpenAIConfig.BaseEndpoint;

    private const string DeploymentName = AzureOpenAIConfig.DeploymentName;

    private const string ApiKey = AzureOpenAIConfig.ApiKey;

    private static readonly HttpClient HttpClient =
        new()
        {
            Timeout = TimeSpan.FromSeconds(60)
        };

    public async Task<string> GetCompletionAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Prompt cannot be empty.", nameof(prompt));

        try
        {
            var payload = new
            {
                model = DeploymentName,
                messages = new object[]
                {
                    new
                    {
                        role = "system",
                        content =
                            "You are NutriLens, an expert nutrition coach for users focused on " +
                            "diabetes-friendly eating. Create a concise, helpful daily nutrition " +
                            "insight in plain English based on the data provided. " +
                            "Keep it under two sentences, encouraging, practical, and personalized. " +
                            "No markdown, no bullet list, no preamble."
                    },
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                },

                // FIX (Bug 1): gpt-5-mini is a REASONING model. The old value
                // of 180 tokens was entirely consumed by internal reasoning,
                // producing EMPTY content on every call. Match the parameter
                // pattern of AzureOpenAIIngredientService, which works:
                // a generous token budget + minimal reasoning effort.
                max_completion_tokens = 800,
                reasoning_effort = "minimal"
            };

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    $"{BaseEndpoint.TrimEnd('/')}/chat/completions");

            request.Headers.Add("api-key", ApiKey);
            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var json = JsonSerializer.Serialize(payload);

            request.Content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            using var response = await HttpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            var responseText =
                await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine(
                    $"[DailyInsight AI] HTTP {(int)response.StatusCode}: {responseText}");

                return string.Empty;
            }

            using var document = JsonDocument.Parse(responseText);

            if (document.RootElement.TryGetProperty("choices", out var choices) &&
                choices.ValueKind == JsonValueKind.Array &&
                choices.GetArrayLength() > 0)
            {
                var choice = choices[0];

                // If the model ran out of tokens while reasoning, content is
                // empty — log the finish reason so this is never silent again.
                if (choice.TryGetProperty("finish_reason", out var finishReason))
                {
                    Debug.WriteLine($"[DailyInsight AI] finish_reason: {finishReason.GetString()}");
                }

                if (choice.TryGetProperty("message", out var message) &&
                    message.TryGetProperty("content", out var content) &&
                    content.ValueKind == JsonValueKind.String)
                {
                    var text = content.GetString();

                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        return text.Trim();
                    }
                }
            }

            Debug.WriteLine("[DailyInsight AI] Response contained no usable content.");
            return string.Empty;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DailyInsight AI] Exception: {ex}");
            return string.Empty;
        }
    }
}