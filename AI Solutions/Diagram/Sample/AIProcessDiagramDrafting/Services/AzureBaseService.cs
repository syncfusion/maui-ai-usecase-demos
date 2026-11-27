using Azure;
using Azure.AI.OpenAI;

namespace AIProcessDiagramDrafting.Services;

public abstract class AzureBaseService
{
    public static readonly string Endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? string.Empty;
    public static readonly string ApiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? string.Empty;
    public static readonly string DeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? string.Empty;
    protected static readonly string ApiVersion = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_VERSION") ?? "2024-08-01-preview";

    protected static bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Endpoint) &&
        !string.IsNullOrWhiteSpace(DeploymentName) &&
        !string.IsNullOrWhiteSpace(ApiKey);

    protected static AzureOpenAIClient CreateClient()
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("Azure OpenAI credentials are not configured.");
        }

        if (!Uri.TryCreate(Endpoint, UriKind.Absolute, out var endpointUri))
        {
            throw new InvalidOperationException("Azure OpenAI endpoint is not a valid absolute URL.");
        }

        return new AzureOpenAIClient(endpointUri, new AzureKeyCredential(ApiKey));
    }

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
