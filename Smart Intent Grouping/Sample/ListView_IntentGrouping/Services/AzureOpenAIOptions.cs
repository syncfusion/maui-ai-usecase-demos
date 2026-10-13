namespace ListView_IntentGrouping.Services;

/// <summary>
/// Strongly-typed holder for the Azure OpenAI settings used by
/// <see cref="AzureOpenAIIntentAnalyzer"/>. Populated by
/// <see cref="AzureBaseService.CreateOptions"/> from the credential
/// constants defined in <see cref="AzureBaseService"/>.
/// </summary>
public class AzureOpenAIOptions
{
    /// <summary>
    /// The full endpoint of the Azure OpenAI resource, e.g.
    /// <c>https://my-resource.openai.azure.com/</c>. Must include
    /// the trailing slash.
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// API key copied from the "Keys and Endpoint" blade of the
    /// Azure OpenAI resource in the Azure portal. Must come from
    /// the same resource whose hostname is in <see cref="Endpoint"/>.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Deployment name as configured on the Azure OpenAI resource
    /// (NOT the underlying model name). For example, a deployment
    /// named <c>gpt-4o</c> might back the <c>gpt-4o</c> model.
    /// </summary>
    public string DeploymentName { get; set; } = string.Empty;

    /// <summary>
    /// The model the deployment is expected to back. Used only for
    /// the status banner in the UI. Defaults to <c>gpt-4o</c>.
    /// </summary>
    public string ModelName { get; set; } = "gpt-4o";

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
}
