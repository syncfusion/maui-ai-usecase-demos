using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using MauiNotes.Models.AI;

namespace MauiNotes.Services.AI;

public sealed class AzureOpenAiEditingService : IAiEditingService
{
    private const int MaximumInputLength = 20000;
    private const int MaximumOutputTokens = 8000;
    private readonly HttpClient httpClient;
    private readonly AzureOpenAiOptions options;

    public AzureOpenAiEditingService(HttpClient httpClient, AzureOpenAiOptions options)
    {
        this.httpClient = httpClient;
        this.options = options;
    }

    public async Task<string> GenerateAsync(
        AiEditingRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            throw new InvalidOperationException(
                "AI editing is not configured. Set MAUINOTES_AZURE_OPENAI_API_KEY for local development.");
        }

        if (request.Html.Length > MaximumInputLength * 4)
            throw new InvalidOperationException("The selected HTML is too large to send for AI processing.");

        string instruction = AiEditingInstructions.GetInstruction(request.Action, request.CustomInstruction);
        Uri requestUri = options.GetChatCompletionsEndpoint();
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri);
        httpRequest.Headers.Add("api-key", options.ApiKey);
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpRequest.Content = JsonContent.Create(new
        {
            model = options.DeploymentName,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = AiEditingInstructions.SystemPrompt
                },
                new
                {
                    role = "user",
                    content = $"Requested change:\n{instruction}\n\nTarget HTML:\n{request.Html}"
                }
            },
            max_completion_tokens = MaximumOutputTokens
        });

        using HttpResponseMessage response = await httpClient.SendAsync(
            httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string message = response.StatusCode switch
            {
                HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden =>
                    "Azure OpenAI rejected the credentials. Check the configured endpoint, deployment, and key.",
                HttpStatusCode.NotFound =>
                    "The Azure OpenAI deployment or API route was not found. Verify the deployment name and v1 API availability.",
                HttpStatusCode.TooManyRequests =>
                    "Azure OpenAI is busy or the deployment quota was exceeded. Try again shortly.",
                _ => $"Azure OpenAI returned HTTP {(int)response.StatusCode} ({response.ReasonPhrase})."
            };
            throw new HttpRequestException(message, null, response.StatusCode);
        }

        using JsonDocument payload = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);

        if (!payload.RootElement.TryGetProperty("choices", out JsonElement choices) ||
            choices.GetArrayLength() == 0 ||
            !choices[0].TryGetProperty("message", out JsonElement messageElement) ||
            !messageElement.TryGetProperty("content", out JsonElement contentElement) ||
            contentElement.ValueKind != JsonValueKind.String)
        {
            throw new InvalidOperationException("Azure OpenAI returned an empty or unsupported response.");
        }

        if (choices[0].TryGetProperty("finish_reason", out JsonElement finishReason) &&
            finishReason.ValueKind == JsonValueKind.String &&
            finishReason.GetString() == "length")
        {
            throw new InvalidOperationException("The AI response reached its output limit. Try a shorter note or smaller selection.");
        }

        string result = contentElement.GetString()?.Trim() ?? string.Empty;
        if (result.Length == 0)
            throw new InvalidOperationException("Azure OpenAI returned no edited text.");

        return result;
    }
}
