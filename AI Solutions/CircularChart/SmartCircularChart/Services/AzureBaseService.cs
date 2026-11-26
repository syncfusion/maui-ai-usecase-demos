namespace SmartCircularChart;

public abstract class AzureBaseService
{
    protected static readonly string Endpoint = "AZURE_OPENAI_ENDPOINT";
    protected static readonly string DeploymentName = "AZURE_OPENAI_DEPLOYMENT";
    protected static readonly string ApiKey = "AZURE_OPENAI_KEY";
    protected static readonly string ApiVersion = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_VERSION") ?? "2024-08-01-preview";

    protected static bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Endpoint) &&
        !string.IsNullOrWhiteSpace(DeploymentName) &&
        !string.IsNullOrWhiteSpace(ApiKey);

    protected static Uri GetChatCompletionsUri()
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("Azure OpenAI credentials are not configured.");
        }

        if (!Uri.TryCreate(Endpoint, UriKind.Absolute, out var endpointUri))
        {
            throw new InvalidOperationException("Azure OpenAI endpoint is not a valid absolute URL.");
        }

        return new Uri($"{endpointUri.AbsoluteUri.TrimEnd('/')}/openai/deployments/{DeploymentName}/chat/completions?api-version={ApiVersion}");
    }
}
