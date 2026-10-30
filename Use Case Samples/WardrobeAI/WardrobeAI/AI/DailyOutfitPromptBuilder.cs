using System.Text;
using System.Text.Json;
using WardrobeAI.AI.Contracts;

namespace WardrobeAI.AI;

/// <summary>
/// Builds the user-prompt JSON for <see cref="IAIOutfitRecommendationService"/>.
/// Kept as a tiny static helper so the service implementation stays focused
/// on the HTTP call + JSON-schema plumbing. The system prompt itself is
/// held in <c>AI/PromptTemplates/DailyOutfitRecommendation.md</c> for review
/// and is also inlined into <see cref="SystemPrompt"/> below so the build
/// does not need to wire up embedded resources.
/// </summary>
internal static class DailyOutfitPromptBuilder
{
    private static readonly JsonSerializerOptions ClosetJsonOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };
    /// <summary>
    /// System prompt inlined verbatim from
    /// <c>AI/PromptTemplates/DailyOutfitRecommendation.md</c>. Kept in sync
    /// with the markdown file — any change must touch both.
    /// </summary>
    public const string SystemPrompt =
        "You are Wardrobe AI, a personal stylist. Your job is to pick a single complete outfit from the " +
        "user's closet for the occasion + weather below, and to explain in 1-3 sentences why each item was chosen.\n\n" +

        "Hard rules:\n" +
        "1. Only use items the user already owns. Pick `id`s from the `closet[]` list. If you do not see a category the user needs (e.g. they have no footwear), leave that slot empty rather than inventing an item.\n" +
        "2. Return JSON only. The response MUST match the `OutfitSuggestion` schema. No prose outside the JSON object. No markdown fences.\n" +
        "3. One primary outfit + up to 2 alternatives. The primary is the best match for the occasion + weather; alternatives should be genuinely different (e.g. swap a blazer for a cardigan, swap chinos for jeans), not trivial re-orderings of the same items.\n" +
        "4. Slots: every outfit must include at least one `Top` and one `Bottom` (or a `Dress` covering both). `Footwear` is required. `Outerwear` and `Accessory` are optional. If the weather is Rainy / Snowy / Cold (≤ 15 °C), `Outerwear` is strongly preferred and the explanation should mention it.\n" +
        "5. PII rules: never reference the user's name, email, exact location, age, or any image. The closet list is already filtered to safe fields only.\n" +
        "6. Confidence: the `matchScore` is your honest 0-100 estimate of how well the outfit fits the request. The client recomputes a deterministic score from the same closet, so your score is a fallback only - but you should still report something you believe in (do not default to 95).\n" +
        "7. Style coherence: prefer solid / dark colors for Business and Formal occasions; allow bolder colors for Casual / Party; for Wedding / Formal, prefer classic silhouettes and add an Accessory slot if the closet has one. For Travel, prefer wrinkle-resistant fabrics and layering pieces.";

    /// <summary>
    /// System prompt for the multi-occasion Outfit Planner request. Same
    /// stylist persona as <see cref="SystemPrompt"/>, but the response carries
    /// one <c>categoryEntry</c> per requested occasion (Work / Meeting /
    /// Casual / Party) so the planner page renders all four tabs in a
    /// single round-trip.
    /// </summary>
    public const string PlannerSystemPrompt =
        "You are Wardrobe AI, a personal stylist. Your job is to build a complete outfit plan for the user " +
        "across every requested occasion (categories below), using ONLY items they already own.\n\n" +

        "Hard rules:\n" +
        "1. Only use items the user already owns. Pick `id`s from the `closet[]` list. If a category is missing a slot (e.g. they have no footwear), leave the slot empty rather than inventing an item.\n" +
        "2. Return JSON only. The response MUST match the `outfit_plan` schema: an `entries` array, one entry per requested `category`. No prose outside the JSON. No markdown fences.\n" +
        "3. For each category, build ONE primary outfit + up to 2 ALTERNATIVE looks. Alternatives must be genuinely different (e.g. swap a blazer for a knit, swap chinos for jeans), not trivial re-orderings of the same items.\n" +
        "4. Slots: every outfit must include at least one `Top` and one `Bottom` (or a `Dress` covering both). `Footwear` is required. `Outerwear` and `Accessory` are optional. If the weather is Rainy / Snowy / Cold (≤ 15 °C), `Outerwear` is strongly preferred and the `whyThisWorks` blurb should mention it.\n" +
        "5. Per-category style rules: 'Work' and 'Meeting' prefer solid / dark colors, classic tailoring, and minimal accessories. 'Casual' allows bolder colors, knits, denim, and sneakers. 'Party' allows richer textures, statement pieces, and bolder colors.\n" +
        "6. PII rules: never reference the user's name, email, exact location, age, or any image. The closet list is already filtered to safe fields only.\n" +
        "7. Confidence: each `matchScore` is your honest 0-100 estimate of how well the outfit fits the request. The client recomputes a deterministic score from the same closet, so your score is a fallback only — but you should still report something you believe in (do not default to 95).\n" +
        "8. `whyThisWorks` must be a 1-3 sentence explanation tailored to the category and weather (mention specific item names, color harmony, fabric choice, or weather fit).";

    /// <summary>
    /// Serializes the request as a single user message. The closet is the
    /// biggest chunk — we keep it as a JSON array of
    /// <see cref="ClothingItemSummary"/> records so the LLM can scan it
    /// linearly.
    /// </summary>
    public static string BuildUserPrompt(OutfitRecommendationRequest request)
    {
        var sb = new StringBuilder(1024);
        sb.AppendLine("## Request");
        sb.Append("Occasion: ").AppendLine(request.Occasion);
        sb.Append("Weather: ")
          .Append(Math.Round(request.Weather.TemperatureC)).Append(" °C, ")
          .Append(request.Weather.Condition);
        if (!string.IsNullOrWhiteSpace(request.Weather.LocationLabel))
        {
            sb.Append(" @ ").Append(request.Weather.LocationLabel);
        }
        if (request.Weather.HumidityPercent > 0)
        {
            sb.Append(" (humidity ").Append(request.Weather.HumidityPercent).Append("%)");
        }
        sb.AppendLine();
        sb.Append("Gender: ").AppendLine(string.IsNullOrWhiteSpace(request.Gender) ? "Unisex" : request.Gender);
        if (!string.IsNullOrWhiteSpace(request.AgeGroup))
        {
            sb.Append("Age group: ").AppendLine(request.AgeGroup);
        }
        if (!string.IsNullOrWhiteSpace(request.StylePreference))
        {
            sb.Append("Style preference: ").AppendLine(request.StylePreference);
        }
        if (!string.IsNullOrWhiteSpace(request.BudgetLevel))
        {
            sb.Append("Budget: ").AppendLine(request.BudgetLevel);
        }

        sb.AppendLine();
        sb.AppendLine("## Closet (pick `id`s from this list only)");
        sb.AppendLine("```json");
        sb.AppendLine(JsonSerializer.Serialize(request.Closet, ClosetJsonOptions));
        sb.AppendLine("```");

        sb.AppendLine();
        sb.AppendLine("## Response (strict JSON, matches the OutfitSuggestion schema)");

        return sb.ToString();
    }

    /// <summary>
    /// Builds the user prompt for the multi-occasion Outfit Planner request.
    /// Same context block as <see cref="BuildUserPrompt"/>, but lists every
    /// requested category so the model returns one entry per occasion.
    /// </summary>
    public static string BuildPlannerUserPrompt(OutfitRecommendationRequest request, IReadOnlyList<string> categories)
    {
        var sb = new StringBuilder(2048);
        sb.AppendLine("## Request");
        sb.Append("Weather: ")
          .Append(Math.Round(request.Weather.TemperatureC)).Append(" °C, ")
          .Append(request.Weather.Condition);
        if (!string.IsNullOrWhiteSpace(request.Weather.LocationLabel))
        {
            sb.Append(" @ ").Append(request.Weather.LocationLabel);
        }
        if (request.Weather.HumidityPercent > 0)
        {
            sb.Append(" (humidity ").Append(request.Weather.HumidityPercent).Append("%)");
        }
        sb.AppendLine();
        sb.Append("Gender: ").AppendLine(string.IsNullOrWhiteSpace(request.Gender) ? "Unisex" : request.Gender);
        if (!string.IsNullOrWhiteSpace(request.AgeGroup))
        {
            sb.Append("Age group: ").AppendLine(request.AgeGroup);
        }
        if (!string.IsNullOrWhiteSpace(request.StylePreference))
        {
            sb.Append("Style preference: ").AppendLine(request.StylePreference);
        }
        if (!string.IsNullOrWhiteSpace(request.BudgetLevel))
        {
            sb.Append("Budget: ").AppendLine(request.BudgetLevel);
        }

        sb.AppendLine();
        sb.Append("Categories (build one entry per category): ")
          .AppendLine(string.Join(", ", categories));

        sb.AppendLine();
        sb.AppendLine("## Closet (pick `id`s from this list only)");
        sb.AppendLine("```json");
        sb.AppendLine(JsonSerializer.Serialize(request.Closet, ClosetJsonOptions));
        sb.AppendLine("```");

        sb.AppendLine();
        sb.AppendLine("## Response (strict JSON, matches the outfit_plan schema)");

        return sb.ToString();
    }
}
