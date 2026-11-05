using WardrobeAI.Models;

namespace WardrobeAI.Services;

/// <summary>
/// One entry from the curated <c>Resources/Raw/wardrobe.json</c> "essential
/// closet" catalog. The shopping page renders one card per entry the user
/// still needs.
/// </summary>
/// <param name="Name">Display name (e.g. "White Crew Neck T-Shirt").</param>
/// <param name="Image">PNG file name (resolved against <see cref="MauiAsset"/> at runtime).</param>
/// <param name="Link">Amazon search deep link (preserved verbatim from the JSON).</param>
public sealed record WardrobeEssentialItem(
    string Name,
    string Image,
    string Link);

/// <summary>
/// One essential-item card projected for the Shopping page, with the
/// priority reasoning + how many of the user's existing outfits the new
/// piece would unlock.
/// </summary>
/// <param name="Name">Display name (e.g. "White Crew Neck T-Shirt").</param>
/// <param name="ImageSource">A live <see cref="ImageSource"/> for the embedded PNG, or <c>null</c> when the curated asset is missing — the XAML template renders a placeholder in that case.</param>
/// <param name="Link">Amazon search deep link (preserved verbatim from the JSON).</param>
/// <param name="Category">Closet category the item belongs to (e.g. "T-Shirts").</param>
/// <param name="Priority">"HIGH" / "MEDIUM" / "LOW" — derived from how many of the user's existing pieces the new item would combine with.</param>
/// <param name="Reason">One sentence explaining why this essential is recommended.</param>
/// <param name="PairsWithCount">How many of the user's existing outfits the new item would unlock.</param>
/// <param name="Price">Display price (e.g. "$30") — parsed from the JSON link, falls back to "—" when unknown.</param>
public sealed record EssentialItemCard(
    string Name,
    ImageSource? ImageSource,
    string Link,
    string Category,
    string Priority,
    string Reason,
    int PairsWithCount,
    string Price);

/// <summary>
/// Reads the curated <c>wardrobe.json</c> "essential closet" catalog, diffs
/// it against the signed-in user's real <see cref="ClothingItem"/> rows, and
/// returns the items the user is still missing — ranked by priority
/// (highest first).
/// </summary>
public interface IWardrobeEssentialsCatalog
{
    /// <summary>
    /// Loads the embedded <c>wardrobe.json</c> from the app package. The
    /// result is cached for the lifetime of the process; callers can call
    /// this on every page visit without paying the read/parse cost twice.
    /// </summary>
    Task<IReadOnlyList<WardrobeEssentialItem>> LoadCatalogAsync();

    /// <summary>
    /// Computes the essentials the user is still missing, ranked by
    /// <see cref="EssentialItemCard.Priority"/> (high → medium → low) and
    /// then by <see cref="EssentialItemCard.Name"/>.
    /// </summary>
    /// <param name="closet">All <see cref="ClothingItem"/> rows owned by the user (may be empty).</param>
    Task<IReadOnlyList<EssentialItemCard>> BuildMissingEssentialsAsync(IReadOnlyList<ClothingItem> closet);
}
