using WardrobeAI.Models;

namespace WardrobeAI.AI.Contracts;

/// <summary>
/// Compact projection of a <see cref="ClothingItem"/> that is safe to ship to
/// the LLM (PII / image-bytes omitted per wardrobe.prompt §3.5). Mirrors
/// <see cref="ClothingItemSummary"/> in spirit but is dedicated to the
/// smart-shopping use case so the request payload stays small (the catalog
/// already has 21 items, and the LLM only needs the closet's category
/// distribution + a couple of anchor pieces per category to reason about
/// pairings).
/// </summary>
/// <param name="Id">DB id used for join-back in the audit log only.</param>
/// <param name="Name">Display name chosen by the user (e.g. "Navy Oxford").</param>
/// <param name="Category">Free-form category label (T-Shirts, Jeans, Blazer, …).</param>
/// <param name="Color">Color tag from the catalog.</param>
/// <param name="Pattern">Solid / Striped / Printed / …</param>
/// <param name="Fabric">Cotton / Linen / Wool / …</param>
/// <param name="Style">Slim Fit / Streetwear / …</param>
/// <param name="Occasion">Casual / Business / Wedding / …</param>
public sealed record SmartShoppingClosetItem(
    int Id,
    string Name,
    string Category,
    string Color,
    string Pattern,
    string Fabric,
    string Style,
    string Occasion);

/// <summary>
/// One entry from the curated <c>Resources/Raw/wardrobe.json</c> "essential
/// closet" catalog. Mirrors <see cref="Services.WardrobeEssentialItem"/>
/// but lives in the AI contract namespace so the LLM payload is
/// self-contained (the model does not need to know about MauiAsset paths).
/// </summary>
/// <param name="Name">Display name (e.g. "White Crew Neck T-Shirt").</param>
/// <param name="Category">Closet category the item belongs to (e.g. "T-Shirts").</param>
public sealed record SmartShoppingCatalogItem(
    string Name,
    string Category);

/// <summary>
/// Request to <see cref="IAISmartShoppingService"/>.
/// Carries the user's closet (compact projection) and the curated
/// 21-item essential closet catalog, plus the user's style preferences and
/// the monthly shopping budget so the model can reason about both
/// pair-with-closet and value-for-money in a single round-trip.
/// </summary>
/// <param name="UserId">DB id, used for join-back in the audit log only.</param>
/// <param name="StylePreference">Free-form (e.g. "Casual, Streetwear").</param>
/// <param name="BudgetLevel">Low / Medium / Premium / Luxury.</param>
/// <param name="MonthlyBudgetUsd">Numeric monthly budget for affordability reasoning.</param>
/// <param name="Closet">Compact projection of the user's owned items (may be empty).</param>
/// <param name="Catalog">Curated 21-item essential-closet catalog (e.g. <c>wardrobe.json</c>).</param>
/// <param name="MaxResults">Soft cap on how many essentials the LLM should return (default 5).</param>
public sealed record AISmartShoppingRequest(
    int UserId,
    string StylePreference,
    string BudgetLevel,
    decimal MonthlyBudgetUsd,
    IReadOnlyList<SmartShoppingClosetItem> Closet,
    IReadOnlyList<SmartShoppingCatalogItem> Catalog,
    int MaxResults = 6);

/// <summary>
/// One essential the LLM picked from the catalog, plus a 1-sentence
/// pair-aware reason that the Shopping page renders under the card. The
/// pick is anchored to a <c>CatalogName</c> so the VM can join back to
/// the real catalog row to render the image + link.
/// </summary>
/// <param name="CatalogName">The exact <c>name</c> of the catalog row the LLM chose (case-insensitive match).</param>
/// <param name="Category">Closet category the chosen item belongs to (e.g. "T-Shirts").</param>
/// <param name="Priority">"HIGH" / "MEDIUM" / "LOW" — how urgently the LLM thinks the user needs this piece.</param>
/// <param name="PairsWithNames">Names of the user's existing items the new piece would pair with.</param>
/// <param name="Reason">1-sentence pair-aware reason tailored to the user's closet.</param>
public sealed record AISmartShoppingPick(
    string CatalogName,
    string Category,
    string Priority,
    IReadOnlyList<string> PairsWithNames,
    string Reason);

/// <summary>
/// Final result of <see cref="IAISmartShoppingService"/>.
/// </summary>
/// <param name="Picks">The LLM's picks, ordered by priority (HIGH → LOW).</param>
/// <param name="AnalysisSummary">1-2 sentence human-readable summary of the closet gap analysis.</param>
public sealed record AISmartShoppingResult(
    IReadOnlyList<AISmartShoppingPick> Picks,
    string AnalysisSummary);
