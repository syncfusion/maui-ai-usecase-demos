using Azure;
using Azure.AI.OpenAI;

namespace ListView_IntentGrouping.Services;

/// <summary>
/// Base class that holds the Azure OpenAI credentials used by the
/// sample. Replace the constants below with the Endpoint,
/// DeploymentName, and API key values from your own Azure OpenAI
/// resource (Azure Portal → Azure OpenAI → your resource →
/// "Keys and Endpoint" blade).
///
/// <para>
/// When the constants are left untouched (or an invalid key is
/// supplied), <see cref="IsConfigured"/> returns false and the app
/// surfaces an "invalid key" error banner instead of performing the
/// AI intent grouping. Fill in real credentials and the intent
/// grouping runs through Azure OpenAI.
/// </para>
/// </summary>
public abstract class AzureBaseService
{
    /// <summary>
    /// The endpoint of your Azure OpenAI resource, including the
    /// trailing slash, e.g. <c>https://your-resource.openai.azure.com/</c>.
    /// </summary>
    internal const string Endpoint = "Your Endpoint";

    /// <summary>
    /// The deployment name as configured on the Azure OpenAI
    /// resource (NOT the underlying model name). For example, a
    /// deployment that backs the <c>gpt-4o</c> model might simply be
    /// named <c>gpt-4o</c>.
    /// </summary>
    internal const string DeploymentName = "Your deployment name";

    /// <summary>
    /// API key copied from the "Keys and Endpoint" blade of the same
    /// Azure OpenAI resource whose hostname appears in
    /// <see cref="Endpoint"/>.
    /// </summary>
    internal const string Key = "API key";

    public AzureBaseService()
    {
    }

    /// <summary>
    /// True when none of the credential constants still hold their
    /// placeholder values and all three required values are
    /// populated. The MAUI startup code uses this to decide whether
    /// the AI intent grouping can run, or whether the app should show
    /// the invalid-key error instead.
    /// </summary>
    public static bool IsConfigured =>
        HasRealValue(Endpoint, "YOUR_END_POINT_NAME", requireHttpUri: true) &&
        HasRealValue(DeploymentName, "DEPLOYMENT_NAME", requireHttpUri: false) &&
        HasRealValue(Key, "API_KEY", requireHttpUri: false, minLength: 20);

    /// <summary>
    /// Return true when the value is non-empty, not a recognised
    /// placeholder, and (optionally) looks like a valid https URL or
    /// has the minimum length of an API key.
    /// </summary>
    private static bool HasRealValue(
        string? value,
        string placeholder,
        bool requireHttpUri,
        int minLength = 0)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var trimmed = value.Trim();
        if (trimmed.Equals(placeholder, StringComparison.OrdinalIgnoreCase)) return false;
        if (requireHttpUri)
        {
            return Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) &&
                   (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
        }
        if (minLength > 0 && trimmed.Length < minLength) return false;
        return true;
    }

    /// <summary>
    /// Human-readable list of any credential problems, used in the
    /// invalid-key error banner. Returns an empty string when the
    /// credentials look valid.
    /// </summary>
    public static string GetCredentialsError()
    {
        if (IsConfigured)
        {
            return string.Empty;
        }
        return "Invalid Azure OpenAI credentials. Update Endpoint, DeploymentName, and Key " +
               "in Services/AzureBaseService.cs, then restart the app to enable " +
               "AI intent grouping.";
    }

    /// <summary>
    /// Build the options object consumed by
    /// <see cref="AzureOpenAIIntentAnalyzer"/> from the credential
    /// constants in this class.
    /// </summary>
    public static AzureOpenAIOptions CreateOptions() => new()
    {
        Endpoint = Endpoint,
        DeploymentName = DeploymentName,
        ApiKey = Key,
        Temperature = 0f,
        MaxTokens = 4096,
    };

    /// <summary>
    /// Create an <see cref="AzureOpenAIClient"/> from the credential
    /// constants. Only call this when <see cref="IsConfigured"/> is
    /// true; a placeholder endpoint is not a parseable URI.
    /// </summary>
    public static AzureOpenAIClient CreateClient()
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException(
                "Azure OpenAI credentials have not been configured. " +
                "Set Endpoint, DeploymentName, and Key in " +
                "Services/AzureBaseService.cs.");
        }
        return new AzureOpenAIClient(
            new Uri(Endpoint),
            new AzureKeyCredential(Key));
    }

    // ----------------------------------------------------------------
    // Diagnostics
    //
    // Two helpers that the analyzer + view-model use to render a
    // startup hint AND a one-line "this is the credential currently
    // in use" summary. They surface the *exact* endpoint hostname
    // and the first/last few characters of the active API key so a
    // user pasting values from two different resources (the most
    // common cause of HTTP 401) can spot the mismatch at a glance.
    // ----------------------------------------------------------------

    /// <summary>
    /// Compact, safe-to-log summary of the currently-active
    /// credentials. Shows the endpoint hostname and the first 4 +
    /// last 4 characters of the API key joined with ellipsis (the
    /// full key is never logged). Useful for status banners, error
    /// messages and startup diagnostics.
    /// </summary>
    public static string ActiveCredentialSummary =>
        $"host={HostFromEndpoint(Endpoint)}, key={KeyFingerprint}, deployment={DeploymentName}";

    /// <summary>
    /// Fire-once banner shown in the UI on first launch. Tells the
    /// user exactly which endpoint / key / deployment the app is
    /// about to use.
    /// </summary>
    public static string GetStartupBanner()
    {
        if (!IsConfigured)
        {
            return "Azure OpenAI credentials are not configured; the app is " +
                   "using the bundled local rule-based analyzer.";
        }

        var host = HostFromEndpoint(Endpoint);
        var fingerprint = KeyFingerprint;
        var deployment = DeploymentName;

        return $"Azure OpenAI target: {host} (deployment '{deployment}', key {fingerprint}).";
    }

    /// <summary>
    /// First 4 + "…" + last 4 of the API key. Used in error messages
    /// so the user can verify which key the app is sending without
    /// exposing the full secret in logs / UI.
    /// </summary>
    public static string KeyFingerprint
    {
        get
        {
            var key = Key ?? string.Empty;
            if (key.Length <= 8)
            {
                return string.IsNullOrEmpty(key) ? "(none)" : "(too short to fingerprint)";
            }
            return key.Substring(0, 4) + "…" + key.Substring(key.Length - 4);
        }
    }

    /// <summary>
    /// Extract just the host portion of an Azure OpenAI endpoint
    /// (e.g. <c>https://my-resource.openai.azure.com/</c> =>
    /// <c>my-resource.openai.azure.com</c>). Defensive against
    /// non-parseable URLs.
    /// </summary>
    public static string HostFromEndpoint(string? endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint)) return "(missing)";
        if (Uri.TryCreate(endpoint.Trim(), UriKind.Absolute, out var uri))
        {
            return uri.Host;
        }
        return endpoint.Trim();
    }
}
