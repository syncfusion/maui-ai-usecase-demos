namespace SkyBook.Maui;

/// <summary>Runtime configuration (the React client reads these from <c>.env</c> / Vite).</summary>
public static class AppConfig
{
  /// <summary>
  /// Syncfusion license key (https://www.syncfusion.com/account/manage-trials). Read from the SYNCFUSION_LICENSE_KEY
  /// environment variable or from an untracked <c>Resources/Raw/syncfusion-license.local.txt</c> (the file is the
  /// option that works on Android / iOS devices). Empty = the app runs with the trial banner. Never commit a real key.
  /// </summary>
  public static string SyncfusionLicenseKey { get; } = LoadLicense();

  private static string LoadLicense()
  {
    if (Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY") is { Length: > 0 } fromEnv) return fromEnv.Trim();
    try
    {
      using var stream = FileSystem.OpenAppPackageFileAsync("syncfusion-license.local.txt").GetAwaiter().GetResult();
      using var reader = new StreamReader(stream);
      return reader.ReadToEnd().Trim();
    }
    catch (Exception) { return ""; } // file is optional
  }

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
    string? E(string n) => Environment.GetEnvironmentVariable(n)?.Trim() is { Length: > 0 } v ? v : null;
    string? endpoint = E(""), key = E(""), dep = E(""), ver = E("");
    try
    {
      using var stream = FileSystem.OpenAppPackageFileAsync("azure-openai.local.json").GetAwaiter().GetResult();
      if (System.Text.Json.Nodes.JsonNode.Parse(stream) is System.Text.Json.Nodes.JsonObject o)
      {
        endpoint ??= ((string?)o["endpoint"])?.Trim(); key ??= ((string?)o["apiKey"])?.Trim(); dep ??= ((string?)o["deployment"])?.Trim(); ver ??= ((string?)o["apiVersion"])?.Trim();
      }
    }
    catch (Exception) { /* file is optional */ }
    return endpoint is { Length: > 0 } && key is { Length: > 0 } && dep is { Length: > 0 }
        ? new SkyBook.Core.AzureOpenAiOptions(endpoint, key, dep, ver ?? "2024-11-20") : null;
  }

  /// <summary>Viewport width below which the two panes collapse into Chat / Booking-view tabs (React: max-width 980px).</summary>
  public const double CompactWidth = 980;
}
