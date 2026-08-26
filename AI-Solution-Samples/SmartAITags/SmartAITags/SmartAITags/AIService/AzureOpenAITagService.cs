using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace SmartAITags.AIService;

/// <summary>
/// Azure OpenAI tag-generation service.
///
/// Mirrors the SmartAIDatePicker.AzureOpenAIService pattern: a single direct
/// HttpClient POST to the Azure AI Foundry OpenAI-compatible endpoint
/// ({BaseEndpoint}/chat/completions), authenticated via the "api-key" header.
/// No SemanticKernel, no AzureOpenAIClient SDK, no Gemini.
/// </summary>
public sealed class AzureOpenAITagService
{
    // ── Same endpoint + deployment + key as the working SmartAIDatePicker sample ──
    private const string BaseEndpoint =
        "https://your-openai-resource.openai.azure.com/";

    private const string DeploymentName = "your-DeploymentName";

    private const string ApiKey =
        "your-azure-openai-api-key";

    // ── Curated vocabulary. The AI may ONLY return tags from this list,
    //    which guarantees chips are always valid single-word keywords. ──
    public static readonly string[] TagVocabulary =
    {
        // Auth / Security
        "Login","Logout","Authentication","Authorization","Biometric","Fingerprint",
        "FaceID","TouchID","Security","Encryption","Password","OTP","MFA","TwoFactor",
        "Session","Token","OAuth","SSO","Captcha","PIN","Roles","Permissions","Access",
        "Audit","Compliance","GDPR","Privacy","RBAC",
        // UI / UX
        "UI","UX","Dashboard","Profile","Settings","Search","Filter","Sorting","Navigation",
        "Sidebar","Drawer","Tabs","Modal","Carousel","Grid","List","Cards",
        "Accordion","Stepper","Wizard","Onboarding","Splash","Theme","DarkMode","Localization",
        "Accessibility","Responsive","Animation","Transitions","Gestures","Swipe","Tooltips",
        "ContextMenu","Breadcrumbs","Pagination","Snackbar","Badges","Forms",
        // Data / Backend
        "Database","SQLite","Storage","Cache","CRUD","Migration","Backup","Sync","Offline",
        "OfflineFirst","Export","Import","Repository","Validation","Entities",
        // Comms
        "Chat","Messaging","Email","Notifications","Push","SMS","Comments","Feedback","Contacts",
        // Media
        "Upload","Download","Gallery","Camera","Video","Audio","Image","Player","Streaming",
        "Recorder","Files","Documents","Scanner","QRCode",
        // Payments
        "Payment","Cart","Checkout","Subscription","Invoices","Wallet","Refund","Pricing",
        "Billing","Taxes",
        // Maps / Location
        "Maps","Geolocation","Location","Tracking","Routing","Geofencing",
        // Social
        "Sharing","Likes","Follow","Reviews","Ratings","Posts","Feed",
        // Analytics
        "Analytics","Reports","Charts","Metrics","Insights","Telemetry",
        // Realtime
        "Realtime","WebSocket","Live",
        // Integration
        "API","REST","GraphQL","Webhook","Integration","ThirdParty","SDK","CI","CD",
        // Misc product
        "Wishlist","Bookings","Calendar","Scheduler","Tasks","Todos","Reminder",
        "Survey","Quiz","FAQ","Help","Support","Tickets","Loyalty",
        "Rewards","Coupons","Bluetooth","NFC","Sensors","VoIP","Calls",
        "Favorites","History","Bookmarks"
    };

    /// <summary>System prompt (fixed). The user message is the user's description.</summary>
    private static readonly string SystemPrompt =
    "You are an AI tag-classification assistant for a software feature description.\n" +
    "Analyze the MEANING and INTENT of the user's description and return ONLY tags from the vocabulary.\n\n" +
    "Vocabulary (return ONLY tags from this list, case-insensitive): " +
    string.Join(", ", TagVocabulary) + "\n\n" +
    "RULES:\n" +
    "- Return ALL relevant tags from the vocabulary that match the description.\n" +
    "  Do NOT cap the result; a complex description can produce many tags.\n" +
    "  Return as many vocabulary tags as honestly apply to the request.\n" +
    "- One tag per line, TitleCase exactly as in the vocabulary.\n" +
    "- Order tags by relevance (most strongly implied first).\n" +
    "- INFER implied tags: 'login'→Login+Authentication; 'biometric'→Biometric+Security;\n" +
    "  'payment'→Payment; 'chat'→Chat+Messaging; 'map'/'location'→Maps+Geolocation;\n" +
    "  any description involving access control/sensitive data→include Security.\n" +
    "- Do NOT include tags that don't actually apply. Quality over quantity.\n" +
    "- No bullets, dashes, numbering, quotes, headings, or explanations.\n" +
    "- If no vocabulary tag matches the description, return exactly: Empty";

    // Single shared HttpClient for the lifetime of the service.
    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromSeconds(60)
    };

    public string? LastError { get; private set; }
    public string LastRawResponse { get; private set; } = string.Empty;

    public async Task<string> GetCompletion(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        LastError = null;
        LastRawResponse = string.Empty;

        try
        {
            // ── Build the OpenAI chat completions request ──
            // IMPORTANT: gpt-5-mini is a reasoning model. Reasoning models
            // reject the 'temperature' parameter with HTTP 400
            // ("'temperature' is not supported with this model"). Mirror the
            // working SmartAIDatePicker.AzureOpenAIService exactly: send only
            // model + messages + max_completion_tokens + reasoning_effort.
            var requestBody = new
            {
                model = DeploymentName,
                messages = new object[]
                {
                    new { role = "system", content = SystemPrompt },
                    new { role = "user",   content = prompt }
                },
                max_completion_tokens = 1024,
                reasoning_effort = "minimal"
            };

            var url = $"{BaseEndpoint}/chat/completions";

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("api-key", ApiKey);
            request.Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            using var response = await Http.SendAsync(request, cancellationToken);

            var raw = await response.Content.ReadAsStringAsync(cancellationToken);
            LastRawResponse = raw;

            Debug.WriteLine($"[AzureAI] Status: {(int)response.StatusCode}");
            Debug.WriteLine($"[AzureAI] Raw Response: {raw}");

            if (!response.IsSuccessStatusCode)
            {
                LastError = $"HTTP {(int)response.StatusCode} {response.StatusCode}\n\n{raw}";
                return LastError;
            }

            using var document = JsonDocument.Parse(raw);

            if (!document.RootElement.TryGetProperty("choices", out var choices)
                || choices.GetArrayLength() == 0)
            {
                LastError = $"No 'choices' found.\n\nRaw response:\n{raw}";
                return LastError;
            }

            var content = choices[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(content))
            {
                LastError = $"Azure returned empty content.\n\nRaw response:\n{raw}";
                return LastError;
            }

            return content.Trim();
        }
        catch (Exception ex)
        {
            LastError = $"Exception:\n{ex}";
            Debug.WriteLine($"[AzureAI] Exception: {ex}");
            return LastError;
        }
    }
}