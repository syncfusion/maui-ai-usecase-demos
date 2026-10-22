using WardrobeAI.AI.Contracts;
using WardrobeAI.Helpers;

namespace WardrobeAI.AI;

/// <summary>
/// Picks a single outfit (top + bottom + footwear + optional outerwear / accessory)
/// from the user's closet for a given occasion + weather snapshot, and explains
/// why the items match. SDS §9, Module 5 (Outfit Planner) and Module 2 (Dashboard
/// "Today's Suggestion") share this contract.
///
/// <para>All implementations return <see cref="Result{T}"/> per wardrobe.prompt §10:
/// failures are surfaced as <c>Result.Fail("AI.X", "message")</c>, never as
/// exceptions thrown across the service boundary.</para>
///
/// <para>The interface intentionally takes pre-projected <see cref="ClothingItemSummary"/>
/// records instead of raw EF entities — that keeps PII (Name, Email, ProfileImage)
/// and large payloads (ImageUrl base64) out of the LLM request body by construction.</para>
/// </summary>
public interface IAIOutfitRecommendationService
{
    /// <summary>
    /// Generates a daily outfit recommendation for the dashboard. The LLM is asked
    /// to pick from <see cref="OutfitRecommendationRequest.Closet"/>, never to invent
    /// items, and to justify every pick in 1–3 sentences.
    ///
    /// <para>The implementation is expected to:</para>
    /// <list type="number">
    ///   <item>Build the prompt from <see cref="OutfitRecommendationRequest"/> + an embedded prompt template.</item>
    ///   <item>Call Azure OpenAI Chat Completions with JSON-schema structured outputs bound to
    ///         the response shape that <c>OutfitSuggestion</c> deserializes from.</item>
    ///   <item>Re-validate every returned <c>ItemId</c> against the request closet
    ///         (drop any id that wasn't in the input — prevents prompt-injection from
    ///         the LLM referencing items that don't belong to the user).</item>
    ///   <item>Recompute the numeric <c>MatchScore</c> client-side via
    ///         <c>AI/Scoring/*</c>; the LLM's score is only used as a soft fallback.</item>
    /// </list>
    /// </summary>
    Task<Result<OutfitRecommendationResult>> GetDailyOutfitSuggestionAsync(
        OutfitRecommendationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates one complete outfit plan (primary + 2 alternatives + a
    /// "Why this works" blurb) per requested <paramref name="categories"/>, in
    /// a single Azure OpenAI round-trip. Used by the Outfit Planner page to
    /// render the Work / Meeting / Casual / Party tabs in one shot.
    ///
    /// <para>The implementation is expected to:</para>
    /// <list type="number">
    ///   <item>Build a multi-occasion prompt from the request + the requested
    ///         categories (Work / Meeting / Casual / Party by default).</item>
    ///   <item>Call Azure OpenAI Chat Completions with JSON-schema structured
    ///         outputs bound to the <c>OutfitPlanResult</c> shape.</item>
    ///   <item>Re-validate every returned <c>ItemId</c> against the request closet.</item>
    ///   <item>Recompute the per-axis scores (Match / Style / Comfort / Trend)
    ///         client-side via <c>AI/Scoring/*</c>.</item>
    ///   <item>On any per-category failure (e.g. the LLM missed a slot) fall
    ///         back to a deterministic outfit built from the same closet so
    ///         every requested tab still renders.</item>
    /// </list>
    /// </summary>
    Task<Result<OutfitPlanResult>> GetOutfitsForCategoriesAsync(
        OutfitRecommendationRequest request,
        IReadOnlyList<string> categories,
        CancellationToken cancellationToken = default);
}
