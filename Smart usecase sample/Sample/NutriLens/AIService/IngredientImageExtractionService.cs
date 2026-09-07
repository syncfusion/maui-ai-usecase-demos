using NutriLens.Models;
using OpenAI.Chat;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace NutriLens.Services;

public sealed class IngredientImageExtractionService : IIngredientImageExtractionService
{
    private const string Endpoint = AzureOpenAIConfig.BaseEndpoint;

    private const string DeploymentName = AzureOpenAIConfig.DeploymentName;

    private const string ApiKey = AzureOpenAIConfig.ApiKey;

    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromSeconds(120)
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<string> ExtractContentAsync(
        FileResult image,
        CancellationToken cancellationToken = default)
    {
        if (image is null)
            throw new ArgumentNullException(nameof(image));

        var filePath = image.FullPath;

        if (string.IsNullOrWhiteSpace(filePath))
            throw new InvalidOperationException("No image was selected.");

        var imageBytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
        var base64 = Convert.ToBase64String(imageBytes);
        var extension = Path.GetExtension(filePath).TrimStart('.').ToLowerInvariant();

        if (extension == "jpg")
            extension = "jpeg";

        var imageUrl = $"data:image/{extension};base64,{base64}";

        var options = new OpenAI.OpenAIClientOptions
        {
            Endpoint = new Uri(Endpoint)
        };

        var client = new OpenAI.OpenAIClient(
            new System.ClientModel.ApiKeyCredential(ApiKey),
            options);

        var chatClient = client.GetChatClient(DeploymentName);

        var messages = new List<ChatMessage>
        {
            new UserChatMessage(
            [
                ChatMessageContentPart.CreateTextPart(
                    """
                    Extract ONLY the ingredient statement from this food label image.

                    Rules:
                    - Return only ingredients and ingredient subcomponents.
                    - Do NOT return nutrition facts, serving size, calories, warnings, claims, brand name, or marketing text.
                    - If the label has an "Ingredients:" section, extract just that section.
                    - If ingredients are split across multiple lines, keep them together.
                    - Preserve commas, parentheses, and ordering as printed.
                    - If no ingredients are visible, return an empty response.

                    Output format:
                    - One ingredient per line where possible.
                    - Keep the result concise and exact.
                    """
                ),
                ChatMessageContentPart.CreateImagePart(new Uri(imageUrl))
            ])
        };

        ChatCompletion completion = await chatClient.CompleteChatAsync(
            messages,
            cancellationToken: cancellationToken);

        var text = completion.Content[0].Text;

        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("No ingredients could be extracted from the selected product image.");

        return text.Trim();
    }
}