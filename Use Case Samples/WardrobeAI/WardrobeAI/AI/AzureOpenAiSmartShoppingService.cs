using System.Text.Json;
using System.Text.Json.Serialization;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using WardrobeAI.AI.Contracts;
using WardrobeAI.Helpers;

namespace WardrobeAI.AI;

/// <summary>
/// Azure OpenAI implementation of <see cref="IAISmartShoppingService"/>.
/// Calls the Chat Completions endpoint with JSON-schema structured outputs
/// bound to the <c>smart_shopping_picks</c> shape; re-validates every
/// returned <c>name</c> against the request catalog before returning; and
/// falls back to a deterministic (but properly randomized) selection when
/// Azure is not configured / the LLM fails / the network is down so the
/// Shopping page never goes blank.
///
/// <para>Randomness is critical: the user reported that the previous
/// "first 5" behavior produced the same 5 cards every page load
/// (because the catalog is JSON-ordered and <c>Take(5)</c> returned the
/// same prefix). This service randomises via a per-call
/// <see cref="Random"/> instance so consecutive loads see different
/// orders even when the underlying closet hasn't changed.</para>
/// </summary>
public sealed class AzureOpenAiSmartShoppingService : IAISmartShoppingService
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly AzureOpenAIClient? _client;
    private readonly IOptions<AzureOpenAiOptions> _options;
    private readonly ILogger<AzureOpenAiSmartShoppingService> _logger;

    public AzureOpenAiSmartShoppingService(
        IOptions<AzureOpenAiOptions> options,
        ILogger<AzureOpenAiSmartShoppingService> logger)
    {
        _options = options;
        _logger = logger;

        var opts = options.Value;
        if (opts.IsConfigured)
        {
            _client = new AzureOpenAIClient(new Uri(opts.Endpoint), new AzureKeyCredential(opts.ApiKey));
        }
    }

    /// <inheritdoc />
    public async Task<Result<AISmartShoppingResult>> GetSmartShoppingPicksAsync(
        AISmartShoppingRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return Result<AISmartShoppingResult>.Fail("AI.NullRequest", "Request was null.");
        }

        if (request.Catalog is null || request.Catalog.Count == 0)
        {
            return Result<AISmartShoppingResult>.Fail(
                "AI.EmptyCatalog",
                "The essential-closet catalog is empty.");
        }

        if (request.MaxResults <= 0)
        {
            return Result<AISmartShoppingResult>.Ok(new AISmartShoppingResult(
                Picks: Array.Empty<AISmartShoppingPick>(),
                AnalysisSummary: string.Empty));
        }

        var opts = _options.Value;

        // Fast path: not configured → deterministic + randomized fallback so
        // the Shopping page still renders a useful set of cards without
        // paying the Azure round-trip cost.
        if (!opts.IsConfigured)
        {
            _logger.LogInformation(
                "Azure OpenAI not configured; using deterministic randomized fallback for smart shopping picks.");
            return Result<AISmartShoppingResult>.Ok(BuildDeterministicRandomizedPicks(request));
        }

        try
        {
            if (_client is null)
            {
                return Result<AISmartShoppingResult>.Ok(BuildDeterministicRandomizedPicks(request));
            }

            var chatClient = _client.GetChatClient(opts.Deployment);
            var messages = new ChatMessage[]
            {
                new SystemChatMessage(SmartShoppingPromptBuilder.SystemPrompt),
                new UserChatMessage(SmartShoppingPromptBuilder.BuildUserPrompt(request))
            };
            var chatOptions = new ChatCompletionOptions
            {
                ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                    "smart_shopping_picks",
                    BinaryData.FromString(SmartShoppingPromptBuilder.ResponseJsonSchema),
                    jsonSchemaIsStrict: true),
                Temperature = (float)opts.Temperature,
                MaxOutputTokenCount = opts.MaxOutputTokens
            };
            var completion = await chatClient.CompleteChatAsync(messages, chatOptions, cancellationToken);
            var messageContent = completion.Value.Content.FirstOrDefault()?.Text;

            LlmSmartShoppingResponse? parsed;
            try
            {
                if (string.IsNullOrWhiteSpace(messageContent))
                {
                    _logger.LogWarning("Azure OpenAI returned an empty message body for smart shopping picks.");
                    return Result<AISmartShoppingResult>.Ok(BuildDeterministicRandomizedPicks(request));
                }

                parsed = JsonSerializer.Deserialize<LlmSmartShoppingResponse>(messageContent!, JsonOpts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse Azure OpenAI smart-shopping response.");
                return Result<AISmartShoppingResult>.Ok(BuildDeterministicRandomizedPicks(request));
            }

            if (parsed is null || parsed.Picks is null || parsed.Picks.Length == 0)
            {
                _logger.LogWarning("Azure OpenAI smart-shopping response had no picks; falling back.");
                return Result<AISmartShoppingResult>.Ok(BuildDeterministicRandomizedPicks(request));
            }

            // Re-validate names against the request catalog (prevents
            // prompt-injection / hallucinated picks from leaking into the UI)
            // and dedupe by name so the same item can't be picked twice.
            var catalogByName = new Dictionary<string, SmartShoppingCatalogItem>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in request.Catalog)
            {
                if (string.IsNullOrWhiteSpace(entry.Name))
                {
                    continue;
                }
                catalogByName[entry.Name.Trim()] = entry;
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var picks = new List<AISmartShoppingPick>(parsed.Picks.Length);
            foreach (var raw in parsed.Picks)
            {
                if (string.IsNullOrWhiteSpace(raw.Name))
                {
                    continue;
                }
                var key = raw.Name!.Trim();
                if (!catalogByName.ContainsKey(key))
                {
                    _logger.LogDebug("Discarding LLM pick '{Name}' (not in catalog).", key);
                    continue;
                }
                if (!seen.Add(key))
                {
                    // Same item picked twice — silently skip the second.
                    continue;
                }

                var cat = catalogByName[key];
                var pairs = (raw.PairsWithNames ?? Array.Empty<string>())
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Select(n => n.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(3)
                    .ToList();

                picks.Add(new AISmartShoppingPick(
                    CatalogName: key,
                    Category: string.IsNullOrWhiteSpace(raw.Category) ? cat.Category : raw.Category!.Trim(),
                    Priority: NormalizePriority(raw.Priority),
                    PairsWithNames: pairs,
                    Reason: string.IsNullOrWhiteSpace(raw.Reason)
                        ? BuildDefaultReason(cat, request)
                        : raw.Reason!.Trim()));
            }

            if (picks.Count == 0)
            {
                _logger.LogWarning("All LLM picks failed catalog validation; falling back to deterministic selection.");
                return Result<AISmartShoppingResult>.Ok(BuildDeterministicRandomizedPicks(request));
            }

            // Cap at MaxResults and keep the LLM's randomized order (the
            // prompt explicitly asked for a non-catalog-ordered response).
            var capped = picks.Take(request.MaxResults).ToList();
            var summary = string.IsNullOrWhiteSpace(parsed.AnalysisSummary)
                ? BuildDeterministicSummary(request)
                : parsed.AnalysisSummary!.Trim();

            return Result<AISmartShoppingResult>.Ok(new AISmartShoppingResult(
                Picks: capped,
                AnalysisSummary: summary));
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Azure OpenAI smart-shopping request timed out.");
            return Result<AISmartShoppingResult>.Ok(BuildDeterministicRandomizedPicks(request));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Azure OpenAI HTTP request failed for smart-shopping.");
            return Result<AISmartShoppingResult>.Ok(BuildDeterministicRandomizedPicks(request));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetSmartShoppingPicksAsync.");
            return Result<AISmartShoppingResult>.Ok(BuildDeterministicRandomizedPicks(request));
        }
    }

    /// <summary>
    /// Deterministic + randomized fallback used when Azure OpenAI is not
    /// configured, returns 5xx, or returns a malformed response. Picks
    /// the user's missing categories first, scores each catalog row by
    /// how many of the user's existing items it would pair with, and
    /// randomises the order so consecutive loads see different cards.
    /// </summary>
    private static AISmartShoppingResult BuildDeterministicRandomizedPicks(AISmartShoppingRequest request)
    {
        var rng = new Random(unchecked(Environment.TickCount ^ request.UserId));

        // Per-category ownership of the user's closet.
        var ownedByCategory = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var anchorByCategory = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in request.Closet)
        {
            if (string.IsNullOrWhiteSpace(item.Category))
            {
                continue;
            }
            ownedByCategory[item.Category] = ownedByCategory.TryGetValue(item.Category, out var n) ? n + 1 : 1;
            if (!anchorByCategory.ContainsKey(item.Category))
            {
                anchorByCategory[item.Category] = item.Name;
            }
        }

        // Track owned-names so we can skip catalog rows the user already
        // owns (e.g. user already added a "White Crew Neck T-Shirt" to
        // their closet — don't recommend it again).
        var ownedNames = request.Closet
            .Where(c => !string.IsNullOrWhiteSpace(c.Name))
            .Select(c => c.Name.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Score + annotate each catalog row.
        var scored = new List<(SmartShoppingCatalogItem Item, int Score, string Priority, List<string> Pairs)>();
        foreach (var entry in request.Catalog)
        {
            if (string.IsNullOrWhiteSpace(entry.Name))
            {
                continue;
            }
            if (ownedNames.Contains(entry.Name.Trim()))
            {
                continue;
            }

            var pairings = ResolvePairingsForCategory(entry.Category, ownedByCategory, anchorByCategory);
            var ownedInCategory = ownedByCategory.TryGetValue(entry.Category, out var n) ? n : 0;
            var score = (pairings.Count * 10) + ComputeMissingnessBonus(entry.Category, ownedByCategory);

            var priority = (ownedInCategory == 0 && pairings.Count >= 1) ? "HIGH"
                : pairings.Count >= 2 ? "MEDIUM"
                : "LOW";

            scored.Add((entry, score, priority, pairings));
        }

        if (scored.Count == 0)
        {
            return new AISmartShoppingResult(
                Picks: Array.Empty<AISmartShoppingPick>(),
                AnalysisSummary: "Your closet covers every essential category. Focus on rotation and care to maximise cost-per-wear.");
        }

        // Order by score desc, then randomize within each score bucket so
        // consecutive loads don't surface the same 5 cards.
        var ordered = scored
            .OrderByDescending(s => s.Score)
            .ToList();

        // Group by score and shuffle each bucket.
        var shuffled = new List<(SmartShoppingCatalogItem Item, int Score, string Priority, List<string> Pairs)>(ordered.Count);
        var bucketStart = 0;
        while (bucketStart < ordered.Count)
        {
            var currentScore = ordered[bucketStart].Score;
            var bucketEnd = bucketStart;
            while (bucketEnd < ordered.Count && ordered[bucketEnd].Score == currentScore)
            {
                bucketEnd++;
            }
            var bucket = ordered.GetRange(bucketStart, bucketEnd - bucketStart);
            ShuffleInPlace(bucket, rng);
            shuffled.AddRange(bucket);
            bucketStart = bucketEnd;
        }

        var picks = new List<AISmartShoppingPick>(request.MaxResults);
        var categoriesPicked = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (item, score, priority, pairings) in shuffled)
        {
            if (picks.Count >= request.MaxResults)
            {
                break;
            }
            // Diversify: at most one pick per category, unless the user
            // is missing the same category and the catalog exposes more
            // than one variant. (For the 21-item catalog, the only
            // category with two entries is T-Shirts — we let both
            // through.)
            if (!IsMultiVariantCategory(item.Category) && !categoriesPicked.Add(item.Category))
            {
                continue;
            }
            picks.Add(new AISmartShoppingPick(
                CatalogName: item.Name.Trim(),
                Category: item.Category,
                Priority: priority,
                PairsWithNames: pairings.Take(3).ToList(),
                Reason: BuildDeterministicReason(item, pairings, request, ownedByCategory)));
        }

        // If diversification + maxResults left us short, top up from the
        // remaining shuffled pool.
        if (picks.Count < request.MaxResults)
        {
            var pickedNames = picks.Select(p => p.CatalogName).ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var (item, score, priority, pairings) in shuffled)
            {
                if (picks.Count >= request.MaxResults)
                {
                    break;
                }
                if (pickedNames.Contains(item.Name))
                {
                    continue;
                }
                picks.Add(new AISmartShoppingPick(
                    CatalogName: item.Name.Trim(),
                    Category: item.Category,
                    Priority: priority,
                    PairsWithNames: pairings.Take(3).ToList(),
                    Reason: BuildDeterministicReason(item, pairings, request, ownedByCategory)));
                pickedNames.Add(item.Name);
            }
        }

        return new AISmartShoppingResult(
            Picks: picks,
            AnalysisSummary: BuildDeterministicSummary(request));
    }

    /// <summary>
    /// Fisher-Yates shuffle. <paramref name="rng"/> is the per-call
    /// random source created in <see cref="BuildDeterministicRandomizedPicks"/>.
    /// </summary>
    private static void ShuffleInPlace<T>(IList<T> list, Random rng)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    /// <summary>
    /// Resolves the user's owned items that would pair with a new piece
    /// in <paramref name="category"/>. Returns the anchor names
    /// (first-owned-per-complementary-category) so the LLM-equivalent
    /// reason can name them verbatim.
    /// </summary>
    private static List<string> ResolvePairingsForCategory(
        string category,
        IReadOnlyDictionary<string, int> ownedByCategory,
        IReadOnlyDictionary<string, string> anchorByCategory)
    {
        var complementary = ComplementaryCategories(category);
        var pairings = new List<string>();
        foreach (var c in complementary)
        {
            if (ownedByCategory.TryGetValue(c, out var n) && n > 0
                && anchorByCategory.TryGetValue(c, out var anchor)
                && !string.IsNullOrWhiteSpace(anchor))
            {
                pairings.Add(anchor);
            }
        }
        return pairings;
    }

    /// <summary>
    /// Categories the new piece would pair with. Mirrors the
    /// <c>WardrobeEssentialsCatalog.CountPotentialPairings</c> map so
    /// the deterministic fallback produces pairings consistent with the
    /// non-AI "Pairs with N existing outfits" chip on the card.
    /// </summary>
    private static IReadOnlyList<string> ComplementaryCategories(string category) => category switch
    {
        "T-Shirts" or "Shirts" or "Polo Shirts" => new[] { "Jeans", "Trousers", "Shorts" },
        "Jeans" or "Trousers" or "Shorts" => new[] { "T-Shirts", "Shirts", "Polo Shirts", "Blazers" },
        "Blazers" => new[] { "Shirts", "T-Shirts", "Polo Shirts", "Jeans", "Trousers" },
        "Jackets" or "Hoodies" => new[] { "T-Shirts", "Shirts", "Jeans" },
        "Kurtas" => new[] { "Trousers", "Jeans" },
        "Footwear" => new[] { "Jeans", "Trousers", "Shorts" },
        "Accessories" => new[] { "Jeans", "Trousers" },
        _ => Array.Empty<string>(),
    };

    /// <summary>
    /// Extra "missingness" bonus added to a catalog row's score. High
    /// for foundational slots the user owns zero of (Blazers / Trousers /
    /// Footwear), low for situational slots (Accessories / Activewear).
    /// Mirrors <c>WardrobeAnalysisService.PriorityByCategory</c> so the
    /// fallback surfaces the same "foundational first" feel the analysis
    /// service does.
    /// </summary>
    private static int ComputeMissingnessBonus(
        string category,
        IReadOnlyDictionary<string, int> ownedByCategory)
    {
        var owned = ownedByCategory.TryGetValue(category, out var n) ? n : 0;
        if (owned > 0)
        {
            return -5; // already own something in this category
        }

        return category switch
        {
            "Blazers" or "Trousers" or "Footwear" or "Shirts" or "Jeans" => 30,
            "T-Shirts" or "Polo Shirts" or "Sweaters" => 20,
            "Kurtas" or "Sarees" or "Ethnic Wear" => 15,
            "Shorts" or "Joggers" => 10,
            "Accessories" or "Activewear" or "Sleepwear" or "Innerwear" or "Swimwear" => 5,
            _ => 10,
        };
    }

    /// <summary>
    /// Returns true for catalog categories that expose more than one
    /// variant in <c>wardrobe.json</c> (T-Shirts has 3 color variants,
    /// Shirts has 4). The diversification check lets all variants through
    /// while still collapsing single-variant categories (Blazers,
    /// Trousers, …) to one card.
    /// </summary>
    private static bool IsMultiVariantCategory(string category) =>
        string.Equals(category, "T-Shirts", StringComparison.OrdinalIgnoreCase)
        || string.Equals(category, "Shirts", StringComparison.OrdinalIgnoreCase);

    private static string BuildDeterministicReason(
        SmartShoppingCatalogItem item,
        IReadOnlyList<string> pairings,
        AISmartShoppingRequest request,
        IReadOnlyDictionary<string, int> ownedByCategory)
    {
        if (request.Closet.Count == 0)
        {
            return $"Build a foundational wardrobe starting with {item.Name}.";
        }

        if (pairings.Count >= 2)
        {
            return $"Pairs with your {string.Join(" and ", pairings.Take(2))} for instant new outfits.";
        }
        if (pairings.Count == 1)
        {
            return $"Pairs with your {pairings[0]} to round out your {item.Category.ToLowerInvariant()} rotation.";
        }

        var owned = ownedByCategory.TryGetValue(item.Category, out var n) ? n : 0;
        if (owned == 0)
        {
            return $"You have no {item.Category.ToLowerInvariant()} yet — {item.Name} is a foundational slot.";
        }

        return $"Diversify your {item.Category.ToLowerInvariant()} rotation with {item.Name}.";
    }

    private static string BuildDefaultReason(
        SmartShoppingCatalogItem item,
        AISmartShoppingRequest request)
    {
        return request.Closet.Count == 0
            ? $"Build a foundational wardrobe starting with {item.Name}."
            : $"{item.Name} rounds out your {item.Category.ToLowerInvariant()} rotation.";
    }

    private static string BuildDeterministicSummary(AISmartShoppingRequest request)
    {
        if (request.Closet.Count == 0)
        {
            return "Your closet is empty — start with the foundational essentials and grow from there.";
        }

        var byCategory = request.Closet
            .Where(c => !string.IsNullOrWhiteSpace(c.Category))
            .GroupBy(c => c.Category, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Count())
            .ToList();

        var top = byCategory.FirstOrDefault();
        if (top is null)
        {
            return "Your closet is uncategorised — start with a few foundational essentials to anchor new outfits.";
        }

        var ownedCategories = byCategory.Count;
        var catalogCategoryCount = request.Catalog
            .Select(c => c.Category)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        var missingCategories = Math.Max(0, catalogCategoryCount - ownedCategories);

        return $"Your closet spans {ownedCategories} categories (strongest: {top.Key} with {top.Count()} pieces) — {missingCategories} essential categor{(missingCategories == 1 ? "y is" : "ies are")} still missing.";
    }

    private static string NormalizePriority(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return "MEDIUM";
        }
        var key = raw.Trim().ToUpperInvariant();
        return key switch
        {
            "HIGH" or "MEDIUM" or "LOW" => key,
            _ => "MEDIUM",
        };
    }

    private static string TrimForLog(string body, int max = 240)
    {
        if (string.IsNullOrEmpty(body))
        {
            return string.Empty;
        }
        return body.Length <= max ? body : body[..max] + "…";
    }

    // ---- DTOs for the LLM response ----

    private sealed class LlmSmartShoppingResponse
    {
        [JsonPropertyName("picks")]
        public LlmSmartShoppingPick[]? Picks { get; set; }

        [JsonPropertyName("analysisSummary")]
        public string? AnalysisSummary { get; set; }
    }

    private sealed class LlmSmartShoppingPick
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("priority")]
        public string? Priority { get; set; }

        [JsonPropertyName("reason")]
        public string? Reason { get; set; }

        [JsonPropertyName("pairsWithNames")]
        public string[]? PairsWithNames { get; set; }
    }
}
