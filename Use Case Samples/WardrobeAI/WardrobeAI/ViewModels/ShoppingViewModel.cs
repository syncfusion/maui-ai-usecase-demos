using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WardrobeAI.AI;
using WardrobeAI.AI.Contracts;
using WardrobeAI.Models;
using WardrobeAI.Repositories;
using WardrobeAI.Services;

namespace WardrobeAI.ViewModels;

/// <summary>
/// Smart Shopping view model (SDS Module 7 / Phase 6).
///
/// <para>Mirrors <c>/designs/smart_shopping.html</c>:
/// wardrobe analysis (completion), budget widget, missing essentials,
/// curated product feed with "Why this is recommended" explanations,
/// and a "Shop on &lt;retailer&gt;" deep-link handoff to a real online store.</para>
///
/// <para>The deterministic "Curated For You" feed + completion score +
/// per-category missing-essential list are computed locally by
/// <see cref="IWardrobeAnalysisService"/> from the signed-in user's real
/// <see cref="ClothingItem"/> rows + style preferences + monthly budget
/// — no LLM round-trip is required for the analysis itself.</para>
///
/// <para>The "Missing Essential" card feed, however, is now powered by
/// <see cref="IAISmartShoppingService"/>: the user's real closet is
/// projected to <see cref="SmartShoppingClosetItem"/>, the curated
/// 21-item essential-closet catalog is loaded via
/// <see cref="IWardrobeEssentialsCatalog"/>, and the AI picks a
/// RANDOMIZED set of missing essentials that genuinely pair with what
/// the user already owns. This replaces the previous
/// <c>BuildMissingEssentials(...).Take(6)</c> behaviour, which always
/// surfaced the same first 6 catalog rows in catalog order regardless
/// of the user's actual closet.</para>
/// </summary>
public partial class ShoppingViewModel : ObservableObject, ITabRefreshable
{
    /// <summary>
    /// How many "Missing Essential" cards to render. The LLM prompt
    /// (and the deterministic fallback) cap at this value so the page
    /// stays scannable; the user can re-roll via
    /// <see cref="RefreshEssentialPicksAsyncCommand"/>.
    /// </summary>
    private const int EssentialCardLimit = 6;

    private readonly IClothingItemRepository _closetRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;
    private readonly IWardrobeAnalysisService _analysisService;
    private readonly IStoreDeepLinkService _storeService;
    private readonly IBudgetService _budgetService;
    private readonly IWardrobeEssentialsCatalog _essentialsCatalog;
    private readonly IAISmartShoppingService _smartShoppingService;
    private readonly ILogger<ShoppingViewModel> _logger;

    public ShoppingViewModel(
        IClothingItemRepository closetRepository,
        IUserRepository userRepository,
        IUserContext userContext,
        IWardrobeAnalysisService analysisService,
        IStoreDeepLinkService storeService,
        IBudgetService budgetService,
        IWardrobeEssentialsCatalog essentialsCatalog,
        IAISmartShoppingService smartShoppingService,
        ILogger<ShoppingViewModel> logger)
    {
        _closetRepository = closetRepository;
        _userRepository = userRepository;
        _userContext = userContext;
        _analysisService = analysisService;
        _storeService = storeService;
        _budgetService = budgetService;
        _essentialsCatalog = essentialsCatalog;
        _smartShoppingService = smartShoppingService;
        _logger = logger;

        // Empty defaults; populated by LoadAsync() after the default user is initialized.
        CompletionScore = 0;
        ItemsOwned = 0;
        ItemsTarget = 25;
        MonthlyBudget = "$0";
        BudgetAdvice = "Your shopping budget will appear here.";
        AnalysisTitle = "Wardrobe Analysis";
        AnalysisSubtitle = "Your wardrobe gaps will appear here.";
        SortLabel = "SORT: BEST VALUE";
        IsLoading = false;

        MissingEssentials = new ObservableCollection<EssentialItemRow>();
        Recommendations = new ObservableCollection<ProductCard>();
        EssentialCards = new ObservableCollection<EssentialItemCard>();
        AvailableStores = new ObservableCollection<StoreOption>(
            _storeService.AvailableStores.Select(s => new StoreOption(s.Name, isSelected: s.Name == "Amazon")));
    }

    // ---- Bound scalar state ----
    [ObservableProperty] private int _completionScore;
    [ObservableProperty] private int _itemsOwned;
    [ObservableProperty] private int _itemsTarget;
    [ObservableProperty] private string _monthlyBudget = string.Empty;
    [ObservableProperty] private string _budgetAdvice = string.Empty;
    [ObservableProperty] private string _analysisTitle = string.Empty;
    [ObservableProperty] private string _analysisSubtitle = string.Empty;
    [ObservableProperty] private string _sortLabel = string.Empty;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isUnavailable;
    [ObservableProperty] private bool _hasEssentialCards;

    // ---- Bound collections ----
    public ObservableCollection<EssentialItemRow> MissingEssentials { get; }
    public ObservableCollection<ProductCard> Recommendations { get; }
    public ObservableCollection<EssentialItemCard> EssentialCards { get; }
    public ObservableCollection<StoreOption> AvailableStores { get; }

    // ---- Commands ----

    /// <summary>Pulls closet + budget + user style and rebuilds the page.</summary>
    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsLoading)
        {
            return;
        }

        try
        {
            IsLoading = true;
            var userId = _userContext.GetCurrentUserId();
            if (userId <= 0)
            {
                ResetUnavailableData();
                return;
            }

            IsUnavailable = false;

            // 1. Closet + user.
            var closet = await _closetRepository.GetAllForUserAsync(userId);
            var user = await _userRepository.GetAsync(userId);
            var stylePrefs = (user?.StylePreference ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

            // 2. Budget.
            var monthlyBudgetUsd = _budgetService.GetMonthlyBudget(userId);
            MonthlyBudget = $"${monthlyBudgetUsd:0}";
            BudgetAdvice = BuildBudgetAdvice(monthlyBudgetUsd, closet.Count);

            // 3. Analysis.
            var analysis = _analysisService.Analyze(closet, stylePrefs);
            CompletionScore = analysis.CompletionScore;
            ItemsOwned = analysis.ItemsOwned;
            ItemsTarget = analysis.ItemsTarget;
            AnalysisTitle = $"You own {analysis.ItemsOwned} of {analysis.ItemsTarget} core pieces";
            AnalysisSubtitle = analysis.AnalysisSummary;

            MissingEssentials.Clear();
            foreach (var essential in analysis.MissingEssentials.Take(6))
            {
                MissingEssentials.Add(new EssentialItemRow(
                    name: essential.Category,
                    priority: $"{essential.Priority} PRIORITY",
                    reason: essential.WhyMissing,
                    pairingsPreview: essential.ClosetPairings.Count > 0
                        ? $"Pairs with {essential.ClosetPairings.Count} of your pieces"
                        : "Foundational wardrobe piece",
                    isHigh: string.Equals(essential.Priority, "high", StringComparison.OrdinalIgnoreCase)));
            }

            // 4. Recommendations: cap at 4 cards so the feed stays scannable.
            var recs = _analysisService.BuildRecommendations(
                closet: closet,
                missingEssentials: analysis.MissingEssentials,
                monthlyBudget: monthlyBudgetUsd,
                maxResults: 4);

            Recommendations.Clear();
            foreach (var r in recs)
            {
                Recommendations.Add(new ProductCard(
                    category: r.Category,
                    brand: r.Brand,
                    name: r.Name,
                    price: r.Price,
                    priceUsd: r.PriceUsd,
                    pairsWith: r.PairsWithCloset.Count > 0
                        ? $"Pairs with {r.PairsWithCloset.Count} of your pieces"
                        : "Fills a wardrobe gap",
                    pairsWithNames: r.PairsWithCloset,
                    description: r.Description,
                    whyRecommended: r.WhyRecommended,
                    compatibility: r.CompatibilityScore));
            }

            if (Recommendations.Count == 0)
            {
                SortLabel = "CLOSET COVERED — ALL CORE CATEGORIES PRESENT";
            }
            else
            {
                SortLabel = "SORT: BEST VALUE";
            }

            // 5. Missing-Essential cards: AI-driven, pair-aware, RANDOMIZED,
            //    but only re-rolled when the underlying closet has actually
            //    changed. The user reported that the previous behaviour —
            //    re-running the AI analysis on every tab switch / page
            //    re-appearance — made the curated cards feel unstable
            //    ("it keeps refreshing by analysing again"). Now the
            //    "Missing Essential" section is sticky: it stays as the
            //    user switches tabs and comes back, and only re-rolls
            //    after items are added / removed / edited in the Closet
            //    tab, or after the monthly budget changes materially.
            _lastCloset = closet;
            _lastUser = user;
            _lastMonthlyBudgetUsd = monthlyBudgetUsd;

            var currentClosetHash = BuildClosetSnapshotHash(closet, monthlyBudgetUsd);
            if (!string.Equals(currentClosetHash, _lastEssentialPicksClosetHash, StringComparison.Ordinal)
                || EssentialCards.Count == 0)
            {
                _lastEssentialPicksClosetHash = currentClosetHash;
                await RefreshEssentialPicksAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load shopping recommendations.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Per-user stash from the last successful <see cref="LoadAsync"/>.
    /// Used by <see cref="RefreshEssentialPicksAsync"/> so the user can
    /// re-roll the random pick set without re-hitting the DB.
    /// </summary>
    private IReadOnlyList<ClothingItem>? _lastCloset;
    private User? _lastUser;
    private decimal _lastMonthlyBudgetUsd;

    /// <summary>
    /// Hash of the closet snapshot used the last time the AI-picks were
    /// re-rolled. Compared against the fresh closet on every
    /// <see cref="LoadAsync"/> so the "Missing Essential" cards only
    /// re-roll when the user's closet has actually changed (item added
    /// / removed / edited) — not on every tab switch or page
    /// re-appearance, which was the behaviour the user flagged.
    ///
    /// <para>Empty string means "no picks rendered yet" and forces a
    /// fresh roll on the next <see cref="LoadAsync"/>.</para>
    /// </summary>
    private string _lastEssentialPicksClosetHash = string.Empty;

    /// <summary>
    /// Re-runs the AI smart-shopping pick against the same closet +
    /// budget the last <see cref="LoadAsync"/> resolved, and re-renders
    /// the Essential Cards section. Lets the user "shuffle" the random
    /// recommendations without leaving the page or hitting the DB.
    /// </summary>
    [RelayCommand]
    private async Task RefreshEssentialPicksAsync()
    {
        if (_lastCloset is null)
        {
            return;
        }

        try
        {
            EssentialCards.Clear();
            HasEssentialCards = false;

            var stylePrefs = (_lastUser?.StylePreference ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
            var budgetLevel = _lastUser?.BudgetLevel.ToString() ?? "Medium";

            var picks = await ResolveEssentialPicksAsync(
                closet: _lastCloset,
                stylePreferences: stylePrefs,
                budgetLevel: budgetLevel,
                monthlyBudgetUsd: _lastMonthlyBudgetUsd,
                cancellationToken: CancellationToken.None);

            foreach (var card in picks)
            {
                EssentialCards.Add(card);
            }
            HasEssentialCards = EssentialCards.Count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh essential picks.");
        }
    }

    /// <summary>
    /// Computes a stable, low-collision string that uniquely identifies
    /// the current closet shape plus the monthly budget, used as the
    /// "did the closet actually change?" signal for the AI-picks sticky
    /// behaviour.
    ///
    /// <para>The hash mixes:
    /// <list type="bullet">
    ///   <item>Item count (catches add/remove at a glance).</item>
    ///   <item>Each <see cref="ClothingItem.ItemId"/> (order-independent).</item>
    ///   <item>Each <see cref="ClothingItem.UpdatedDate"/> ticks (catches
    ///         edits, e.g. user renamed a shirt or swapped its category).</item>
    ///   <item>The monthly budget (rounded to whole dollars; cents don't
    ///         change which essentials are recommended).</item>
    /// </list></para>
    ///
    /// <para>Returns a non-cryptographic but collision-resistant hex
    /// string via <see cref="HashCode"/>. String comparison is fine
    /// because the value is only ever compared against
    /// <see cref="_lastEssentialPicksClosetHash"/>.</para>
    /// </summary>
    private static string BuildClosetSnapshotHash(IReadOnlyList<ClothingItem> closet, decimal monthlyBudgetUsd)
    {
        var itemIds = closet
            .Select(c => c.ItemId)
            .OrderBy(id => id)
            .ToArray();
        var updatedTicks = closet
            .Select(c => c.UpdatedDate.Ticks)
            .OrderBy(t => t)
            .ToArray();
        var budgetWhole = (int)Math.Round(monthlyBudgetUsd, MidpointRounding.AwayFromZero);

        var combined = new HashCode();
        combined.Add(closet.Count);
        combined.Add(budgetWhole);
        foreach (var id in itemIds)
        {
            combined.Add(id);
        }
        foreach (var ticks in updatedTicks)
        {
            combined.Add(ticks);
        }
        return combined.ToHashCode().ToString("X8", System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Projects the user's real <see cref="ClothingItem"/> rows + the
    /// curated 21-item catalog into the AI request shape, calls
    /// <see cref="IAISmartShoppingService"/>, validates the picks, and
    /// joins each pick back to the catalog row that produced it so the
    /// card UI can render the bundled image + Amazon deep link.
    ///
    /// <para>On AI failure (Azure not configured, timeout, malformed
    /// response, all picks rejected) the service itself returns a
    /// deterministic + randomized fallback — so the caller always
    /// gets a usable list of cards. This method then filters that list
    /// against the user's closet one more time and renders the
    /// <see cref="EssentialItemCard"/> DTOs the XAML already binds to.</para>
    /// </summary>
    private async Task<IReadOnlyList<EssentialItemCard>> ResolveEssentialPicksAsync(
        IReadOnlyList<ClothingItem> closet,
        IReadOnlyList<string> stylePreferences,
        string budgetLevel,
        decimal monthlyBudgetUsd,
        CancellationToken cancellationToken)
    {
        // 1. Project the user's real closet down to the AI-safe compact
        //    shape (PII-free: no email, no image bytes).
        var closetProjected = new List<SmartShoppingClosetItem>(closet.Count);
        foreach (var item in closet)
        {
            closetProjected.Add(new SmartShoppingClosetItem(
                Id: item.ItemId,
                Name: item.Name ?? string.Empty,
                Category: item.Category ?? string.Empty,
                Color: item.Color ?? string.Empty,
                Pattern: item.Pattern ?? "Solid",
                Fabric: item.Fabric ?? "Cotton",
                Style: item.Style ?? "Casual",
                Occasion: item.Occasion ?? "Casual"));
        }

        // 2. Load the curated 21-item catalog and project it to the AI
        //    request shape (no image / link — the model only needs the
        //    name + category to reason about pairings).
        var rawCatalog = await _essentialsCatalog.LoadCatalogAsync();
        var catalogProjected = new List<SmartShoppingCatalogItem>(rawCatalog.Count);
        foreach (var entry in rawCatalog)
        {
            if (string.IsNullOrWhiteSpace(entry.Name))
            {
                continue;
            }
            catalogProjected.Add(new SmartShoppingCatalogItem(
                Name: entry.Name.Trim(),
                Category: ResolveCategoryForCatalogEntry(entry.Name)));
        }

        if (catalogProjected.Count == 0)
        {
            return Array.Empty<EssentialItemCard>();
        }

        // 3. Call the AI service. Result<T> so the call site can fall
        //    back gracefully on AI failures; the service itself already
        //    substitutes a deterministic randomized selection on any
        //    error, so we only need to handle the catastrophic "no
        //    picks at all" case.
        var stylePrefJoined = string.Join(", ", stylePreferences);
        var aiRequest = new AISmartShoppingRequest(
            UserId: _lastUser?.UserId ?? 0,
            StylePreference: stylePrefJoined,
            BudgetLevel: budgetLevel,
            MonthlyBudgetUsd: monthlyBudgetUsd,
            Closet: closetProjected,
            Catalog: catalogProjected,
            MaxResults: EssentialCardLimit);

        var aiResult = await _smartShoppingService.GetSmartShoppingPicksAsync(aiRequest, cancellationToken);
        if (!aiResult.IsSuccess || aiResult.Value is null || aiResult.Value.Picks.Count == 0)
        {
            // Last-resort: surface the deterministic priority-sorted list
            // from the catalog service. The user still gets a non-empty
            // page even if both the AI and the AI-fallback paths fail.
            _logger.LogWarning(
                "Smart shopping AI returned no picks (code={Code}); falling back to priority-sorted catalog diff.",
                aiResult.Error?.Code ?? "(none)");
            return (await _essentialsCatalog.BuildMissingEssentialsAsync(closet)).Take(EssentialCardLimit).ToList();
        }

        // 4. Join each AI pick back to the catalog row that produced it
        //    (so the card UI can render the bundled image + Amazon
        //    deep link) and dedupe by catalog name.
        var catalogByName = new Dictionary<string, WardrobeEssentialItem>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in rawCatalog)
        {
            if (!string.IsNullOrWhiteSpace(entry.Name))
            {
                catalogByName[entry.Name.Trim()] = entry;
            }
        }
        var ownedNames = closet
            .Where(c => !string.IsNullOrWhiteSpace(c.Name))
            .Select(c => c.Name.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var cards = new List<EssentialItemCard>(aiResult.Value.Picks.Count);
        var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pick in aiResult.Value.Picks)
        {
            if (string.IsNullOrWhiteSpace(pick.CatalogName))
            {
                continue;
            }
            var key = pick.CatalogName.Trim();
            if (!seenNames.Add(key))
            {
                continue;
            }
            if (ownedNames.Contains(key))
            {
                continue; // user already owns it
            }
            if (!catalogByName.TryGetValue(key, out var catalogEntry))
            {
                continue; // catalog moved on since the AI was trained
            }

            var resolvedCategory = ResolveCategoryForCatalogEntry(catalogEntry.Name);
            cards.Add(new EssentialItemCard(
                Name: catalogEntry.Name,
                ImageSource: await ResolveImageSourceAsync(catalogEntry.Image),
                Link: catalogEntry.Link,
                Category: resolvedCategory,
                Priority: NormalizePriority(pick.Priority),
                Reason: pick.Reason,
                PairsWithCount: pick.PairsWithNames.Count,
                Price: ResolvePrice(resolvedCategory, catalogEntry.Name)));
        }

        if (cards.Count == 0)
        {
            // Fallback if every AI pick was rejected (e.g. all duplicates
            // of items the user already owns).
            return (await _essentialsCatalog.BuildMissingEssentialsAsync(closet)).Take(EssentialCardLimit).ToList();
        }

        return cards;
    }

    /// <summary>
    /// Mirrors the heuristic in <c>WardrobeEssentialsCatalog.MapEssentialToCategory</c>
    /// so the card can render the same category label regardless of
    /// whether the pick came from the AI path or the priority-sorted
    /// fallback.
    /// </summary>
    private static string ResolveCategoryForCatalogEntry(string essentialName)
    {
        var n = essentialName.ToLowerInvariant();
        if (n.Contains("blazer")) return "Blazers";
        if (n.Contains("oxford") || n.Contains("formal white shirt") || n.Contains("checked shirt")) return "Shirts";
        if (n.Contains("polo")) return "Polo Shirts";
        if (n.Contains("t-shirt") || n.Contains("tshirt") || n.Contains("crew")) return "T-Shirts";
        if (n.Contains("hoodie")) return "Hoodies";
        if (n.Contains("jacket")) return "Jackets";
        if (n.Contains("jeans")) return "Jeans";
        if (n.Contains("chinos") || n.Contains("trousers")) return "Trousers";
        if (n.Contains("shorts")) return "Shorts";
        if (n.Contains("kurta") || n.Contains("traditional")) return "Kurtas";
        if (n.Contains("sneakers") || n.Contains("loafers") || (n.Contains("formal") && n.Contains("shoes"))) return "Footwear";
        if (n.Contains("running") && n.Contains("shoes")) return "Activewear";
        if (n.Contains("belt")) return "Accessories";
        return "Essentials";
    }

    private static string NormalizePriority(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return "medium";
        }
        var key = raw.Trim().ToLowerInvariant();
        return key switch
        {
            "high" => "high",
            "low" => "low",
            _ => "medium",
        };
    }

    private readonly Dictionary<string, byte[]> _shoppingImageBytes = new(StringComparer.OrdinalIgnoreCase);

    private async Task<ImageSource?> ResolveImageSourceAsync(string imageFileName)
    {
        if (string.IsNullOrWhiteSpace(imageFileName))
        {
            return null;
        }

        // Resolves the curated shopping PNG/JPG via the bundled MauiAsset.
        // The deployed LogicalName flattens to the bare file name at the
        // package root (Windows MSIX unpackage drops the "shopping/"
        // subfolder), so we look up by bare file name. Mirrors
        // WardrobeEssentialsCatalog.ResolveImageSource so both AI-driven and
        // fallback paths produce identical ImageSource values for a given
        // wardrobe.json entry.
        //
        var normalized = imageFileName.Trim();
        if (normalized.Contains('/') || normalized.Contains('\\'))
        {
            return null;
        }

        var stem = Path.GetFileNameWithoutExtension(normalized);
        if (string.IsNullOrEmpty(stem))
        {
            return null;
        }
        var candidates = new[] { normalized }
            .Concat(new[] { ".png", ".jpg", ".jpeg" }
                .Select(ext => stem + ext)
                .Where(c => !string.Equals(c, normalized, StringComparison.OrdinalIgnoreCase)));

        foreach (var candidate in candidates)
        {
            if (!_shoppingImageBytes.TryGetValue(candidate, out var bytes))
            {
                try
                {
                    using var stream = await FileSystem.OpenAppPackageFileAsync(candidate);
                    using var memory = new MemoryStream();
                    await stream.CopyToAsync(memory);
                    bytes = memory.ToArray();
                    _shoppingImageBytes[candidate] = bytes;
                }
                catch
                {
                    continue;
                }
            }

            var cachedBytes = bytes;
            return ImageSource.FromStream(() => new MemoryStream(cachedBytes, writable: false));
        }

        return null;
    }

    /// <summary>
    /// Generates a believable per-item display price. The values are
    /// stable for a given name (so the UI doesn't flicker between
    /// renders) but different items in the same category show different
    /// prices — matching what the user expects from a real shopping feed.
    /// Mirrors <c>WardrobeEssentialsCatalog.ResolvePrice</c> so cards
    /// coming from the AI-driven path render the same price range as
    /// cards coming from the priority-sorted fallback.
    /// </summary>
    private static string ResolvePrice(string category, string essentialName)
    {
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

        var hash = 2166136261u;
        foreach (var character in (essentialName ?? string.Empty).ToLowerInvariant())
        {
            hash = (hash ^ character) * 16777619u;
        }
        var span = Math.Max(1, high - low);
        var offset = (int)(hash % (uint)span);
        return $"${low + offset}";
    }

    /// <summary>
    /// Opens the product's "Shop on &lt;retailer&gt;" deep link in the system
    /// browser. Falls back to the first available store when the chosen one
    /// is unknown (defensive — should never happen in practice).
    /// </summary>
    [RelayCommand]
    private async Task ShopNowAsync(ProductCard? product)
    {
        if (product is null)
        {
            return;
        }

        var selectedStore = AvailableStores.FirstOrDefault(s => s.IsSelected)?.Name
            ?? AvailableStores.FirstOrDefault()?.Name;
        if (string.IsNullOrWhiteSpace(selectedStore))
        {
            return;
        }

        var uri = _storeService.BuildDeepLink(selectedStore, product.Category, product.Name)
            ?? _storeService.BuildDeepLink(_storeService.AvailableStores[0].Name, product.Category, product.Name);
        if (uri is null)
        {
            return;
        }

        try
        {
            await Launcher.Default.OpenAsync(uri);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to launch deep link {Uri}", uri);
        }
    }

    /// <summary>Toggles the active retailer for the next "Shop Now" tap.</summary>
    [RelayCommand]
    private void SelectStore(string? storeName)
    {
        if (string.IsNullOrWhiteSpace(storeName))
        {
            return;
        }

        foreach (var s in AvailableStores)
        {
            s.IsSelected = string.Equals(s.Name, storeName, StringComparison.OrdinalIgnoreCase);
        }
        OnPropertyChanged(nameof(AvailableStores));
    }

    /// <summary>
    /// Opens the essential-item's "Shop Now" deep link in the system
    /// browser. The links are the curated Amazon search URLs shipped in
    /// <c>wardrobe.json</c> — we open them verbatim so the user lands
    /// directly on the search results page.
    /// </summary>
    [RelayCommand]
    private async Task OpenEssentialLinkAsync(EssentialItemCard? card)
    {
        if (card is null || string.IsNullOrWhiteSpace(card.Link))
        {
            return;
        }

        if (!Uri.TryCreate(card.Link, UriKind.Absolute, out var uri))
        {
            _logger.LogWarning("Essential card {Name} has malformed link {Link}.", card.Name, card.Link);
            return;
        }

        try
        {
            await Launcher.Default.OpenAsync(uri);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to launch essential card deep link {Uri}", uri);
        }
    }

    private void ResetUnavailableData()
    {
        IsUnavailable = true;
        CompletionScore = 0;
        ItemsOwned = 0;
        ItemsTarget = 25;
        MonthlyBudget = "$0";
        BudgetAdvice = "Your shopping budget will appear here.";
        AnalysisTitle = "Wardrobe analysis is unavailable";
        AnalysisSubtitle = "Your closet analysis and missing essentials will appear here.";
        SortLabel = "SORT: BEST VALUE";
        MissingEssentials.Clear();
        Recommendations.Clear();
        EssentialCards.Clear();
        HasEssentialCards = false;
        _lastCloset = null;
        _lastUser = null;
        _lastMonthlyBudgetUsd = 0m;
        _lastEssentialPicksClosetHash = string.Empty;
    }

    /// <summary>
    /// Composes a 1-sentence budget recommendation grounded in the user's
    /// real closet size so it doesn't feel canned.
    /// </summary>
    private static string BuildBudgetAdvice(decimal monthlyBudget, int closetSize)
    {
        if (monthlyBudget <= 0)
        {
            return "Set a monthly budget to get value-for-money recommendations.";
        }

        if (closetSize == 0)
        {
            return $"With a ${monthlyBudget:0}/month budget, start with 2-3 versatile staples before adding trend pieces.";
        }

        if (closetSize < 15)
        {
            return $"With {closetSize} items and ${monthlyBudget:0}/month, prioritise high-priority staples over multiple fast-fashion buys.";
        }

        return $"With {closetSize} items and ${monthlyBudget:0}/month, invest in one quality staple per month instead of multiple impulse buys.";
    }

    // ---- View-model DTOs ----

    /// <summary>One row in the "Missing Essentials" sidebar.</summary>
    public sealed class EssentialItemRow
    {
        public string Name { get; }
        public string Priority { get; }
        public string Reason { get; }
        public string PairingsPreview { get; }
        public bool IsHigh { get; }

        public EssentialItemRow(string name, string priority, string reason, string pairingsPreview, bool isHigh)
        {
            Name = name;
            Priority = priority;
            Reason = reason;
            PairingsPreview = pairingsPreview;
            IsHigh = isHigh;
        }
    }

    /// <summary>One curated product card on the feed.</summary>
    public sealed class ProductCard
    {
        public string Category { get; }
        public string Brand { get; }
        public string Name { get; }
        public string Price { get; }
        public decimal PriceUsd { get; }
        public string PairsWith { get; }
        public IReadOnlyList<string> PairsWithNames { get; }
        public string Description { get; }
        public string WhyRecommended { get; }
        public int Compatibility { get; }

        public ProductCard(
            string category,
            string brand,
            string name,
            string price,
            decimal priceUsd,
            string pairsWith,
            IReadOnlyList<string> pairsWithNames,
            string description,
            string whyRecommended,
            int compatibility)
        {
            Category = category;
            Brand = brand;
            Name = name;
            Price = price;
            PriceUsd = priceUsd;
            PairsWith = pairsWith;
            PairsWithNames = pairsWithNames;
            Description = description;
            WhyRecommended = whyRecommended;
            Compatibility = compatibility;
        }
    }

    /// <summary>One retailer option in the "Shop on" picker.</summary>
    public sealed partial class StoreOption : ObservableObject
    {
        public string Name { get; }

        [ObservableProperty] private bool _isSelected;

        public StoreOption(string name, bool isSelected)
        {
            Name = name;
            _isSelected = isSelected;
        }
    }

    /// <summary>
    /// <see cref="ITabRefreshable"/> entry point used by the
    /// <see cref="MainTabHostViewModel"/> when the <c>SfTabView</c> selection
    /// lands on (or re-enters) the Shop tab. Re-runs the same analysis +
    /// recommendations load the old Shell <c>TabBar</c> triggered via
    /// <c>OnAppearing</c>.
    /// </summary>
    public Task RefreshAsync() => LoadAsync();
}
