namespace SkyBook.Maui;

/// <summary>Runtime configuration (the React client reads these from <c>.env</c> / Vite).</summary>
public static class AppConfig
{
    /// <summary>
    /// Syncfusion license key (https://www.syncfusion.com/account/manage-trials). Leave empty to run with the trial banner.
    /// Do not commit a real key; supply it through your own build/CI mechanism.
    /// </summary>
    public static string SyncfusionLicenseKey { get; } = "Ngo9BigBOggjHTQxAR8/V1JBaF5cXmRCf1FpQHxbf1x2ZFRGalhYTnNWUiweQnxTdENiWH9YcXZQQ2RbUUB1V0leZg==";

    /// <summary>Equivalent of VITE_AGENT_URL. The Android emulator reaches the host machine at 10.0.2.2.</summary>
    public static Uri AgentUrl { get; } = new(
        Environment.GetEnvironmentVariable("SKYBOOK_AGENT_URL")
        ?? (DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:10006" : "http://localhost:10006"));

    /// <summary>Set SKYBOOK_AGENT_URL to use a remote A2A (Python) agent; otherwise the in-app <c>LocalAgent</c> is used.</summary>
    public static Uri? RemoteAgentUrl { get; } =
        Environment.GetEnvironmentVariable("SKYBOOK_AGENT_URL") is { Length: > 0 } u ? new Uri(u) : null;

    /// <summary>
    /// Azure OpenAI for free-text requests (optional). Read from AZURE_OPENAI_ENDPOINT / _API_KEY / _DEPLOYMENT / _API_VERSION,
    /// or from an untracked <c>Resources/Raw/azure-openai.local.json</c>. Never commit a real key.
    /// </summary>
    public static SkyBook.Core.AzureOpenAiOptions? AzureOpenAi { get; } = LoadAzure();

    private static SkyBook.Core.AzureOpenAiOptions? LoadAzure()
    {
        string? E(string n) => Environment.GetEnvironmentVariable(n) is { Length: > 0 } v ? v : null;
        string? endpoint = E("AZURE_OPENAI_ENDPOINT"), key = E("AZURE_OPENAI_API_KEY"), dep = E("AZURE_OPENAI_DEPLOYMENT"), ver = E("AZURE_OPENAI_API_VERSION");
        try
        {
            using var stream = FileSystem.OpenAppPackageFileAsync("azure-openai.local.json").GetAwaiter().GetResult();
            if (System.Text.Json.Nodes.JsonNode.Parse(stream) is System.Text.Json.Nodes.JsonObject o)
            {
                endpoint ??= (string?)o["endpoint"]; key ??= (string?)o["apiKey"]; dep ??= (string?)o["deployment"]; ver ??= (string?)o["apiVersion"];
            }
        }
        catch (Exception) { /* file is optional */ }
        return endpoint is { Length: > 0 } && key is { Length: > 0 } && dep is { Length: > 0 }
            ? new SkyBook.Core.AzureOpenAiOptions(endpoint, key, dep, ver ?? "2024-10-21") : null;
    }

    /// <summary>Viewport width below which the two panes collapse into Chat / Booking-view tabs (React: max-width 980px).</summary>
    public const double CompactWidth = 980;
}
