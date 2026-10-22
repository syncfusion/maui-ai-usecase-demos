using WardrobeAI.AI.Contracts;
using WardrobeAI.Models;

namespace WardrobeAI.Services;

/// <summary>
/// Snapshot of how complete a closet is. The Shopping page renders the
/// <see cref="CompletionScore"/> as a hero stat and the
/// <see cref="AnalysisSummary"/> as a "Why this recommendation?" accordion.
/// </summary>
/// <param name="CompletionScore">0–100. Fraction of <c>CoreCategories</c> covered.</param>
/// <param name="ItemsOwned">Total items in the closet.</param>
/// <param name="ItemsTarget">How many core pieces the user should ideally own.</param>
/// <param name="OwnedByCategory">Count per category the user already owns.</param>
/// <param name="MissingEssentials">Categories with zero items, ranked by priority.</param>
/// <param name="AnalysisSummary">1-2 sentence human-readable wardrobe analysis.</param>
public sealed record WardrobeAnalysis(
    int CompletionScore,
    int ItemsOwned,
    int ItemsTarget,
    IReadOnlyDictionary<string, int> OwnedByCategory,
    IReadOnlyList<MissingEssential> MissingEssentials,
    string AnalysisSummary);

/// <summary>
/// A single category the user doesn't own but should, ordered by
/// <see cref="Priority"/> (highest first). The Shopping page renders one row
/// per essential; tapping a row opens a curated product card for that
/// category.
/// </summary>
/// <param name="Category">Closet category (e.g. "Blazers", "Footwear").</param>
/// <param name="Priority">"HIGH" / "MEDIUM" / "LOW".</param>
/// <param name="WhyMissing">One-sentence rationale ("You have 5 shirts but no formal blazer").</param>
/// <param name="ClosetPairings">Names of items already in the closet that would pair with a new piece in this category.</param>
public sealed record MissingEssential(
    string Category,
    string Priority,
    string WhyMissing,
    IReadOnlyList<string> ClosetPairings);

/// <summary>
/// One curated product recommendation. The Shopping page renders one card per
/// <see cref="Recommendation"/>, with a "Shop Now" deep link to a real online
/// store and a "Why this is recommended" accordion underneath the price.
/// </summary>
/// <param name="Category">The closet gap this product fills.</param>
/// <param name="Brand">Suggested brand (deterministic catalog lookup).</param>
/// <param name="Name">Short product name (e.g. "Tailored Navy Blazer").</param>
/// <param name="Price">Display string (e.g. "$185").</param>
/// <param name="PriceUsd">Numeric USD for the budget math.</param>
/// <param name="PairsWithCloset">Names of closet items this would pair with.</param>
/// <param name="Description">2-sentence product description.</param>
/// <param name="WhyRecommended">"Value for money" rationale: 1 sentence explaining why this is the right pick for the user's closet + budget.</param>
/// <param name="CompatibilityScore">0–100. How many of the user's existing outfits this new item would unlock.</param>
public sealed record ShoppingProductRecommendation(
    string Category,
    string Brand,
    string Name,
    string Price,
    decimal PriceUsd,
    IReadOnlyList<string> PairsWithCloset,
    string Description,
    string WhyRecommended,
    int CompatibilityScore);

/// <summary>
/// Static, deterministic wardrobe analyser (SDS Module 7 + Phase 6).
///
/// <para>The shopping recommendations are computed locally from the user's
/// real <see cref="ClothingItem"/> rows + style preferences — no LLM call is
/// required for the analysis itself. The deterministic output is reproducible
/// and offline-first; the LLM is reserved for free-text explanations in
/// later phases.</para>
/// </summary>
public interface IWardrobeAnalysisService
{
    /// <summary>
    /// Builds a <see cref="WardrobeAnalysis"/> for <paramref name="closet"/>: the
    /// completion score, the missing essentials ranked by priority, and a
    /// short summary sentence.
    /// </summary>
    WardrobeAnalysis Analyze(IReadOnlyList<ClothingItem> closet, IReadOnlyList<string> stylePreferences);

    /// <summary>
    /// Composes one <see cref="ShoppingProductRecommendation"/> per missing
    /// essential. Each pick is deterministic for the same input closet (no
    /// LLM round-trip) and capped at <paramref name="maxResults"/> items
    /// sorted by <see cref="ShoppingProductRecommendation.CompatibilityScore"/>
    /// descending.
    /// </summary>
    IReadOnlyList<ShoppingProductRecommendation> BuildRecommendations(
        IReadOnlyList<ClothingItem> closet,
        IReadOnlyList<MissingEssential> missingEssentials,
        decimal monthlyBudget,
        int maxResults = 4);
}
