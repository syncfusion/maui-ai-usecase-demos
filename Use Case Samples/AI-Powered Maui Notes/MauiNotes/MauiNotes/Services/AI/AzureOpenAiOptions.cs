namespace MauiNotes.Services.AI;

public sealed class AzureOpenAiOptions
{
    // Replace these placeholders for a local demo. Do not commit a real key.
    // Desktop builds may also override them with MAUINOTES_AZURE_OPENAI_* environment variables.
    // Device builds cannot read those variables; use a backend proxy instead of shipping a key.
    public const string EndpointPlaceholder = "https://YOUR_RESOURCE.openai.azure.com/";
    public const string DeploymentPlaceholder = "YOUR_DEPLOYMENT_NAME";
    public const string ApiKeyPlaceholder = "YOUR_AZURE_OPENAI_API_KEY";

    private readonly string endpointAddress;

    public string DeploymentName { get; }
    public string ApiKey { get; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ApiKey) &&
        !string.Equals(ApiKey, ApiKeyPlaceholder, StringComparison.Ordinal) &&
        !string.Equals(DeploymentName, DeploymentPlaceholder, StringComparison.Ordinal) &&
        Uri.TryCreate(endpointAddress, UriKind.Absolute, out Uri? endpoint) &&
        endpoint.Scheme == Uri.UriSchemeHttps &&
        !endpoint.Host.Contains("YOUR_RESOURCE", StringComparison.OrdinalIgnoreCase);

    public AzureOpenAiOptions()
    {
        endpointAddress = FirstConfigured(
            Environment.GetEnvironmentVariable("MAUINOTES_AZURE_OPENAI_ENDPOINT"),
            EndpointPlaceholder);
        DeploymentName = FirstConfigured(
            Environment.GetEnvironmentVariable("MAUINOTES_AZURE_OPENAI_DEPLOYMENT"),
            DeploymentPlaceholder);
        ApiKey = FirstConfigured(
            Environment.GetEnvironmentVariable("MAUINOTES_AZURE_OPENAI_API_KEY"),
            ApiKeyPlaceholder);
    }

    public Uri GetChatCompletionsEndpoint()
    {
        if (!IsConfigured || !Uri.TryCreate(endpointAddress, UriKind.Absolute, out Uri? endpointUri))
        {
            throw new InvalidOperationException(
                "Azure OpenAI is not configured. Set the placeholders in AzureOpenAiOptions or the MAUINOTES_AZURE_OPENAI_* environment variables. Do not ship an API key in a device build; call a backend proxy instead.");
        }

        Uri baseUri = new(endpointUri.AbsoluteUri.TrimEnd('/') + "/", UriKind.Absolute);
        return new Uri(baseUri, "openai/v1/chat/completions");
    }

    private static string FirstConfigured(string? environmentValue, string placeholder) =>
        string.IsNullOrWhiteSpace(environmentValue) ? placeholder : environmentValue;
}
