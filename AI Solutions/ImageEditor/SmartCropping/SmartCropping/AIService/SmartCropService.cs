using System.ClientModel;
using System.Diagnostics;
using System.Text.Json;
using Azure;
using OpenAI.Chat;

namespace SmartCropping;

/// <summary>
/// Provides Azure OpenAI Vision integration for suggesting a single crop region
/// around the most important visual content of an image.
/// </summary>
/// <remarks>
/// The image is sent as a vision user message together with a prompt that asks
/// the model to return exactly one rectangular crop region as JSON. The raw
/// completion is cleaned of markdown fences, deserialized into strongly typed
/// models, and returned to the caller for validation against the loaded image.
/// </remarks>
public sealed class SmartCropService : AzureBaseService, ISmartCropService
{
    private const string SystemPrompt =
    """
You are an image cropping assistant.

Analyze the supplied image and identify its most important visual content.

Suggest one rectangular crop region that:

- Keeps the important content visible.
- Removes unnecessary surrounding space.
- Preserves a balanced composition.
- Uses the original image dimensions.
- Remains within image boundaries.

Return only valid JSON matching the schema.

{
  "framing": "Main subject with reduced surrounding space",
  "cropRegion": {
    "x": 140,
    "y": 80,
    "width": 760,
    "height": 620
  }
}
""";

    private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Sends the supplied image to Azure OpenAI and returns the suggested crop region.
    /// </summary>
    /// <param name="imageBytes">The encoded image (PNG/JPEG) to analyze.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    /// <returns>
    /// A <see cref="SmartCropResponse"/> containing the proposed framing and
    /// crop region, or <see langword="null"/> when the service could not
    /// produce a valid suggestion.
    /// </returns>
    public async Task<SmartCropResponse?> SuggestCropAsync(byte[] imageBytes, CancellationToken cancellationToken = default)
    {
        if (imageBytes.Length == 0)
        {
            return null;
        }

        try
        {
            var client = CreateClient();
            ChatClient chatClient = client.GetChatClient(DeploymentName);

            string imageDataUri = BuildDataUri(imageBytes);

            List<ChatMessage> messages =
            [
                new SystemChatMessage(SystemPrompt),

                new UserChatMessage(
                [
                    ChatMessageContentPart.CreateTextMessagePart(
                        "Analyze the supplied image and identify its most important visual content. " +
                        "Suggest one rectangular crop region that keeps the important content visible, " +
                        "removes unnecessary surrounding space, preserves a balanced composition, " +
                        "uses the original image dimensions, and remains within image boundaries. " +
                        "Return only valid JSON matching the schema."),

                    ChatMessageContentPart.CreateImagePart(
                        ChatImageContentFormat.Png,
                        imageDataUri)
                ])
            ];

            var chatOptions = new ChatCompletionOptions
            {
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
            };

            // Add timeout to prevent indefinite hanging
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(60));

            ClientResult<ChatCompletion> result = await chatClient.CompleteChatAsync(messages, chatOptions, cts.Token);

            ChatCompletion completion = result.Value;

            if (completion.Content == null || completion.Content.Count == 0)
            {
                Debug.WriteLine("Azure OpenAI returned no content.");
                return null;
            }

            string rawJson = string.Concat(completion.Content.Select(part => part.Text ?? string.Empty));

            if (string.IsNullOrWhiteSpace(rawJson))
            {
                return null;
            }

            rawJson = CleanJson(rawJson);

            SmartCropResponse? response = JsonSerializer.Deserialize<SmartCropResponse>(rawJson, SerializerOptions);

            if (response?.CropRegion == null)
            {
                Debug.WriteLine("Azure OpenAI returned an invalid crop response.");
                return null;
            }

            return response;
        }
        catch (RequestFailedException ex)
        {
            Debug.WriteLine($"Azure request failed: {ex.Status} | {ex.ErrorCode} | {ex.Message}");

            throw new InvalidOperationException(
                $"Azure OpenAI request failed ({ex.Status}: {ex.ErrorCode}). {ex.Message}", ex);
        }
        catch (ClientResultException ex)
        {
            Debug.WriteLine($"OpenAI client request failed: {ex.Status} | {ex.Message}");

            throw new InvalidOperationException(
                $"Azure OpenAI request failed ({ex.Status}). {ex.Message}", ex);
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine("Azure OpenAI request cancelled.");
            throw;
        }
        catch (JsonException ex)
        {
            Debug.WriteLine($"Invalid JSON returned by Azure AI: {ex.Message}");

            throw new InvalidOperationException("Azure AI returned an invalid crop response.", ex);
        }
    }

    private static string BuildDataUri(byte[] imageBytes)
    {
        return $"data:image/png;base64,{Convert.ToBase64String(imageBytes)}";
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