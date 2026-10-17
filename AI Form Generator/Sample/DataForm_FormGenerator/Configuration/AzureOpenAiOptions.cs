namespace DataForm_FormGenerator.Configuration;

/// <summary>
/// Strongly-typed configuration bound from <c>AzureOpenAI</c> section of appsettings.json
/// (or overloads such as environment variables / MAUI asset files).
/// </summary>
public sealed class AzureOpenAiOptions
{
    public string Endpoint { get; set; } = "Your Endpoint";
    public string DeploymentName { get; set; } = "Your deployement name";
    public string ApiKey { get; set; } = "API Key";

    /// <summary>
    /// Generates the error message shown when any field is missing or invalid. Empty when valid.
    /// </summary>
    public string? ValidationError
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Endpoint) ||
                !(Endpoint.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                  Endpoint.StartsWith("http://", StringComparison.OrdinalIgnoreCase)))
            {
                return "Azure OpenAI endpoint is missing or malformed (expected https://<resource>.openai.azure.com/).";
            }

            if (string.IsNullOrWhiteSpace(DeploymentName))
            {
                return "Azure OpenAI deployment name is missing. Set AZURE_OPENAI_DEPLOYMENT.";
            }

            if (string.IsNullOrWhiteSpace(ApiKey) || ApiKey.Length < 20)
            {
                return "Invalid API key. Please configure a valid Azure OpenAI API key.";
            }

            if (ApiKey.StartsWith("API key", StringComparison.OrdinalIgnoreCase) ||
                ApiKey.StartsWith("Your ", StringComparison.OrdinalIgnoreCase) ||
                ApiKey.Contains(' ', StringComparison.Ordinal))
            {
                return "Invalid API key. Please configure a valid Azure OpenAI API key.";
            }

            return null;
        }
    }
}