namespace MauiNotes.Services.AI;

public sealed class AzureOpenAiOptions
{
    private const string DefaultEndpoint = "";
    private const string DefaultDeploymentName = "";
    private const string DefaultApiKey = "";

    private string EndpointAddress { get; }
    public string DeploymentName { get; }
    public string ApiKey { get; }

    public AzureOpenAiOptions()
    {
        EndpointAddress = Environment.GetEnvironmentVariable("MAUINOTES_AZURE_OPENAI_ENDPOINT")
            ?? DefaultEndpoint;
        DeploymentName = Environment.GetEnvironmentVariable("MAUINOTES_AZURE_OPENAI_DEPLOYMENT")
            ?? DefaultDeploymentName;
        ApiKey = Environment.GetEnvironmentVariable("MAUINOTES_AZURE_OPENAI_API_KEY")
            ?? DefaultApiKey;
    }

    public Uri GetChatCompletionsEndpoint()
    {
        if (!Uri.TryCreate(EndpointAddress, UriKind.Absolute, out Uri? endpointUri) ||
            endpointUri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException(
                "MAUINOTES_AZURE_OPENAI_ENDPOINT must be an absolute HTTPS endpoint.");
        }

        if (string.IsNullOrWhiteSpace(DeploymentName))
            throw new InvalidOperationException("MAUINOTES_AZURE_OPENAI_DEPLOYMENT is required.");

        Uri baseUri = new(endpointUri.AbsoluteUri.TrimEnd('/') + "/", UriKind.Absolute);
        return new Uri(baseUri, "openai/v1/chat/completions");
    }
}
