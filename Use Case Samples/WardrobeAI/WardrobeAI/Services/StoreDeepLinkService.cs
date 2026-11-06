using System.Web;
using Microsoft.Extensions.Logging;

namespace WardrobeAI.Services;

/// <summary>
/// Static, deterministic deep-link service. The "search URL" pattern is
/// the same one Amazon / Flipkart / Myntra render when you type into
/// their own search bar, so the handoff is clean and the user lands on a
/// real, browsable list of matching products — no affiliate cookies,
/// no redirects, no sketchy clicks.
/// </summary>
public sealed class StoreDeepLinkService : IStoreDeepLinkService
{
    private readonly ILogger<StoreDeepLinkService> _logger;

    /// <summary>
    /// Curated retailer list. Order matters: <c>AvailableStores[0]</c> is the
    /// default "Shop Now" target when the user doesn't pick one.
    /// </summary>
    private static readonly IReadOnlyList<StoreLinkEntry> Stores = new[]
    {
        new StoreLinkEntry("Amazon",   "https://www.amazon.com/s?k={0}"),
        new StoreLinkEntry("Flipkart", "https://www.flipkart.com/search?q={0}"),
        new StoreLinkEntry("Myntra",   "https://www.myntra.com/{0}"),
        new StoreLinkEntry("Nordstrom","https://www.nordstrom.com/sr?keyword={0}"),
        new StoreLinkEntry("ASOS",     "https://www.asos.com/search/?q={0}"),
    };

    public StoreDeepLinkService(ILogger<StoreDeepLinkService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public IReadOnlyList<StoreLinkEntry> AvailableStores => Stores;

    /// <inheritdoc />
    public Uri? BuildDeepLink(string storeName, string category, string storeQuery)
    {
        if (string.IsNullOrWhiteSpace(storeName))
        {
            return null;
        }

        var entry = Stores.FirstOrDefault(s =>
            string.Equals(s.Name, storeName, StringComparison.OrdinalIgnoreCase));
        if (entry is null)
        {
            _logger.LogDebug("Unknown store '{Store}' requested for deep link.", storeName);
            return null;
        }

        // Build the query string. Prefer the curated storeQuery (better SEO
        // keywords), fall back to the category label. We always URL-encode
        // so spaces and special characters don't break the link.
        var rawQuery = !string.IsNullOrWhiteSpace(storeQuery) ? storeQuery : category;
        if (string.IsNullOrWhiteSpace(rawQuery))
        {
            return null;
        }

        var encoded = HttpUtility.UrlEncode(rawQuery.Trim());
        if (string.IsNullOrEmpty(encoded))
        {
            return null;
        }

        // Myntra's path-segment search (myntra.com/<query>) needs the query
        // stripped of URL-encoded slashes & spaces; replace them with hyphens.
        if (string.Equals(entry.Name, "Myntra", StringComparison.OrdinalIgnoreCase))
        {
            var myntraSlug = string.Join(
                "-",
                rawQuery.Trim()
                    .ToLowerInvariant()
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(Uri.EscapeDataString));
            return new Uri($"https://www.myntra.com/{myntraSlug}");
        }

        var url = string.Format(entry.SearchUrlTemplate, encoded);
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            _logger.LogWarning("Failed to build deep link for store {Store} query {Query}", storeName, rawQuery);
            return null;
        }

        return uri;
    }
}
