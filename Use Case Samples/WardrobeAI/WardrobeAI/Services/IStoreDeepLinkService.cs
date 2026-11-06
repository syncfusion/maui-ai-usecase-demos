namespace WardrobeAI.Services;

/// <summary>
/// A real-world retailer we can deep-link to from the Shopping page. Each
/// entry is a (Name, SearchUrlTemplate) pair where the template is a
/// <c>{0}</c> placeholder for the URL-encoded search query.
/// </summary>
/// <param name="Name">Human-readable name ("Amazon").</param>
/// <param name="SearchUrlTemplate">URL pattern with <c>{0}</c> for the query ("https://www.amazon.com/s?k={0}").</param>
public sealed record StoreLinkEntry(string Name, string SearchUrlTemplate);

/// <summary>
/// Resolves a <see cref="ShoppingProductRecommendation"/> to a real, online
/// store search URL. Built so the user can tap "Shop Now" and land on the
/// retailer with their search query pre-filled — no affiliate links, no
/// sketchy redirects, just a clean handoff to a known retailer.
/// </summary>
public interface IStoreDeepLinkService
{
    /// <summary>
    /// The set of retailers exposed in the Shopping page's "Shop on" picker.
    /// Ordered by global availability: Amazon (worldwide) first, then the
    /// regional favourites.
    /// </summary>
    IReadOnlyList<StoreLinkEntry> AvailableStores { get; }

    /// <summary>
    /// Builds a search URL on <paramref name="storeName"/> for the product
    /// described by <paramref name="category"/> + <paramref name="storeQuery"/>.
    /// Returns <c>null</c> if the store is unknown — the caller should
    /// default to the first available store in that case.
    /// </summary>
    Uri? BuildDeepLink(string storeName, string category, string storeQuery);
}
