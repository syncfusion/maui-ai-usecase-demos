using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MauiNotes.Models.AI;

namespace MauiNotes.Services.AI;

public sealed class AzureOpenAiEditingService : IAiEditingService
{
    private const int MaximumInputLength = 20000;
    private const int MaximumOutputTokens = 4000;
    private readonly HttpClient httpClient;
    private readonly AzureOpenAiOptions options;
    private readonly ILogger<AzureOpenAiEditingService> logger;

    public AzureOpenAiEditingService(
        HttpClient httpClient,
        AzureOpenAiOptions options,
        ILogger<AzureOpenAiEditingService> logger)
    {
        this.httpClient = httpClient;
        this.options = options;
        this.logger = logger;
    }

    public async Task<string> GenerateAsync(
        AiEditingRequest request,
        CancellationToken cancellationToken)
    {
        if (!options.IsConfigured)
        {
            throw new InvalidOperationException(
                "AI editing is not configured. Replace the placeholders in AzureOpenAiOptions, or set MAUINOTES_AZURE_OPENAI_* on desktop. Device builds should call a backend proxy instead of embedding a key.");
        }

        if (request.Text.Length > MaximumInputLength)
            throw new InvalidOperationException($"AI input cannot exceed {MaximumInputLength:N0} characters.");

        if (request.Html.Length > MaximumInputLength * 4)
            throw new InvalidOperationException("The selected HTML is too large to send for AI processing.");

        string instruction = AiPrompts.GetInstruction(request.Action, request.CustomInstruction);
        if (string.IsNullOrWhiteSpace(instruction))
            throw new InvalidOperationException("Enter instructions before requesting a custom edit.");

        bool sendHtml = !string.IsNullOrWhiteSpace(request.Html);
        string userContent = sendHtml
            ? $"Requested change:\n{instruction}\n\nTarget HTML:\n{request.Html}"
            : $"Requested change:\n{instruction}\n\nTarget text:\n{request.Text}";

        Uri requestUri = options.GetChatCompletionsEndpoint();
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri);
        httpRequest.Headers.Add("api-key", options.ApiKey);
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpRequest.Content = JsonContent.Create(new
        {
            model = options.DeploymentName,
            messages = new object[]
            {
                new { role = "system", content = AiPrompts.System },
                new { role = "user", content = userContent }
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

        string result = HtmlSanitizer.Sanitize(contentElement.GetString()?.Trim() ?? string.Empty);
        if (result.Length == 0)
            throw new InvalidOperationException("Azure OpenAI returned no edited text.");

        logger.LogInformation("AI edit completed for {Action}.", request.Action);
        return result;
    }
}
