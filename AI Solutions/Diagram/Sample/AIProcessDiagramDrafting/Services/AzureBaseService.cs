using Azure;
using Azure.AI.OpenAI;

namespace AIProcessDiagramDrafting.Services;

public abstract class AzureBaseService
{
    protected static readonly string Endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? string.Empty;
    protected static readonly string DeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") ?? string.Empty;
    protected static readonly string ApiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")
        ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY")
        ?? string.Empty;

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
}
