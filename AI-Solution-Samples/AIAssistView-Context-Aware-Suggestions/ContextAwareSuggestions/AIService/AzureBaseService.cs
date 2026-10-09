using Azure;
using Azure.AI.OpenAI;

namespace ContextAwareSuggestions;

/// <summary>
/// Provides the base implementation for Azure AI service integrations used by the application.
/// </summary>
/// <remarks>
/// This abstract class contains shared functionality required to communicate with Azure AI services,
/// including client initialization, credential validation, endpoint configuration, and common request handling.
/// Derived classes can extend this base class to implement specific AI capabilities such as chat completion,
/// context-aware suggestions, smart search, or content generation.
/// </remarks>
public abstract class AzureBaseService
{
    protected virtual string Endpoint => "YOUR_ENDPOINT_HERE";

    protected virtual string ApiKey => "YOUR_API_KEY_HERE";

    protected virtual string DeploymentName => "YOUR_DEPLOYMENT_NAME_HERE";

    protected AzureOpenAIClient CreateClient()
    {
        return new AzureOpenAIClient(new Uri(Endpoint), new AzureKeyCredential(ApiKey));
    }
}