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
You are an image cropping assistant. You will be told how the image will be
used (the crop intent). Let that intent drive how tight or wide the crop is.

Rules that always apply:

1. Suggest exactly ONE rectangular crop region.
2. Use the original image's pixel dimensions (top-left origin).
3. Stay strictly inside the image boundary on every side.
4. Never cut through the main subject the intent refers to.

Return ONLY valid JSON with no commentary, no markdown fences, and no
extra keys. Schema:

{
  "framing": "Short description of what the crop keeps visible",
  "cropRegion": {
    "x": <integer>,
    "y": <integer>,
    "width": <integer>,
    "height": <integer>
  }
}
""";

    private const string DefaultIntentPrompt =
    """
Analyze the supplied image.

Suggest one crop region that:

- Keeps the most important content fully visible.
- Removes unnecessary surrounding space.
- Preserves a balanced composition.

Return only valid JSON matching the schema.
""";

    private static readonly Dictionary<string, string> IntentPrompts = new()
    {
        ["Profile Photo"] =
        """
Analyze the supplied image.

The image will be used as a professional profile photograph.

Suggest one crop region that:

- Keeps the most prominent person visible.
- Prioritizes the face and upper body.
- Centers the subject.
- Removes unnecessary background.
- Maintains balanced spacing around the subject.
- Works well as a profile photo, avatar, or LinkedIn-style photograph.

Return only valid JSON matching the schema.
""",
        ["Product Image"] =
        """
Analyze the supplied image.

The image will be used as a product showcase.

Suggest one crop region that:

- Keeps the complete product visible.
- Removes unnecessary surrounding space.
- Preserves product details.
- Maintains professional product framing.
- Avoids clipping any portion of the product.

Return only valid JSON matching the schema.
""",
        ["Pet Portrait"] =
        """
Analyze the supplied image.

The image will be used as a pet portrait.

Suggest one crop region that:

- Keeps the pet fully visible.
- Prioritizes the pet's face and body.
- Centers the animal.
- Removes unnecessary surrounding space.
- Preserves natural visual balance.

Return only valid JSON matching the schema.
""",
        ["Food Photography"] =
        """
Analyze the supplied image.

The image will be used for food photography.

Suggest one crop region that:

- Keeps the food item fully visible.
- Highlights presentation and texture.
- Reduces distracting background elements.
- Maintains appetizing composition.
- Preserves the primary dish as the focal point.

Return only valid JSON matching the schema.
""",
        ["Hero Banner"] =
        """
Analyze the supplied image.

The image will be used as a website hero banner.

Suggest one crop region that:

- Preserves the primary subject.
- Maintains surrounding context.
- Supports a wide landscape composition.
- Provides strong visual balance.
- Leaves suitable whitespace for potential banner content.

Return only valid JSON matching the schema.
""",
        ["Object Focus"] =
        """
Analyze the supplied image.

The image will be used to highlight the primary object.

Suggest one crop region that:

- Focuses tightly on the most important object.
- Removes as much unnecessary surrounding space as possible.
- Keeps the object fully visible.
- Preserves a visually balanced composition.

Return only valid JSON matching the schema.
"""
    };

    /// <summary>Builds the user prompt for the selected crop intent.</summary>
    internal static string BuildIntentPrompt(string intent) =>
        IntentPrompts.TryGetValue(intent, out string? prompt) ? prompt : DefaultIntentPrompt;

    private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Sends the supplied image to Azure OpenAI and returns the suggested crop region.
    /// </summary>
    /// <param name="imageBytes">The encoded image (PNG/JPEG) to analyze.</param>
    /// <param name="intent">The selected crop intent that shapes the prompt.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    /// <returns>
    /// A <see cref="SmartCropResponse"/> containing the proposed framing and
    /// crop region, or <see langword="null"/> when the service could not
    /// produce a valid suggestion.
    /// </returns>
    public async Task<SmartCropResponse?> SuggestCropAsync(byte[] imageBytes, string intent, CancellationToken cancellationToken = default)
    {
        if (imageBytes.Length == 0)
        {
            return null;
        }

        try
        {
            var client = CreateClient();
            ChatClient chatClient = client.GetChatClient(DeploymentName);

            BinaryData imageBinary = BinaryData.FromBytes(imageBytes);

            List<ChatMessage> messages =
            [
                new SystemChatMessage(SystemPrompt),

                new UserChatMessage(
                [
                    ChatMessageContentPart.CreateTextPart(BuildIntentPrompt(intent)),

                    ChatMessageContentPart.CreateImagePart(imageBinary, "image/png", ChatImageDetailLevel.High)
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