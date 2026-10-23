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

Analyze the supplied image and identify its most important visual content
(the main subject: a person, animal, object, or focal element). Then suggest
ONE rectangular crop region around that subject.

CRITICAL — the crop rectangle MUST:

1. Fully contain the entire main subject, including any extremities
   (animal tails and ears, human hands and feet, antennas, accessories,
   overhanging branches, handles, etc.). Never crop into a limb, tail,
   edge, or appendage.
2. Leave a comfortable safety margin (roughly 5 to 15 percent of the
   subject's longer dimension) of surrounding context on ALL four sides.
   Prefer a slightly loose frame over a tight one.
3. Remove obviously empty surrounding space (solid sky, plain wall,
   background gradient) while still keeping enough breathing room so the
   subject does not touch the crop edge.
4. Preserve a balanced composition. If the subject is off-centre, recentre
   the crop so the subject sits comfortably inside the new frame.
5. Use the original image's pixel dimensions (top-left origin).
6. Stay strictly inside the image boundary on every side.

If the image has no clear single subject (for example a landscape, a
pattern, or a scene with no dominant object), return the largest
rectangle that still removes the most empty or repeated border area
while keeping the most visually dense region fully visible.

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

            BinaryData imageBinary = BinaryData.FromBytes(imageBytes);

            List<ChatMessage> messages =
            [
                new SystemChatMessage(SystemPrompt),

                new UserChatMessage(
                [
                    ChatMessageContentPart.CreateTextPart(
                        "Analyze the supplied image and identify its most important visual content. " +
                        "Suggest one rectangular crop region that keeps the important content visible, " +
                        "removes unnecessary surrounding space, preserves a balanced composition, " +
                        "uses the original image dimensions, and remains within image boundaries. " +
                        "Return only valid JSON matching the schema."),

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