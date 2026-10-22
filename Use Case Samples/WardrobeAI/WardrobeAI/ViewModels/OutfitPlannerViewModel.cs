using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WardrobeAI.AI;
using WardrobeAI.AI.Contracts;
using WardrobeAI.Contracts;
using WardrobeAI.Models;
using WardrobeAI.Repositories;
using WardrobeAI.Services;

namespace WardrobeAI.ViewModels;

/// <summary>
/// Outfit Planner view model (SDS Modules 4 &amp; 5). Mirrors /designs/outfit_planner.html:
/// occasion chips, weather context, AI recommendation, match metrics, alternatives,
/// explanation — plus a FAB-triggered "Plan" popup that persists a stand-alone
/// <see cref="PlannedOutfit"/> so it surfaces in the Dashboard's Upcoming Plans.
///
/// <para>For the four planner categories (Work / Meeting / Casual / Party) the
/// VM asks the AI service for a full per-occasion plan in one round-trip, then
/// keeps the per-category scores + alternatives + "Why this works" blurb in
/// memory. Tapping an occasion chip just flips which category is rendered; the
/// underlying data is loaded once when the page appears (or when the user taps
/// Generate).</para>
/// </summary>
public partial class OutfitPlannerViewModel : ObservableObject, ITabRefreshable
{
    /// <summary>
    /// The four planner categories. Centralised so the XAML chip group and
    /// the AI service request stay in sync.
    /// </summary>
    public static readonly IReadOnlyList<string> DefaultCategories = new[]
    {
        "Work", "Meeting", "Casual", "Party"
    };

    private readonly IPlannedOutfitRepository _plannedOutfitRepository;
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;
    private readonly IClothingItemRepository _clothingItemRepository;
    private readonly IWeatherService _weatherService;
    private readonly IAIOutfitRecommendationService _outfitAi;
    private readonly ILogger<OutfitPlannerViewModel> _logger;

    public OutfitPlannerViewModel(
        IPlannedOutfitRepository plannedOutfitRepository,
        IUserContext userContext,
        IUserRepository userRepository,
        IClothingItemRepository clothingItemRepository,
        IWeatherService weatherService,
        IAIOutfitRecommendationService outfitAi,
        ILogger<OutfitPlannerViewModel> logger)
    {
        _plannedOutfitRepository = plannedOutfitRepository;
        _userContext = userContext;
        _userRepository = userRepository;
        _clothingItemRepository = clothingItemRepository;
        _weatherService = weatherService;
        _outfitAi = outfitAi;
        _logger = logger;

        Occasions = new ObservableCollection<OccasionChip>();
        foreach (var c in DefaultCategories)
        {
            Occasions.Add(new OccasionChip(c, string.Equals(c, "Work", StringComparison.OrdinalIgnoreCase)));
        }

        // Pre-select the first chip ("Work") so the SfChipGroup's SelectedItem
        // binding has an initial value. Without this, SelectedOccasionChip is
        // null until the user taps a chip and SelectedCategoryChanged fires:
        // SfChipGroup therefore initializes with no chip visually highlighted,
        // even though OccasionChip("Work", isSelected: true) carries the
        // internal flag. Setting the field directly (not the property) avoids
        // the OnSelectedCategoryChanged <-> OnSelectedOccasionChipChanged
        // re-entry that would otherwise re-set SelectedCategory.
        _selectedOccasionChip = Occasions[0];

        PlanOccasionOptions = new ObservableCollection<string>
        {
            "Work", "Meeting", "Casual", "Party", "Formal", "Travel", "Sport", "Date"
        };

        // Per-category state. The VM keeps one <see cref="CategoryViewState"/>
        // per planner category so flipping chips only repaints the bound
        // properties — no LLM re-call needed.
        Categories = new ObservableCollection<CategoryViewState>(
            DefaultCategories.Select(c => new CategoryViewState(c)));

        // Weather + outfit are loaded live by LoadContextAsync() on every
        // OnAppearing. Defaults keep the chip + gauges from rendering blank
        // during the first lookup.
        WeatherLabel = "—";
        WeatherNote = "Loading today's weather…";
        Temperature = "—";
        TemperatureLabel = string.Empty;
        WeatherIconKey = "cloudy";

        IsGenerating = false;
        HasOutfits = false;
        EmptyStateMessage = "Add a few clothing items to your closet, then tap Generate to plan an outfit.";

        // Plan form state — defaults so the popup opens with sensible values.
        PlanName = string.Empty;
        PlanDate = DateTime.Today.AddDays(1);
        PlanOccasion = "Work";
        IsPlanPopupOpen = false;
        IsPlanDatePickerOpen = false;
        PlanDateMinimum = DateTime.Today;
        PlanDateDisplay = PlanDate.ToString("dddd, MMM d, yyyy");
        PlanValidationError = null;
        IsSavingPlan = false;

        // Pending chip selection — when the user taps a chip while a previous
        // AI regeneration is still in flight, we remember their choice and
        // re-run the analysis as soon as the current call completes. Without
        // this the new chip would visually flip but the gauges / hero /
        // alternatives would stay on the in-flight category.
        _pendingChipSelection = null;
    }

    /// <summary>
    /// When non-null, a chip selection arrived while <see cref="IsGenerating"/>
    /// was <c>true</c>. <see cref="LoadContextAsync"/> honours this after the
    /// in-flight call finishes so the user always sees the outfit for the
    /// chip they actually tapped (not the chip that was in flight).
    /// </summary>
    private string? _pendingChipSelection;

    public ObservableCollection<OccasionChip> Occasions { get; }
    public ObservableCollection<CategoryViewState> Categories { get; }

    /// <summary>Options shown in the Plan popup's occasion picker.</summary>
    public ObservableCollection<string> PlanOccasionOptions { get; }

    // ---- Weather context (live) ----
    // Note: the field is intentionally `_weatherLabel` (not `_weatherCondition`)
    // to avoid colliding with the WardrobeAI.Contracts.WeatherCondition enum
    // that the [ObservableProperty] source generator would otherwise shadow.
    [ObservableProperty] private string _weatherLabel = string.Empty;
    [ObservableProperty] private string _weatherNote = string.Empty;
    [ObservableProperty] private string _temperature = string.Empty;
    [ObservableProperty] private string _temperatureLabel = string.Empty;

    /// <summary>
    /// Logical icon key (e.g. <c>"sunny"</c>, <c>"cloudy"</c>) chosen by the
    /// weather provider. <see cref="WeatherImage"/> is what XAML actually binds
    /// to — it resolves this key to a packaged <c>.png</c> under
    /// <c>Resources/Images/</c>. Two-way changes to <see cref="WeatherImage"/>
    /// are ignored on purpose (it's a derived view); the source of truth is
    /// <see cref="WeatherIconKey"/>.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WeatherImage))]
    private string _weatherIconKey = "cloudy";

    /// <summary>
    /// Image shown next to the weather label in the planner's hero card.
    /// Banner of <see cref="WeatherIconKey"/> from the
    /// <c>Resources/Images/</c> folder (matching the asset names used by
    /// <c>DashboardViewModel.WeatherImage</c>). Falls back to the bundled
    /// <c>cloudy.png</c> when the resolved key is empty.
    /// </summary>
    public ImageSource WeatherImage
    {
        get
        {
            var key = string.IsNullOrWhiteSpace(WeatherIconKey) ? "cloudy" : WeatherIconKey;
            return ImageSource.FromFile(key + ".png");
        }
    }

    // ---- Top-level generation state ----
    [ObservableProperty] private bool _isGenerating;
    [ObservableProperty] private bool _hasOutfits;
    [ObservableProperty] private string _emptyStateMessage = string.Empty;

    // ---- Plan popup form state ----
    [ObservableProperty] private bool _isPlanPopupOpen;
    [ObservableProperty] private string _planName = string.Empty;
    [ObservableProperty] private DateTime _planDate;
    [ObservableProperty] private string _planOccasion = string.Empty;
    [ObservableProperty] private string? _planValidationError;
    [ObservableProperty] private bool _isSavingPlan;

    // ---- Plan date sub-picker state ----
    [ObservableProperty] private bool _isPlanDatePickerOpen;
    [ObservableProperty] private DateTime _planDateMinimum;
    [ObservableProperty] private string _planDateDisplay = string.Empty;

    /// <summary>Currently rendered category (drives the bound chip selection).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActiveCategory))]
    private string _selectedCategory = "Work";

    /// <summary>
    /// Source-of-truth for the <c>SfChipGroup</c> bound to the suggestion chip
    /// row. Two-way bound to <c>SelectedItem</c> in XAML; the partial setter
    /// keeps <see cref="SelectedCategory"/> (and therefore every other bound
    /// surface — gauges, hero, alternatives, "Why this works") in sync whenever
    /// the chip group raises <c>SelectionChanged</c>. The code-behind handler
    /// also routes the chip change to <c>SelectOccasionAsync</c> via its
    /// generated <c>SelectOccasionAsyncCommand</c> so we still kick off an AI
    /// regeneration exactly like the previous per-chip <c>Command</c> binding
    /// did.
    /// </summary>
    [ObservableProperty]
    private OccasionChip? _selectedOccasionChip;

    public CategoryViewState? ActiveCategory
    {
        get
        {
            foreach (var c in Categories)
            {
                if (string.Equals(c.Category, SelectedCategory, StringComparison.OrdinalIgnoreCase))
                {
                    return c;
                }
            }
            return Categories.Count > 0 ? Categories[0] : null;
        }
    }

    partial void OnSelectedOccasionChipChanged(OccasionChip? value)
    {
        if (value is null)
        {
            return;
        }
        // Keep the planner category in lockstep with the chip group selection
        // so ActiveCategory (and all its bound surfaces) refresh. The
        // SelectionChanged code-behind is the one that actually triggers the
        // AI regeneration — this setter just mirrors the change onto the
        // string-based SelectedCategory the rest of the VM already uses.
        SelectedCategory = value.Label;
    }

    partial void OnSelectedCategoryChanged(string value)
    {
        // Flip each chip's IsSelected flag so the OccasionChip class stays
        // internally consistent (it still computes BackgroundBrush / TextColor
        // off IsSelected). The XAML no longer binds to those brushes — the
        // SfChipGroup paints the selected chip via SelectedChipBackground /
        // SelectedChipTextColor — but OccasionChip.IsSelected is the VM's
        // single source of truth for "which planner category is active".
        foreach (var o in Occasions)
        {
            o.IsSelected = string.Equals(o.Label, value, StringComparison.OrdinalIgnoreCase);
        }

        // Keep the SfChipGroup's SelectedItem in sync so its built-in
        // SelectedChipBackground / SelectedChipTextColor visual flips even
        // when the change is initiated programmatically (e.g.
        // SelectOccasionAsyncCommand from the Generate button, or a Plan
        // popup default). The OnSelectedOccasionChipChanged partial below
        // short-circuits the recursive SelectedCategory mirror.
        var match = Occasions.FirstOrDefault(o =>
            string.Equals(o.Label, value, StringComparison.OrdinalIgnoreCase));
        if (match is not null && !ReferenceEquals(match, SelectedOccasionChip))
        {
            SelectedOccasionChip = match;
        }

        // The radial gauges bind to nested paths like ActiveCategory.MatchScore.
        // The default binding system tracks the leaf property (MatchScore) and
        // does not re-evaluate the path when only the intermediate
        // ActiveCategory reference changes, so the RangePointer keeps its
        // previous value and the ring renders fully filled. Re-fire
        // PropertyChanged for every score/path bound off ActiveCategory so
        // the gauges pick up the new category's values.
        var active = ActiveCategory;
        if (active is not null)
        {
            active.NotifyAllGaugesChanged();
        }

        // The BindableLayout for "Alternative Looks" and the SfRotator for
        // the hero gallery both subscribe to nested paths off ActiveCategory
        // (e.g. ItemsSource="{Binding ActiveCategory.Items}" and
        // ItemsSource="{Binding ActiveCategory.Alternatives}"). When the
        // intermediate reference changes, the SfRotator's hosted image
        // controls and the BindableLayout cells do not always re-template
        // their children: SfRotator uses its own hosted item-view pool, and
        // BindableLayout on Windows can keep the previous chip's image
        // cached in its realisations. Re-fire PropertyChanged for the
        // Items + Alternatives collections so each gallery's source
        // collection is re-evaluated end-to-end and the per-item
        // Image.Source binding pipeline picks up the new chip's base64
        // image bytes.
        ActiveCategory?.NotifyCollectionsChanged();
    }

    partial void OnPlanDateChanged(DateTime value)
    {
        PlanDateDisplay = value.ToString("dddd, MMM d, yyyy");
        if (value < PlanDateMinimum)
        {
            PlanDateMinimum = value.Date < DateTime.Today ? DateTime.Today : value.Date;
        }
    }

    [RelayCommand]
    private async Task SelectOccasionAsync(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return;
        }
        var trimmed = label.Trim();
        if (string.Equals(SelectedCategory, trimmed, StringComparison.OrdinalIgnoreCase))
        {
            // Tapping the already-active chip is a no-op so the gauge colors
            // don't churn on repeated taps.
            return;
        }
        SelectedCategory = trimmed;

        // Sync the SfChipGroup's SelectedItem so its built-in
        // SelectedChipBackground / SelectedChipTextColor colouring flips
        // even when the change originates outside the chip group (e.g. via
        // the Generate button). Setting it to the matching OccasionChip
        // echoes the user's tap visually.
        var match = Occasions.FirstOrDefault(o =>
            string.Equals(o.Label, trimmed, StringComparison.OrdinalIgnoreCase));
        if (match is not null)
        {
            SelectedOccasionChip = match;
        }

        // If a regeneration is already in flight (e.g. the user tapped a
        // chip while the OnAppearing LoadContextAsync is still running),
        // remember the latest chip choice and let LoadContextAsync re-run
        // for it after the in-flight call finishes — otherwise the new
        // selection would visually flip but the gauges / hero / alternatives
        // would stay on the in-flight category until the user taps Generate.
        if (IsGenerating)
        {
            _pendingChipSelection = trimmed;
            return;
        }

        // Regenerate the AI plan for the newly-selected occasion so the
        // gauges, hero, alternatives, and "Why this works" copy all reflect
        // the new chip. Always re-query on a real chip change (not just when
        // a previous plan is cached) so the very first chip switch after
        // OnAppearing also re-evaluates the analysis.
        await LoadContextAsync(forceRegenerate: true);
    }

    [RelayCommand]
    private void SeeAllAlternatives()
    {
        // Hook for a future "Alternatives" bottom-sheet / SfCarousel modal.
        // No-op for now; the alternatives strip already shows up to 2 items.
    }

    /// <summary>
    /// Loads the signed-in user, the live weather, and the closet, then asks
    /// the AI service for a four-category outfit plan. The result is mapped
    /// onto the bound <see cref="Categories"/> collection so the page can
    /// flip between Work / Meeting / Casual / Party without re-querying.
    /// Safe to call on every OnAppearing — it coalesces via
    /// <see cref="IsGenerating"/>.
    /// </summary>
    [RelayCommand]
    public async Task LoadContextAsync(bool forceRegenerate = false)
    {
        if (IsGenerating)
        {
            // A load is already in flight. If a chip selection arrived while
            // we were busy (see SelectOccasionAsync), remember it so we
            // re-run for that chip once the in-flight call finishes —
            // otherwise the user would see the new chip's color but the old
            // category's data.
            if (forceRegenerate && !string.IsNullOrWhiteSpace(_pendingChipSelection))
            {
                _pendingChipSelection = SelectedCategory;
            }
            return;
        }

        try
        {
            IsGenerating = true;

            var userId = _userContext.GetCurrentUserId();
            if (userId <= 0)
            {
                ApplyEmptyState("Outfit planning is unavailable right now.");
                return;
            }

            var user = await _userRepository.GetAsync(userId);
            if (user is null)
            {
                ApplyEmptyState("User profile not found.");
                return;
            }

            // Live weather — same source the Dashboard uses.
            var weatherResult = await _weatherService.GetCurrentWeatherAsync();
            var weather = weatherResult.IsSuccess
                ? weatherResult.Value!
                : new WeatherInfo(0, Contracts.WeatherCondition.Unknown, 0, string.Empty);
            ApplyWeather(weather);

            // Closet — short-circuit with a friendly empty state when the user
            // hasn't added any clothing yet.
            var closet = await _clothingItemRepository.GetAllForUserAsync(userId);
            if (closet.Count == 0)
            {
                ApplyEmptyState("Add a few clothing items to your closet, then tap Generate to plan an outfit.");
                return;
            }

            // Already have outfits? Skip the AI call unless the user explicitly
            // tapped Generate. Keeps tab switches snappy.
            if (!forceRegenerate && HasOutfits)
            {
                return;
            }

            var request = new OutfitRecommendationRequest(
                UserId: user.UserId,
                Gender: user.Gender.ToString(),
                AgeGroup: user.AgeGroup,
                StylePreference: user.StylePreference,
                BudgetLevel: user.BudgetLevel.ToString(),
                Location: weather.LocationLabel,
                Occasion: SelectedCategory,
                Weather: new WeatherSummary(
                    TemperatureC: weather.TemperatureC,
                    Condition: weather.Condition.ToString(),
                    HumidityPercent: weather.HumidityPercent,
                    LocationLabel: weather.LocationLabel),
                Closet: closet.Select(MapToSummary).ToList(),
                TopNItemsPerCategory: 12);

            var result = await _outfitAi.GetOutfitsForCategoriesAsync(request, DefaultCategories);
            if (!result.IsSuccess)
            {
                _logger.LogWarning(
                    "Outfit planner AI call failed: {Code} {Message}",
                    result.Error!.Code, result.Error!.Message);
                ApplyEmptyState(FriendlyFallback(result.Error!.Code, result.Error!.Message));
                return;
            }

            ApplyPlanResult(result.Value!, closet);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OutfitPlannerViewModel.LoadContextAsync threw unexpectedly.");
            ApplyEmptyState("Could not reach the AI service. Please try again.");
        }
        finally
        {
            IsGenerating = false;

            // If a chip selection arrived while we were busy, re-run the
            // analysis now so the gauges / hero / alternatives reflect the
            // chip the user actually tapped (not the chip that was in
            // flight when they tapped it).
            var pending = _pendingChipSelection;
            if (!string.IsNullOrWhiteSpace(pending) &&
                !string.Equals(pending, SelectedCategory, StringComparison.OrdinalIgnoreCase))
            {
                _pendingChipSelection = null;
                // Fire-and-forget: the inner LoadContextAsync will reset
                // IsGenerating as it finishes. Any exceptions are swallowed
                // and surfaced via the empty-state fallback.
                _ = LoadContextAsync(forceRegenerate: true);
            }
            else
            {
                _pendingChipSelection = null;
            }
        }
    }

    private static ClothingItemSummary MapToSummary(ClothingItem c) => new(
        Id: c.ItemId,
        Name: c.Name,
        Category: c.Category,
        Color: c.Color,
        Pattern: c.Pattern,
        Fabric: c.Fabric,
        Season: c.Season,
        Style: c.Style,
        Occasion: c.Occasion,
        Favorite: c.Favorite,
        UsageCount: c.UsageCount);

    private void ApplyWeather(WeatherInfo info)
    {
        Temperature = info.TemperatureDisplay;
        WeatherIconKey = info.IconKey;
        if (info.Condition == Contracts.WeatherCondition.Unknown)
        {
            WeatherLabel = "—";
            WeatherNote = "Weather unavailable. The plan is anchored to today's planned occasion.";
            TemperatureLabel = string.Empty;
            return;
        }
        WeatherLabel = ConditionLabel(info.Condition);
        WeatherNote = NoteForCondition(info.Condition, info.TemperatureC);
        TemperatureLabel = info.TemperatureC switch
        {
            >= 28 => "High",
            >= 18 => "Mild",
            _ => "Low"
        };
    }

    private static string ConditionLabel(Contracts.WeatherCondition c) => c switch
    {
        Contracts.WeatherCondition.Sunny => "Sunny & Clear",
        Contracts.WeatherCondition.Cloudy => "Cloudy",
        Contracts.WeatherCondition.Rainy => "Rainy",
        Contracts.WeatherCondition.Snowy => "Snowy",
        Contracts.WeatherCondition.Stormy => "Stormy",
        _ => "—"
    };

    private static string NoteForCondition(Contracts.WeatherCondition c, double temperatureC)
    {
        if (temperatureC >= 28) return "Light, breathable layers recommended";
        if (temperatureC <= 12) return "Layer up with outerwear";
        return c switch
        {
            Contracts.WeatherCondition.Rainy => "Waterproof outerwear recommended",
            Contracts.WeatherCondition.Snowy => "Insulated layers recommended",
            Contracts.WeatherCondition.Stormy => "Stay dry — waterproof outerwear",
            _ => "Perfect for a balanced look"
        };
    }

    private void ApplyPlanResult(OutfitPlanResult result, IReadOnlyList<ClothingItem> closet)
    {
        var closetById = closet.ToDictionary(c => c.ItemId);

        // Reset the per-category state, then fill from the AI result. We
        // preserve the order of <see cref="DefaultCategories"/> so the
        // chip group and the bound collection stay aligned.
        Categories.Clear();
        foreach (var category in DefaultCategories)
        {
            var entry = result.Entries
                .FirstOrDefault(e => string.Equals(e.Category, category, StringComparison.OrdinalIgnoreCase));
            var state = new CategoryViewState(category);
            if (entry is not null)
            {
                PopulateCategoryState(state, entry, closetById);
            }
            Categories.Add(state);
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            OnPropertyChanged(nameof(ActiveCategory));
        });
        HasOutfits = Categories.Any(c => c.HasOutfit);

        // The radial gauges bind to nested paths off ActiveCategory (e.g.
        // ActiveCategory.MatchScore). The default binding system only re-
        // evaluates the leaf path when its owning object fires
        // PropertyChanged, so when ActiveCategory returns a new instance we
        // must re-announce the leaf properties to refresh the RangePointer.
        ActiveCategory?.NotifyAllGaugesChanged();

        // Re-announce the Items + Alternatives collections on the freshly
        // populated ActiveCategory so the SfRotator hero gallery and the
        // BindableLayout "Alternative Looks" strip both update their
        // ItemsSource references. The MAUI binding system tracks nested
        // paths off ActiveCategory and only re-evaluates the leaf
        // ObservableCollection on Add/Remove/Reset events from the same
        // collection instance; when the entire CategoryViewState is
        // replaced (which Categories.Clear() + Add() does), the leaf
        // collection identity changes too and BindableLayout occasionally
        // misses the swap on Windows. Firing PropertyChanged("Items") /
        // PropertyChanged("Alternatives") on the new state forces every
        // binding to re-pull the value and the per-item Image control to
        // reset its Source through the Base64ToImageSourceConverter.
        ActiveCategory?.NotifyCollectionsChanged();

        if (!HasOutfits)
        {
            ApplyEmptyState("Your closet doesn't have the pieces for a complete look yet. Add a top, a bottom, and footwear, then tap Generate again.");
        }
        else
        {
            EmptyStateMessage = string.Empty;
        }
    }

    private static void PopulateCategoryState(
        CategoryViewState state,
        OutfitPlanEntry entry,
        Dictionary<int, ClothingItem> closetById)
    {
        state.Title = entry.Primary.Title;
        state.Subtitle = BuildSubtitle(entry.Primary, closetById);
        state.MatchScore = Math.Clamp(entry.Primary.MatchScore, 0, 100);
        state.StyleScore = entry.Primary.Scores?.Style ?? 0;
        state.ComfortScore = entry.Primary.Scores?.Comfort ?? 0;
        state.TrendScore = entry.Primary.Scores?.Trend ?? 0;
        state.OccasionScore = entry.Primary.Scores?.Occasion ?? 0;
        state.WeatherScore = entry.Primary.Scores?.Weather ?? 0;
        state.StyleLabel = ScoreLabel(state.StyleScore);
        state.ComfortLabel = ScoreLabel(state.ComfortScore);
        state.TrendLabel = ScoreLabel(state.TrendScore);
        state.HasOutfit = entry.Primary.Items.Count > 0;
        state.Explanation = string.IsNullOrWhiteSpace(entry.Primary.Reason)
            ? entry.WhyThisWorks
            : entry.Primary.Reason;
        state.Reason = entry.Primary.Reason;
        state.Items.Clear();
        foreach (var pick in entry.Primary.Items)
        {
            if (!closetById.TryGetValue(pick.ItemId, out var closetItem))
            {
                continue;
            }
            state.Items.Add(new OutfitItemCard(
                itemId: closetItem.ItemId,
                role: pick.Role,
                name: closetItem.Name,
                category: closetItem.Category,
                color: closetItem.Color,
                imageBase64: closetItem.ImageUrl));
        }
        state.Alternatives.Clear();
        foreach (var alt in entry.Alternatives.Take(2))
        {
            state.Alternatives.Add(new AlternativeLook(
                title: alt.Title,
                match: alt.MatchScore,
                imageBase64: ResolveFirstItemImage(alt, closetById)));
        }
    }

    /// <summary>
    /// Returns the base64 image of the first item in an alternative outfit
    /// (or null if the alternative references no closet item). Used to
    /// give each alternative card a real preview without going through the
    /// full per-slot projection.
    /// </summary>
    private static string? ResolveFirstItemImage(OutfitSuggestion alt, Dictionary<int, ClothingItem> closetById)
    {
        foreach (var pick in alt.Items)
        {
            if (closetById.TryGetValue(pick.ItemId, out var item) && !string.IsNullOrWhiteSpace(item.ImageUrl))
            {
                return item.ImageUrl;
            }
        }
        return null;
    }

    private static string BuildSubtitle(OutfitSuggestion primary, Dictionary<int, ClothingItem> closetById)
    {
        var parts = new List<string>();
        var tops = primary.Items.Count(i => string.Equals(i.Role, "Top", StringComparison.OrdinalIgnoreCase));
        var bottoms = primary.Items.Count(i => string.Equals(i.Role, "Bottom", StringComparison.OrdinalIgnoreCase));
        var dresses = primary.Items.Count(i => string.Equals(i.Role, "Dress", StringComparison.OrdinalIgnoreCase));
        var footwear = primary.Items.Count(i => string.Equals(i.Role, "Footwear", StringComparison.OrdinalIgnoreCase));
        var outerwear = primary.Items.Count(i => string.Equals(i.Role, "Outerwear", StringComparison.OrdinalIgnoreCase));

        if (dresses > 0) parts.Add("Dress");
        if (tops > 0) parts.Add("Top");
        if (bottoms > 0) parts.Add("Bottom");
        if (outerwear > 0) parts.Add("Outerwear");
        if (footwear > 0) parts.Add("Footwear");

        var fabrics = primary.Items
            .Select(i => closetById.TryGetValue(i.ItemId, out var c) ? c.Fabric : null)
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(2);
        var fabricPart = string.Join(" / ", fabrics);
        if (!string.IsNullOrWhiteSpace(fabricPart))
        {
            parts.Add($"{fabricPart}");
        }
        return string.Join(" \u2022 ", parts);
    }

    private static string ScoreLabel(int score) => score switch
    {
        >= 85 => "Excellent",
        >= 70 => "Great",
        >= 55 => "Good",
        _ => "Fair"
    };

    private void ApplyEmptyState(string message)
    {
        foreach (var c in Categories)
        {
            c.Reset();
        }
        HasOutfits = false;
        EmptyStateMessage = message;
        OnPropertyChanged(nameof(ActiveCategory));

        // Re-announce the score properties on the (reset) ActiveCategory so
        // the radial RangePointer updates to the reset values instead of
        // keeping the previously-rendered fill.
        ActiveCategory?.NotifyAllGaugesChanged();

        // Mirror the collection re-announcement from ApplyPlanResult so the
        // SfRotator + BindableLayout also release their old item bindings
        // when the planner is reset to the empty state.
        ActiveCategory?.NotifyCollectionsChanged();
    }

    private static string FriendlyFallback(string code, string message)
    {
        if (code == "AI.NotConfigured")
        {
            return "AI suggestions aren't configured yet. Add your Azure OpenAI details to enable the planner.";
        }
        if (code == "AI.EmptyCloset")
        {
            return message;
        }
        if (code == "AI.Timeout" || code == "AI.Network")
        {
            return "Could not reach the AI service. Please try again in a moment.";
        }
        return "Tap \u201cGenerate\u201d to try again.";
    }

    // ---- Plan popup commands (unchanged) ------------------------------------

    /// <summary>
    /// Opens the Plan popup and resets the form to sensible defaults.
    /// Wired to the FAB on the Outfit Planner page.
    /// </summary>
    [RelayCommand]
    private void OpenPlanPopup()
    {
        PlanValidationError = null;
        if (string.IsNullOrWhiteSpace(PlanName))
        {
            PlanName = string.Empty;
        }
        if (PlanDate < DateTime.Today)
        {
            PlanDate = DateTime.Today;
        }
        PlanDateMinimum = DateTime.Today;
        IsPlanDatePickerOpen = false;
        // Default the plan's occasion to whatever chip is currently selected
        // so the Plan popup is contextually correct out of the gate.
        if (string.IsNullOrWhiteSpace(PlanOccasion) ||
            !PlanOccasionOptions.Contains(PlanOccasion, StringComparer.OrdinalIgnoreCase))
        {
            PlanOccasion = string.IsNullOrWhiteSpace(SelectedCategory) ? "Work" : SelectedCategory;
        }
        IsPlanPopupOpen = true;
    }

    /// <summary>
    /// Toggles the inline <c>SfDatePicker</c> open so the user can pick a date.
    /// The selected date is always reflected in the read-only date display.
    /// </summary>
    [RelayCommand]
    private void OpenPlanDatePicker()
    {
        IsPlanDatePickerOpen = !IsPlanDatePickerOpen;
    }

    /// <summary>
    /// Quick-action: sets the plan date to today and closes the inline picker.
    /// </summary>
    [RelayCommand]
    private void SetPlanDateToToday()
    {
        PlanDate = DateTime.Today;
        IsPlanDatePickerOpen = false;
    }

    /// <summary>
    /// Closes the Plan popup without saving. The form fields are preserved so the
    /// user can reopen the popup and continue if they tap Cancel by accident.
    /// </summary>
    [RelayCommand]
    private void ClosePlanPopup()
    {
        IsPlanPopupOpen = false;
    }

    /// <summary>
    /// Validates the Plan form and persists a stand-alone <see cref="PlannedOutfit"/>
    /// (OutfitId = null). The Dashboard's OnAppearing handler will pick it up on
    /// the next navigation.
    /// </summary>
    [RelayCommand]
    private async Task SavePlanAsync()
    {
        if (IsSavingPlan)
        {
            return;
        }

        var name = (PlanName ?? string.Empty).Trim();
        var occasion = (PlanOccasion ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            PlanValidationError = "Please enter a plan name.";
            return;
        }

        if (name.Length > 160)
        {
            PlanValidationError = "Plan name must be 160 characters or fewer.";
            return;
        }

        if (string.IsNullOrWhiteSpace(occasion))
        {
            PlanValidationError = "Please pick an occasion.";
            return;
        }

        if (PlanDate.Date < DateTime.Today)
        {
            PlanValidationError = "Plan date can't be in the past.";
            return;
        }

        var userId = _userContext.GetCurrentUserId();
        if (userId <= 0)
        {
            PlanValidationError = "Default user profile is not available.";
            return;
        }

        try
        {
            IsSavingPlan = true;
            PlanValidationError = null;

            var plan = new PlannedOutfit
            {
                OutfitId = null,
                UserId = userId,
                PlanDate = PlanDate.Date,
                Name = name,
                Occasion = occasion,
            };

            var saved = await _plannedOutfitRepository.AddAsync(plan);
            _logger.LogInformation(
                "Saved stand-alone plan '{PlanName}' for {PlanDate:yyyy-MM-dd} (user {UserId})",
                plan.Name, plan.PlanDate, userId);

            PlanEvents.RaisePlanSaved(new PlanSavedEventArgs(saved.PlanId, userId, saved.PlanDate));

            IsPlanPopupOpen = false;
            PlanName = string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save planned outfit.");
            PlanValidationError = "Couldn't save the plan. Please try again.";
        }
        finally
        {
            IsSavingPlan = false;
        }
    }

    // ---- Nested types -------------------------------------------------------

    /// <summary>
    /// One occasion chip in the Outfit Planner (Work / Meeting / Casual / Party).
    /// Inherits <see cref="ObservableObject"/> and exposes
    /// <see cref="BackgroundBrush"/> + <see cref="TextColor"/> so the XAML can
    /// bind to them directly instead of using a <c>DataTrigger</c>. A
    /// <c>DataTrigger</c> on <c>SfButton.Background</c> competes with the
    /// <c>VisualStateManager</c>'s <c>CommonStates</c> (<c>Normal</c> /
    /// <c>PointerOver</c> / <c>Pressed</c>) and the previous color can
    /// "stick" on subsequent chip taps. Direct property bindings to the chip's
    /// own brush / color values are evaluated every time the chip raises
    /// <c>PropertyChanged</c> for those properties, so the visual update is
    /// deterministic.
    /// </summary>
    public sealed partial class OccasionChip : ObservableObject
    {
        /// <summary>Selected chip background — matches the design's "Primary" navy.</summary>
        private static readonly Brush SelectedBackground = new SolidColorBrush(Color.FromArgb("#091523"));

        /// <summary>Selected chip text — white so it reads on the navy fill.</summary>
        private static readonly Color SelectedTextColor = Colors.White;

        /// <summary>Unselected chip background — neutral surface.</summary>
        private static readonly Brush UnselectedBackgroundLight = new SolidColorBrush(Color.FromArgb("#E5E2DD")); // SurfaceContainerHighest
        private static readonly Brush UnselectedBackgroundDark = new SolidColorBrush(Color.FromArgb("#404040"));   // Gray600

        /// <summary>Unselected chip text — readable on the neutral fill.</summary>
        private static readonly Color UnselectedTextColorLight = Color.FromArgb("#1C1C19"); // OnSurface
        private static readonly Color UnselectedTextColorDark = Color.FromArgb("#F4F1EB");  // OnSurfaceDark

        public OccasionChip(string label, bool isSelected)
        {
            Label = label;
            // Assign backing fields directly so the initial binding pass
            // captures the correct brush / color without a redundant
            // PropertyChanged raise.
            _isSelected = isSelected;
            ApplyVisualState();
        }

        public string Label { get; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(BackgroundBrush))]
        [NotifyPropertyChangedFor(nameof(TextColor))]
        private bool _isSelected;

        /// <summary>Resolved <see cref="Brush"/> for the chip's <c>Background</c>.</summary>
        public Brush BackgroundBrush => IsSelected
            ? SelectedBackground
            : (Application.Current?.RequestedTheme == AppTheme.Dark
                ? UnselectedBackgroundDark
                : UnselectedBackgroundLight);

        /// <summary>Resolved <see cref="Color"/> for the chip's <c>TextColor</c>.</summary>
        public Color TextColor => IsSelected
            ? SelectedTextColor
            : (Application.Current?.RequestedTheme == AppTheme.Dark
                ? UnselectedTextColorDark
                : UnselectedTextColorLight);

        partial void OnIsSelectedChanged(bool value)
        {
            // Push the new brush / color to any subscribers. PropertyChanged
            // for both BackgroundBrush and TextColor is raised automatically
            // by the [NotifyPropertyChangedFor] attributes.
            ApplyVisualState();
        }

        private void ApplyVisualState()
        {
            // No-op for now — kept as the explicit hook so the brush / color
            // recompute is colocated with the state flip.
        }
    }

    /// <summary>
    /// Card-friendly projection of an alternative outfit returned for a
    /// planner category. Holds only what's needed to render the
    /// "Alternative Looks" thumbnail card. Inherits
    /// <see cref="ObservableObject"/> so each property assignment raises
    /// <c>PropertyChanged</c> — matching <see cref="OutfitItemCard"/>, whose
    /// observable <c>ImageBase64</c> was already the pattern that fixed the
    /// SfRotator hero gallery. Without observability here, BindableLayout's
    /// item-template realisations on the alternative strip occasionally
    /// captured the constructor's <c>null</c> default for
    /// <see cref="ImageBase64"/> (templatizing before the constructor body
    /// ran) and the cached converter result made the Image control keep
    /// showing the placeholder even after the bound instance was assigned
    /// fresh values by <c>PopulateCategoryState</c>. With PropertyChanged
    /// raised by the constructor assignments the item template re-evaluates
    /// <c>Source</c> and the new file path is loaded synchronously into the
    /// MAUI Image pipeline.
    /// </summary>
    public sealed partial class AlternativeLook : ObservableObject
    {
        public AlternativeLook(string title, int match, string? imageBase64 = null)
        {
            // Assign via the generated properties so subscribers (XAML
            // bindings on Source / Title / Match) receive the change
            // notification. Mirrors OutfitItemCard's constructor pattern.
            Title = title;
            Match = match;
            ImageBase64 = imageBase64;
        }

        [ObservableProperty] private string _title = string.Empty;
        [ObservableProperty] private int _match;
        [ObservableProperty] private string? _imageBase64;
    }

    /// <summary>
    /// Card-friendly projection of a single <see cref="ClothingItem"/> picked
    /// by the AI for a planner category. Holds only what's needed to render
    /// the hero image stack + a tiny role label; no DB tracking, no PII.
    /// Mirrors <see cref="DashboardViewModel.OutfitItemCard"/> so the same
    /// XAML converter (<c>Base64ToImageSourceConverter</c>) renders both
    /// surfaces.
    /// </summary>
    /// <summary>
    /// Card-friendly projection of a single <see cref="ClothingItem"/> picked
    /// by the AI for a planner category. Inherits <see cref="ObservableObject"/>
    /// so the bound <see cref="ImageBase64"/> property raises
    /// <c>PropertyChanged</c> when assigned. This is required because
    /// <c>SfRotator</c> realises its <c>ItemTemplate</c> data context after the
    /// initial binding pass; without a notification the template's
    /// <c>IsVisible</c> binding stays at the default <c>false</c> it captured
    /// at template construction, and the real image never appears.
    /// </summary>
    public sealed partial class OutfitItemCard : ObservableObject
    {
        public OutfitItemCard(int itemId, string role, string name, string category, string color, string? imageBase64)
        {
            ItemId = itemId;
            Role = role;
            Name = name;
            Category = category;
            Color = color;
            // Assign via the generated property so subscribers (XAML bindings
            // on Source/IsVisible) receive the change notification.
            ImageBase64 = imageBase64;
        }

        public int ItemId { get; }
        public string Role { get; }
        public string Name { get; }
        public string Category { get; }
        public string Color { get; }

        /// <summary>Base64 image bytes captured by the user; null when the item has no photo yet.</summary>
        [ObservableProperty] private string? _imageBase64;

        public string DisplayName => string.IsNullOrWhiteSpace(Name) ? Category : Name;
    }

    /// <summary>
    /// All bound state for a single planner category (Work / Meeting / Casual /
    /// Party). The VM keeps one of these per category and flips which one is
    /// rendered by changing <see cref="OutfitPlannerViewModel.SelectedCategory"/>.
    /// </summary>
    public sealed partial class CategoryViewState : ObservableObject
    {
        public CategoryViewState(string category)
        {
            Category = category;
            Title = $"{category} Look";
            Subtitle = string.Empty;
            MatchScore = 0;
            StyleScore = 0;
            ComfortScore = 0;
            TrendScore = 0;
            OccasionScore = 0;
            WeatherScore = 0;
            StyleLabel = ComfortLabel = TrendLabel = "—";
            Explanation = string.Empty;
            Reason = string.Empty;
            HasOutfit = false;
            Items = new ObservableCollection<OutfitItemCard>();
            Alternatives = new ObservableCollection<AlternativeLook>();
        }

        public string Category { get; }

        [ObservableProperty] private string _title = string.Empty;
        [ObservableProperty] private string _subtitle = string.Empty;
        [ObservableProperty] private int _matchScore;
        [ObservableProperty] private int _styleScore;
        [ObservableProperty] private int _comfortScore;
        [ObservableProperty] private int _trendScore;
        [ObservableProperty] private int _occasionScore;
        [ObservableProperty] private int _weatherScore;
        [ObservableProperty] private string _styleLabel = string.Empty;
        [ObservableProperty] private string _comfortLabel = string.Empty;
        [ObservableProperty] private string _trendLabel = string.Empty;
        [ObservableProperty] private string _explanation = string.Empty;
        [ObservableProperty] private string _reason = string.Empty;
        [ObservableProperty] private bool _hasOutfit;

        /// <summary>Closet items the AI picked for this category, in display order (Top → Dress → Bottom → Outerwear → Footwear).</summary>
        public ObservableCollection<OutfitItemCard> Items { get; }

        public ObservableCollection<AlternativeLook> Alternatives { get; }

        public void Reset()
        {
            Title = $"{Category} Look";
            Subtitle = string.Empty;
            MatchScore = StyleScore = ComfortScore = TrendScore = OccasionScore = WeatherScore = 0;
            StyleLabel = ComfortLabel = TrendLabel = "—";
            Explanation = Reason = string.Empty;
            HasOutfit = false;
            Items.Clear();
            Alternatives.Clear();
        }

        /// <summary>
        /// Re-fires <see cref="ObservableObject.OnPropertyChanged"/> for every
        /// property the radial gauges (RangePointer.Value) and their
        /// annotations (Label.Text) bind to. The MAUI binding system tracks
        /// the leaf path of nested bindings (e.g. <c>ActiveCategory.MatchScore</c>)
        /// and only refreshes when the leaf property's source fires
        /// PropertyChanged. When the VM swaps <c>ActiveCategory</c> to a new
        /// instance, the leaf source changes, so we re-announce every bound
        /// score on the new instance. Without this the RangePointer keeps its
        /// previous value and the ring renders fully filled.
        /// </summary>
        public void NotifyAllGaugesChanged()
        {
            OnPropertyChanged(nameof(MatchScore));
            OnPropertyChanged(nameof(StyleScore));
            OnPropertyChanged(nameof(ComfortScore));
            OnPropertyChanged(nameof(TrendScore));
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(Subtitle));
            OnPropertyChanged(nameof(Category));
        }

        /// <summary>
        /// Re-fires <see cref="ObservableObject.OnPropertyChanged"/> for the
        /// <see cref="Items"/> and <see cref="Alternatives"/> collections so
        /// their <c>ItemsSource</c>-bound controls (<c>SfRotator</c> for the
        /// hero gallery, <c>BindableLayout</c> for the alternative-strip
        /// thumbnails) re-evaluate the leaf source end-to-end. The default
        /// MAUI binding tracks nested paths off <c>ActiveCategory</c> and
        /// only refreshes the leaf when its previous instance raises a
        /// <c>CollectionChanged</c> event; when the parent state is replaced
        /// observe a brand-new <c>ObservableCollection</c> that has already
        /// been populated, BindableLayout / SfRotator occasionally retain the
        /// last-rendered child view because their subscription was already
        /// bound to the previous collection. Firing <c>PropertyChanged</c>
        /// for the property names forces a fresh re-subscription and a full
        /// re-template so the new image source reaches every cell.
        /// </summary>
        public void NotifyCollectionsChanged()
        {
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(Alternatives));
        }
    }

    /// <summary>
    /// <see cref="ITabRefreshable"/> entry point used by the
    /// <see cref="MainTabHostViewModel"/> when the <c>SfTabView</c> selection
    /// lands on (or re-enters) the Plan tab. Re-runs the same context load
    /// the old Shell <c>TabBar</c> triggered via <c>OnAppearing</c>. Pass
    /// <c>forceRegenerate: false</c> so tab switches reuse the cached plan
    /// rather than re-querying Azure OpenAI every time.
    /// </summary>
    public Task RefreshAsync() => LoadContextAsync(forceRegenerate: false);
}
