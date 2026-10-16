namespace NutriLens.Services;

/// <summary>
/// Centralized Azure OpenAI configuration. Values are placeholders —
/// replace them with your own endpoint, deployment name, and API key
/// before running the app. The sample never commits a real secret.
/// </summary>
public static class AzureOpenAIConfig
{
    public const string BaseEndpoint = "https://your-openai-resource.openai.azure.com/";
    public const string DeploymentName = "your-DeploymentName";
    public const string ApiKey = "your-azure-openai-api-key";

    public static string ConfiguredEndpoint =>
        Environment.GetEnvironmentVariable("NUTRILENS_AZURE_OPENAI_ENDPOINT")
        ?? BaseEndpoint;

    public static string ConfiguredDeploymentName =>
        Environment.GetEnvironmentVariable("NUTRILENS_AZURE_OPENAI_DEPLOYMENT")
        ?? DeploymentName;

    public static string ConfiguredApiKey =>
        Environment.GetEnvironmentVariable("NUTRILENS_AZURE_OPENAI_API_KEY")
        ?? ApiKey;

    public static void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(ConfiguredEndpoint) ||
            ConfiguredEndpoint.Contains("your-openai-resource", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(ConfiguredDeploymentName) ||
            ConfiguredDeploymentName.StartsWith("your-", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(ConfiguredApiKey) ||
            ConfiguredApiKey.StartsWith("your-", StringComparison.OrdinalIgnoreCase) ||
            !Uri.TryCreate(ConfiguredEndpoint, UriKind.Absolute, out var endpoint) ||
            endpoint.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException(
                "Azure OpenAI is not configured. Set the NutriLens endpoint, deployment, and API key before analyzing labels.");
        }
    }

    public static Uri GetChatCompletionsUri()
    {
        return new Uri($"{GetOpenAIClientEndpoint().ToString().TrimEnd('/')}/chat/completions");
    }

    public static Uri GetOpenAIClientEndpoint()
    {
        EnsureConfigured();

        var endpoint = ConfiguredEndpoint.TrimEnd('/');
        if (endpoint.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase))
            endpoint = endpoint[..^"/chat/completions".Length];

        if (endpoint.EndsWith("/openai/v1", StringComparison.OrdinalIgnoreCase))
            return new Uri(endpoint + "/");

        if (endpoint.EndsWith("/openai", StringComparison.OrdinalIgnoreCase))
            return new Uri(endpoint + "/v1/");

        return new Uri(endpoint + "/openai/v1/");
    }
}