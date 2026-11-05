using Azure;
using Azure.AI.OpenAI;
using Azure.AI.Vision.ImageAnalysis;

namespace SmartCropping;

/// <summary>
/// Provides the base implementation for Azure AI service integrations used by the application.
/// </summary>
/// <remarks>
/// This abstract class contains shared functionality required to communicate with Azure AI services,
/// including client initialization and credential configuration.
/// Derived classes can extend this base class to implement specific AI capabilities such as chat completion
/// or image analysis.
/// </remarks>
public abstract class AzureBaseService
{
    protected virtual string Endpoint =>
        Environment.GetEnvironmentVariable("AZURE_VISION_ENDPOINT") ?? "YOUR_ENDPOINT_HERE";

    protected virtual string ApiKey =>
        Environment.GetEnvironmentVariable("AZURE_VISION_KEY") ?? "YOUR_API_KEY_HERE";

    protected virtual string DeploymentName => "YOUR_DEPLOYMENT_NAME_HERE";

    protected AzureOpenAIClient CreateClient()
    {
        return new AzureOpenAIClient(GetEndpointUri(), new AzureKeyCredential(ApiKey));
    }

    protected ImageAnalysisClient CreateImageAnalysisClient()
    {
        return new ImageAnalysisClient(GetEndpointUri(), new AzureKeyCredential(ApiKey));
    }

    private Uri GetEndpointUri()
    {
        if (!Uri.TryCreate(Endpoint, UriKind.Absolute, out Uri? endpointUri))
        {
            throw new InvalidOperationException("A valid Azure Vision endpoint must be configured before using AI features.");
        }

        return endpointUri;
    }
}
