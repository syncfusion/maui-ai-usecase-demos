using Azure;
using Azure.AI.OpenAI;

namespace AIProcessDiagramDrafting.Services;

public abstract class AzureBaseService
{
    private static string GetSetting(params string[] names)
        => names.Select(Environment.GetEnvironmentVariable).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;

    protected static string Endpoint => GetSetting("AZURE_OPENAI_ENDPOINT", "AZURE_OPENAI_ENDPOINT_URL");

    protected static string DeploymentName => GetSetting("AZURE_OPENAI_DEPLOYMENT", "AZURE_OPENAI_DEPLOYMENT_NAME", "AZURE_OPENAI_CHAT_DEPLOYMENT_NAME");

    protected static string ApiKey => GetSetting("AZURE_OPENAI_API_KEY", "OPENAI_API_KEY");

    protected static AzureOpenAIClient CreateClient()
    {
        if (string.IsNullOrWhiteSpace(Endpoint))
        {
            throw new InvalidOperationException("Azure OpenAI endpoint is not configured. Set AZURE_OPENAI_ENDPOINT.");
        }

        if (string.IsNullOrWhiteSpace(DeploymentName))
        {
            throw new InvalidOperationException("Azure OpenAI deployment is not configured. Set AZURE_OPENAI_DEPLOYMENT.");
        }

        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            throw new InvalidOperationException("Azure OpenAI API key is not configured. Set AZURE_OPENAI_API_KEY.");
        }

        if (!Uri.TryCreate(Endpoint, UriKind.Absolute, out var endpointUri))
        {
            throw new InvalidOperationException("Azure OpenAI endpoint is not a valid absolute URL.");
        }

        return new AzureOpenAIClient(endpointUri, new AzureKeyCredential(ApiKey));
    }
}
