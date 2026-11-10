using System.Text.Json;
using System.Text.RegularExpressions;

namespace SupportReplyAIDraft.Services;

public sealed class AzureOpenAISettings
{
    private static readonly Regex HttpsUrlPattern = new("https://[^\\s\\\"'<>]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public string Endpoint { get; init; } = string.Empty;
    public string ApiKey { get; init; } = string.Empty;
    public string DeploymentName { get; init; } = string.Empty;

    public static async Task<AzureOpenAISettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
        var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME");

        if (string.IsNullOrWhiteSpace(endpoint)
            || string.IsNullOrWhiteSpace(apiKey)
            || string.IsNullOrWhiteSpace(deploymentName))
        {
            await using var stream = await FileSystem.OpenAppPackageFileAsync("azure-openai.local.json").ConfigureAwait(false);
            var localSettings = await JsonSerializer.DeserializeAsync<AzureOpenAISettings>(
                stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                cancellationToken).ConfigureAwait(false);

            if (localSettings is null)
            {
                throw new InvalidOperationException("The local Azure OpenAI settings file is empty or invalid.");
            }

            endpoint = localSettings.Endpoint;
            apiKey = localSettings.ApiKey;
            deploymentName = localSettings.DeploymentName;
        }

        var normalizedEndpoint = ExtractEndpoint(endpoint);
        if (!Uri.TryCreate(normalizedEndpoint, UriKind.Absolute, out var endpointUri)
            || endpointUri.Scheme != Uri.UriSchemeHttps
            || !(endpointUri.Host.EndsWith(".openai.azure.com", StringComparison.OrdinalIgnoreCase)
                || endpointUri.Host.EndsWith(".cognitiveservices.azure.com", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Azure OpenAI endpoint must be an absolute HTTPS Azure OpenAI URL.");
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Azure OpenAI API key is missing.");
        }

        if (string.IsNullOrWhiteSpace(deploymentName))
        {
            throw new InvalidOperationException("Azure OpenAI deployment name is missing.");
        }

        return new AzureOpenAISettings
        {
            Endpoint = endpointUri.AbsoluteUri.TrimEnd('/'),
            ApiKey = apiKey,
            DeploymentName = deploymentName.Trim()
        };
    }

    private static string ExtractEndpoint(string? endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return string.Empty;
        }

        var trimmed = endpoint.Trim();
        var match = HttpsUrlPattern.Match(trimmed);
        if (match.Success)
        {
            return match.Value.TrimEnd('.', ',', ')', ']');
        }

        return trimmed;
    }
}
