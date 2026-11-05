using WardrobeAI.AI.Contracts;

namespace WardrobeAI.AI.Scoring;

/// <summary>
/// Deterministic, client-side outfit scoring (SDS §9.1 "Scoring formulas").
/// The LLM only returns <c>itemIds</c> and free-text explanations; every
/// numeric score the UI shows is recomputed here so the result is
/// reproducible for the same closet + occasion + weather, independent of
/// the model's mood.
/// </summary>
public static class OutfitScorer
{
    /// <summary>
    /// 0–100. Fraction of outfit slots whose <c>Occasion</c> tag contains the
    /// requested occasion (case-insensitive substring match — handles
    /// "Business" vs "Office / Business", "Wedding" vs "Festive / Wedding", etc.).
    /// </summary>
    public static int OccasionMatch(IReadOnlyList<ClothingItemSummary> items, string occasion)
    {
        if (items.Count == 0 || string.IsNullOrWhiteSpace(occasion))
        {
            return 0;
        }

        var needle = occasion.Trim();
        var hits = items.Count(item =>
            !string.IsNullOrWhiteSpace(item.Occasion) &&
            item.Occasion.Contains(needle, StringComparison.OrdinalIgnoreCase));

        return (int)Math.Round(100.0 * hits / items.Count);
    }

    /// <summary>
    /// 0–100. Fraction of items whose <c>Season</c> + <c>Fabric</c> align with
    /// the current temperature (Cold ≤ 10 °C, Mild 11–22 °C, Warm ≥ 23 °C).
    /// "All Season" is treated as a hit for any bucket.
    /// </summary>
    public static int WeatherMatch(IReadOnlyList<ClothingItemSummary> items, double temperatureC)
    {
        if (items.Count == 0)
        {
            return 0;
        }

        var bucket = TemperatureBucket(temperatureC);
        var hits = items.Count(item =>
            string.Equals(item.Season, "All Season", StringComparison.OrdinalIgnoreCase) ||
            FabricFitsBucket(item.Fabric, bucket) ||
            SeasonFitsBucket(item.Season, bucket));

        return (int)Math.Round(100.0 * hits / items.Count);
    }

    /// <summary>
    /// 0–100. Naive color-harmony proxy: rewards neutral palettes (Black /
    /// White / Grey / Beige / Navy / Brown) and penalises same-item same-color
    /// clashes. We don't have CIE76 ΔE in this client; the heuristic is good
    /// enough for a dashboard chip.
    /// </summary>
    public static int ColorHarmony(IReadOnlyList<ClothingItemSummary> items)
    {
        if (items.Count <= 1)
        {
            return 90; // nothing to clash with
        }

        var neutrals = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Black", "White", "Grey", "Beige", "Brown", "Blue", "Navy"
        };

        // Dedup on color (case-insensitive) — same-colored top+bottom is the
        // classic "monochrome failure" that this scorer catches.
        var distinct = items
            .Select(i => i.Color)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var neutralCount = distinct.Count(c => neutrals.Contains(c));
        var score = 60 + (neutralCount * 8);

        // Slight bonus when the outfit has at least one neutral anchor.
        if (neutralCount > 0)
        {
            score += 5;
        }

        return Math.Clamp(score, 0, 100);
    }

    /// <summary>
    /// 0–100. Style match: Jaccard similarity of the free-form
    /// <c>style</c> + <c>occasion</c> tags against the user's
    /// <c>stylePreference</c> string (comma-separated).
    /// </summary>
    public static int StyleMatch(IReadOnlyList<ClothingItemSummary> items, string? userStylePreference)
    {
        if (items.Count == 0 || string.IsNullOrWhiteSpace(userStylePreference))
        {
            return 70; // neutral default — don't penalise users who skipped onboarding
        }

        var prefs = userStylePreference
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => s.ToLowerInvariant())
            .ToHashSet();

        if (prefs.Count == 0)
        {
            return 70;
        }

        var matchCount = 0;
        var total = 0;
        foreach (var item in items)
        {
            total++;
            var tags = (item.Style + "," + item.Occasion)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => s.ToLowerInvariant());

            if (tags.Any(prefs.Contains))
            {
                matchCount++;
            }
        }

        return total == 0 ? 0 : (int)Math.Round(100.0 * matchCount / total);
    }

    /// <summary>
    /// 0–100. Weighted blend per SDS §9.1 "Scoring formulas":
    /// <c>0.25·Color + 0.25·Style + 0.30·Occasion + 0.20·Weather</c>.
    /// </summary>
    public static int Overall(int colorHarmony, int styleMatch, int occasionMatch, int weatherMatch)
        => (int)Math.Round(0.25 * colorHarmony + 0.25 * styleMatch + 0.30 * occasionMatch + 0.20 * weatherMatch);

    /// <summary>
    /// 0–100. Comfort score: rewards breathable fabrics (Cotton / Linen / Bamboo /
    /// Organic Cotton / Rayon) and "All Season" / "Summer" / "Spring" tags, and
    /// penalises heavy fabrics in warm weather. Each item is scored, then
    /// averaged across the outfit.
    /// </summary>
    public static int Comfort(IReadOnlyList<ClothingItemSummary> items, double temperatureC)
    {
        if (items.Count == 0)
        {
            return 0;
        }

        var bucket = TemperatureBucket(temperatureC);
        var total = 0;
        foreach (var item in items)
        {
            total += ComfortItemScore(item, bucket);
        }
        return Math.Clamp((int)Math.Round((double)total / items.Count), 0, 100);
    }

    /// <summary>
    /// 0–100. Trend score: classic silhouettes (Slim Fit, Classic, Regular Fit,
    /// Modern) score high; dated or niche tags (Oversized, Vintage, Streetwear)
    /// are bucketed based on the requested occasion. Falls back to 70 when no
    /// occasion is supplied (we don't penalise neutral outfits).
    /// </summary>
    public static int Trend(IReadOnlyList<ClothingItemSummary> items, string? occasion)
    {
        if (items.Count == 0)
        {
            return 0;
        }

        // Occasion-keyed trend bias. Party / Casual lean current-season
        // Streetwear / Oversized; Work / Meeting lean Classic / Slim Fit.
        var key = (occasion ?? string.Empty).Trim().ToLowerInvariant();
        var preferStreet = key is "casual" or "party" or "party wear" or "festive" or "travel";
        var preferClassic = key is "work" or "meeting" or "business" or "formal" or "office" or "wedding";

        var total = 0;
        foreach (var item in items)
        {
            total += TrendItemScore(item, preferStreet, preferClassic);
        }
        return Math.Clamp((int)Math.Round((double)total / items.Count), 0, 100);
    }

    /// <summary>
    /// 0–100. Convenience wrapper: builds every sub-score from the same inputs
    /// and returns the weighted <see cref="Overall"/>. Use this from the
    /// service implementation so every score is reproducible.
    /// </summary>
    public static Contracts.OutfitScores ScoreAll(
        IReadOnlyList<ClothingItemSummary> items,
        string occasion,
        string? stylePreference,
        double temperatureC)
    {
        var color = ColorHarmony(items);
        var style = StyleMatch(items, stylePreference);
        var occasionScore = OccasionMatch(items, occasion);
        var weatherScore = WeatherMatch(items, temperatureC);
        var comfortScore = Comfort(items, temperatureC);
        var trendScore = Trend(items, occasion);
        return new Contracts.OutfitScores(
            Match: Overall(color, style, occasionScore, weatherScore),
            Style: style,
            Comfort: comfortScore,
            Trend: trendScore,
            Occasion: occasionScore,
            Weather: weatherScore);
    }

    // -- internals ------------------------------------------------------------

    private static string TemperatureBucket(double t) => t switch
    {
        <= 10 => "Cold",
        <= 22 => "Mild",
        _ => "Warm"
    };

    private static bool SeasonFitsBucket(string season, string bucket)
    {
        if (string.IsNullOrWhiteSpace(season))
        {
            return false;
        }

        return bucket switch
        {
            "Cold" => season.Equals("Winter", StringComparison.OrdinalIgnoreCase),
            "Mild" => season is "Spring" or "Autumn" or "All Season",
            "Warm" => season is "Summer" or "All Season",
            _ => false
        };
    }

    private static bool FabricFitsBucket(string fabric, string bucket)
    {
        if (string.IsNullOrWhiteSpace(fabric))
        {
            return false;
        }

        // Heuristic — Linen / Cotton / Rayon lean warm-weather; Wool / Leather / Denim lean cold.
        return bucket switch
        {
            "Cold" => fabric is "Wool" or "Leather" or "Denim",
            "Warm" => fabric is "Cotton" or "Linen" or "Rayon" or "Bamboo Fabric" or "Organic Cotton",
            _ => true // Mild accepts everything
        };
    }

    private static int ComfortItemScore(ClothingItemSummary item, string bucket)
    {
        var fabric = (item.Fabric ?? string.Empty).Trim();
        var season = (item.Season ?? string.Empty).Trim();
        var score = 70; // neutral default

        // Fabric weight — breathable fabrics always comfortable; heavy fabrics
        // are penalised in warm weather and rewarded in cold weather.
        if (IsBreathableFabric(fabric))
        {
            score += bucket == "Warm" ? 18 : 8;
        }
        else if (IsHeavyFabric(fabric))
        {
            score += bucket == "Cold" ? 12 : bucket == "Warm" ? -12 : 0;
        }

        // Season weight — items in the matching bucket get a small bonus.
        if (bucket == "Warm" && season.Equals("Summer", StringComparison.OrdinalIgnoreCase))
        {
            score += 8;
        }
        else if (bucket == "Cold" && season.Equals("Winter", StringComparison.OrdinalIgnoreCase))
        {
            score += 8;
        }
        else if (season.Equals("All Season", StringComparison.OrdinalIgnoreCase) ||
                 season.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            score += 4;
        }

        return Math.Clamp(score, 0, 100);
    }

    private static int TrendItemScore(ClothingItemSummary item, bool preferStreet, bool preferClassic)
    {
        var style = (item.Style ?? string.Empty).Trim();
        var pattern = (item.Pattern ?? string.Empty).Trim();
        var color = (item.Color ?? string.Empty).Trim();

        var score = 70; // neutral default

        // Pattern weight — solids are safe, simple patterns pick up small bonuses,
        // loud patterns get a small penalty unless the occasion wants them.
        if (string.Equals(pattern, "Solid", StringComparison.OrdinalIgnoreCase))
        {
            score += 6;
        }
        else if (pattern.Equals("Striped", StringComparison.OrdinalIgnoreCase) ||
                 pattern.Equals("Checked", StringComparison.OrdinalIgnoreCase))
        {
            score += preferClassic ? 4 : 0;
        }
        else if (pattern.Equals("Printed", StringComparison.OrdinalIgnoreCase) ||
                 pattern.Equals("Graphic", StringComparison.OrdinalIgnoreCase))
        {
            score += preferStreet ? 4 : -6;
        }

        // Style weight
        if (style.Equals("Classic", StringComparison.OrdinalIgnoreCase) ||
            style.Equals("Slim Fit", StringComparison.OrdinalIgnoreCase) ||
            style.Equals("Regular Fit", StringComparison.OrdinalIgnoreCase) ||
            style.Equals("Modern", StringComparison.OrdinalIgnoreCase))
        {
            score += preferClassic ? 8 : 4;
        }
        else if (style.Equals("Streetwear", StringComparison.OrdinalIgnoreCase) ||
                 style.Equals("Oversized", StringComparison.OrdinalIgnoreCase))
        {
            score += preferStreet ? 8 : -4;
        }
        else if (style.Equals("Vintage", StringComparison.OrdinalIgnoreCase))
        {
            score += preferStreet ? 2 : -4;
        }
        else if (style.Equals("Minimalist", StringComparison.OrdinalIgnoreCase))
        {
            score += 6;
        }
        else if (style.Equals("Athletic Fit", StringComparison.OrdinalIgnoreCase) ||
                 style.Equals("Relaxed Fit", StringComparison.OrdinalIgnoreCase))
        {
            score += 2;
        }

        // Color trend bias — neutrals are timeless, bold colors are in-vogue
        // for casual / party occasions.
        if (IsNeutralColor(color))
        {
            score += 4;
        }
        else if (IsBoldColor(color))
        {
            score += preferStreet ? 4 : -2;
        }

        return Math.Clamp(score, 0, 100);
    }

    private static bool IsBreathableFabric(string fabric) =>
        fabric is "Cotton" or "Linen" or "Rayon" or "Bamboo Fabric" or "Organic Cotton";

    private static bool IsHeavyFabric(string fabric) =>
        fabric is "Wool" or "Leather" or "Denim" or "Polyester";

    private static bool IsNeutralColor(string color) =>
        color is "Black" or "White" or "Grey" or "Beige" or "Brown" or "Navy" or "Blue";

    private static bool IsBoldColor(string color) =>
        color is "Red" or "Green" or "Yellow" or "Pink" or "Purple";
}
