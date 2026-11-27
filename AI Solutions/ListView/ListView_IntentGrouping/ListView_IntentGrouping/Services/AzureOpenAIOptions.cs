namespace ListView_IntentGrouping.Services;

/// <summary>
/// Strongly-typed holder for the Azure OpenAI settings used by
/// <see cref="AzureOpenAIIntentAnalyzer"/>. Populated by
/// <see cref="AzureBaseService.CreateOptions"/> from the credential
/// constants defined in <see cref="AzureBaseService"/>.
/// </summary>
public class AzureOpenAIOptions
{
    public static readonly string Endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? string.Empty;
    public static readonly string Key = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? string.Empty;
    public static readonly string DeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? string.Empty;

    /// <summary>
    /// Sampling temperature for the chat completion call. Defaults
    /// to <c>0</c> so the same input always produces the same
    /// categorization (the sample is deterministic for repeatable
    /// demos).
    /// </summary>
    public float Temperature { get; set; } = 0f;

    /// <summary>
    /// Maximum tokens to budget for the chat completion response.
    /// Defaults to a generous value that comfortably covers the JSON
    /// response payload for the sample datasets.
    /// </summary>
    public int MaxTokens { get; set; } = 4096;
    public string? ModelName { get; internal set; }
}
