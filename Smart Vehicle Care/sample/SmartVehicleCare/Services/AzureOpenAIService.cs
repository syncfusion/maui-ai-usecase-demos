using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SmartVehicleCare.Services;

internal class AzureOpenAIService
{
    // Base endpoint — uses the Azure AI Foundry /openai/v1 project endpoint
    const string BaseEndpoint          = "API_ENDPOINT";
    const string DefaultDeploymentName = "gpt-5-mini";

    internal const string SecureStorageKey       = "AzureOpenAI_ApiKey";
    internal const string DeploymentPreferenceKey = "AzureOpenAI_DeploymentName";

    private static string _deploymentName = DefaultDeploymentName;
    internal static string DeploymentName
    {
        get => string.IsNullOrWhiteSpace(_deploymentName) ? DefaultDeploymentName : _deploymentName;
        set => _deploymentName = value?.Trim() ?? DefaultDeploymentName;
    }

    internal static void LoadDeploymentName()
        => _deploymentName = Preferences.Get(DeploymentPreferenceKey, DefaultDeploymentName);

    internal static void SaveDeploymentName(string? name)
    {
        _deploymentName = string.IsNullOrWhiteSpace(name) ? DefaultDeploymentName : name.Trim();
        Preferences.Set(DeploymentPreferenceKey, _deploymentName);
    }

    // In-memory key — populated via SecureStorage; never stored in source code
    private static string _apiKey = string.Empty;

    // Fired whenever the key is saved or cleared so subscribers can auto-refresh
    internal static event Action? ApiKeyChanged;

    internal static bool HasApiKey => !string.IsNullOrWhiteSpace(_apiKey);

    internal static void SetApiKey(string? key) => _apiKey = key?.Trim() ?? string.Empty;

    internal static async Task LoadApiKeyFromStorageAsync()
    {
        try   { _apiKey = await SecureStorage.GetAsync(SecureStorageKey) ?? string.Empty; }
        catch { _apiKey = string.Empty; }
    }

    internal static async Task SaveApiKeyToStorageAsync(string? key)
    {
        _apiKey = key?.Trim() ?? string.Empty;
        try
        {
            if (string.IsNullOrEmpty(_apiKey))
                SecureStorage.Remove(SecureStorageKey);
            else
                await SecureStorage.SetAsync(SecureStorageKey, _apiKey);
        }
        catch { /* SecureStorage unavailable — key held in memory only */ }
        ApiKeyChanged?.Invoke();
    }

    internal string? LastError       { get; private set; }
    internal string  LastRawResponse { get; private set; } = string.Empty;

    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(30) };

    internal async Task<string> GetResultsFromAI(string userPrompt, string systemMessage = "You are a helpful assistant.")
    {
        LastError = null;
        LastRawResponse = string.Empty;
        try
        {
            if (!HasApiKey)
            {
                LastError = "No API key configured. Set your Azure OpenAI key in Settings.";
                return string.Empty;
            }

            var body = new
            {
                model    = DeploymentName,
                messages = new object[]
                {
                    new { role = "system", content = systemMessage },
                    new { role = "user",   content = userPrompt },
                },
                max_completion_tokens = 8000,
            };

            var url     = $"{BaseEndpoint}/chat/completions";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("api-key", _apiKey);
            request.Content = new StringContent(
                JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var response = await Http.SendAsync(request);
            var raw      = await response.Content.ReadAsStringAsync();
            LastRawResponse = raw; // always capture for diagnostics

            if (!response.IsSuccessStatusCode)
            {
                LastError = $"HTTP {(int)response.StatusCode} — {raw[..Math.Min(120, raw.Length)]}";
                System.Diagnostics.Debug.WriteLine($"[AzureAI] {LastError}");
                return string.Empty;
            }

            // Parse choices[0].message.content from the OpenAI response envelope
            var doc     = JsonDocument.Parse(raw);
            if (!doc.RootElement.TryGetProperty("choices", out var choices))
            {
                LastError = $"No 'choices' in response. Raw: {raw[..Math.Min(300, raw.Length)]}";
                return string.Empty;
            }
            var content = choices[0]
                             .GetProperty("message")
                             .GetProperty("content")
                             .GetString() ?? string.Empty;

            // Strip any code fences the model may have added
            content = Regex.Replace(content, @"^```(?:json)?\s*", string.Empty, RegexOptions.Multiline);
            content = Regex.Replace(content, @"```\s*$",          string.Empty, RegexOptions.Multiline);
            content = content.Trim();

            System.Diagnostics.Debug.WriteLine($"[AzureAI] Response: {content[..Math.Min(200, content.Length)]}");
            return content;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            System.Diagnostics.Debug.WriteLine($"[AzureAI] Exception: {ex.Message}");
            return string.Empty;
        }
    }
}