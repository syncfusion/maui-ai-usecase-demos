using System.ClientModel;
using System.Text.Json;
using System.Text.RegularExpressions;
using Azure;
using Azure.AI.OpenAI;
using ListView_IntentGrouping.Models;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace ListView_IntentGrouping.Services;

/// <summary>
/// AI analyzer that uses the Azure OpenAI Chat Completions API to
/// categorize items into intents. The chat prompt asks the model
/// to return a strict JSON object whose shape matches the existing
/// <see cref="IntentAnalysisResult"/> so the rest of the app (the
/// view-model, the SfListView, the KPI strip) does not need to
/// change when this analyzer is in use.
///
/// <para>
/// The intent taxonomy sent in the system prompt comes from
/// <see cref="IntentTaxonomies.ForDomain"/>, the same source used by
/// the local rule-based analyzer. This keeps the AI's label space
/// aligned with the per-category icons, colours, and summary
/// templates the UI already knows about.
/// </para>
/// </summary>
public class AzureOpenAIIntentAnalyzer : AzureBaseService, IIntentAnalyzer
{
    private readonly AzureOpenAIClient _client;
    private readonly AzureOpenAIOptions _options;
    private readonly ILogger<AzureOpenAIIntentAnalyzer>? _logger;

    public IntentAnalyzerMode Mode => IntentAnalyzerMode.AzureOpenAI;

    public string DisplayName =>
        $"Azure OpenAI ({(_options.ModelName ?? _options.DeploymentName)})";

    /// <summary>
    /// True when the credentials in <see cref="AzureBaseService"/>
    /// have been filled in. When false, the app refuses to run the
    /// AI intent grouping and shows the invalid-key error banner.
    /// </summary>
    public static bool IsAzureConfigured => IsConfigured;

    /// <summary>
    /// The invalid-key error shown by the UI when the credentials in
    /// <see cref="AzureBaseService"/> are missing or still hold their
    /// placeholder values.
    /// </summary>
    public static string CredentialsError => GetCredentialsError();

    public AzureOpenAIIntentAnalyzer(
        AzureOpenAIClient client,
        AzureOpenAIOptions options,
        ILogger<AzureOpenAIIntentAnalyzer>? logger = null)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
    }

    /// <summary>
    /// Convenience factory used by <c>MauiProgram</c> when the
    /// credentials in <see cref="AzureBaseService"/> are valid.
    /// </summary>
    public static AzureOpenAIIntentAnalyzer Create(
        ILogger<AzureOpenAIIntentAnalyzer>? logger = null)
    {
        return new AzureOpenAIIntentAnalyzer(CreateClient(), CreateOptions(), logger);
    }

    public async Task<IntentAnalysisResult> AnalyzeAsync(
        string prompt,
        string domain,
        IEnumerable<FeedbackItem> items,
        CancellationToken cancellationToken = default)
    {
        var itemsList = items.ToList();
        var intents = IntentTaxonomies.ForDomain(domain);
        var detectedDomain = string.IsNullOrWhiteSpace(domain) ? "Customer Feedback" : domain;

        if (itemsList.Count == 0)
        {
            return new IntentAnalysisResult
            {
                DetectedDomain = detectedDomain,
                TotalItems = 0,
                OverallSummary = $"No items were supplied for {detectedDomain}."
            };
        }

        // ----------------------------------------------------------------
        // 1. Build the chat prompt
        // ----------------------------------------------------------------
        var systemPrompt = BuildSystemPrompt(intents);
        var userPrompt = BuildUserPrompt(prompt, detectedDomain, itemsList);

        // ----------------------------------------------------------------
        // 2. Call Azure OpenAI
        // ----------------------------------------------------------------
        ChatCompletion completion;
        try
        {
            var chat = _client.GetChatClient(_options.DeploymentName);
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userPrompt),
            };
            var chatOptions = new ChatCompletionOptions
            {
                Temperature = _options.Temperature,
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat(),
            };
            if (_options.MaxTokens > 0)
            {
                chatOptions.MaxOutputTokenCount = _options.MaxTokens;
            }
            completion = await chat.CompleteChatAsync(messages, chatOptions, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (RequestFailedException ex)
        {
            _logger?.LogWarning(ex, "Azure OpenAI request failed: {Status}", ex.Status);
            throw new IntentAnalysisException(
                DescribeHttpFailure(ex.Status, ex.ErrorCode, ex.Message), ex);
        }
        catch (ClientResultException ex)
        {
            // Azure.AI.OpenAI 2.x surfaces non-2xx responses as
            // System.ClientModel.ClientResultException (401 = bad
            // key, 404 = wrong deployment/endpoint).
            _logger?.LogWarning(ex, "Azure OpenAI request failed: {Status}", ex.Status);
            throw new IntentAnalysisException(
                DescribeHttpFailure(ex.Status, "Message", ex.Message), ex);
        }
        catch (Exception ex) when (ex is not IntentAnalysisException)
        {
            _logger?.LogWarning(ex, "Azure OpenAI call failed");
            throw new IntentAnalysisException(
                $"Azure OpenAI call failed: {ex.Message}", ex);
        }

        var rawJson = string.Concat(completion.Content.Select(p => p.Text ?? string.Empty));

        // ----------------------------------------------------------------
        // 3. Defensive: confirm the response starts with '{' before we
        //    hand it to System.Text.Json. A non-JSON body is almost
        //    always a misconfigured endpoint or wrong API version; see
        //    the `azure-openai-non-json-response.md` memory note.
        // ----------------------------------------------------------------
        var trimmed = rawJson.TrimStart();
        if (trimmed.Length == 0 || trimmed[0] != '{')
        {
            throw new IntentAnalysisException(
                "Azure OpenAI returned a non-JSON response. Check the " +
                "AZURE_OPENAI_ENDPOINT URL and the deployment name. " +
                "Response starts with: " + Truncate(rawJson, 200));
        }

        // ----------------------------------------------------------------
        // 4. Parse the JSON response into an IntentAnalysisResult
        // ----------------------------------------------------------------
        AiResponsePayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<AiResponsePayload>(
                rawJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException ex)
        {
            throw new IntentAnalysisException(
                "Failed to parse the JSON returned by Azure OpenAI. " +
                "Response starts with: " + Truncate(rawJson, 200),
                ex);
        }
        if (payload is null)
        {
            throw new IntentAnalysisException(
                "Azure OpenAI returned an empty JSON object. " +
                "Response starts with: " + Truncate(rawJson, 200));
        }

        // ----------------------------------------------------------------
        // 5. Build the IntentCategory list and assign each item
        // ----------------------------------------------------------------
        var result = new IntentAnalysisResult
        {
            DetectedDomain = string.IsNullOrWhiteSpace(payload.DetectedDomain) ? detectedDomain : payload.DetectedDomain!,
            TotalItems = itemsList.Count,
            OverallSummary = payload.OverallSummary ?? string.Empty,
            TopThemes = payload.TopThemes ?? new List<string>(),
        };

        var categoryByName = new Dictionary<string, IntentCategory>(StringComparer.OrdinalIgnoreCase);
        var incomingCategories = payload.Categories ?? new List<AiCategoryPayload>();

        // Seed every declared category up front so empty groups still
        // show up in the UI (matching the local analyzer's behaviour).
        foreach (var incoming in incomingCategories)
        {
            var name = string.IsNullOrWhiteSpace(incoming.Name) ? "Other" : incoming.Name!;
            var def = intents.FirstOrDefault(x =>
                x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            var category = new IntentCategory
            {
                Name = def?.Name ?? name,
                Description = def?.Description ?? incoming.Description ?? string.Empty,
                Icon = def?.Icon ?? "\uE73E",
                AccentColor = def?.AccentColor ?? "#5F6368",
                IconBackground = def?.IconBackground ?? "#E8EAED",
                SummaryTemplate = def?.SummaryTemplate
                    ?? "{count} items in this category. Recurring themes: {keywords}.",
                Summary = incoming.Summary ?? string.Empty,
            };
            categoryByName[name] = category;
        }

        // If the AI omitted every category, fall back to the local
        // taxonomy so at least the canonical headers are present.
        if (categoryByName.Count == 0)
        {
            foreach (var def in intents)
            {
                categoryByName[def.Name] = new IntentCategory
                {
                    Name = def.Name,
                    Description = def.Description,
                    Icon = def.Icon,
                    AccentColor = def.AccentColor,
                    IconBackground = def.IconBackground,
                    SummaryTemplate = def.SummaryTemplate,
                };
            }
        }

        // ----------------------------------------------------------------
        // 6. Place each item into its assigned category
        // ----------------------------------------------------------------
        var fallbackName = categoryByName.Keys.FirstOrDefault() ?? intents[0]?.Name ?? "Feedback";
        int unclassified = 0;
        var assignments = payload.Assignments ?? new List<AiAssignmentPayload>();
        var assignmentById = assignments
            .Where(a => !string.IsNullOrWhiteSpace(a.Id))
            .ToDictionary(a => a.Id!, a => a, StringComparer.OrdinalIgnoreCase);

        var overallThemeCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in itemsList)
        {
            AiAssignmentPayload? assignment = null;
            assignmentById.TryGetValue(item.Id, out assignment);
            var assignedName = assignment?.Intent;

            IntentCategory? category;
            if (!string.IsNullOrWhiteSpace(assignedName) &&
                categoryByName.TryGetValue(assignedName, out category))
            {
                // matched
            }
            else
            {
                category = categoryByName[fallbackName];
                unclassified++;
            }

            item.AssignedIntent = category.Name;
            item.Confidence = assignment?.Confidence is double c
                ? Math.Clamp(c, 0.0, 1.0)
                : 0.5;
            item.Priority = ComputePriority(item, category.Name);
            item.GroupKey = category;
            category.Items.Add(item);
            category.AverageRating = (int)Math.Round(category.Items.Average(x => x.Rating));
            category.VisibleCount = category.Items.Count;

            // Tally keyword themes when the AI supplied them.
            if (assignment?.MatchedKeywords is { Count: > 0 } matched)
            {
                foreach (var kw in matched)
                {
                    if (string.IsNullOrWhiteSpace(kw)) continue;
                    overallThemeCounts.TryGetValue(kw, out var existing);
                    overallThemeCounts[kw] = existing + 1;
                }
            }
        }

        // ----------------------------------------------------------------
        // 7. Order categories by item count (most frequent first) and
        //    back-fill any per-category summaries that the AI did not
        //    produce. The local-analyzer summaries are still useful
        //    when the AI returns only the per-item assignments.
        // ----------------------------------------------------------------
        result.Categories = categoryByName.Values
            .OrderByDescending(c => c.Items.Count)
            .ThenBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var category in result.Categories)
        {
            if (string.IsNullOrWhiteSpace(category.Summary))
            {
                var keywords = ExtractTopKeywords(category.Items.Select(x => x.Text), topN: 4);
                var joined = keywords.Count == 0
                    ? "no dominant keywords detected"
                    : string.Join(", ", keywords);
                category.Summary = category.SummaryTemplate
                    .Replace("{count}", category.Items.Count.ToString())
                    .Replace("{keywords}", joined);
            }
        }

        // Merge in keyword tallies from the AI response and fall back
        // to a local extraction when the AI did not provide any.
        if (result.TopThemes.Count == 0 && overallThemeCounts.Count > 0)
        {
            result.TopThemes = overallThemeCounts
                .Where(kv => kv.Value > 1)
                .OrderByDescending(kv => kv.Value)
                .Take(5)
                .Select(kv => $"{kv.Key} ({kv.Value})")
                .ToList();
        }
        if (result.TopThemes.Count == 0)
        {
            var localThemes = ExtractTopKeywords(itemsList.Select(x => x.Text), topN: 5);
            result.TopThemes = localThemes;
        }

        result.UnclassifiedCount = unclassified;
        if (string.IsNullOrWhiteSpace(result.OverallSummary))
        {
            result.OverallSummary = BuildOverallSummary(result);
        }
        return result;
    }

    // ----------------------------------------------------------------
    // Prompt construction
    // ----------------------------------------------------------------

    private static string BuildSystemPrompt(IReadOnlyList<IntentDefinition> intents)
    {
        var taxonomyLines = string.Join("\n", intents.Select(i =>
            $"- \"{i.Name}\": {i.Description}"));
        return
            "You are an AI assistant that categorises short customer " +
            "feedback, support tickets, survey responses, and product " +
            "reviews into intent groups.\n\n" +
            "You MUST choose each item's intent from the following " +
            "taxonomy (use the exact name):\n" +
            taxonomyLines + "\n\n" +
            "Return a single JSON object with this shape:\n" +
            "{\n" +
            "  \"detectedDomain\": string,\n" +
            "  \"overallSummary\": string,\n" +
            "  \"topThemes\": [string, ...],\n" +
            "  \"categories\": [\n" +
            "    {\n" +
            "      \"name\": string,\n" +
            "      \"description\": string,\n" +
            "      \"summary\": string\n" +
            "    }, ...\n" +
            "  ],\n" +
            "  \"assignments\": [\n" +
            "    {\n" +
            "      \"id\": string,\n" +
            "      \"intent\": string,\n" +
            "      \"confidence\": number between 0 and 1,\n" +
            "      \"matchedKeywords\": [string, ...]\n" +
            "    }, ...\n" +
            "  ]\n" +
            "}\n\n" +
            "Rules:\n" +
            "- Every item in the input MUST appear exactly once in " +
            "  assignments with its id.\n" +
            "- intent MUST be one of the taxonomy names above.\n" +
            "- overallSummary should be one or two sentences, suitable " +
            "  for showing in a UI status banner.\n" +
            "- topThemes should be 3 to 5 short recurring terms.\n" +
            "- Return JSON only. No prose, no markdown.";
    }

    private static string BuildUserPrompt(string prompt, string domain, IReadOnlyList<FeedbackItem> items)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Domain: {domain}");
        if (!string.IsNullOrWhiteSpace(prompt))
        {
            sb.AppendLine($"User request: {prompt}");
        }
        sb.AppendLine();
        sb.AppendLine("Items (id | rating | text):");
        foreach (var item in items)
        {
            // Escape pipes and newlines so the model sees a clean row.
            var safeText = (item.Text ?? string.Empty)
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("|", "\\|");
            sb.AppendLine($"- {item.Id} | {item.Rating} | {safeText}");
        }
        return sb.ToString();
    }

    // ----------------------------------------------------------------
    // Scoring helpers (mirrors the local analyzer)
    // ----------------------------------------------------------------

    private static int ComputePriority(FeedbackItem item, string intentName)
    {
        bool criticalIntent = intentName is "Account Management"
            or "Security" or "Billing" or "Performance"
            or "Login Issues" or "Payment Issues" or "Technical Issues"
            or "Negative Feedback" or "Performance & Bugs"
            or "Dissatisfied" or "Customer Support";
        int score = 0;
        if (item.Rating <= 2) score += 2;
        if (item.Rating == 3) score += 1;
        if (criticalIntent && item.Rating <= 2) score += 2;
        if (Regex.IsMatch(item.Text, @"\b(urgent|asap|immediately|critical|emergency|blocked)\b",
                RegexOptions.IgnoreCase))
        {
            score += 1;
        }
        return Math.Clamp(score, 1, 5);
    }

    private static List<string> ExtractTopKeywords(IEnumerable<string> texts, int topN)
    {
        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var raw in texts)
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;
            var tokens = Regex.Split(raw.ToLowerInvariant(), @"[^a-z0-9]+");
            foreach (var token in tokens)
            {
                if (string.IsNullOrWhiteSpace(token)) continue;
                if (token.Length < 4) continue;
                if (IntentTaxonomies.StopWords.Contains(token)) continue;
                if (!char.IsLetter(token[0])) continue;
                counts.TryGetValue(token, out var c);
                counts[token] = c + 1;
            }
        }
        return counts
            .OrderByDescending(kv => kv.Value)
            .ThenBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .Take(topN)
            .Select(kv => kv.Key)
            .ToList();
    }

    private static string BuildOverallSummary(IntentAnalysisResult result)
    {
        var dominant = result.Categories.FirstOrDefault();
        if (dominant == null)
        {
            return $"No clear intent patterns were detected in the supplied {result.DetectedDomain}.";
        }
        var second = result.Categories.Skip(1).FirstOrDefault();
        var phrase = second == null
            ? $"The dataset is dominated by a single intent: {dominant.Name}."
            : $"The two strongest intents are {dominant.Name} ({dominant.Items.Count} items) and {second.Name} ({second.Items.Count} items).";
        var themes = result.TopThemes.Count == 0
            ? string.Empty
            : $" Most frequent terms: {string.Join(", ", result.TopThemes)}.";
        return $"{result.DetectedDomain} were grouped into {result.Categories.Count} intent categories covering {result.TotalItems} items. {phrase}{themes}";
    }

    private static string Truncate(string s, int max)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        return s.Length <= max ? s : s.Substring(0, max);
    }

    /// <summary>
    /// Translate an HTTP failure from the Azure OpenAI endpoint into
    /// a user-actionable message. The 401 / 404 / 429 cases are the
    /// ones users hit most often:
    ///   401 — the API key and endpoint are from different resources
    ///         (or the key was rotated / is invalid).
    ///   404 — the deployment name in
    ///         <see cref="AzureBaseService.DeploymentName"/> does not
    ///         match any deployment on the resource.
    ///
    /// The messages include the active endpoint hostname and the
    /// first-4 / last-4 characters of the active API key so the user
    /// can immediately spot a key/endpoint mismatch — the most
    /// common cause of HTTP 401.
    /// </summary>
    private static string DescribeHttpFailure(
        int? status, string? errorCode, string message)
    {
        // Pull the *currently active* credentials so the error
        // message reflects exactly what the SDK is sending. These
        // helpers exist on AzureBaseService — they are safe to call
        // because the analyzer is only invoked after IsConfigured
        // passed in MauiProgram.
        var host = AzureBaseService.HostFromEndpoint(
            AzureBaseService.Endpoint);
        var fingerprint = AzureBaseService.KeyFingerprint;
        var deployment = AzureBaseService.DeploymentName;

        switch (status)
        {
            case 401:
                return "Invalid API key (HTTP 401). The Azure OpenAI endpoint " +
                       "rejected the key. The two most common causes:\n" +
                       "  1) The API key and Endpoint are from DIFFERENT Azure " +
                       "OpenAI resources. Copy the KEY from the SAME resource's " +
                       "\"Keys and Endpoint\" blade whose hostname appears in " +
                       "the Endpoint setting.\n" +
                       "  2) The key has been rotated/regenerated in the Azure " +
                       "portal; the cached copy is now stale.\n" +
                       $"\nCurrently active: host={host}, deployment={deployment}, " +
                       $"key={fingerprint}.";
            case 404 when (errorCode ?? string.Empty).Contains("Deployment", StringComparison.OrdinalIgnoreCase):
                return $"Deployment not found (HTTP 404). The deployment " +
                       $"'{deployment}' does not exist on the Azure OpenAI " +
                       $"resource at {host}. Open the Azure portal → Azure " +
                       $"OpenAI → your resource → 'Model deployments' and copy " +
                       $"the EXACT deployment name (it is NOT the underlying model " +
                       $"name like 'gpt-4o' — it is what you typed when creating " +
                       $"the deployment). Currently active: key={fingerprint}.";
            case 404:
                return $"Resource not found (HTTP 404). The endpoint {host} " +
                       $"does not resolve to an Azure OpenAI resource. Verify " +
                       $"the resource name in the URL (typically " +
                       $"'<resource>.openai.azure.com'). Currently active: " +
                       $"deployment={deployment}, key={fingerprint}.";
            case 429:
                return "Rate limit / quota exceeded (HTTP 429). Wait a moment " +
                       "and try again, or raise the Tokens-Per-Minute quota on " +
                       "the Azure OpenAI deployment in the Azure portal.";
            default:
                return $"Azure OpenAI call failed (HTTP {status} {errorCode}) " +
                       $"against {host}. {message}";
        }
    }

    // ----------------------------------------------------------------
    // DTOs that match the JSON contract sent to / returned by the model
    // ----------------------------------------------------------------
    private sealed class AiResponsePayload
    {
        public string? DetectedDomain { get; set; }
        public string? OverallSummary { get; set; }
        public List<string>? TopThemes { get; set; }
        public List<AiCategoryPayload>? Categories { get; set; }
        public List<AiAssignmentPayload>? Assignments { get; set; }
    }

    private sealed class AiCategoryPayload
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Summary { get; set; }
    }

    private sealed class AiAssignmentPayload
    {
        public string? Id { get; set; }
        public string? Intent { get; set; }
        public double? Confidence { get; set; }
        public List<string>? MatchedKeywords { get; set; }
    }
}
