using System.ClientModel;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;

namespace SmartAITags;

/// <summary>
/// Azure OpenAI tag-generation service.
///
/// Mirrors the SmartAIDatePicker.AzureOpenAIService pattern: a single direct
/// HttpClient POST to the Azure AI Foundry OpenAI-compatible endpoint
/// ({BaseEndpoint}/chat/completions), authenticated via the "api-key" header.
/// No SemanticKernel, no AzureOpenAIClient SDK, no Gemini.
///
/// Errors are surfaced as exceptions (the caller wraps in try/catch and
/// publishes to its bound ErrorMessage); the service never returns a
/// magic-prefixed error string.
/// </summary>
public sealed class AzureOpenAITagService : IAzureTagService
{
    // ── Same endpoint + deployment + key as the working SmartAIDatePicker sample.
    private static readonly string Endpoint =
        Environment.GetEnvironmentVariable("AZURE_API_BASE") ?? "";

    private const string DeploymentName = "gpt-4.1-mini";

    private static readonly string ApiKey =
        Environment.GetEnvironmentVariable("AZURE_API_KEY") ?? "";

    // Curated vocabulary. The AI may ONLY return tags from this list,
    //    which guarantees chips are always valid single-word keywords.
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
        // Data / Backend.
        "Database","SQLite","Storage","Cache","CRUD","Migration","Backup","Sync","Offline",
        "OfflineFirst","Export","Import","Repository","Validation","Entities",
        // Comms.
        "Chat","Messaging","Email","Notifications","Push","SMS","Comments","Feedback","Contacts",
        // Media.
        "Upload","Download","Gallery","Camera","Video","Audio","Image","Player","Streaming",
        "Recorder","Files","Documents","Scanner","QRCode",
        // Payments.
        "Payment","Cart","Checkout","Subscription","Invoices","Wallet","Refund","Pricing",
        "Billing","Taxes",
        // Maps / Location.
        "Maps","Geolocation","Location","Tracking","Routing","Geofencing",
        // Social.
        "Sharing","Likes","Follow","Reviews","Ratings","Posts","Feed",
        // Analytics.
        "Analytics","Reports","Charts","Metrics","Insights","Telemetry",
        // Realtime.
        "Realtime","WebSocket","Live",
        // Integration.
        "API","REST","GraphQL","Webhook","Integration","ThirdParty","SDK","CI","CD",
        // Misc product.
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

    /// <summary>
    /// Sends the user prompt through the Azure OpenAI SDK and returns the raw
    /// completion text. Throws on request failures so the caller can surface
    /// them through its own error channel.
    /// </summary>
    public async Task<string> GetCompletionAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        var client = new AzureOpenAIClient(new Uri(Endpoint), new AzureKeyCredential(ApiKey));
        var chatClient = client.GetChatClient(DeploymentName);
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(SystemPrompt),
            new UserChatMessage(prompt)
        };

        ChatCompletion completion;
        try
        {
            completion = await chatClient.CompleteChatAsync(
                messages,
                cancellationToken: cancellationToken);
        }
        catch (ClientResultException ex) when (ex.Status == 401)
        {
            throw new InvalidOperationException(
                "Azure OpenAI returned 401. Verify that the API key belongs to the resource at the configured endpoint.", ex);
        }
        catch (ClientResultException ex) when (ex.Status == 404)
        {
            throw new InvalidOperationException(
                $"Azure OpenAI returned 404. Verify that deployment '{DeploymentName}' exists on the configured resource.", ex);
        }

        var content = string.Concat(completion.Content.Select(part => part.Text));
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("Azure OpenAI returned empty content.");

        return content.Trim();
    }
}
