namespace MauiNotes.Services.AI;

/// <summary>
/// Azure OpenAI settings. Defaults are placeholders so the sample compiles and
/// runs on every platform, including Android and iOS where process environment
/// variables are not available.
/// </summary>
/// <remarks>
/// Replace the placeholders for a local desktop run, or set
/// <c>MAUINOTES_AZURE_OPENAI_*</c> environment variables where the host exposes
/// them. Do not ship a real key inside a mobile package. Production apps should
/// call a backend proxy that holds the key. See the sample README.
/// </remarks>
public sealed class AzureOpenAiOptions
{
    public const string PlaceholderEndpoint = "https://your-resource.openai.azure.com/";
    public const string PlaceholderDeployment = "your-deployment-name";
    public const string PlaceholderApiKey = "your-azure-openai-api-key";

    public string Endpoint { get; set; } =
        Environment.GetEnvironmentVariable("MAUINOTES_AZURE_OPENAI_ENDPOINT")
        ?? PlaceholderEndpoint;

    public string DeploymentName { get; set; } =
        Environment.GetEnvironmentVariable("MAUINOTES_AZURE_OPENAI_DEPLOYMENT")
        ?? PlaceholderDeployment;

    public string ApiKey { get; set; } =
        Environment.GetEnvironmentVariable("MAUINOTES_AZURE_OPENAI_API_KEY")
        ?? PlaceholderApiKey;

    public bool IsConfigured =>
        !IsPlaceholder(Endpoint, "your-resource") &&
        !IsPlaceholder(DeploymentName, "your-deployment") &&
        !IsPlaceholder(ApiKey, "your-azure-openai") &&
        Uri.TryCreate(Endpoint, UriKind.Absolute, out Uri? endpoint) &&
        endpoint.Scheme == Uri.UriSchemeHttps;

    public Uri GetChatCompletionsEndpoint()
    {
        if (!IsConfigured ||
            !Uri.TryCreate(Endpoint, UriKind.Absolute, out Uri? endpointUri) ||
            endpointUri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException(
                "Azure OpenAI is not configured. Set the endpoint, deployment, and API key. " +
                "On a device, use a backend proxy instead of embedding a key. See the sample README.");
        }

        Uri baseUri = new(endpointUri.AbsoluteUri.TrimEnd('/') + "/", UriKind.Absolute);
        return new Uri(baseUri, "openai/v1/chat/completions");
    }

    private static bool IsPlaceholder(string value, string marker) =>
        string.IsNullOrWhiteSpace(value) ||
        value.Contains(marker, StringComparison.OrdinalIgnoreCase);
}
