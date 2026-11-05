using WardrobeAI.AI.Contracts;
using WardrobeAI.Helpers;

namespace WardrobeAI.AI;

/// <summary>
/// Picks a small, **randomized** set of missing essentials from the curated
/// 21-item "essential closet" catalog by comparing it to the user's
/// actual closet + style preferences + monthly budget (SDS §9.1, Phase 6
/// "Smart Shopping" — pair-aware + random selection).
///
/// <para>Per wardrobe.prompt §10, every public method returns
/// <see cref="Result{T}"/>; failures are surfaced as
/// <c>Result.Fail("AI.X", "message")</c> instead of being thrown across
/// the service boundary.</para>
///
/// <para>Like <see cref="IAIOutfitRecommendationService"/>, the
/// implementation is intentionally tolerant of misconfiguration: until the
/// user fills in <c>AzureOpenAiOptions.{Endpoint,ApiKey,Deployment}</c>,
/// the deterministic fallback still produces a sensible randomized
/// selection so the Shopping page never blocks on a missing Azure
/// subscription.</para>
/// </summary>
public interface IAISmartShoppingService
{
    /// <summary>
    /// Compares the user's <see cref="AISmartShoppingRequest.Closet"/>
    /// against <see cref="AISmartShoppingRequest.Catalog"/>, returns a
    /// randomized set of missing essentials (capped at
    /// <see cref="AISmartShoppingRequest.MaxResults"/>) prioritized by
    /// how many of the user's existing outfits each new piece would
    /// unlock, plus a 1-2 sentence human-readable summary of the
    /// closet gap.
    /// </summary>
    Task<Result<AISmartShoppingResult>> GetSmartShoppingPicksAsync(
        AISmartShoppingRequest request,
        CancellationToken cancellationToken = default);
}
