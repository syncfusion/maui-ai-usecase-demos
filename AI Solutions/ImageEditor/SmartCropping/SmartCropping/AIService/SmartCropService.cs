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
You are an image cropping assistant. The user will tell you how the image
will be used (the crop intent) and you must let that intent drive how
tight or wide the crop is. Crops that ignore the intent are wrong.

Rules that always apply:

1. Suggest exactly ONE rectangular crop region.
2. Use the original image's pixel dimensions (top-left origin).
3. Stay strictly inside the image boundary on every side.
4. Never cut through the main subject the intent refers to.

The "framing" field MUST explain how the crop satisfies the intent in one
or two sentences, and the crop region MUST match the subject-prominence
percentage called for by the intent. Different intents must produce
visibly different rectangles: a Profile Photo crop must be much tighter
and more face-centred than a Hero Banner crop, an Object Focus crop must
be tighter and more aggressive than a Pet Portrait crop, a Pet Portrait
crop must show the entire pet while an Object Focus crop zooms in.

Return ONLY valid JSON with no commentary, no markdown fences, and no
extra keys. Schema:

{
  "framing": "Short description of how the crop satisfies the intent",
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
The image will be used as a profile photo.

Identify the primary subject.

If the primary subject contains a face:

- Prioritize the face.
- Include head and upper portion of the subject.
- Center the face.
- Reduce unnecessary background.

If no face is visible:

- Focus on the most recognizable portion of the subject.

The crop should:

- Work well as an avatar.
- Work well in professional profile systems.
- Prefer square or portrait-oriented framing.
- Minimize background.

The primary subject should occupy approximately 50–70% of the crop area.

Examples:

- Human: face and shoulders.
- Pet: face and upper body.
- Object: main recognizable portion.

Return JSON only.
""",
        ["Product Image"] =
        """
The image will be used as a product showcase.

Identify the primary product.

The crop should:

- Keep the entire product visible.
- Never clip the product.
- Maintain balanced spacing on all sides.
- Reduce unnecessary background.
- Preserve product details.

The product should occupy approximately 60–80% of the crop area.

This crop should look suitable for:

- E-commerce
- Marketplaces
- Catalogs

Return JSON only.
""",
        ["Pet Portrait"] =
        """
The image will be used as a pet portrait.

Identify the primary pet.

The crop should:

- Keep the entire pet visible.
- Preserve natural spacing around the pet.
- Avoid cutting off the tail, ears, head, or legs.
- Include limited environmental context.

The pet should occupy approximately 40–60% of the crop area.

This crop should feel natural and balanced.

Return JSON only.
""",
        ["Food Photography"] =
        """
The image will be used for food photography.

Identify the primary dish or food item.

The crop should:

- Keep the food fully visible.
- Highlight presentation and texture.
- Remove distracting background elements.
- Preserve appealing composition.

The food should occupy approximately 60–75% of the crop area.

This crop should feel suitable for:

- Menus
- Food blogs
- Delivery apps

Return JSON only.
""",
        ["Hero Banner"] =
        """
The image will be used as a website hero banner.

Identify the primary subject.

The crop should:

- Preserve substantial surrounding context.
- Retain scenery and environmental details.
- Maintain visual balance.
- Avoid tight crops.

Prefer a wide landscape composition.

The primary subject should occupy approximately 20–40% of the crop area.

Examples:

- Human: include surroundings.
- Pet: include landscape and scenery.
- Object: include supporting context.

The crop should feel suitable for:

- Website banners
- Landing pages
- Marketing headers

Return JSON only.
""",
        ["Object Focus"] =
        """
The image will be used to emphasize the primary subject.

Identify the primary subject.

The crop should:

- Focus tightly on the subject.
- Remove as much surrounding content as possible.
- Keep the subject fully visible.
- Aggressively reduce background space.

The subject should occupy approximately 75–85% of the crop area.

Examples:

- Human: subject dominates crop.
- Pet: pet dominates crop.
- Product: product dominates crop.
- Object: object dominates crop.

This should be the most aggressive crop among all intents.

Return JSON only.
"""
    };

    /// <summary>
    /// Tells the model the exact pixel size so returned coordinates are in the
    /// image's own pixel space rather than a guessed or resized one.
    /// </summary>
    internal static string BuildSizeNote(int width, int height) =>
        width > 0 && height > 0
            ? $"\n\nThe image is exactly {width} pixels wide and {height} pixels tall. " +
              $"All crop coordinates must be integers in this pixel space with the origin at the top-left: " +
              $"x + width must be <= {width}, y + height must be <= {height}, and x and y must be >= 0."
            : string.Empty;

    /// <summary>Builds the user prompt for the selected crop intent.</summary>
    internal static string BuildIntentPrompt(string intent) =>
        IntentPrompts.TryGetValue(intent, out string? prompt) ? prompt : DefaultIntentPrompt;

    private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Sends the supplied image to Azure OpenAI and returns the suggested crop region.
    /// </summary>
    /// <param name="imageBytes">The encoded image (PNG/JPEG) to analyze.</param>
    /// <param name="intent">The selected crop intent that shapes the prompt.</param>
    /// <param name="imageWidth">Width of the image in pixels.</param>
    /// <param name="imageHeight">Height of the image in pixels.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    /// <returns>
    /// A <see cref="SmartCropResponse"/> containing the proposed framing and
    /// crop region, or <see langword="null"/> when the service could not
    /// produce a valid suggestion.
    /// </returns>
    public async Task<SmartCropResponse?> SuggestCropAsync(byte[] imageBytes, string intent, int imageWidth, int imageHeight, CancellationToken cancellationToken = default)
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
                    ChatMessageContentPart.CreateTextPart(BuildIntentPrompt(intent) + BuildSizeNote(imageWidth, imageHeight)),

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