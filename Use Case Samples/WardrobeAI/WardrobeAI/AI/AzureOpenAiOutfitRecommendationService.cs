using System.Text.Json;
using System.Text.Json.Serialization;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using WardrobeAI.AI.Contracts;
using WardrobeAI.AI.Scoring;
using WardrobeAI.Helpers;

namespace WardrobeAI.AI;

/// <summary>
/// Azure OpenAI implementation of <see cref="IAIOutfitRecommendationService"/>.
/// Calls the Chat Completions endpoint with JSON-schema structured outputs
/// bound to the <see cref="LlmOutfitResponse"/> shape below; re-validates
/// every returned <c>itemId</c> against the request closet before
/// returning; recomputes the numeric <c>MatchScore</c> client-side via
/// <see cref="OutfitScorer"/>.
///
/// <para>The service is intentionally tolerant of misconfiguration: until the
/// user fills in <see cref="AzureOpenAiOptions.Endpoint"/>,
/// <see cref="AzureOpenAiOptions.ApiKey"/> and
/// <see cref="AzureOpenAiOptions.Deployment"/>, calls fail fast with
/// <c>Result.Fail("AI.NotConfigured", "...")</c> so the dashboard can fall
/// back to its static placeholder copy without throwing.</para>
/// </summary>
public sealed class AzureOpenAiOutfitRecommendationService : IAIOutfitRecommendationService
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    // JSON schema sent to the LLM via response_format.json_schema.
    // The schema is hand-written (not generated via JsonSchemaExporter) so it
    // stays stable across SDK upgrades and can be copy-pasted into the Azure
    // OpenAI "Structured outputs" playground for testing.
    private const string ResponseJsonSchema = @"{
        ""type"": ""object"",
        ""additionalProperties"": false,
        ""required"": [""primary"", ""alternatives""],
        ""properties"": {
            ""primary"": {
                ""type"": ""object"",
                ""additionalProperties"": false,
                ""required"": [""title"", ""occasion"", ""items"", ""reason"", ""matchScore""],
                ""properties"": {
                    ""title"":       { ""type"": ""string"", ""maxLength"": 80 },
                    ""occasion"":    { ""type"": ""string"" },
                    ""reason"":      { ""type"": ""string"", ""maxLength"": 320 },
                    ""matchScore"":  { ""type"": ""integer"", ""minimum"": 0, ""maximum"": 100 },
                    ""items"": {
                        ""type"": ""array"",
                        ""minItems"": 1,
                        ""items"": {
                            ""type"": ""object"",
                            ""additionalProperties"": false,
                            ""required"": [""itemId"", ""role""],
                            ""properties"": {
                                ""itemId"": { ""type"": ""integer"", ""minimum"": 0 },
                                ""role"":   { ""type"": ""string"", ""enum"": [""Top"", ""Bottom"", ""Footwear"", ""Outerwear"", ""Accessory"", ""Dress""] }
                            }
                        }
                    }
                }
            },
            ""alternatives"": {
                ""type"": ""array"",
                ""maxItems"": 2,
                ""items"": { ""$ref"": ""#/$defs/suggestion"" }
            }
        },
        ""$defs"": {
            ""suggestion"": {
                ""type"": ""object"",
                ""additionalProperties"": false,
                ""required"": [""title"", ""occasion"", ""items"", ""reason"", ""matchScore""],
                ""properties"": {
                    ""title"":      { ""type"": ""string"", ""maxLength"": 80 },
                    ""occasion"":   { ""type"": ""string"" },
                    ""reason"":     { ""type"": ""string"", ""maxLength"": 320 },
                    ""matchScore"": { ""type"": ""integer"", ""minimum"": 0, ""maximum"": 100 },
                    ""items"": {
                        ""type"": ""array"",
                        ""minItems"": 1,
                        ""items"": {
                            ""type"": ""object"",
                            ""additionalProperties"": false,
                            ""required"": [""itemId"", ""role""],
                            ""properties"": {
                                ""itemId"": { ""type"": ""integer"", ""minimum"": 0 },
                                ""role"":   { ""type"": ""string"", ""enum"": [""Top"", ""Bottom"", ""Footwear"", ""Outerwear"", ""Accessory"", ""Dress""] }
                            }
                        }
                    }
                }
            }
        }
    }";

    // JSON schema for the per-occasion Outfit Planner response. The model
    // returns one entry per requested category (Work / Meeting / Casual /
    // Party) so the planner page can render all four tabs in one shot.
    private const string MultiCategoryJsonSchema = @"{
        ""type"": ""object"",
        ""additionalProperties"": false,
        ""required"": [""entries""],
        ""properties"": {
            ""entries"": {
                ""type"": ""array"",
                ""minItems"": 1,
                ""items"": { ""$ref"": ""#/$defs/categoryEntry"" }
            }
        },
        ""$defs"": {
            ""categoryEntry"": {
                ""type"": ""object"",
                ""additionalProperties"": false,
                ""required"": [""category"", ""primary"", ""alternatives"", ""whyThisWorks""],
                ""properties"": {
                    ""category"":      { ""type"": ""string"", ""minLength"": 1, ""maxLength"": 32 },
                    ""whyThisWorks"":  { ""type"": ""string"", ""maxLength"": 360 },
                    ""primary"":       { ""$ref"": ""#/$defs/suggestion"" },
                    ""alternatives"":  {
                        ""type"": ""array"",
                        ""maxItems"": 2,
                        ""items"": { ""$ref"": ""#/$defs/suggestion"" }
                    }
                }
            },
            ""suggestion"": {
                ""type"": ""object"",
                ""additionalProperties"": false,
                ""required"": [""title"", ""occasion"", ""items"", ""reason"", ""matchScore""],
                ""properties"": {
                    ""title"":      { ""type"": ""string"", ""maxLength"": 80 },
                    ""occasion"":   { ""type"": ""string"" },
                    ""reason"":     { ""type"": ""string"", ""maxLength"": 320 },
                    ""matchScore"": { ""type"": ""integer"", ""minimum"": 0, ""maximum"": 100 },
                    ""items"": {
                        ""type"": ""array"",
                        ""minItems"": 1,
                        ""items"": {
                            ""type"": ""object"",
                            ""additionalProperties"": false,
                            ""required"": [""itemId"", ""role""],
                            ""properties"": {
                                ""itemId"": { ""type"": ""integer"", ""minimum"": 0 },
                                ""role"":   { ""type"": ""string"", ""enum"": [""Top"", ""Bottom"", ""Footwear"", ""Outerwear"", ""Accessory"", ""Dress""] }
                            }
                        }
                    }
                }
            }
        }
    }";

    private readonly AzureOpenAIClient? _client;
    private readonly IOptions<AzureOpenAiOptions> _options;
    private readonly ILogger<AzureOpenAiOutfitRecommendationService> _logger;

    public AzureOpenAiOutfitRecommendationService(
        IOptions<AzureOpenAiOptions> options,
        ILogger<AzureOpenAiOutfitRecommendationService> logger)
    {
        _options = options;
        _logger = logger;

        var opts = options.Value;
        if (opts.IsConfigured)
        {
            _client = new AzureOpenAIClient(new Uri(opts.Endpoint), new AzureKeyCredential(opts.ApiKey));
        }
    }

    public async Task<Result<OutfitRecommendationResult>> GetDailyOutfitSuggestionAsync(
        OutfitRecommendationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return Result<OutfitRecommendationResult>.Fail("AI.NullRequest", "Request was null.");
        }

        var opts = _options.Value;
        if (!opts.IsConfigured || _client is null)
        {
            return Result<OutfitRecommendationResult>.Fail(
                "AI.NotConfigured",
                "Azure OpenAI is not configured. Add Endpoint, ApiKey, and Deployment to use AI suggestions.");
        }

        if (request.Closet.Count == 0)
        {
            return Result<OutfitRecommendationResult>.Fail(
                "AI.EmptyCloset",
                "Your closet is empty. Add a few clothing items to receive an AI suggestion.");
        }

        try
        {
            var chatClient = _client.GetChatClient(opts.Deployment);
            var messages = new ChatMessage[]
            {
                new SystemChatMessage(DailyOutfitPromptBuilder.SystemPrompt),
                new UserChatMessage(DailyOutfitPromptBuilder.BuildUserPrompt(request))
            };
            var chatOptions = new ChatCompletionOptions
            {
                ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                    "outfit_suggestion",
                    BinaryData.FromString(ResponseJsonSchema),
                    jsonSchemaIsStrict: true),
            };
            var completion = await chatClient.CompleteChatAsync(messages, chatOptions, cancellationToken);
            var messageContent = completion.Value.Content.FirstOrDefault()?.Text;

            LlmOutfitResponse? parsed;
            try
            {
                if (string.IsNullOrWhiteSpace(messageContent))
                {
                    return Result<OutfitRecommendationResult>.Fail(
                        "AI.EmptyResponse", "Azure OpenAI returned an empty message body.");
                }

                parsed = JsonSerializer.Deserialize<LlmOutfitResponse>(messageContent!, JsonOpts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse Azure OpenAI response body.");
                return Result<OutfitRecommendationResult>.Fail(
                    "AI.ParseError", "Could not parse the AI response.");
            }

            if (parsed is null || parsed.Primary is null)
            {
                return Result<OutfitRecommendationResult>.Fail(
                    "AI.Malformed", "AI response did not include a primary outfit.");
            }

            // Re-validate item ids against the request closet (prevents prompt
            // injection / hallucinated ids from leaking into the UI).
            var closetById = request.Closet.ToDictionary(c => c.Id);
            var primary = MaterializeSuggestion(parsed.Primary, request, closetById, isPrimary: true);
            var alternatives = (parsed.Alternatives ?? Array.Empty<LlmSuggestion>())
                .Take(2)
                .Select(a => MaterializeSuggestion(a, request, closetById, isPrimary: false))
                .Where(s => s.Items.Count > 0)
                .ToList();

            if (primary.Items.Count == 0)
            {
                return Result<OutfitRecommendationResult>.Fail(
                    "AI.NoValidItems", "The AI picked no items that exist in your closet.");
            }

            return Result<OutfitRecommendationResult>.Ok(new OutfitRecommendationResult(
                Primary: primary,
                Alternatives: alternatives,
                WeatherEcho: request.Weather,
                OccasionEcho: request.Occasion));
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Azure OpenAI outfit request timed out.");
            return Result<OutfitRecommendationResult>.Fail("AI.Timeout", "The AI request timed out.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Azure OpenAI HTTP request failed.");
            return Result<OutfitRecommendationResult>.Fail("AI.Network", "Could not reach Azure OpenAI.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetDailyOutfitSuggestionAsync.");
            return Result<OutfitRecommendationResult>.Fail("AI.Failed", ex.Message);
        }
    }

    /// <summary>
    /// Translates the LLM's raw suggestion into the strongly-typed
    /// <see cref="OutfitSuggestion"/> contract. Drops any item whose id
    /// isn't in the request closet; recomputes the numeric <c>MatchScore</c>
    /// via <see cref="OutfitScorer"/> so it matches the deterministic
    /// pipeline (LLM score is kept only as a soft fallback).
    /// </summary>
    private static OutfitSuggestion MaterializeSuggestion(
        LlmSuggestion raw,
        OutfitRecommendationRequest request,
        Dictionary<int, ClothingItemSummary> closetById,
        bool isPrimary,
        string? occasionOverride = null)
    {
        var occasion = !string.IsNullOrWhiteSpace(occasionOverride) ? occasionOverride : request.Occasion;
        var validItems = (raw.Items ?? Array.Empty<LlmItemPick>())
            .Where(i => closetById.ContainsKey(i.ItemId))
            .Select(i => new OutfitItemPick(i.ItemId, NormalizeRole(i.Role)))
            .ToList();

        // Preserve a stable display order: Top → Dress → Bottom → Outerwear → Footwear → Accessory.
        validItems = validItems
            .OrderBy(i => SlotOrder(i.Role))
            .ToList();

        var pickedSummaries = validItems
            .Select(i => closetById[i.ItemId])
            .ToList();

        // Recompute every per-axis score client-side so the planner's
        // Match / Style / Comfort / Trend gauges line up with the
        // deterministic scorer (not the LLM's mood).
        var scores = pickedSummaries.Count > 0
            ? OutfitScorer.ScoreAll(
                pickedSummaries,
                occasion,
                request.StylePreference,
                request.Weather.TemperatureC)
            : null;

        var deterministic = scores?.Match
            ?? Math.Clamp(raw.MatchScore ?? 70, 0, 100);

        return new OutfitSuggestion(
            Title: string.IsNullOrWhiteSpace(raw.Title) ? $"{occasion} Look" : raw.Title!.Trim(),
            Occasion: string.IsNullOrWhiteSpace(raw.Occasion) ? occasion : raw.Occasion!,
            Items: validItems,
            Reason: string.IsNullOrWhiteSpace(raw.Reason)
                ? "Picked from your closet to match the occasion and current weather."
                : raw.Reason!.Trim(),
            MatchScore: deterministic,
            Scores: scores);
    }

    public async Task<Result<OutfitPlanResult>> GetOutfitsForCategoriesAsync(
        OutfitRecommendationRequest request,
        IReadOnlyList<string> categories,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return Result<OutfitPlanResult>.Fail("AI.NullRequest", "Request was null.");
        }

        if (categories is null || categories.Count == 0)
        {
            return Result<OutfitPlanResult>.Fail(
                "AI.NoCategories",
                "At least one occasion category is required.");
        }

        var opts = _options.Value;
        // Configuration check is intentionally not enforced: the dashboard /
        // planner must still render a usable plan when Azure OpenAI hasn't
        // been wired up yet. The deterministic fallback below produces a
        // sensible outfit for every requested category from the user's closet.

        if (request.Closet.Count == 0)
        {
            return Result<OutfitPlanResult>.Fail(
                "AI.EmptyCloset",
                "Your closet is empty. Add a few clothing items to receive an AI suggestion.");
        }

        // Fast path: not configured → deterministic fallback so the planner
        // never blocks on a missing Azure subscription.
        if (!opts.IsConfigured)
        {
            _logger.LogInformation(
                "Azure OpenAI not configured; using deterministic fallback for {Count} categories.",
                categories.Count);
            var fallbackEntries = BuildDeterministicPlan(request, categories);
            return Result<OutfitPlanResult>.Ok(new OutfitPlanResult(
                Entries: fallbackEntries,
                WeatherEcho: request.Weather));
        }

        try
        {
            var chatClient = _client!.GetChatClient(opts.Deployment);
            var messages = new ChatMessage[]
            {
                new SystemChatMessage(DailyOutfitPromptBuilder.PlannerSystemPrompt),
                new UserChatMessage(DailyOutfitPromptBuilder.BuildPlannerUserPrompt(request, categories))
            };
            var chatOptions = new ChatCompletionOptions
            {
                ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                    "outfit_plan",
                    BinaryData.FromString(MultiCategoryJsonSchema),
                    jsonSchemaIsStrict: true)
            };
            var completion = await chatClient.CompleteChatAsync(messages, chatOptions, cancellationToken);
            var messageContent = completion.Value.Content.FirstOrDefault()?.Text;

            LlmPlanResponse? parsed;
            try
            {
                if (string.IsNullOrWhiteSpace(messageContent))
                {
                    var fb0 = BuildDeterministicPlan(request, categories);
                    return Result<OutfitPlanResult>.Ok(new OutfitPlanResult(fb0, request.Weather));
                }

                parsed = JsonSerializer.Deserialize<LlmPlanResponse>(messageContent!, JsonOpts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse Azure OpenAI planner response.");
                var fb = BuildDeterministicPlan(request, categories);
                return Result<OutfitPlanResult>.Ok(new OutfitPlanResult(fb, request.Weather));
            }

            if (parsed is null || parsed.Entries is null || parsed.Entries.Length == 0)
            {
                var fb = BuildDeterministicPlan(request, categories);
                return Result<OutfitPlanResult>.Ok(new OutfitPlanResult(fb, request.Weather));
            }

            // Re-validate item ids and materialize per category. Categories
            // that the LLM didn't return get a deterministic fallback so every
            // requested tab still has an outfit.
            var closetById = request.Closet.ToDictionary(c => c.Id);
            var byCategory = new Dictionary<string, LlmCategoryEntry>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in parsed.Entries)
            {
                if (string.IsNullOrWhiteSpace(entry.Category))
                {
                    continue;
                }
                byCategory[entry.Category.Trim()] = entry;
            }

            var materialized = new List<OutfitPlanEntry>(categories.Count);
            foreach (var category in categories)
            {
                var key = category.Trim();
                if (byCategory.TryGetValue(key, out var llm) && llm.Primary is not null)
                {
                    var entry = MaterializeCategoryEntry(key, llm, request, closetById);
                    if (entry is not null)
                    {
                        materialized.Add(entry);
                        continue;
                    }
                }
                // Fallback per category — keeps the UI populated even if the
                // LLM skipped a slot.
                var fallback = BuildDeterministicPlan(
                    request with { Occasion = key },
                    new[] { key }).FirstOrDefault();
                  if (fallback is not null)
                {
                    materialized.Add(fallback);
                }
            }

            // If for some reason we still have no entries, top up with the
            // fully-deterministic plan to guarantee a non-empty result.
            if (materialized.Count == 0)
            {
                materialized.AddRange(BuildDeterministicPlan(request, categories));
            }

            return Result<OutfitPlanResult>.Ok(new OutfitPlanResult(
                Entries: materialized,
                WeatherEcho: request.Weather));
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Azure OpenAI outfit planner request timed out.");
            var fb = BuildDeterministicPlan(request, categories);
            return Result<OutfitPlanResult>.Ok(new OutfitPlanResult(fb, request.Weather));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Azure OpenAI HTTP request failed for outfit planner.");
            var fb = BuildDeterministicPlan(request, categories);
            return Result<OutfitPlanResult>.Ok(new OutfitPlanResult(fb, request.Weather));
        }
        catch (Exception ex)
        {
            // Last-resort safety net: even unexpected exceptions (e.g. a raw
            // "Socket closed" IOException that doesn't surface as
            // HttpRequestException, a JsonException leaking out of the
            // deserializer, or a transient TLS reset) must not blank the
            // planner. Fall back to the deterministic plan so the user
            // always gets a usable result and we can investigate the
            // root cause from the log without losing the page render.
            _logger.LogError(ex, "Unexpected error in GetOutfitsForCategoriesAsync.");
            var fb = BuildDeterministicPlan(request, categories);
            return Result<OutfitPlanResult>.Ok(new OutfitPlanResult(fb, request.Weather));
        }
    }

    /// <summary>
    /// Translates one <see cref="LlmCategoryEntry"/> into the strongly-typed
    /// <see cref="OutfitPlanEntry"/>. Returns null when the LLM failed to
    /// pick any items that exist in the closet (caller falls back to the
    /// deterministic pipeline).
    /// </summary>
    private static OutfitPlanEntry? MaterializeCategoryEntry(
        string category,
        LlmCategoryEntry raw,
        OutfitRecommendationRequest request,
        Dictionary<int, ClothingItemSummary> closetById)
    {
        var occasionRequest = request with { Occasion = category };
        var primary = MaterializeSuggestion(
            raw.Primary!,
            occasionRequest,
            closetById,
            isPrimary: true,
            occasionOverride: category);
        if (primary.Items.Count == 0)
        {
            return null;
        }

        var alternatives = (raw.Alternatives ?? Array.Empty<LlmSuggestion>())
            .Take(2)
            .Select(a => MaterializeSuggestion(
                a,
                occasionRequest,
                closetById,
                isPrimary: false,
                occasionOverride: category))
            .Where(s => s.Items.Count > 0)
            .ToList();

        var why = string.IsNullOrWhiteSpace(raw.WhyThisWorks)
            ? BuildWhyThisWorks(primary, request, category)
            : raw.WhyThisWorks!.Trim();

        return new OutfitPlanEntry(
            Category: category,
            Primary: primary,
            Alternatives: alternatives,
            WhyThisWorks: why);
    }

    /// <summary>
    /// Deterministic, no-LLM outfit plan. Picks the top-reasoning outfit for
    /// every requested category by scoring every permutation of the user's
    /// closet (top + bottom + footwear) and keeping the highest-scoring
    /// combination. Returns a primary outfit + 2 alternatives + a
    /// synthesised "Why this works" blurb.
    /// </summary>
    private static List<OutfitPlanEntry> BuildDeterministicPlan(
        OutfitRecommendationRequest request,
        IReadOnlyList<string> categories)
    {
        var entries = new List<OutfitPlanEntry>(categories.Count);
        var closet = request.Closet;

        var tops = closet.Where(IsTop).ToList();
        var bottoms = closet.Where(IsBottom).ToList();
        var dresses = closet.Where(IsDress).ToList();
        var footwear = closet.Where(IsFootwear).ToList();
        var outerwear = closet.Where(IsOuterwear).ToList();

        foreach (var category in categories)
        {
            var key = category.Trim();
            var occasionRequest = request with { Occasion = key };

            // Build every candidate combo (top + bottom + footwear OR a single
            // dress + footwear) and score each. Cap to a reasonable upper
            // bound to keep this O(n) on small closets; the closet is
            // typically < 100 items, so the Cartesian product is bounded.
            var candidates = new List<List<ClothingItemSummary>>();

            if (dresses.Count > 0 && footwear.Count > 0)
            {
                foreach (var dress in dresses)
                {
                    foreach (var shoe in footwear)
                    {
                        candidates.Add(new List<ClothingItemSummary> { dress, shoe });
                    }
                }
            }
            if (tops.Count > 0 && bottoms.Count > 0 && footwear.Count > 0)
            {
                foreach (var top in tops)
                {
                    foreach (var bottom in bottoms)
                    {
                        foreach (var shoe in footwear)
                        {
                            candidates.Add(new List<ClothingItemSummary> { top, bottom, shoe });
                        }
                    }
                }
            }

            // Cap candidates — sort by deterministic score first, then trim.
            if (candidates.Count > 200)
            {
                candidates = candidates
                    .OrderByDescending(c => OutfitScorer.ScoreAll(c, key, request.StylePreference, request.Weather.TemperatureC).Match)
                    .Take(200)
                    .ToList();
            }

            var scored = candidates
                .Select(c =>
                {
                    var s = OutfitScorer.ScoreAll(c, key, request.StylePreference, request.Weather.TemperatureC);
                    return (Items: c, Scores: s);
                })
                .OrderByDescending(t => t.Scores.Match)
                .ToList();

            if (scored.Count == 0)
            {
                // Closet doesn't have the right slots for this occasion —
                // emit an empty entry so the UI can show a friendly placeholder.
                entries.Add(new OutfitPlanEntry(
                    Category: key,
                    Primary: new OutfitSuggestion(
                        Title: $"{key} Look",
                        Occasion: key,
                        Items: Array.Empty<OutfitItemPick>(),
                        Reason: "Add a top, a bottom, and footwear to your closet to unlock this look.",
                        MatchScore: 0,
                        Scores: null),
                    Alternatives: Array.Empty<OutfitSuggestion>(),
                    WhyThisWorks: "Your closet doesn't have the pieces for a complete look yet."));
                continue;
            }

            OutfitPlanEntry BuildEntry(List<(List<ClothingItemSummary> Items, OutfitScores Scores)> top3, string title)
            {
                var primary = top3[0];
                var pickItems = primary.Items
                    .Select(i => new OutfitItemPick(i.Id, MapCategoryToRole(i.Category)))
                    .ToList();
                var alts = top3.Skip(1)
                    .Select(t => new OutfitSuggestion(
                        Title: $"{title} · Alt",
                        Occasion: key,
                        Items: t.Items
                            .Select(i => new OutfitItemPick(i.Id, MapCategoryToRole(i.Category)))
                            .ToList(),
                        Reason: "Alternate combination of the same pieces.",
                        MatchScore: t.Scores.Match,
                        Scores: t.Scores))
                    .ToList();
                return new OutfitPlanEntry(
                    Category: key,
                    Primary: new OutfitSuggestion(
                        Title: title,
                        Occasion: key,
                        Items: pickItems,
                        Reason: BuildReason(primary.Items, occasionRequest),
                        MatchScore: primary.Scores.Match,
                        Scores: primary.Scores),
                    Alternatives: alts,
                    WhyThisWorks: BuildWhyThisWorks(
                        new OutfitSuggestion(
                            Title: title,
                            Occasion: key,
                            Items: pickItems,
                            Reason: string.Empty,
                            MatchScore: primary.Scores.Match,
                            Scores: primary.Scores),
                        occasionRequest,
                        key));
            }

            // For each top-3 candidate, prefer picks that include outerwear
            // in cold/rainy weather.
            if (outerwear.Count > 0 && (string.Equals(request.Weather.Condition, "Rainy", StringComparison.OrdinalIgnoreCase) || string.Equals(request.Weather.Condition, "Snowy", StringComparison.OrdinalIgnoreCase) || request.Weather.TemperatureC <= 15))
            {
                scored = scored
                    .OrderByDescending(t =>
                        t.Items.Any(IsOuterwear) ? t.Scores.Match + 4 : t.Scores.Match)
                    .ToList();
            }

            var top3 = scored.Take(3).ToList();
            entries.Add(BuildEntry(top3, ComposeTitle(key, top3[0].Items)));
        }

        return entries;
    }

    /// <summary>
    /// Synthesises a 1-2 sentence "Why this works" blurb from the deterministic
    /// scores. Used by both the fallback and as the LLM-string default.
    /// </summary>
    private static string BuildWhyThisWorks(OutfitSuggestion primary, OutfitRecommendationRequest request, string category)
    {
        if (primary.Items.Count == 0)
        {
            return "Add a few more pieces to your closet to unlock a complete look.";
        }

        var scores = primary.Scores;
        if (scores is null)
        {
            return $"Picked from your closet to match the {category.ToLowerInvariant()} occasion.";
        }

        var strengths = new List<string>();
        if (scores.Occasion >= 80)
        {
            strengths.Add($"the pieces are clearly tagged for {category.ToLowerInvariant()} occasions");
        }
        if (scores.Weather >= 80)
        {
            strengths.Add($"the fabrics line up with the current {request.Weather.Condition.ToLowerInvariant()} weather");
        }
        if (scores.Comfort >= 80)
        {
            strengths.Add("breathable fabrics keep you comfortable all day");
        }
        if (scores.Style >= 80)
        {
            strengths.Add("the silhouette matches your style preference");
        }
        if (scores.Trend >= 80)
        {
            strengths.Add("the look stays on-trend for the category");
        }
        if (strengths.Count == 0)
        {
            return $"Picked from your closet to match the {category.ToLowerInvariant()} occasion; the colors play well together for a clean silhouette.";
        }
        return $"This {category.ToLowerInvariant()} look works because {string.Join("; ", strengths.Take(3))}.";
    }

    private static string BuildReason(IReadOnlyList<ClothingItemSummary> items, OutfitRecommendationRequest request)
    {
        if (items.Count == 0)
        {
            return "Picked from your closet to match the occasion and current weather.";
        }

        var tops = items.Where(i => IsTop(i) || IsDress(i)).Take(2).Select(i => i.Name);
        var footwear = items.FirstOrDefault(IsFootwear);
        var fragments = new List<string> { string.Join(" + ", tops) };
        if (footwear is not null)
        {
            fragments.Add($"{footwear.Name} for a {request.Weather.Condition.ToLowerInvariant()} day");
        }
        return $"{string.Join(", ", fragments)} — chosen for the {request.Occasion.ToLowerInvariant()} occasion.";
    }

    private static string ComposeTitle(string category, IReadOnlyList<ClothingItemSummary> primary)
    {
        if (primary.Count == 0)
        {
            return $"{category} Look";
        }

        return category.Trim() switch
        {
            "Work" => "Executive Workday",
            "Meeting" => "Client-Ready Meeting",
            "Casual" => "Relaxed Casual",
            "Party" => "After-Hours Party",
            _ => $"{category} Look"
        };
    }

    private static bool IsTop(ClothingItemSummary i) =>
        i.Category is "T-Shirts" or "Shirts" or "Polo Shirts" or "Hoodies"
            or "Sweatshirts" or "Sweaters" or "Blazers" or "Jackets" or "Coats"
            or "Kurtas" or "Ethnic Wear" or "Activewear";

    private static bool IsBottom(ClothingItemSummary i) =>
        i.Category is "Jeans" or "Trousers" or "Shorts" or "Joggers"
            or "Leggings" or "Skirts";

    private static bool IsDress(ClothingItemSummary i) =>
        i.Category is "Dresses" or "Sarees";

    private static bool IsFootwear(ClothingItemSummary i) =>
        i.Category is not "Dresses" and not "Sarees" and not "Kurtas" and not "Ethnic Wear"
            && (i.Name?.Contains("shoe", StringComparison.OrdinalIgnoreCase) == true
                || i.Name?.Contains("sneaker", StringComparison.OrdinalIgnoreCase) == true
                || i.Name?.Contains("boot", StringComparison.OrdinalIgnoreCase) == true
                || i.Name?.Contains("loafer", StringComparison.OrdinalIgnoreCase) == true
                || i.Name?.Contains("heel", StringComparison.OrdinalIgnoreCase) == true
                || i.Name?.Contains("sandal", StringComparison.OrdinalIgnoreCase) == true);

    private static bool IsOuterwear(ClothingItemSummary i) =>
        i.Category is "Blazers" or "Jackets" or "Coats";

    private static string MapCategoryToRole(string category) => category switch
    {
        "T-Shirts" or "Shirts" or "Polo Shirts" or "Hoodies" or "Sweatshirts"
            or "Sweaters" or "Kurtas" or "Ethnic Wear" or "Activewear" => "Top",
        "Jeans" or "Trousers" or "Shorts" or "Joggers" or "Leggings" or "Skirts" => "Bottom",
        "Dresses" or "Sarees" => "Dress",
        "Blazers" or "Jackets" or "Coats" => "Outerwear",
        _ => "Top"
    };

    private static int SlotOrder(string role) => role switch
    {
        "Top" => 0,
        "Dress" => 1,
        "Bottom" => 2,
        "Outerwear" => 3,
        "Footwear" => 4,
        "Accessory" => 5,
        _ => 99
    };

    private static string NormalizeRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return "Top";
        }

        return role.Trim() switch
        {
            "Top" => "Top",
            "Bottom" => "Bottom",
            "Footwear" => "Footwear",
            "Outerwear" => "Outerwear",
            "Accessory" => "Accessory",
            "Dress" => "Dress",
            _ => "Top"
        };
    }

    // ---- DTOs that match the JSON schema sent to the model ------------------

    private sealed class LlmOutfitResponse
    {
        [JsonPropertyName("primary")] public LlmSuggestion? Primary { get; set; }
        [JsonPropertyName("alternatives")] public LlmSuggestion[]? Alternatives { get; set; }
    }

    private sealed class LlmSuggestion
    {
        [JsonPropertyName("title")] public string? Title { get; set; }
        [JsonPropertyName("occasion")] public string? Occasion { get; set; }
        [JsonPropertyName("reason")] public string? Reason { get; set; }
        [JsonPropertyName("matchScore")] public int? MatchScore { get; set; }
        [JsonPropertyName("items")] public LlmItemPick[]? Items { get; set; }
    }

    private sealed class LlmItemPick
    {
        [JsonPropertyName("itemId")] public int ItemId { get; set; }
        [JsonPropertyName("role")] public string? Role { get; set; }
    }

    // ---- DTOs for the per-occasion planner response ------------------------

    private sealed class LlmPlanResponse
    {
        [JsonPropertyName("entries")] public LlmCategoryEntry[]? Entries { get; set; }
    }

    private sealed class LlmCategoryEntry
    {
        [JsonPropertyName("category")] public string? Category { get; set; }
        [JsonPropertyName("whyThisWorks")] public string? WhyThisWorks { get; set; }
        [JsonPropertyName("primary")] public LlmSuggestion? Primary { get; set; }
        [JsonPropertyName("alternatives")] public LlmSuggestion[]? Alternatives { get; set; }
    }
}
