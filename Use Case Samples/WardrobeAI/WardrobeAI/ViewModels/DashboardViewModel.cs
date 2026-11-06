using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WardrobeAI.AI;
using WardrobeAI.AI.Contracts;
using WardrobeAI.Contracts;
using WardrobeAI.Data;
using WardrobeAI.Models;
using WardrobeAI.Repositories;
using WardrobeAI.Services;

namespace WardrobeAI.ViewModels;

/// <summary>
/// Dashboard view model (SDS Module 2). Mirrors the /designs/dashboard.html mockup:
/// greeting, today's AI suggestion, quick actions, AI insight, and upcoming plans.
/// The Upcoming Plans list is sourced live from the user's <see cref="Data.WardrobeDbContext"/>
/// <c>PlannedOutfits</c> rows — never dummy data.
/// </summary>
public partial class DashboardViewModel : ObservableObject, ITabRefreshable, IDisposable
{
    private readonly WardrobeDbContext _db;
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;
    private readonly IWeatherService _weatherService;
    private readonly IClothingItemRepository _clothingItemRepository;
    private readonly IAIOutfitRecommendationService _outfitAi;
    private readonly INavigationService _navigation;
    private readonly ILogger<DashboardViewModel> _logger;
    private int _lastLoadedClosetItemCount = -1;
    private bool _outfitRefreshPending;
    private bool _firstItemAddedRefreshHandled;

    public DashboardViewModel(
        WardrobeDbContext db,
        IUserContext userContext,
        IUserRepository userRepository,
        IWeatherService weatherService,
        IClothingItemRepository clothingItemRepository,
        IAIOutfitRecommendationService outfitAi,
        INavigationService navigation,
        ILogger<DashboardViewModel> logger)
    {
        _db = db;
        _userContext = userContext;
        _userRepository = userRepository;
        _weatherService = weatherService;
        _clothingItemRepository = clothingItemRepository;
        _outfitAi = outfitAi;
        _navigation = navigation;
        _logger = logger;

        // UserName / Greeting / ProfileImageBase64 are populated by LoadUserContextAsync() on every
        // dashboard appearance so the greeting + avatar always reflect the active
        // user. P7 retired the auth + onboarding flows; the dashboard now resolves
        // the active user through IUserContext (seeded with AppConstants.DefaultUserName).
        // Pre-seed the display name with the constant default so the side-rail
        // footer card and avatar initials never flash blank before
        // LoadUserContextAsync() resolves the signed-in user.
        UserName = AppConstants.DefaultUserName;
        ProfileImageBase64 = string.Empty;
        Greeting = ComposeGreeting();
        TodayDate = DateTime.Today.ToString("dddd, MMM d");
        // Weather is loaded live by RefreshWeatherAsync() on every OnAppearing.
        // The defaults below are the pre-P2.7 placeholder values so the chip
        // never renders blank while the first lookup is in flight.
        Weather = "—";
        WeatherIconSource = "cloudy";
        WeatherLocationLabel = string.Empty;
        IsWeatherLoading = false;
        OutfitMatchScore = 0;
        OutfitDescription = "Add clothing to your closet so AI can curate a look for today's occasion and weather.";
        OutfitTitle = "Today's Look";
        OutfitOccasion = string.Empty;
        OutfitItems = new ObservableCollection<OutfitItemCard>();
        HasOutfitItems = false;
        IsOutfitLoading = false;
        AiInsightTitle = "Wardrobe Insight";
        AiInsightBody = "You haven't worn your navy blazer in 28 days. Consider styling it with your beige chinos tomorrow.";

        QuickActions = new ObservableCollection<QuickAction>
        {
            new("Add Clothing", "add_circle", QuickActionAccent.Primary),
            new("Capture Photo", "photo_camera", QuickActionAccent.Secondary),
            new("Plan Outfit", "calendar_month", QuickActionAccent.Tertiary),
            new("Shop Similar", "storefront", QuickActionAccent.Neutral),
        };

        // Real, DB-bound upcoming plans. Populated by LoadUpcomingPlansAsync().
        UpcomingPlans = new ObservableCollection<UpcomingPlanCard>();
        HasUpcomingPlans = false;

        // ---- Profile-info popup (desktop-only) ----
        // The avatar in the desktop top-bar (DashboardShoppingSplitPage.xaml)
        // opens a small profile summary card when tapped. The data lives on
        // this VM because the avatar is already bound to it — adding another
        // VM would either duplicate the user context fetch or create a
        // circular DI reference. The data is refreshed alongside the rest of
        // LoadUserContextAsync() so it stays correct after login / onboarding.
        ProfileStylePreferences = new ObservableCollection<string>();
        ProfilePaletteHexes = new ObservableCollection<string>();
        ProfileStyleBadge = string.Empty;
        ProfileOutfitCount = 0;
        ProfileClothingItemCount = 0;
        IsProfilePopupOpen = false;

        IsLoading = false;

        // Live refresh hook: when a new plan is saved (e.g. from the Outfit
        // Planner FAB) reload the Upcoming Plans list so the user sees it
        // without having to navigate back to the Dashboard tab.
        PlanEvents.PlanSaved += OnPlanSaved;
        ClothingItemEvents.ItemAdded += OnClothingItemAdded;

        // Belt-and-braces: start the initial dashboard load when this VM is
        // constructed. The DashboardPage is reparented into the tab/split host,
        // where its own OnAppearing lifecycle is not dependable. Host refreshes
        // remain in place for subsequent tab and pane switches.
        _ = InitialRefreshAsync();
    }

    private async Task InitialRefreshAsync()
    {
        try
        {
            await LoadUserContextAsync();
            await RefreshWeatherAsync();
            await LoadUpcomingPlansAsync();
            await RefreshTodaysOutfitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Dashboard initial refresh failed.");
        }
    }

    public void Dispose()
    {
        PlanEvents.PlanSaved -= OnPlanSaved;
        ClothingItemEvents.ItemAdded -= OnClothingItemAdded;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InitialsAvatarName))]
    private string _userName = string.Empty;
    /// <summary>User's saved profile photo as a data-URL/base64 string. Mirror of
    /// <see cref="Models.User.ProfileImage"/>, refreshed by
    /// <see cref="LoadUserContextAsync"/>. The split-page shell binds the top-bar
    /// avatar directly to this property so the user sees their photo on desktop
    /// without having to navigate to the Profile tab.</summary>
    [ObservableProperty] private string _profileImageBase64 = string.Empty;

    /// <summary>True when <see cref="ProfileImageBase64"/> holds a renderable
    /// payload. The split-page shell uses this to drive the top-bar avatar's
    /// "real photo vs. initials" overlay — see
    /// <c>Views/DashboardShoppingSplitPage.xaml</c>. Mirrors the pattern used
    /// by <c>ProfileViewModel.HasProfileImage</c> and
    /// <c>AuthViewModel.HasProfileImage</c>.</summary>
    public bool HasProfileImage => !string.IsNullOrWhiteSpace(ProfileImageBase64);

    /// <summary>Display name fed into <c>SfAvatarView</c> when there is no
    /// profile photo so the top-bar avatar shows the user's initials rather
    /// than collapsing to a blank circle. Falls back to
    /// <see cref="AppConstants.DefaultUserName"/> when <see cref="UserName"/>
    /// is empty/missing so the initials match the "Alex Rivera" constant used
    /// everywhere else, then trims to up-to-two uppercase initials for the
    /// same reason <see cref="ProfileViewModel.InitialsAvatarName"/> does.</summary>
    public string InitialsAvatarName => ComputeInitialsAvatarName(UserName);

    private static string ComputeInitialsAvatarName(string? name)
    {
        var source = string.IsNullOrWhiteSpace(name) ? AppConstants.DefaultUserName : name;
        var parts = source.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return string.Empty;
        }

        return parts.Length == 1
            ? parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpperInvariant()
            : string.Concat(parts[0][0].ToString(), parts[^1][0].ToString()).ToUpperInvariant();
    }
    [ObservableProperty] private string _greeting = string.Empty;
    [ObservableProperty] private string _todayDate = string.Empty;
    [ObservableProperty] private string _weather = string.Empty;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WeatherImage))]
    private string _weatherIconSource = "cloudy";
    [ObservableProperty] private string _weatherLocationLabel = string.Empty;
    [ObservableProperty] private bool _isWeatherLoading;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OutfitMatchScoreFormatted))]
    private int _outfitMatchScore;
    [ObservableProperty] private string _outfitDescription = string.Empty;
    [ObservableProperty] private string _outfitTitle = string.Empty;
    [ObservableProperty] private string _outfitOccasion = string.Empty;
    [ObservableProperty] private bool _isOutfitLoading;
    [ObservableProperty] private bool _hasOutfitItems;

    /// <summary>Picked items for today's outfit (top, bottom, footwear, …). Empty until the AI returns a pick.</summary>
    public ObservableCollection<OutfitItemCard> OutfitItems { get; }
    [ObservableProperty] private string _aiInsightTitle = string.Empty;
    [ObservableProperty] private string _aiInsightBody = string.Empty;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EmptyStateMessage))]
    private bool _hasUpcomingPlans;

    public string OutfitMatchScoreFormatted => $"{OutfitMatchScore}%";
    public string EmptyStateMessage => "No upcoming plans yet. Plan an outfit from the Plan tab to see it here.";

    public ObservableCollection<QuickAction> QuickActions { get; }
    public ObservableCollection<UpcomingPlanCard> UpcomingPlans { get; }

    public ImageSource WeatherImage => ImageSource.FromFile(WeatherIconSource + ".png");

    // ---- Profile popup (desktop-only) ----
    [ObservableProperty] private bool _isProfilePopupOpen;

    /// <summary>Total outfits owned by the signed-in user. Drives the "Catalog" stat on the popup.</summary>
    [ObservableProperty] private int _profileOutfitCount;

    /// <summary>Total clothing items owned by the signed-in user. Drives the "Archive" stat on the popup.</summary>
    [ObservableProperty] private int _profileClothingItemCount;

    /// <summary>Top-3 color hex codes used by the user's closet. Drives the "Palette" swatches on the popup.</summary>
    public ObservableCollection<string> ProfilePaletteHexes { get; }

    /// <summary>User's selected style tags (split from the comma-joined column). Drives the chip row on the popup.</summary>
    public ObservableCollection<string> ProfileStylePreferences { get; }

    /// <summary>Short "<first-style> · <tier>" label rendered under the display name on the popup.</summary>
    [ObservableProperty] private string _profileStyleBadge = string.Empty;

    /// <summary>
    /// Opens the profile summary popup. Wired to the desktop top-bar
    /// avatar's <c>TapGestureRecognizer</c> in
    /// <c>DashboardShoppingSplitPage.xaml</c>. The popup is desktop-only —
    /// the phone / tablet host doesn't show this avatar at all so the
    /// tap handler is never invoked.
    /// </summary>
    [RelayCommand]
    private void ToggleProfilePopup()
    {
        IsProfilePopupOpen = !IsProfilePopupOpen;
    }

    /// <summary>
    /// Routes to the Profile tab via <see cref="INavigationService"/> so the
    /// user can edit their style preferences, profile photo, etc. Triggered
    /// by the "Curate Profile" link on the popup.
    /// </summary>
    [RelayCommand]
    private async Task CurateProfileAsync()
    {
        IsProfilePopupOpen = false;
        try
        {
            await _navigation.NavigateToProfileAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Curate-profile navigation failed.");
        }
    }

    /// <summary>
    /// Loads the popup's data (outfit/item counts, top-3 palette, style
    /// preferences, badge) alongside the rest of <see cref="LoadUserContextAsync"/>.
    /// Called from the main <c>OnAppearing</c> path of the desktop split
    /// page so the values are always live.
    /// </summary>
    private async Task LoadProfileSummaryAsync(int userId)
    {
        try
        {
            var startOfYear = new DateTime(DateTime.Today.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            ProfileOutfitCount = await _db.Outfits
                .AsNoTracking()
                .CountAsync(o => o.UserId == userId && o.CreatedDate >= startOfYear);

            var clothingForUser = _db.ClothingItems.AsNoTracking().Where(c => c.UserId == userId);
            ProfileClothingItemCount = await clothingForUser.CountAsync();

            // Top-3 palette for the popup.
            var topColors = await clothingForUser
                .GroupBy(c => c.Color)
                .Select(g => new { Color = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.Color)
                .Take(3)
                .ToListAsync();

            ProfilePaletteHexes.Clear();
            foreach (var c in topColors)
            {
                ProfilePaletteHexes.Add(ColorNameToHex(c.Color));
            }

            // Style preferences: comma-split the persisted column and trim.
            // Falls back to a single entry when the column has no separator
            // (the onboarding service writes a single string when the user
            // picked one style).
            var styleCol = await _userRepository.GetAsync(userId);
            ProfileStylePreferences.Clear();
            if (styleCol is not null && !string.IsNullOrWhiteSpace(styleCol.StylePreference))
            {
                var parts = styleCol.StylePreference
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length == 0)
                {
                    ProfileStylePreferences.Add(styleCol.StylePreference.Trim());
                }
                else
                {
                    foreach (var p in parts)
                    {
                        ProfileStylePreferences.Add(p);
                    }
                }
            }

            ProfileStyleBadge = ComposePopupStyleBadge(styleCol?.StylePreference, styleCol?.BudgetLevel ?? BudgetLevel.Medium);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Profile summary load failed; popup will show empty stats.");
            ProfileOutfitCount = 0;
            ProfileClothingItemCount = 0;
            ProfilePaletteHexes.Clear();
            ProfileStylePreferences.Clear();
            ProfileStyleBadge = string.Empty;
        }
    }

    /// <summary>
    /// Maps a free-text clothing color name to a representative hex code for
    /// the popup's palette swatches. Falls back to neutral grey for unknown
    /// labels so we never throw on a custom color value.
    /// </summary>
    private static string ColorNameToHex(string? colorName)
    {
        if (string.IsNullOrWhiteSpace(colorName))
        {
            return "#9E9E9E";
        }

        var key = colorName.Trim().ToLowerInvariant();
        return key switch
        {
            "black" => "#1C1C1C",
            "white" => "#F6F3EE",
            "blue" => "#3B5BA9",
            "navy" => "#1F2A44",
            "red" => "#C0392B",
            "green" => "#2E7D4F",
            "olive" => "#6B7A3A",
            "yellow" => "#E6B800",
            "pink" => "#D67BA0",
            "purple" => "#6C4F8E",
            "brown" => "#6B4423",
            "grey" or "gray" => "#7A7A7A",
            "beige" => "#D9C9B0",
            "burgundy" => "#7A1F2B",
            "earth tones" => "#8B6F47",
            _ => "#9E9E9E"
        };
    }

    /// <summary>
    /// Composes a short uppercase badge for the popup header
    /// (e.g. "BUSINESS STYLE CONNOISSEUR"). Mirrors the badge under the
    /// avatar on ProfilePage.xaml so the popup matches the design language.
    /// </summary>
    private static string ComposePopupStyleBadge(string? stylePreference, BudgetLevel budgetLevel)
    {
        // Take the first style token as the badge prefix, fallback to "Style".
        var firstStyle = "Style";
        if (!string.IsNullOrWhiteSpace(stylePreference))
        {
            var parts = stylePreference
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length > 0)
            {
                firstStyle = parts[0];
            }
        }

        var tier = budgetLevel switch
        {
            BudgetLevel.Low => "Smart Saver",
            BudgetLevel.Medium => "Style Curator",
            BudgetLevel.Premium => "Style Connoisseur",
            BudgetLevel.Luxury => "Luxury Stylist",
            _ => "Style Curator"
        };

        return $"{firstStyle} {tier}".ToUpperInvariant();
    }

    /// <summary>
    /// Loads the signed-in user's upcoming plans (today + future) from the database
    /// and projects them into <see cref="UpcomingPlanCard"/> instances. Safe to call
    /// multiple times — the collection is cleared before being repopulated.
    ///
    /// Both stand-alone plans (created from the Outfit Planner FAB) and outfit-linked
    /// plans are included. For stand-alone plans the plan's own <c>Name</c> and
    /// <c>Occasion</c> snapshots drive the card text.
    /// </summary>
    [RelayCommand]
    public async Task LoadUpcomingPlansAsync()
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
                UpcomingPlans.Clear();
                HasUpcomingPlans = false;
                return;
            }

            var today = DateTime.Today;

            // Single query that handles both ownership paths and falls back to the
            // plan's own Name/Occasion snapshots for stand-alone plans.
            var plans = await _db.PlannedOutfits
                .AsNoTracking()
                .Where(p => p.PlanDate >= today)
                .Where(p => p.UserId == userId
                            || (p.Outfit != null && p.Outfit.UserId == userId))
                .OrderBy(p => p.PlanDate)
                .Take(10)
                .Select(p => new UpcomingPlanCard(
                    planId: p.PlanId,
                    planDate: p.PlanDate,
                    occasion: p.Outfit != null ? p.Outfit.Occasion : p.Occasion,
                    outfitName: p.Outfit != null ? p.Outfit.Name : p.Name,
                    icon: IconForOccasion(p.Outfit != null ? p.Outfit.Occasion : p.Occasion)))
                .ToListAsync();

            UpcomingPlans.Clear();
            foreach (var card in plans)
            {
                UpcomingPlans.Add(card);
            }
            HasUpcomingPlans = UpcomingPlans.Count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load upcoming plans for dashboard.");
            UpcomingPlans.Clear();
            HasUpcomingPlans = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Live-refresh handler: when any VM broadcasts a <see cref="PlanSavedEventArgs"/>,
    /// re-run the upcoming plans query (only for the active user) so the new plan
    /// appears on the Dashboard immediately.
    /// </summary>
    private async void OnClothingItemAdded(object? sender, ClothingItemAddedEventArgs e)
    {
        try
        {
            if (e.UserId != _userContext.GetCurrentUserId() || _firstItemAddedRefreshHandled)
            {
                return;
            }

            _firstItemAddedRefreshHandled = true;
            await RefreshTodaysOutfitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Closet-item curation refresh failed.");
        }
    }

    private async void OnPlanSaved(object? sender, PlanSavedEventArgs e)
    {
        try
        {
            var userId = _userContext.GetCurrentUserId();
            if (userId <= 0 || e.UserId != userId)
            {
                return;
            }
            await LoadUpcomingPlansAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Plan-saved refresh failed.");
        }
    }

    /// <summary>
    /// Triggered by the "Generate New" button on the Today's Suggestion card.
    /// Re-runs the AI outfit picker. Coalesces concurrent calls via
    /// <see cref="IsOutfitLoading"/> so quick double-taps don't double-bill
    /// Azure OpenAI.
    /// </summary>
    [RelayCommand]
    private async Task GenerateNewAsync()
    {
        if (IsOutfitLoading)
        {
            return;
        }
        await RefreshTodaysOutfitAsync();
    }

    /// <summary>
    /// Loads today's outfit suggestion by combining (in order):
    ///  1. the user's planned occasion for today (or "Casual" as a safe default),
    ///  2. the latest weather snapshot (or Unknown if weather hasn't loaded yet),
    ///  3. the user's closet projected to <see cref="ClothingItemSummary"/> DTOs,
    ///  4. the user's gender / style / budget preferences.
    /// On any failure (Azure OpenAI not configured, no closet, network down,
    /// malformed response) the dashboard keeps its last good outfit — the
    /// placeholder copy is only shown when no pick has ever succeeded.
    /// </summary>
    [RelayCommand]
    public async Task RefreshTodaysOutfitAsync()
    {
        if (IsOutfitLoading)
        {
            _outfitRefreshPending = true;
            return;
        }

        IsOutfitLoading = true;
        try
        {
            var userId = _userContext.GetCurrentUserId();
            var closet = await _clothingItemRepository.GetAllForUserAsync(userId);
            if (closet.Count == 0)
            {
                _lastLoadedClosetItemCount = 0;
                SetOutfitPlaceholder("Add clothing to your closet to start curation for today's occasion and weather.");
                return;
            }

            var user = await _userRepository.GetAsync(userId);
            // Occasion: prefer today's planned outfit. Fall back to "Casual".
            var today = DateTime.Today;
            var occasion = await ResolveTodaysOccasionAsync(userId, today)
                           ?? "Casual";

            // Weather: re-fetch a fresh snapshot so the suggestion is
            // anchored to the same temp/condition the chip is showing.
            var weatherResult = await _weatherService.GetCurrentWeatherAsync();
            var weather = weatherResult.IsSuccess
                ? weatherResult.Value!
                : new WeatherInfo(0, WeatherCondition.Unknown, 0, string.Empty);

            // Closet: project DB rows → LLM-safe summaries.
            var closetChanged = closet.Count != _lastLoadedClosetItemCount;
            _lastLoadedClosetItemCount = closet.Count;
            if (closetChanged)
            {
                SetOutfitPlaceholder("Finding today's look from your updated closet...");
            }

            var request = new OutfitRecommendationRequest(
                UserId: user.UserId,
                Gender: user.Gender.ToString(),
                AgeGroup: user.AgeGroup,
                StylePreference: user.StylePreference,
                BudgetLevel: user.BudgetLevel.ToString(),
                Location: weather.LocationLabel,
                Occasion: occasion,
                Weather: new WeatherSummary(
                    TemperatureC: weather.TemperatureC,
                    Condition: weather.Condition.ToString(),
                    HumidityPercent: weather.HumidityPercent,
                    LocationLabel: weather.LocationLabel),
                Closet: closet.Select(MapToSummary).ToList());

            var result = await _outfitAi.GetDailyOutfitSuggestionAsync(request);
            if (!result.IsSuccess)
            {
                _logger.LogWarning(
                    "Today's outfit suggestion failed: {Code} {Message}",
                    result.Error!.Code, result.Error!.Message);
                // Keep the last good outfit unless the closet changed since
                // that suggestion was generated. In that case, don't show
                // items from the previous closet as today's fresh plan.
                if (OutfitItems.Count == 0 || closetChanged)
                {
                    SetOutfitPlaceholder(FriendlyFallback(result.Error!.Code, result.Error!.Message));
                }
                return;
            }

            ApplyOutfitResult(result.Value!, closet);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RefreshTodaysOutfitAsync threw unexpectedly.");
            if (OutfitItems.Count == 0)
            {
                SetOutfitPlaceholder("Could not reach the AI service. Please try again.");
            }
        }
        finally
        {
            IsOutfitLoading = false;
            if (_outfitRefreshPending)
            {
                _outfitRefreshPending = false;
                await RefreshTodaysOutfitAsync();
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

    /// <summary>
    /// Resolves the occasion label for today: the first planned outfit whose
    /// <c>PlanDate</c> falls on today's date, with the outfit-linked
    /// <c>Occasion</c> taking precedence over the stand-alone snapshot.
    /// Returns null when nothing is planned today.
    /// </summary>
    private async Task<string?> ResolveTodaysOccasionAsync(int userId, DateTime today)
    {
        try
        {
            var tomorrow = today.AddDays(1);
            var plan = await _db.PlannedOutfits
                .AsNoTracking()
                .Where(p => p.PlanDate >= today && p.PlanDate < tomorrow)
                .Where(p => p.UserId == userId
                            || (p.Outfit != null && p.Outfit.UserId == userId))
                .OrderBy(p => p.PlanDate)
                .Select(p => p.Outfit != null ? p.Outfit.Occasion : p.Occasion)
                .FirstOrDefaultAsync();
            return string.IsNullOrWhiteSpace(plan) ? null : plan!.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ResolveTodaysOccasionAsync failed; falling back to 'Casual'.");
            return null;
        }
    }

    private void ApplyOutfitResult(OutfitRecommendationResult result, IReadOnlyList<ClothingItem> closet)
    {
        var closetById = closet.ToDictionary(c => c.ItemId);
        OutfitTitle = result.Primary.Title;
        OutfitOccasion = result.Primary.Occasion;
        OutfitMatchScore = Math.Clamp(result.Primary.MatchScore, 0, 100);
        OutfitDescription = result.Primary.Reason;

        // AI Insight (dashboard): do not reuse `Reason`.
        // Generate a short, user-facing insight from the outfit itself
        // (title/occasion + a few key items) so the insight always renders.
        AiInsightTitle = "Wardrobe Insight";
        AiInsightBody = BuildAiInsightBody(result.Primary, closetById);

        OutfitItems.Clear();
        foreach (var pick in result.Primary.Items)
        {
            if (!closetById.TryGetValue(pick.ItemId, out var item))
            {
                continue;
            }
            OutfitItems.Add(new OutfitItemCard(
                itemId: item.ItemId,
                role: pick.Role,
                name: item.Name,
                category: item.Category,
                color: item.Color,
                imageBase64: item.ImageUrl,
                fabric: item.Fabric,
                style: item.Style));
        }
        HasOutfitItems = OutfitItems.Count > 0;
    }

    private void SetOutfitPlaceholder(string message)
    {
        OutfitTitle = "Today's Look";
        OutfitOccasion = string.Empty;
        OutfitMatchScore = 0;
        OutfitDescription = message;
        OutfitItems.Clear();
        HasOutfitItems = false;
    }

    private static string FriendlyFallback(string code, string message)
    {
        if (code == "AI.NotConfigured")
        {
            return "AI suggestions aren't configured yet. Add your Azure OpenAI details to enable today's look.";
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

    /// <summary>
    /// Live weather refresh for the dashboard greeting chip. Idempotent and
    /// cancellation-safe — concurrent calls coalesce via <see cref="IsWeatherLoading"/>
    /// so the dashboard never issues two parallel location requests on quick
    /// tab switches.
    ///
    /// On any failure (permission denied, GPS off, API timeout, offline) we
    /// keep the last good values so the chip stays stable and the user never
    /// sees a "broken" empty state. The first attempt populates the defaults
    /// ("—" + cloudy) so the chip is never blank.
    /// </summary>
    [RelayCommand]
    public async Task RefreshWeatherAsync()
    {
        if (IsWeatherLoading)
        {
            return;
        }

        try
        {
            IsWeatherLoading = true;
            var result = await _weatherService.GetCurrentWeatherAsync();
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Weather refresh failed: {Code} {Message}",
                    result.Error!.Code, result.Error!.Message);
                // Keep the existing weather values so the chip doesn't flicker.
                return;
            }

            var info = result.Value!;
            Weather = info.TemperatureDisplay;
            WeatherIconSource = info.IconKey;
            WeatherLocationLabel = info.LocationLabel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RefreshWeatherAsync threw unexpectedly.");
        }
        finally
        {
            IsWeatherLoading = false;
        }
    }

    private static string BuildAiInsightBody(
        OutfitSuggestion primary,
        IReadOnlyDictionary<int, ClothingItem> closetById)
    {
        if (primary is null)
        {
            return "Your next look is ready.";
        }

        var occasion = string.IsNullOrWhiteSpace(primary.Occasion) ? "today" : primary.Occasion.Trim();
        var title = string.IsNullOrWhiteSpace(primary.Title) ? "" : primary.Title.Trim();

        // Collect up to 3 item names in slot order.
        var itemNames = primary.Items
            .Select(p => {
                if (closetById.TryGetValue(p.ItemId, out var c))
                {
                    return c.Name;
                }
                return null;
            })
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct()
            .Take(3)
            .ToList();

        var itemsPart = itemNames.Count switch
        {
            0 => "your closet",
            1 => itemNames[0],
            _ => $"{itemNames[0]}, {itemNames[1]}" + (itemNames.Count > 2 ? $" and {itemNames[2]}" : "")
        };

        // 2 short sentences so it fits in the insight card.
        // (Dashboards wraps by XAML; keep it concise.)
        return string.IsNullOrWhiteSpace(title)
            ? $"For {occasion}, this look pairs {itemsPart}."
            : $"{title} for {occasion}: {itemsPart}.";
    }

    /// <summary>
    /// Resolves a Quick Action tap into the matching main tab. The keys
    /// match the <c>CommandParameter</c>s wired up in
    /// <c>DashboardPage.xaml</c>:
    /// <list type="bullet">
    ///   <item><description><c>add</c>  → Closet (Add Clothing)</description></item>
    ///   <item><description><c>plan</c> → Outfit Planner (Plan Outfit)</description></item>
    ///   <item><description><c>shop</c> → Shopping (Shop Similar)</description></item>
    /// </list>
    /// Anything else is logged and ignored.
    ///
    /// <para>Implementation note: this used to call
    /// <c>Shell.Current.GoToAsync("//main/closet")</c>, but the main tabs
    /// are now hosted inside a Syncfusion <c>SfTabView</c> on
    /// <c>MainTabHostPage</c> (not the platform Shell <c>TabBar</c>).
    /// <c>GoToAsync</c> resolved the registered per-tab route to a fresh
    /// <c>ClosetPage</c> instance and <i>pushed it on top of</i> the
    /// current page, so the user kept seeing the Dashboard and the
    /// "navigated" page hid the active tab underneath. Switching the
    /// tab by mutating <c>MainTabHostViewModel.SelectedIndex</c> goes
    /// through <c>INavigationService</c> instead, which is what the
    /// host's <c>SelectionChanged</c> handler is wired for.</para>
    /// </summary>
    [RelayCommand]
    private async Task SelectQuickAction(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        var action = key.Trim().ToLowerInvariant() switch
        {
            "add" => new Func<Task>(_navigation.NavigateToClosetAsync),
            "plan" => new Func<Task>(_navigation.NavigateToPlannerAsync),
            "shop" => new Func<Task>(_navigation.NavigateToShoppingAsync),
            _ => null,
        };

        if (action is null)
        {
            _logger.LogWarning("SelectQuickAction received unknown key {Key}.", key);
            return;
        }

        try
        {
            await action();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Quick action navigation failed for key {Key}.", key);
        }
    }

    /// <summary>
    /// Loads the signed-in user's display name from the User row and refreshes the
    /// greeting copy. Safe to call repeatedly; called from <c>OnAppearing</c> so a
    /// freshly-registered or re-onboarded user sees their real name immediately.
    /// </summary>
    [RelayCommand]
    public async Task LoadUserContextAsync()
    {
        try
        {
            var userId = _userContext.GetCurrentUserId();
            if (userId <= 0)
            {
                _logger.LogWarning(
                    "Dashboard LoadUserContextAsync: no signed-in user (UserId=0). " +
                    "IUserContext.EnsureUserAsync may not have completed yet.");
                UserName = string.Empty;
                ProfileImageBase64 = string.Empty;
                Greeting = ComposeGreeting();
                return;
            }

            var user = await _userRepository.GetAsync(userId);
            if (user is null)
            {
                // AuthService returned a user id but no matching User row —
                // the auth preference is stale (most likely the local DB was
                // cleared but the preference wasn't). Log it and fall back
                // to a neutral display name so the chip doesn't look broken.
                _logger.LogWarning(
                    "Dashboard LoadUserContextAsync: UserId {UserId} has no matching row.", userId);
                UserName = "there";
                ProfileImageBase64 = string.Empty;
                Greeting = ComposeGreeting();
                return;
            }

            var name = user?.Name;
            UserName = !string.IsNullOrWhiteSpace(name) ? name!.Trim() : "there";
            // Pull the saved profile-photo bytes for the desktop shell's top-bar
            // avatar. Stored as base64 by OnboardingService / AuthService — the
            // split page's avatar Image converts this to an ImageSource via
            // Converters.Base64ToImageSourceConverter.
            ProfileImageBase64 = !string.IsNullOrWhiteSpace(user?.ProfileImage) ? user!.ProfileImage! : string.Empty;
            Greeting = ComposeGreeting();

            // Refresh the desktop profile-popup data (outfit / item counts,
            // palette swatches, style chips, badge). Lives next to the user
            // context load so the popup always reflects the currently signed-in
            // user — important because transient VM lifetime means a fresh
            // DashboardViewModel can be constructed after a sign-out / sign-in
            // round-trip with no prior profile state.
            await LoadProfileSummaryAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load user context for dashboard.");
            UserName = "there";
            Greeting = ComposeGreeting();
            // Clear popup state on failure so the user doesn't see stale stats
            // from a previous signed-in user.
            ProfileStylePreferences.Clear();
            ProfilePaletteHexes.Clear();
            ProfileStyleBadge = string.Empty;
            ProfileOutfitCount = 0;
            ProfileClothingItemCount = 0;
        }
    }

    /// <summary>
    /// Time-of-day greeting prefix. Defaults to a neutral "Hello" so we never
    /// produce a misleading "Good morning" at midnight.
    /// </summary>
    private static string ComposeGreeting()
    {
        var hour = DateTime.Now.Hour;
        return hour switch
        {
            >= 5 and < 12 => "Good morning,",
            >= 12 and < 17 => "Good afternoon,",
            >= 17 and < 22 => "Good evening,",
            _ => "Hello,",
        };
    }

    [RelayCommand]
    private void OpenPlan(UpcomingPlanCard? plan)
    {
        // Hook for future navigation to plan detail.
    }

    /// <summary>
    /// Maps a free-text occasion (e.g. "Business", "Casual", "Formal") to a Material Symbols
    /// icon name. Falls back to a generic event icon.
    /// </summary>
    private static string IconForOccasion(string? occasion)
    {
        if (string.IsNullOrWhiteSpace(occasion))
        {
            return "event";
        }

        var key = occasion.Trim().ToLowerInvariant();
        return key switch
        {
            var k when k.Contains("business") || k.Contains("office") || k.Contains("work") => "business_center",
            var k when k.Contains("formal") || k.Contains("wedding") || k.Contains("gala") => "celebration",
            var k when k.Contains("casual") || k.Contains("weekend") || k.Contains("park") => "park",
            var k when k.Contains("sport") || k.Contains("gym") || k.Contains("run") => "fitness_center",
            var k when k.Contains("travel") || k.Contains("trip") => "flight",
            var k when k.Contains("dinner") || k.Contains("date") => "restaurant",
            var k when k.Contains("party") => "celebration",
            _ => "event",
        };
    }

    public sealed record QuickAction(string Label, string Icon, QuickActionAccent Accent);

    /// <summary>
    /// Card-friendly projection of a single <see cref="ClothingItem"/> selected
    /// by the AI for today's outfit. Inherits <see cref="ObservableObject"/>
    /// so the bound <see cref="ImageBase64"/> property raises
    /// <c>PropertyChanged</c> when assigned. This is required so XAML
    /// <c>IsVisible</c>/<c>Source</c> bindings on the card re-evaluate when the
    /// rotator/template realises the data context after the initial pass.
    /// </summary>
    public sealed partial class OutfitItemCard : ObservableObject
    {
        public OutfitItemCard(int itemId, string role, string name, string category, string color,
            string? imageBase64, string fabric = "", string style = "")
        {
            ItemId = itemId;
            Role = role;
            Name = name;
            Category = category;
            Color = color;
            Fabric = fabric;
            Style = style;
            // Assign via the generated property so subscribers receive the
            // change notification.
            ImageBase64 = imageBase64;
        }

        public int ItemId { get; }
        public string Role { get; }
        public string Name { get; }
        public string Category { get; }
        public string Color { get; }
        public string Fabric { get; }
        public string Style { get; }

        /// <summary>Base64 image bytes captured by the user; null when the item has no photo yet.</summary>
        [ObservableProperty] private string? _imageBase64;

        public string DisplayName => string.IsNullOrWhiteSpace(Name) ? Category : Name;

        /// <summary>
        /// Subtitle shown under the featured-item title on the Desktop hero card
        /// (e.g. "Casual Essential"). Falls back to the Style tag when no
        /// occasion label is available.
        /// </summary>
        public string Subtitle => string.IsNullOrWhiteSpace(Style) ? "Casual Essential" : Style;

        /// <summary>
        /// Three short feature chips shown beneath the featured item on the
        /// Desktop hero card. Derived from the actual fabric + color so the
        /// labels stay honest even though we can't infer breathability.
        /// </summary>
        public IReadOnlyList<string> FeatureTags
        {
            get
            {
                var tags = new List<string> { "Lightweight", "Soft Touch" };
                if (Fabric.Contains("Linen", StringComparison.OrdinalIgnoreCase) ||
                    Fabric.Contains("Cotton", StringComparison.OrdinalIgnoreCase) ||
                    Fabric.Contains("Silk", StringComparison.OrdinalIgnoreCase))
                {
                    tags.Insert(1, "Breathable");
                }
                else
                {
                    tags.Insert(1, "All-Day Wear");
                }
                return tags;
            }
        }
    }

    /// <summary>
    /// Featured item projection for the Desktop hero card. Surfaces the first
    /// item of <see cref="DashboardViewModel.OutfitItems"/> with computed display
    /// strings, so the XAML can bind to a single object instead of indexing
    /// into the rotator collection.  Returns null when the rotator is empty.
    /// </summary>
    public OutfitItemCard? FeaturedItem =>
        OutfitItems.Count == 0 ? null : OutfitItems[0];

    /// <summary>Number of items in the curated outfit (used for the "+ N more"
    /// chip on the Desktop hero card).</summary>
    public int AdditionalItemCount => Math.Max(0, OutfitItems.Count - 1);

    /// <summary>
    /// Card-friendly projection of a <see cref="WardrobeAI.Models.PlannedOutfit"/> for the
    /// dashboard's horizontal Upcoming Plans list. Holds only what's needed for display —
    /// no entity-tracking, no PII beyond what's already in the joined <c>Outfit</c>.
    /// </summary>
    public sealed class UpcomingPlanCard
    {
        public UpcomingPlanCard(int planId, DateTime planDate, string occasion, string outfitName, string icon)
        {
            PlanId = planId;
            PlanDate = planDate;
            Occasion = occasion;
            OutfitName = outfitName;
            Icon = icon;
        }

        public int PlanId { get; }
        public DateTime PlanDate { get; }
        public string Occasion { get; }
        public string OutfitName { get; }
        public string Icon { get; }

        public string When => PlanDate.ToString("dddd, h:mm tt");
        public string ShortWhen => PlanDate.ToString("MMM d, h:mm tt");
        public string Day => PlanDate.ToString("ddd").ToUpperInvariant();
        public string DayNumber => PlanDate.Day.ToString();
        public string Month => PlanDate.ToString("MMM").ToUpperInvariant();
    }

    public enum QuickActionAccent { Primary, Secondary, Tertiary, Neutral }

    /// <summary>
    /// <see cref="ITabRefreshable"/> entry point used by the
    /// <see cref="MainTabHostViewModel"/> when the <c>SfTabView</c> selection
    /// lands on (or re-enters) the Dashboard tab. Runs the same three load
    /// steps the old Shell <c>TabBar</c> triggered via <c>OnAppearing</c>:
    /// user context, live weather, and today's AI outfit suggestion. Each
    /// individual step is already idempotent and self-coalescing.
    /// </summary>
    public async Task RefreshAsync()
    {
        try
        {
            await LoadUserContextAsync();
            await RefreshWeatherAsync();
            await LoadUpcomingPlansAsync();
            await RefreshTodaysOutfitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dashboard refresh failed.");
        }
    }
}
