using WardrobeAI.Models;

namespace WardrobeAI.AI.Contracts;

/// <summary>
/// Compact projection of a <see cref="ClothingItem"/> that is safe to ship to
/// the LLM (PII / image-bytes omitted per wardrobe.prompt §3.5). Holds only
/// the fields the outfit recommendation service actually needs to reason
/// about a candidate garment.
/// </summary>
/// <param name="Id">DB id used to join the LLM's response back to the user's closet.</param>
/// <param name="Name">Display name chosen by the user (e.g. "Navy Oxford").</param>
/// <param name="Category">Free-form category label (T-Shirts, Jeans, Blazer, …).</param>
/// <param name="Color">Color tag from the catalog.</param>
/// <param name="Pattern">Solid / Striped / Printed / …</param>
/// <param name="Fabric">Cotton / Linen / Wool / …</param>
/// <param name="Season">Summer / Winter / All Season / …</param>
/// <param name="Style">Slim Fit / Streetwear / …</param>
/// <param name="Occasion">Casual / Business / Wedding / …</param>
/// <param name="Favorite">Promoted as a soft tie-breaker by the LLM.</param>
/// <param name="UsageCount">Lower usage = "needs love" bias, higher usage = "trusted" bias.</param>
public sealed record ClothingItemSummary(
    int Id,
    string Name,
    string Category,
    string Color,
    string Pattern,
    string Fabric,
    string Season,
    string Style,
    string Occasion,
    bool Favorite,
    int UsageCount);

/// <summary>
/// Coarse weather summary sent to the LLM. The dashboard already maps
/// <see cref="Contracts.WeatherCondition"/> onto an icon and a temperature
/// string; we re-package the same facts for the outfit service so the
/// request payload is self-contained and the LLM is never asked to call the
/// weather API itself.
/// </summary>
/// <param name="TemperatureC">Current air temperature in °C.</param>
/// <param name="Condition">Coarse condition (Sunny / Rainy / Snowy / Stormy / Cloudy / Unknown).</param>
/// <param name="HumidityPercent">0–100.</param>
/// <param name="LocationLabel">Coarse "City, CC" label (PII: never a street address).</param>
public sealed record WeatherSummary(
    double TemperatureC,
    string Condition,
    int HumidityPercent,
    string LocationLabel);

/// <summary>
/// Request to <see cref="WardrobeAI.AI.IAIOutfitRecommendationService"/>.
/// Carries the minimum payload the LLM needs to recommend a single outfit:
/// the active user (gender-aware), the occasion (from today's planned event
/// or the user's free-text override), the current weather, the user's
/// style/budget preferences, and the full closet to choose from.
/// </summary>
/// <param name="UserId">DB id, used for join-back in the audit log only.</param>
/// <param name="Gender">Drives the SDS §9.1 capability matrix (Men / Women / Unisex / Undisclosed).</param>
/// <param name="AgeGroup">Coarse label (e.g. "22-35").</param>
/// <param name="StylePreference">Free-form (e.g. "Casual, Streetwear").</param>
/// <param name="BudgetLevel">Low / Medium / Premium / Luxury.</param>
/// <param name="Location">Coarse climate zone only ("Pune, IN" — never a street address).</param>
/// <param name="Occasion">The occasion the outfit must suit (e.g. "Work", "Casual", "Wedding").</param>
/// <param name="Weather">Current weather snapshot.</param>
/// <param name="Closet">All clothing items the user owns. Caller is responsible for filtering out items the user has explicitly excluded.</param>
/// <param name="TopNItemsPerCategory">Soft cap on how many items per category the LLM may pick; the service still pre-sorts by usage+favorite.</param>
public sealed record OutfitRecommendationRequest(
    int UserId,
    string Gender,
    string? AgeGroup,
    string StylePreference,
    string BudgetLevel,
    string? Location,
    string Occasion,
    WeatherSummary Weather,
    IReadOnlyList<ClothingItemSummary> Closet,
    int TopNItemsPerCategory = 12)
{
    /// <summary>Stable hash of the request body, used as the cache key (24h TTL).</summary>
    public string CacheKey
    {
        get
        {
            var input = string.Join("|",
                UserId, Gender, AgeGroup ?? "-", StylePreference, BudgetLevel, Location ?? "-",
                Occasion,
                Weather.TemperatureC, Weather.Condition, Weather.HumidityPercent, Weather.LocationLabel,
                TopNItemsPerCategory,
                string.Join(",", Closet.Select(c => $"{c.Id}:{c.Category}")));
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }
    }
}

/// <summary>
/// One selected clothing item inside the LLM's primary outfit. The service
/// re-validates <see cref="ItemId"/> against the request closet before
/// returning, so a bad id from the model is silently dropped and the slot
/// is reported as <c>null</c>.
/// </summary>
/// <param name="ItemId">DB id of the chosen <see cref="ClothingItem"/>.</param>
/// <param name="Role">Slot label rendered in the dashboard (Top / Bottom / Footwear / Outerwear / Accessory).</param>
public sealed record OutfitItemPick(int ItemId, string Role);

/// <summary>
/// The LLM's primary outfit pick plus a short explanation and a confidence
/// score. The numeric <see cref="MatchScore"/> is re-computed client-side by
/// <c>AI/Scoring/*</c> from the same closet; the LLM's score is kept as a
/// soft signal and surfaced only as a fallback when the deterministic
/// scorer cannot run (e.g. the closet is empty).
/// </summary>
/// <param name="Title">Short title (e.g. "Business Casual Friday").</param>
/// <param name="Occasion">The occasion the outfit was built for (echo of the request).</param>
/// <param name="Items">Ordered list of picked items (Top / Bottom / Footwear / …).</param>
/// <param name="Reason">3-sentence max explanation of why this outfit fits the occasion + weather.</param>
/// <param name="MatchScore">LLM confidence, 0-100. Replaced by the deterministic scorer on the way out.</param>
/// <param name="Scores">Per-axis scores (Match / Style / Comfort / Trend) recomputed by the deterministic scorer on the way out. <c>null</c> when the scorer could not run.</param>
public sealed record OutfitSuggestion(
    string Title,
    string Occasion,
    IReadOnlyList<OutfitItemPick> Items,
    string Reason,
    int MatchScore,
    OutfitScores? Scores = null);

/// <summary>
/// Final result of <see cref="WardrobeAI.AI.IAIOutfitRecommendationService.GetDailyOutfitSuggestionAsync"/>.
/// The alternative list is currently capped at 2 to keep the dashboard
/// card scannable; the primary outfit is always the first one returned.
/// </summary>
/// <param name="Primary">The recommended outfit for today.</param>
/// <param name="Alternatives">One or two alternate outfits the user can switch to.</param>
/// <param name="WeatherEcho">Echo of the weather summary that was used (handy for the ViewModel to render).</param>
/// <param name="OccasionEcho">Echo of the occasion that was used.</param>
public sealed record OutfitRecommendationResult(
    OutfitSuggestion Primary,
    IReadOnlyList<OutfitSuggestion> Alternatives,
    WeatherSummary WeatherEcho,
    string OccasionEcho);

/// <summary>
/// Per-occasion outfit card returned by
/// <see cref="WardrobeAI.AI.IAIOutfitRecommendationService.GetOutfitsForCategoriesAsync"/>.
/// Carries the primary outfit for the requested category plus 2 alternative
/// looks and a short "Why this works" explanation that the planner page
/// renders beneath the score gauges.
/// </summary>
/// <param name="Category">The requested occasion (e.g. "Work").</param>
/// <param name="Primary">The recommended outfit for this category.</param>
/// <param name="Alternatives">Two alternate looks for the same category. Capped at 2 for layout density.</param>
/// <param name="WhyThisWorks">3-sentence max explanation tailored to the category + weather.</param>
public sealed record OutfitPlanEntry(
    string Category,
    OutfitSuggestion Primary,
    IReadOnlyList<OutfitSuggestion> Alternatives,
    string WhyThisWorks);

/// <summary>
/// Result of <see cref="WardrobeAI.AI.IAIOutfitRecommendationService.GetOutfitsForCategoriesAsync"/>.
/// Carries one <see cref="OutfitPlanEntry"/> per requested category so the
/// Outfit Planner can render all four tabs in one shot.
/// </summary>
/// <param name="Entries">Per-category outfit plans in the order they were requested.</param>
/// <param name="WeatherEcho">Echo of the weather summary that was used.</param>
public sealed record OutfitPlanResult(
    IReadOnlyList<OutfitPlanEntry> Entries,
    WeatherSummary WeatherEcho);

/// <summary>
/// Convenience snapshot of every per-axis score for a single outfit.
/// Mirrors <see cref="WardrobeAI.AI.Scoring.OutfitScores"/> so the contract
/// surface doesn't depend on the scoring namespace. The service fills this in
/// from <c>OutfitScorer.ScoreAll</c> after the LLM responds so the planner
/// gauges are reproducible for the same closet + occasion + weather.
/// </summary>
/// <param name="Match">Weighted overall score (0–100).</param>
/// <param name="Style">Style coherence vs. the user's style preference (0–100).</param>
/// <param name="Comfort">Breathable fabrics + season alignment (0–100).</param>
/// <param name="Trend">Classic / street bias vs. the requested occasion (0–100).</param>
/// <param name="Occasion">Fraction of items tagged for the requested occasion (0–100).</param>
/// <param name="Weather">Fraction of items whose fabric / season align with the current temperature (0–100).</param>
public sealed record OutfitScores(
    int Match,
    int Style,
    int Comfort,
    int Trend,
    int Occasion,
    int Weather);
