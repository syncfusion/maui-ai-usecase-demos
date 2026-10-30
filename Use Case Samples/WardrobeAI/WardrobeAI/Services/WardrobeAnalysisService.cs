using Microsoft.Extensions.Logging;
using WardrobeAI.AI.Contracts;
using WardrobeAI.Constants;
using WardrobeAI.Models;

namespace WardrobeAI.Services;

/// <summary>
/// Deterministic implementation of <see cref="IWardrobeAnalysisService"/>.
/// Score and priority heuristics are documented inline so future tuning is
/// localized to a single file.
/// </summary>
public sealed class WardrobeAnalysisService : IWardrobeAnalysisService
{
    private readonly ILogger<WardrobeAnalysisService> _logger;

    /// <summary>
    /// Core closet slots the AI considers "must have" for a working
    /// professional wardrobe. Mirrors <c>designs/smart_shopping.html</c>
    /// ("Target: 25 Core Pieces") and is the denominator for
    /// <see cref="WardrobeAnalysis.CompletionScore"/>.
    /// </summary>
    private static readonly IReadOnlyList<string> CoreCategories = new[]
    {
        "T-Shirts", "Shirts", "Polo Shirts", "Hoodies", "Sweatshirts",
        "Jackets", "Coats", "Blazers", "Sweaters", "Jeans", "Trousers",
        "Shorts", "Joggers", "Skirts", "Dresses", "Kurtas",
        "Sarees", "Ethnic Wear", "Activewear", "Sleepwear", "Innerwear",
        "Swimwear"
    };

    /// <summary>
    /// Per-category priority. <c>high</c> means the category is foundational
    /// (e.g. a blazer elevates every smart-casual outfit); <c>low</c> means
    /// the slot is more situational (e.g. swimwear).
    /// </summary>
    private static readonly IReadOnlyDictionary<string, string> PriorityByCategory =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Blazers"] = "high",
            ["Shirts"] = "high",
            ["Trousers"] = "high",
            ["Jeans"] = "high",
            ["Jackets"] = "high",
            ["T-Shirts"] = "medium",
            ["Polo Shirts"] = "medium",
            ["Sweaters"] = "medium",
            ["Coats"] = "medium",
            ["Hoodies"] = "medium",
            ["Sweatshirts"] = "medium",
            ["Dresses"] = "medium",
            ["Kurtas"] = "medium",
            ["Sarees"] = "medium",
            ["Ethnic Wear"] = "medium",
            ["Skirts"] = "medium",
            ["Shorts"] = "low",
            ["Joggers"] = "low",
            ["Activewear"] = "low",
            ["Sleepwear"] = "low",
            ["Innerwear"] = "low",
            ["Swimwear"] = "low",
        };

    /// <summary>
    /// Curated product catalog (one entry per core category). Prices are
    /// anchored on a "value for money" sweet-spot tier — premium enough to
    /// last several seasons, not luxury. <c>ProductName</c> is what the
    /// product card renders; <c>StoreQuery</c> is what the deep-link
    /// service uses to build a search URL on a real online store.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, ProductCatalogEntry> CatalogByCategory =
        new Dictionary<string, ProductCatalogEntry>(StringComparer.OrdinalIgnoreCase)
        {
            ["Blazers"] = new("LUXE MENSWEAR", "Structured Navy Blazer", "tailored navy blazer slim fit", 185m,
                "This essential piece instantly elevates your smart-casual collection. Constructed from lightweight, breathable wool-blend fabric, perfect for year-round wear."),
            ["Shirts"] = new("HERITAGE ESSENTIALS", "Classic White Oxford Shirt", "white oxford shirt slim fit", 65m,
                "A crisp, breathable cotton Oxford that anchors formal and smart-casual outfits alike. Pairs with everything from chinos to denim."),
            ["Trousers"] = new("TAILORED BASICS", "Wool-Blend Dress Trousers", "wool blend dress trousers slim fit", 95m,
                "Mid-rise wool-blend trousers with a clean drape. A workhorse for office wear that also reads well on weekends."),
            ["Jeans"] = new("DENIM CO", "Slim-Fit Indigo Jeans", "slim fit indigo jeans", 75m,
                "A durable mid-stretch denim in a versatile indigo wash. Holds shape wash after wash and pairs with tees, shirts, and knits."),
            ["Jackets"] = new("OUTERWEAR EDIT", "Lightweight Bomber Jacket", "lightweight bomber jacket", 110m,
                "A weather-ready layer that adds polish without bulk. Works over a tee for evenings or under a coat on cold commutes."),
            ["T-Shirts"] = new("DAILY ESSENTIALS", "Cotton Crew-Neck 3-Pack", "cotton crew neck t shirt pack", 45m,
                "Soft, pre-shrunk cotton crew-necks in core neutrals. The foundation of every casual outfit — high cost-per-wear."),
            ["Polo Shirts"] = new("CLASSIC SPORT", "Pique Cotton Polo", "pique cotton polo shirt", 55m,
                "A breathable pique polo that dresses up shorts and dresses down chinos. A wardrobe multiplier at this price point."),
            ["Sweaters"] = new("WOOLHOUSE", "Merino Crew Sweater", "merino wool crew sweater", 90m,
                "A fine-gauge merino sweater that resists odours and layers cleanly over a shirt. Comfortable across three seasons."),
            ["Coats"] = new("OUTERWEAR EDIT", "Wool-Blend Topcoat", "wool blend topcoat", 220m,
                "A timeless single-breasted topcoat in a warm camel or charcoal. The single most worn outerwear piece in any working wardrobe."),
            ["Hoodies"] = new("STREET BASICS", "Fleece Pullover Hoodie", "fleece pullover hoodie", 55m,
                "A brushed-fleece hoodie in a neutral colourway. Lounge, travel, and weekend uniform in one."),
            ["Sweatshirts"] = new("STREET BASICS", "Loopwheel Sweatshirt", "loopwheel sweatshirt", 80m,
                "A heavyweight loopwheel-knit sweatshirt that softens with every wash. Quietly outperforms fast-fashion alternatives."),
            ["Dresses"] = new("EVERYDAY EDIT", "Wrap Midi Dress", "wrap midi dress", 95m,
                "A flattering wrap midi in a season-spanning jersey. Transitions from desk to dinner with a swap of footwear."),
            ["Kurtas"] = new("ETHNIC CRAFT", "Cotton Straight Kurta", "cotton straight kurta", 60m,
                "A breathable cotton straight kurta with subtle detailing. Reads well at work and on festive occasions."),
            ["Sarees"] = new("ETHNIC CRAFT", "Handloom Cotton Saree", "handloom cotton saree", 120m,
                "A handloom cotton saree in a versatile drape. Comfortable for long days and pairs with a wide blouse rotation."),
            ["Ethnic Wear"] = new("ETHNIC CRAFT", "Embroidered Nehru Jacket", "embroidered nehru jacket", 110m,
                "A versatile Nehru jacket that elevates a plain kurta for evening events. Strong cost-per-wear across the festive calendar."),
            ["Skirts"] = new("EVERYDAY EDIT", "Pleated Midi Skirt", "pleated midi skirt", 80m,
                "A flowy pleated midi in a neutral tone. Pairs with sneakers for weekends and boots for the office."),
            ["Shorts"] = new("SUMMER BASICS", "Tailored Chino Shorts", "tailored chino shorts", 50m,
                "Mid-length chino shorts with a clean hem. Dressier than sport shorts and a summer staple."),
            ["Joggers"] = new("STREET BASICS", "Tapered Knit Joggers", "tapered knit joggers", 60m,
                "Tapered knit joggers in a refined cut. Comfortable enough for travel, smart enough for a coffee run."),
            ["Activewear"] = new("MOVE TECH", "Performance Tee & Shorts Set", "performance tee shorts set", 85m,
                "Moisture-wicking performance set for gym, runs, and weekend hikes. Quick-dry and tag-free for comfort."),
            ["Sleepwear"] = new("NIGHT EDIT", "Cotton Lounge Set", "cotton loungewear set", 55m,
                "A soft cotton lounge set that doubles as WFH wear. Long-fibre cotton holds up wash after wash."),
            ["Innerwear"] = new("DAILY BASICS", "Cotton Essentials Bundle", "cotton innerwear bundle", 40m,
                "A 5-pack of breathable cotton essentials. The single highest cost-per-wear item you can buy."),
            ["Swimwear"] = new("BEACH EDIT", "Quick-Dry Swim Shorts", "quick dry swim shorts", 45m,
                "Quick-dry swim shorts with a mesh lining. Versatile enough to wear as a beach cover-up too."),
        };

    public WardrobeAnalysisService(ILogger<WardrobeAnalysisService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public WardrobeAnalysis Analyze(IReadOnlyList<ClothingItem> closet, IReadOnlyList<string> stylePreferences)
    {
        if (closet is null)
        {
            closet = Array.Empty<ClothingItem>();
        }

        // 1. Count per category.
        var owned = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in closet)
        {
            if (string.IsNullOrWhiteSpace(item.Category))
            {
                continue;
            }

            owned[item.Category] = owned.TryGetValue(item.Category, out var n) ? n + 1 : 1;
        }

        // 2. Target is the size of the core catalog (matches the design's
        //    "25 Core Pieces" header; the catalog is fixed so this stays stable
        //    as the user adds items).
        var itemsTarget = CoreCategories.Count;
        var itemsOwned = closet.Count;

        // 3. Completion score: how many core categories the user covers
        //    (presence of at least one item), weighted lightly by total depth.
        var coveredCore = CoreCategories.Count(c => owned.ContainsKey(c));
        var rawScore = itemsTarget == 0 ? 0 : (int)Math.Round(100.0 * coveredCore / itemsTarget);
        // Soft bonus for depth (multiple items per category) — cap at 100.
        var depthBonus = Math.Min(10, (itemsOwned - coveredCore) / 2);
        var completion = Math.Clamp(rawScore + depthBonus, 0, 100);

        // 4. Missing essentials: every core category with zero items, ranked
        //    by priority (high → medium → low) and then by category name.
        var missing = new List<MissingEssential>();
        foreach (var category in CoreCategories)
        {
            if (owned.ContainsKey(category))
            {
                continue;
            }

            var priority = PriorityByCategory.TryGetValue(category, out var p) ? p : "medium";

            // Closet pairings: surface what they already own that the new
            // piece would combine with (e.g. "Navy Blazer" → "[White Shirt,
            // Indigo Jeans, Brown Loafers]"). Top 3 names, most-worn first.
            var pairings = closet
                .OrderByDescending(c => c.UsageCount)
                .ThenByDescending(c => c.Favorite)
                .Take(20)
                .Where(c => !string.Equals(c.Category, category, StringComparison.OrdinalIgnoreCase))
                .Select(c => c.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(3)
                .ToList();

            missing.Add(new MissingEssential(
                Category: category,
                Priority: priority.ToUpperInvariant(),
                WhyMissing: BuildWhyMissing(category, owned, stylePreferences),
                ClosetPairings: pairings));
        }

        // Sort: high → medium → low, then category name.
        var priorityOrder = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["high"] = 0, ["medium"] = 1, ["low"] = 2,
        };
        var orderedMissing = missing
            .OrderBy(m => priorityOrder.TryGetValue(m.Priority.ToLowerInvariant(), out var o) ? o : 1)
            .ThenBy(m => m.Category, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var summary = BuildSummary(coveredCore, itemsTarget, orderedMissing, stylePreferences);

        return new WardrobeAnalysis(
            CompletionScore: completion,
            ItemsOwned: itemsOwned,
            ItemsTarget: itemsTarget,
            OwnedByCategory: owned,
            MissingEssentials: orderedMissing,
            AnalysisSummary: summary);
    }

    /// <inheritdoc />
    public IReadOnlyList<ShoppingProductRecommendation> BuildRecommendations(
        IReadOnlyList<ClothingItem> closet,
        IReadOnlyList<MissingEssential> missingEssentials,
        decimal monthlyBudget,
        int maxResults = 4)
    {
        if (missingEssentials is null || missingEssentials.Count == 0)
        {
            return Array.Empty<ShoppingProductRecommendation>();
        }

        var results = new List<ShoppingProductRecommendation>();
        foreach (var essential in missingEssentials)
        {
            if (!CatalogByCategory.TryGetValue(essential.Category, out var entry))
            {
                continue;
            }

            // Compatibility score: how many of the user's existing items would
            // form a complete outfit with this new piece. We use the
            // essential's pre-computed ClosetPairings as a proxy (3 = 100, 2
            // = 80, 1 = 60, 0 = 40) so the user sees a real number.
            var pairCount = essential.ClosetPairings.Count;
            var compatibility = pairCount switch
            {
                >= 3 => 95,
                2 => 84,
                1 => 70,
                _ => 55,
            };

            // "Value for money" rationale — we tailor the message to whether
            // the user can afford this on their current monthly budget.
            var why = BuildValueForMoneyRationale(entry, essential, monthlyBudget, compatibility);

            results.Add(new ShoppingProductRecommendation(
                Category: essential.Category,
                Brand: entry.Brand,
                Name: entry.ProductName,
                Price: $"${entry.PriceUsd:0}",
                PriceUsd: entry.PriceUsd,
                PairsWithCloset: essential.ClosetPairings,
                Description: entry.Description,
                WhyRecommended: why,
                CompatibilityScore: compatibility));
        }

        return results
            .OrderByDescending(r => r.CompatibilityScore)
            .ThenBy(r => r.PriceUsd)
            .Take(maxResults)
            .ToList();
    }

    /// <summary>
    /// Composes the 1-sentence "why you're missing this" note. Tries to use
    /// the user's actual closet stats so the message doesn't feel canned.
    /// </summary>
    private static string BuildWhyMissing(
        string category,
        IReadOnlyDictionary<string, int> owned,
        IReadOnlyList<string> stylePreferences)
    {
        // Pick the closest adjacent category the user already owns a lot of.
        // e.g. missing "Blazers" but has 5 "Shirts" → mention the shirts.
        var adjacentCandidates = category switch
        {
            "Blazers" or "Coats" or "Jackets" or "Sweaters" or "Hoodies" or "Sweatshirts"
                => new[] { "Shirts", "T-Shirts", "Polo Shirts" },
            "Trousers" or "Jeans" or "Shorts" or "Joggers" or "Skirts"
                => new[] { "T-Shirts", "Shirts" },
            "Shirts" or "T-Shirts" or "Polo Shirts"
                => new[] { "Jeans", "Trousers" },
            "Kurtas" or "Sarees" or "Ethnic Wear"
                => new[] { "Ethnic Wear", "Kurtas", "Sarees" },
            _ => Array.Empty<string>(),
        };

        var topAdjacent = adjacentCandidates
            .Where(c => owned.TryGetValue(c, out var n) && n > 0)
            .OrderByDescending(c => owned[c])
            .FirstOrDefault();

        if (topAdjacent is not null)
        {
            return $"You have {owned[topAdjacent]} {topAdjacent} but no {category} to anchor formal or elevated outfits.";
        }

        // Generic fallback — still grounded in the user's style preferences.
        if (stylePreferences is { Count: > 0 } && (stylePreferences.Contains("Business", StringComparer.OrdinalIgnoreCase)
            || stylePreferences.Contains("Formal", StringComparer.OrdinalIgnoreCase)))
        {
            return $"Your '{string.Join(", ", stylePreferences)}' style profile expects a {category} for a complete professional wardrobe.";
        }

        return $"Your closet is missing a {category} — a foundational piece for most wardrobes.";
    }

    /// <summary>
    /// Composes the 1-sentence "value for money" rationale that the Shopping
    /// page surfaces under each product price. Tells the user *why this pick
    /// is worth their money*, not just what it is.
    /// </summary>
    private static string BuildValueForMoneyRationale(
        ProductCatalogEntry entry,
        MissingEssential essential,
        decimal monthlyBudget,
        int compatibility)
    {
        // Pair-aware: "pairs with N items you already own" is the strongest
        // value-for-money signal we can give.
        var pairsNote = essential.ClosetPairings.Count switch
        {
            >= 3 => $"Pairs with {essential.ClosetPairings.Count} of your existing pieces — strong cost-per-wear.",
            2 => "Combines with 2 of your closet items for multiple outfit rotations.",
            1 => "Completes one of your existing outfit formulas.",
            _ => "Fills a gap in your closet so you stop re-wearing the same pieces.",
        };

        // Budget-aware: if the price is over half the user's monthly budget,
        // we explicitly call it out as a "staple buy" instead of a fast-fashion
        // impulse; if it's well within budget, we lean into the affordability.
        if (monthlyBudget > 0)
        {
            var ratio = entry.PriceUsd / monthlyBudget;
            if (ratio >= 0.5m)
            {
                return $"This is a higher-ticket staple ({ratio:P0} of your ${monthlyBudget:0} budget), so it's built to anchor many outfits. {pairsNote}";
            }

            if (ratio <= 0.25m)
            {
                return $"At {ratio:P0} of your ${monthlyBudget:0} budget it's an easy yes — and {pairsNote.ToLowerInvariant()}";
            }
        }

        // High-compatibility fallback (catches the case where monthlyBudget
        // is 0 because the user hasn't set one yet).
        if (compatibility >= 90)
        {
            return $"{pairsNote} Picked because it maximises outfit combinations from your existing closet.";
        }

        return $"{pairsNote} A versatile staple that earns its place in a working wardrobe.";
    }

    /// <summary>
    /// Builds the 1-2 sentence summary the "Analysis" hero card shows.
    /// </summary>
    private static string BuildSummary(
        int coveredCore,
        int itemsTarget,
        IReadOnlyList<MissingEssential> missing,
        IReadOnlyList<string> stylePreferences)
    {
        var highPriorityCount = missing.Count(m => string.Equals(m.Priority, "HIGH", StringComparison.OrdinalIgnoreCase));

        if (missing.Count == 0)
        {
            return "Your closet covers every core category. Focus on rotation and care to maximise cost-per-wear.";
        }

        var styleNote = stylePreferences is { Count: > 0 }
            ? $" and matches your '{string.Join(", ", stylePreferences)}' profile"
            : string.Empty;

        if (highPriorityCount > 0)
        {
            return $"Your closet covers {coveredCore} of {itemsTarget} core categories{styleNote}. {highPriorityCount} high-priority gap{(highPriorityCount == 1 ? string.Empty : "s")} would unlock the most new outfits.";
        }

        return $"Your closet covers {coveredCore} of {itemsTarget} core categories{styleNote}. Pick a few staples below to close the remaining gaps.";
    }

    /// <summary>
    /// Internal record for one catalog row. <see cref="StoreQuery"/> is the
    /// free-text phrase the deep-link service submits to the store search
    /// (kept separate from <see cref="ProductName"/> so we can use better
    /// search keywords than the marketing label).
    /// </summary>
    private sealed record ProductCatalogEntry(
        string Brand,
        string ProductName,
        string StoreQuery,
        decimal PriceUsd,
        string Description);
}
