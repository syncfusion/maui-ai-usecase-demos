using System.Text.Json;
using Microsoft.Extensions.Logging;
using WardrobeAI.Models;

namespace WardrobeAI.Services;

/// <summary>
/// Default <see cref="IWardrobeEssentialsCatalog"/> implementation.
///
/// <para>Reads the curated <c>Resources/Raw/wardrobe.json</c> embedded
/// asset once on first use and caches the parsed list. Diff logic maps
/// every JSON entry to a <c>ClothingItem.Category</c> bucket and counts
/// how many of the user's existing outfits the missing piece would
/// unlock (priority heuristic).</para>
/// </summary>
public sealed class WardrobeEssentialsCatalog : IWardrobeEssentialsCatalog
{
    /// <summary>
    /// Path of the embedded asset inside the app package. The
    /// <c>MauiAsset</c> MSBuild item in <c>WardrobeAI.csproj</c> deploys
    /// everything under <c>Resources/Raw</c> with the
    /// <c>%(RecursiveDir)%(Filename)%(Extension)</c> logical name, so the
    /// file is accessible at the package root under its bare file name.
    /// </summary>
    private const string AssetFileName = "wardrobe.json";

    private readonly ILogger<WardrobeEssentialsCatalog> _logger;
    private List<WardrobeEssentialItem>? _cache;

    public WardrobeEssentialsCatalog(ILogger<WardrobeEssentialsCatalog> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public IReadOnlyList<WardrobeEssentialItem> LoadCatalog()
    {
        if (_cache is not null)
        {
            return _cache;
        }

        try
        {
            using var stream = FileSystem.OpenAppPackageFileAsync(AssetFileName)
                .GetAwaiter()
                .GetResult();
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();

            var parsed = JsonSerializer.Deserialize<List<WardrobeEssentialItem>>(json, JsonOptions);
            if (parsed is null || parsed.Count == 0)
            {
                _logger.LogWarning("wardrobe.json parsed as empty; no essentials will be shown.");
                _cache = new List<WardrobeEssentialItem>();
                return _cache;
            }

            _logger.LogInformation("Loaded {Count} essential items from {Asset}.", parsed.Count, AssetFileName);
            _cache = parsed;
            return _cache;
        }
        catch (Exception ex)
        {
            // We never want the shopping page to crash because the JSON is
            // malformed or missing — just degrade gracefully and log.
            _logger.LogError(ex, "Failed to load {Asset}; essential cards will be empty.", AssetFileName);
            _cache = new List<WardrobeEssentialItem>();
            return _cache;
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<EssentialItemCard> BuildMissingEssentials(IReadOnlyList<ClothingItem> closet)
    {
        closet ??= Array.Empty<ClothingItem>();
        var catalog = LoadCatalog();
        if (catalog.Count == 0)
        {
            return Array.Empty<EssentialItemCard>();
        }

        // Normalise the user's owned names for cheap case-insensitive
        // "already own something like this" matching.
        var ownedNames = closet
            .Where(c => !string.IsNullOrWhiteSpace(c.Name))
            .Select(c => c.Name.Trim().ToLowerInvariant())
            .ToHashSet(StringComparer.Ordinal);

        // Count how many owned items the new piece would "pair with" by
        // looking at every other category in the closet.
        var ownedByCategory = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in closet)
        {
            if (string.IsNullOrWhiteSpace(item.Category))
            {
                continue;
            }

            ownedByCategory[item.Category] = ownedByCategory.TryGetValue(item.Category, out var n) ? n + 1 : 1;
        }

        var cards = new List<EssentialItemCard>(capacity: catalog.Count);
        foreach (var entry in catalog)
        {
            if (string.IsNullOrWhiteSpace(entry.Name))
            {
                continue;
            }

            // The user already owns something that looks like this — skip it.
            if (ownedNames.Contains(entry.Name.Trim().ToLowerInvariant()))
            {
                continue;
            }

            // Fuzzy "already covered" check: if the user owns an item whose
            // name overlaps the JSON entry's noun, treat it as covered.
            // e.g. owns "My favourite white tee" → covers "White Crew Neck T-Shirt".
            if (IsLikelyCoveredByName(entry.Name, ownedNames))
            {
                continue;
            }

            var category = MapEssentialToCategory(entry.Name);
            var priority = ResolvePriority(entry.Name, category, ownedByCategory, closet.Count);
            var reason = BuildReason(entry.Name, category, ownedByCategory, closet.Count);
            var pairsCount = CountPotentialPairings(category, ownedByCategory);
            var price = ResolvePrice(category, entry.Name);

            cards.Add(new EssentialItemCard(
                Name: entry.Name,
                ImageSource: ResolveImageSource(entry.Image),
                Link: entry.Link,
                Category: category,
                Priority: priority,
                Reason: reason,
                PairsWithCount: pairsCount,
                Price: price));
        }

        // Order by priority (high → medium → low), then by name for stability.
        var priorityOrder = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["high"] = 0, ["medium"] = 1, ["low"] = 2,
        };
        return cards
            .OrderBy(c => priorityOrder.TryGetValue(c.Priority, out var p) ? p : 1)
            .ThenBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// True when one of the user's owned item names shares a distinctive
    /// token with the JSON entry's name. We tokenise on whitespace and
    /// require at least one strong noun ("blazer", "jeans", "kurta", "sneakers", …)
    /// to overlap so the heuristic stays precise.
    /// </summary>
    private static bool IsLikelyCoveredByName(string essentialName, HashSet<string> ownedNames)
    {
        var tokens = Tokenise(essentialName);
        if (tokens.Count == 0)
        {
            return false;
        }

        // Nouns that uniquely identify a clothing slot. We don't match on
        // generic tokens like "shirt" alone because every user owns at
        // least one.
        var strongNouns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "blazer", "kurta", "saree", "sneakers", "loafers", "oxford",
            "hoodie", "blazer", "sweatshirt", "jacket", "polo", "shorts",
            "belt", "jeans", "chinos", "trousers", "running",
        };

        var strongMatches = tokens.Where(t => strongNouns.Contains(t)).ToList();
        if (strongMatches.Count == 0)
        {
            return false;
        }

        foreach (var owned in ownedNames)
        {
            var ownedTokens = Tokenise(owned);
            if (strongMatches.Any(m => ownedTokens.Contains(m)))
            {
                return true;
            }
        }

        return false;
    }

    private static List<string> Tokenise(string value)
    {
        return value
            .ToLowerInvariant()
            .Split(new[] { ' ', '\t', '-', '_', ',', '.', '/', '(' , ')' },
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    /// <summary>
    /// Maps a <c>wardrobe.json</c> display name to the canonical closet
    /// category it belongs to. This is what lets us correlate an essential
    /// with the user's existing pieces and rank it by "how many existing
    /// outfits it would unlock".
    /// </summary>
    private static string MapEssentialToCategory(string essentialName)
    {
        var n = essentialName.ToLowerInvariant();
        if (n.Contains("blazer")) return "Blazers";
        if (n.Contains("oxford") || n.Contains("formal white shirt")) return "Shirts";
        if (n.Contains("checked shirt")) return "Shirts";
        if (n.Contains("polo")) return "Polo Shirts";
        if (n.Contains("t-shirt") || n.Contains("tshirt") || n.Contains("crew")) return "T-Shirts";
        if (n.Contains("hoodie")) return "Hoodies";
        if (n.Contains("jacket")) return "Jackets";
        if (n.Contains("jeans")) return "Jeans";
        if (n.Contains("chinos")) return "Trousers";
        if (n.Contains("trousers")) return "Trousers";
        if (n.Contains("shorts")) return "Shorts";
        if (n.Contains("kurta") || n.Contains("traditional")) return "Kurtas";
        if (n.Contains("sneakers")) return "Footwear";
        if (n.Contains("loafers") || n.Contains("formal") && n.Contains("shoes")) return "Footwear";
        if (n.Contains("running") && n.Contains("shoes")) return "Activewear";
        if (n.Contains("belt")) return "Accessories";
        return "Essentials";
    }

    /// <summary>
    /// Priority heuristic: a high-priority essential is one the user has
    /// 0 of in its category AND owns many items in complementary slots
    /// (so adding it would unlock lots of new outfits). Low priority is
    /// the opposite — niche / situational pieces.
    /// </summary>
    private static string ResolvePriority(
        string essentialName,
        string category,
        IReadOnlyDictionary<string, int> ownedByCategory,
        int totalClosetSize)
    {
        // Core slots the SDS calls "foundational" — missing one of these
        // is a high-priority gap.
        var foundational = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "T-Shirts", "Shirts", "Jeans", "Trousers", "Blazers", "Footwear",
        };

        var ownedInCategory = ownedByCategory.TryGetValue(category, out var c) ? c : 0;

        if (foundational.Contains(category) && totalClosetSize >= 5)
        {
            return ownedInCategory == 0 ? "high" : "medium";
        }

        // Accessory-type or situational items are always low priority.
        if (category is "Accessories" or "Activewear")
        {
            return "low";
        }

        return ownedInCategory == 0 && totalClosetSize > 0 ? "medium" : "low";
    }

    private static string BuildReason(
        string essentialName,
        string category,
        IReadOnlyDictionary<string, int> ownedByCategory,
        int totalClosetSize)
    {
        if (totalClosetSize == 0)
        {
            return $"Build a foundational wardrobe starting with {essentialName}.";
        }

        var ownedInCategory = ownedByCategory.TryGetValue(category, out var c) ? c : 0;
        if (ownedInCategory == 0)
        {
            return $"You have no {category.ToLowerInvariant()} yet — this is a foundational slot.";
        }

        return $"Diversify your {category.ToLowerInvariant()} rotation with {essentialName}.";
    }

    private static int CountPotentialPairings(
        string category,
        IReadOnlyDictionary<string, int> ownedByCategory)
    {
        // Count owned items in *complementary* categories. The mapping is
        // intentionally generous — every owned shirt pairs with a new
        // trouser, etc.
        var complementary = category switch
        {
            "T-Shirts" or "Shirts" or "Polo Shirts" => new[] { "Jeans", "Trousers", "Shorts" },
            "Jeans" or "Trousers" or "Shorts" => new[] { "T-Shirts", "Shirts", "Polo Shirts", "Blazers" },
            "Blazers" => new[] { "Shirts", "T-Shirts", "Polo Shirts", "Jeans", "Trousers" },
            "Jackets" or "Hoodies" => new[] { "T-Shirts", "Shirts", "Jeans" },
            "Kurtas" => new[] { "Trousers", "Jeans" },
            "Footwear" => new[] { "Jeans", "Trousers", "Shorts" },
            "Accessories" => new[] { "Jeans", "Trousers" },
            _ => Array.Empty<string>(),
        };

        var total = 0;
        foreach (var c in complementary)
        {
            if (ownedByCategory.TryGetValue(c, out var n))
            {
                total += n;
            }
        }

        return total;
    }

    private static ImageSource? ResolveImageSource(string imageFileName)
    {
        if (string.IsNullOrWhiteSpace(imageFileName))
        {
            return null;
        }

        // Resolves the curated shopping PNG/JPG via the bundled MauiAsset
        // (see WardrobeAI.csproj). The LogicalName reflection of
        // %(RecursiveDir)%(Filename)%(Extension) for Resources\Images\shopping\**
        // ends up bundling assets with a bare file name at the package root
        // on every platform (Windows MSIX unpackage drops the "shopping/"
        // segment), so we look up the bare file name. FromStream() works the
        // same on Windows / Android / iOS / Mac and lets us fall back
        // across .png / .jpg / .jpeg — wardrobe.json has a few entries
        // whose extension drifted from the shipped file.
        //
        // The factory passed to ImageSource.FromStream must yield a FRESH
        // readable stream each call — MAUI invokes the factory more than
        // once per image (initial decode + reload on layout / theme
        // changes / virtualization recycling). Capturing a single opened
        // stream in the closure leaves the second decode reading from an
        // exhausted/disposed handle, which silently produces a blank
        // <Image> on the shopping page. Open the candidate lazily inside
        // the factory instead so each call returns a brand-new stream.
        var normalized = imageFileName.Trim();
        if (normalized.Contains('/') || normalized.Contains('\\'))
        {
            return null;
        }
        var candidates = BuildCandidateNames(normalized).ToArray();
        return ImageSource.FromStream(() => OpenShoppingAsset(candidates));
    }

    private static Stream? OpenShoppingAsset(IReadOnlyList<string> candidates)
    {
        foreach (var candidate in candidates)
        {
            try
            {
                return FileSystem.OpenAppPackageFileAsync(candidate)
                    .GetAwaiter()
                    .GetResult();
            }
            catch
            {
                // try the next extension variant
            }
        }
        return null;
    }

    /// <summary>
    /// wardrobe.json sometimes refers to ".png" / ".jpg" / ".jpeg"
    /// interchangeably for the same conceptual asset, and the curated
    /// folder ships them at different extensions. Try each on the same
    /// stem until one resolves.
    /// </summary>
    private static IEnumerable<string> BuildCandidateNames(string fileName)
    {
        yield return fileName; // honour whatever JSON says first
        var stem = Path.GetFileNameWithoutExtension(fileName);
        if (string.IsNullOrEmpty(stem))
        {
            yield break;
        }
        foreach (var ext in new[] { ".png", ".jpg", ".jpeg" })
        {
            var candidate = stem + ext;
            if (!string.Equals(candidate, fileName, StringComparison.OrdinalIgnoreCase))
            {
                yield return candidate;
            }
        }
    }

    /// <summary>
    /// Generates a believable, category-appropriate, item-specific display
    /// price for each essential. Real pricing isn't bundled in
    /// <c>wardrobe.json</c> (it's just an Amazon search URL), so we hash
    /// the item name into a stable random offset inside a per-category
    /// USD range. The same item always shows the same price on every
    /// render — so it never flickers between values — but two different
    /// essentials in the same category will surface different prices
    /// (e.g. a white crew tee lands at $14, a black crew tee at $16,
    /// a navy tee at $12), which is what the user expects from a real
    /// shopping feed.
    /// </summary>
    private static string ResolvePrice(string category, string essentialName)
    {
        // (low, high) USD range the category typically occupies.
        var (low, high) = category switch
        {
            "T-Shirts"     => (10, 25),
            "Polo Shirts"  => (18, 35),
            "Shirts"       => (25, 55),
            "Jeans"        => (30, 70),
            "Trousers"     => (30, 65),
            "Shorts"       => (18, 40),
            "Hoodies"      => (30, 60),
            "Jackets"      => (55, 120),
            "Blazers"      => (80, 180),
            "Kurtas"       => (25, 70),
            "Footwear"     => (45, 120),
            "Activewear"   => (60, 140),
            "Accessories"  => (15, 45),
            _              => (20, 60),
        };

        // Stable, deterministic-per-name offset: same name → same price,
        // different name → different price within the band. Use
        // string.GetHashCode (process-local but stable for the AppDomain
        // lifetime, which is fine since the price is recomputed on every
        // page load and just needs to be internally consistent across
        // the visible cards in one render).
        var hash = Math.Abs((essentialName ?? string.Empty)
            .ToLowerInvariant()
            .GetHashCode());
        var span = Math.Max(1, high - low);
        var offset = hash % span;
        var price = low + offset;
        return $"${price}";
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };
}
