namespace WardrobeAI.AI;

/// <summary>
/// Options for the Azure OpenAI services. The app currently registers these
/// options with their defaults in <c>MauiProgram.cs</c>; it does not bind them
/// from an <c>appsettings.json</c> file or .NET MAUI Preferences.
///
/// <para>The default values are placeholders. Replace them for local testing
/// or explicitly add a secure configuration source in <c>MauiProgram.cs</c>
/// to enable Azure OpenAI. When the endpoint, key, or deployment is not
/// configured, the AI services use their fallback behavior instead.</para>
/// </summary>
public sealed class AzureOpenAiOptions
{
    /// <summary>
    /// e.g. <c>https://your-resource.openai.azure.com/</c> (trailing slash required
    /// by the Azure OpenAI REST contract).
    /// </summary>
    public string Endpoint { get; set; } = Environment.GetEnvironmentVariable("AZURE_API_BASE") ?? "";

    /// <summary>Azure OpenAI API key. Prefer Managed Identity in production; this is the dev-only path.</summary>
    public string ApiKey { get; set; } = Environment.GetEnvironmentVariable("AZURE_API_KEY") ?? "";

    /// <summary>Deployment name (a.k.a. "model" in the Azure portal — e.g. <c>gpt-4o</c>).</summary>
    public string Deployment { get; set; } = "gpt-4.1-mini";

    /// <summary>API version. Defaults to a stable GA version known to support JSON-schema structured outputs.</summary>
    public string ApiVersion { get; set; } = "API_VERSION";

    /// <summary>Sampling temperature. 0.3 keeps the outfit picker stable + reproducible for the same closet.</summary>
    public double Temperature { get; set; } = 0.3;

    /// <summary>Max completion tokens. The structured-output payload is small (~600 tokens) so 800 is plenty.</summary>
    public int MaxOutputTokens { get; set; } = 800;

    /// <summary>
    /// True when the user has wired Azure OpenAI in. Drives the friendly
    /// fallback path.
    /// </summary>
    /// <remarks>
    /// The endpoint must parse as an absolute HTTP/HTTPS URI. When no
    /// environment endpoint is configured, the empty default keeps the Azure
    /// client disabled until an endpoint is supplied. The Azure OpenAI
    /// services' singleton constructors can therefore avoid eagerly calling
    /// <c>new AzureOpenAIClient(new Uri(opts.Endpoint), ...)</c>, throwing
    /// <c>UriFormatException: Invalid URI: The format of the URI could not
    /// be determined.</c> at app startup before any fallback path could
    /// run. Validating the URI here keeps the singletons constructable
    /// against placeholder config so the fallback path can take over without
    /// crashing the app on first run.
    /// </remarks>
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Endpoint)
        && !string.IsNullOrWhiteSpace(ApiKey)
        && !string.IsNullOrWhiteSpace(Deployment)
        && Uri.TryCreate(Endpoint, UriKind.Absolute, out var endpointUri)
        && (endpointUri.Scheme == Uri.UriSchemeHttp || endpointUri.Scheme == Uri.UriSchemeHttps);
}
