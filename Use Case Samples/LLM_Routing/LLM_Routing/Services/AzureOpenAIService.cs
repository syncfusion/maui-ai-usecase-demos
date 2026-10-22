using System.ClientModel;
using Azure.AI.OpenAI;
using OpenAI.Chat;

namespace LLM_Routing.Services;

/// <summary>
/// Result of a single chat completion: the generated text plus the request/response token counts.
/// </summary>
public sealed record ChatResult(string Text, int InputTokens, int OutputTokens);

/// <summary>
/// Routes customer queries to one of three Azure OpenAI models and generates responses.
/// Routing uses the low-end model (nano) to classify query complexity first.
///
/// Each model can be backed by its own Azure OpenAI resource. Set per-model credentials
/// via environment variables (model name uppercased, '.', '-', ' ' replaced by '_'):
///   AZURE_API_KEY_{MODEL}   — e.g. AZURE_API_KEY_GPT_4_1_NANO
///   AZURE_API_BASE_{MODEL}  — e.g. AZURE_API_BASE_GPT_4_1_NANO
///
/// When a per-model credential is not set, the service falls back to the shared
///   AZURE_API_KEY / AZURE_API_BASE
/// values, and finally to dummy placeholders so the app still compiles and runs.
/// </summary>
public sealed class AzureOpenAIService
{
    private const string LowModel = "gpt-4.1-nano";
    private const string StandardModel = "gpt-4.1-mini";
    private const string HighModel = "gpt-5.6-luna";

    private const string FallbackEndpoint = "https://YOUR-RESOURCE.openai.azure.com/";
    private const string FallbackApiKey = "DUMMY_API_KEY";

    private readonly ChatClient _lowClient;
    private readonly ChatClient _standardClient;
    private readonly ChatClient _highClient;

    public AzureOpenAIService()
    {
        _lowClient = CreateClient(LowModel);
        _standardClient = CreateClient(StandardModel);
        _highClient = CreateClient(HighModel);
    }

    /// <summary>
    /// Uses the low-end model to classify the query and returns the model that should answer it.
    /// </summary>
    public async Task<string> SelectModelAsync(string query, CancellationToken cancellationToken = default)
    {
        const string systemPrompt =
            "You are a router. Classify the user's query into one of three tiers: " +
            "LOW, STANDARD, or HIGH. Reply with ONLY the tier label.";

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage($"Customer query:\n<query>\n{query}\n</query>")
        };

        var result = await _lowClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);
        var tier = (result.Value.Content[0].Text ?? string.Empty).Trim().ToUpperInvariant();

        return tier switch
        {
            "HIGH" => HighModel,
            "STANDARD" => StandardModel,
            _ => LowModel
        };
    }

    public async Task<ChatResult> GenerateResponseAsync(string model, string query, CancellationToken cancellationToken = default)
    {
        var client = GetClient(model);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are a helpful AI assistant. Answer accurately and concisely."),
            new UserChatMessage(query)
        };

        var completion = (await client.CompleteChatAsync(messages, cancellationToken: cancellationToken)).Value;
        return ToChatResult(completion);
    }

    /// <summary>
    /// Sends the customer's query together with an attached image. The model analyzes the image in context of the prompt.
    /// </summary>
    public async Task<ChatResult> GenerateResponseAsync(
        string model,
        string query,
        byte[] imageBytes,
        string imageMediaType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(imageBytes);
        var client = GetClient(model);

        var userMessage = new UserChatMessage(new[]
        {
            ChatMessageContentPart.CreateTextPart(query),
            ChatMessageContentPart.CreateImagePart(
                BinaryData.FromBytes(imageBytes),
                imageMediaType,
                ChatImageDetailLevel.Auto)
        });

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are a helpful AI assistant. Analyze the attached image and answer the user's question about it accurately and concisely."),
            userMessage
        };

        var completion = (await client.CompleteChatAsync(messages, cancellationToken: cancellationToken)).Value;
        return ToChatResult(completion);
    }

    private ChatClient GetClient(string model) => model switch
    {
        HighModel => _highClient,
        StandardModel => _standardClient,
        _ => _lowClient
    };

    private static ChatResult ToChatResult(ChatCompletion completion) => new(
        completion.Content[0].Text ?? string.Empty,
        completion.Usage.InputTokenCount,
        completion.Usage.OutputTokenCount);

    /// <summary>
    /// Builds a <see cref="ChatClient"/> for the given model, picking credentials from
    /// per-model env vars when present and otherwise falling back to the shared ones.
    /// </summary>
    private static ChatClient CreateClient(string model)
    {
        var suffix = CostCalculator.ModelToEnvSuffix(model);
        var endpoint = Environment.GetEnvironmentVariable($"AZURE_API_BASE_{suffix}")
            ?? Environment.GetEnvironmentVariable("AZURE_API_BASE")
            ?? FallbackEndpoint;
        var apiKey = Environment.GetEnvironmentVariable($"AZURE_API_KEY_{suffix}")
            ?? Environment.GetEnvironmentVariable("AZURE_API_KEY")
            ?? FallbackApiKey;

        var credential = new ApiKeyCredential(apiKey);
        var azureClient = new AzureOpenAIClient(new Uri(endpoint), credential);
        return azureClient.GetChatClient(model);
    }
}
