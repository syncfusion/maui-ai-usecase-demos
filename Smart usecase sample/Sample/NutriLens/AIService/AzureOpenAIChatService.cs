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

        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            Debug.WriteLine(
                "[AzureOpenAI] Missing AZURE_OPENAI_API_KEY. Daily insight generation will fall back.");
            return string.Empty;
        }

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
                            "You are an expert nutrition coach. " +
                            "Create a concise, helpful daily nutrition insight in plain English. " +
                            "Keep it under two sentences, encouraging, practical, and personalized. " +
                            "No markdown, no bullet list, no preamble."
                    },
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                },
                max_completion_tokens = 180,
                temperature = 0.7
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
                    $"[AzureOpenAI] HTTP {(int)response.StatusCode}: {responseText}");

                return string.Empty;
            }

            using var document = JsonDocument.Parse(responseText);

            if (document.RootElement.TryGetProperty("choices", out var choices) &&
                choices.ValueKind == JsonValueKind.Array &&
                choices.GetArrayLength() > 0)
            {
                var content = choices[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (!string.IsNullOrWhiteSpace(content))
                {
                    return content.Trim();
                }
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AzureOpenAI] Exception: {ex}");
            return string.Empty;
        }
    }

    private static string GetSetting(string key, string fallback)
    {
        var value = Environment.GetEnvironmentVariable(key);

        if (string.IsNullOrWhiteSpace(value))
            return fallback;

        return value.Trim();
    }
}