using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WardrobeAI.Data;
using WardrobeAI.Models;
using WardrobeAI.Repositories;
using WardrobeAI.Services;

namespace WardrobeAI.ViewModels;

/// <summary>
/// User Profile + Analytics view model (SDS Modules 8 & 9). Mirrors /designs/profile.html:
/// profile header, style preferences chips, analytics bento grid, achievements, settings list.
///
/// All user-specific fields (name, profile image, style preferences, analytics, achievements)
/// are sourced live from the signed-in <see cref="User"/> row plus the
/// <see cref="WardrobeDbContext"/> clothing / outfit tables — no hard-coded values.
/// </summary>
public partial class ProfileViewModel : ObservableObject, ITabRefreshable
{
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;
    private readonly WardrobeDbContext _db;
    private readonly IBudgetService _budgetService;
    private readonly ILogger<ProfileViewModel> _logger;

    public ProfileViewModel(
        IUserRepository userRepository,
        IUserContext userContext,
        WardrobeDbContext db,
        IBudgetService budgetService,
        ILogger<ProfileViewModel> logger)
    {
        _userRepository = userRepository;
        _userContext = userContext;
        _db = db;
        _budgetService = budgetService;
        _logger = logger;

        // Empty defaults; populated by LoadAsync() once a user is signed in.
        UserName = string.Empty;
        StyleBadge = string.Empty;
        ProfileImageBase64 = null;
        Email = string.Empty;
        Location = string.Empty;
        AgeGroup = string.Empty;
        BudgetLevelLabel = string.Empty;
        Gender = string.Empty;
        OutfitsThisYear = 0;
        ClosetGrowthNote = string.Empty;

        // Budget defaults: the slider range is $0–$1000 in $25 steps; the
        // default monthly budget is $200 (same as the budget service default).
        MonthlyBudgetUsd = 200m;
        MonthlyBudgetMinUsd = 0m;
        MonthlyBudgetMaxUsd = 1000m;
        MonthlyBudgetStepUsd = 25m;
        IsEditingBudget = false;
        BudgetHelperText = "Drag the slider to set your monthly shopping allowance.";

        StylePreferences = new ObservableCollection<string>();
        TopColors = new ObservableCollection<ColorSwatch>();
        Achievements = new ObservableCollection<Achievement>();
        Settings = new ObservableCollection<SettingItem>
        {
            new("AI Preferences", "\uE748"),
            new("Privacy & Security", "\uE77B"),
            new("Notifications", "\uE78F")
        };
    }

    [ObservableProperty] private string _userName = string.Empty;
    [ObservableProperty] private string _styleBadge = string.Empty;

    /// <summary>
    /// Base64-encoded profile picture from <see cref="User.ProfileImage"/>.
    /// Bound through <c>Base64ToImageSourceConverter</c> in XAML to render the avatar.
    /// Null when the user has no profile picture.
    /// </summary>
    [ObservableProperty] private string? _profileImageBase64;

    /// <summary>
    /// True when the signed-in user has a profile picture. Used by the XAML
    /// to decide between the initials fallback and the custom image avatar.
    /// </summary>
    public bool HasProfileImage => !string.IsNullOrWhiteSpace(ProfileImageBase64);

    /// <summary>
    /// Name used to drive the <c>SfAvatarView</c> initials fallback when the
    /// user has no profile picture. Up to two characters are rendered.
    /// </summary>
    public string InitialsAvatarName => ComputeInitialsAvatarName(UserName);

    private static string ComputeInitialsAvatarName(string? name)
    {
        var parts = name?.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [];
        if (parts.Length == 0)
        {
            return string.Empty;
        }

        return parts.Length == 1
            ? parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpperInvariant()
            : string.Concat(parts[0][0].ToString(), parts[^1][0].ToString()).ToUpperInvariant();
    }

    partial void OnProfileImageBase64Changed(string? value) => OnPropertyChanged(nameof(HasProfileImage));
    partial void OnUserNameChanged(string value) => OnPropertyChanged(nameof(InitialsAvatarName));
    partial void OnMonthlyBudgetUsdChanged(decimal value)
    {
        OnPropertyChanged(nameof(MonthlyBudgetDisplay));
        // Keep the helper text in sync as the user drags the slider, without
        // hitting the database. The Save command persists the value.
        BudgetHelperText = ComposeBudgetHelper(value);
    }

    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _location = string.Empty;
    [ObservableProperty] private string _ageGroup = string.Empty;
    [ObservableProperty] private string _budgetLevelLabel = string.Empty;
    [ObservableProperty] private string _gender = string.Empty;
    [ObservableProperty] private int _outfitsThisYear;
    [ObservableProperty] private string _closetGrowthNote = string.Empty;
    [ObservableProperty] private bool _isLoading;

    // ---- Monthly shopping budget (slider + edit affordance) ----
    /// <summary>Current monthly budget in USD, persisted by <see cref="IBudgetService"/>.</summary>
    [ObservableProperty] private decimal _monthlyBudgetUsd;

    /// <summary>Slider lower bound (USD). Hard-coded at $0 — users can opt out.</summary>
    [ObservableProperty] private decimal _monthlyBudgetMinUsd;

    /// <summary>Slider upper bound (USD). Hard-coded at $1000 to keep the range sensible.</summary>
    [ObservableProperty] private decimal _monthlyBudgetMaxUsd;

    /// <summary>Slider step (USD). $25 increments keep the value meaningful without over-granular taps.</summary>
    [ObservableProperty] private decimal _monthlyBudgetStepUsd;

    /// <summary>True while the user is dragging / editing the budget slider. Hides the static "$200" pill.</summary>
    [ObservableProperty] private bool _isEditingBudget;

    /// <summary>Short hint under the slider that updates with the chosen value (e.g. "5 quality staples @ $40").</summary>
    [ObservableProperty] private string _budgetHelperText = string.Empty;

    /// <summary>Display string for the budget pill — formatted as <c>$XXX</c>.</summary>
    public string MonthlyBudgetDisplay => $"${(int)MonthlyBudgetUsd}";

    public ObservableCollection<string> StylePreferences { get; }
    public ObservableCollection<ColorSwatch> TopColors { get; }
    public ObservableCollection<Achievement> Achievements { get; }
    public ObservableCollection<SettingItem> Settings { get; }

    /// <summary>
    /// Loads the signed-in user's profile (name, profile image, style preferences,
    /// analytics) from the database. Safe to call multiple times; the
    /// collections are cleared and repopulated on every call so live updates
    /// (e.g. a new clothing item) are reflected after a refresh.
    /// </summary>
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
                ResetForSignedOut();
                return;
            }

            var user = await _userRepository.GetAsync(userId);
            if (user is null)
            {
                _logger.LogWarning("Profile load: signed-in user {UserId} no longer exists in DB.", userId);
                ResetForSignedOut();
                return;
            }

            // ---- Profile header ----
            UserName = string.IsNullOrWhiteSpace(user.Name) ? AppConstants.DefaultUserName : user.Name;
            ProfileImageBase64 = user.ProfileImage; // base64 string; converted to ImageSource in XAML
            Email = user.Email ?? string.Empty;
            Location = user.Location ?? string.Empty;
            AgeGroup = user.AgeGroup ?? string.Empty;
            BudgetLevelLabel = user.BudgetLevel.ToString();
            Gender = user.Gender.ToString();

            // Style badge reflects the user's style + budget tier (something
            // meaningful that we can show without inventing achievements).
            StyleBadge = ComposeStyleBadge(user.StylePreference, user.BudgetLevel);

            // ---- Style preferences (chips) ----
            // Onboarding persists StylePreference as a comma-separated string
            // (e.g. "Business, Minimalist, Smart Casual"). Split it back into
            // individual chips; fall back to the single value if it didn't
            // contain a separator.
            StylePreferences.Clear();
            var styles = (user.StylePreference ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (styles.Length == 0 && !string.IsNullOrWhiteSpace(user.StylePreference))
            {
                StylePreferences.Add(user.StylePreference.Trim());
            }
            else
            {
                foreach (var s in styles)
                {
                    StylePreferences.Add(s);
                }
            }

            // ---- Analytics ----
            var startOfYear = new DateTime(DateTime.Today.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            OutfitsThisYear = await _db.Outfits
                .AsNoTracking()
                .CountAsync(o => o.UserId == userId && o.CreatedDate >= startOfYear);

            // Top 3 colors in the user's closet (most-used first; ties broken by name).
            var clothingForUser = _db.ClothingItems.AsNoTracking().Where(c => c.UserId == userId);
            var topColors = await clothingForUser
                .GroupBy(c => c.Color)
                .Select(g => new { Color = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.Color)
                .Take(3)
                .ToListAsync();

            TopColors.Clear();
            foreach (var c in topColors)
            {
                TopColors.Add(new ColorSwatch(NameToHex(c.Color)));
            }

            // Closet growth note: how many items were added this month.
            var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var itemsThisMonth = await clothingForUser.CountAsync(c => c.CreatedDate >= startOfMonth);
            ClosetGrowthNote = itemsThisMonth == 0
                ? "No new items this month"
                : $"+{itemsThisMonth} item{(itemsThisMonth == 1 ? string.Empty : "s")} this month";

            // Monthly shopping budget: load from IBudgetService (per-user,
            // stored in Preferences). OnPropertyChanged for MonthlyBudgetDisplay
            // so the pill text re-renders without a manual refresh.
            var storedBudget = _budgetService.GetMonthlyBudget(userId);
            MonthlyBudgetUsd = storedBudget;
            BudgetHelperText = ComposeBudgetHelper(storedBudget);
            OnPropertyChanged(nameof(MonthlyBudgetDisplay));

            // ---- Achievements (derived from real milestones, not hard-coded) ----
            var totalItems = await clothingForUser.CountAsync();
            var totalOutfits = await _db.Outfits.AsNoTracking().CountAsync(o => o.UserId == userId);
            var totalPlans = await _db.PlannedOutfits.AsNoTracking()
                .CountAsync(p => p.UserId == userId
                                 || (p.Outfit != null && p.Outfit.UserId == userId));
            var favorites = await clothingForUser.CountAsync(c => c.Favorite);

            Achievements.Clear();
            Achievements.Add(new Achievement("Fashion Explorer", totalItems >= 5));
            Achievements.Add(new Achievement("Closet Organizer", favorites >= 3));
            Achievements.Add(new Achievement("Trend Setter", totalOutfits >= 10));
            Achievements.Add(new Achievement("Planner Pro", totalPlans >= 1));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load profile.");
            ResetForSignedOut();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ResetForSignedOut()
    {
        UserName = string.Empty;
        ProfileImageBase64 = null;
        Email = string.Empty;
        Location = string.Empty;
        AgeGroup = string.Empty;
        BudgetLevelLabel = string.Empty;
        Gender = string.Empty;
        StyleBadge = string.Empty;
        StylePreferences.Clear();
        TopColors.Clear();
        Achievements.Clear();
        OutfitsThisYear = 0;
        ClosetGrowthNote = string.Empty;
        MonthlyBudgetUsd = 200m;
        IsEditingBudget = false;
        BudgetHelperText = "Drag the slider to set your monthly shopping allowance.";
        OnPropertyChanged(nameof(MonthlyBudgetDisplay));
    }

    /// <summary>
    /// Composes a short human-readable style badge from the user's selected
    /// style + budget tier. Empty when the user hasn't completed onboarding yet.
    /// </summary>
    private static string ComposeStyleBadge(string? stylePreference, BudgetLevel budgetLevel)
    {
        if (string.IsNullOrWhiteSpace(stylePreference))
        {
            return string.Empty;
        }

        // Take the first style token (e.g. "Business" from "Business, Minimalist").
        var firstStyle = stylePreference
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault() ?? stylePreference.Trim();

        var tier = budgetLevel switch
        {
            BudgetLevel.Low => "Smart Saver",
            BudgetLevel.Medium => "Style Curator",
            BudgetLevel.Premium => "Style Connoisseur",
            BudgetLevel.Luxury => "Luxury Stylist",
            _ => "Style Curator"
        };

        return $"{firstStyle} · {tier}";
    }

    /// <summary>
    /// Maps a clothing color name to a representative hex code for the
    /// "Top Colors" swatch grid. Falls back to a neutral grey for unknown
    /// labels so we never crash on a free-form color value.
    /// </summary>
    private static string NameToHex(string? colorName)
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

    // ---- Add-style popup state ----

    /// <summary>Drives the <c>SfPopup</c> open/closed state.</summary>
    [ObservableProperty] private bool _isAddStylePopupOpen;

    /// <summary>Catalogue of styles the user can pick from (mirrors <c>ClothingOptions.Styles</c>).</summary>
    public IReadOnlyList<string> AvailableStyles { get; } = WardrobeAI.Constants.ClothingOptions.Styles;

    /// <summary>Style highlighted inside the chip group (two-way bound).</summary>
    [ObservableProperty] private string? _newStyleSelection;

    /// <summary>Free-form fallback so the user can add a style we don't ship.</summary>
    [ObservableProperty] private string _newStyleCustom = string.Empty;

    /// <summary>Inline error shown at the top of the popup when validation fails.</summary>
    [ObservableProperty] private string? _newStyleValidationError;

    /// <summary>Opens the Add Style popup and clears any prior validation/error state.</summary>
    [RelayCommand]
    private void OpenAddStylePopup()
    {
        NewStyleValidationError = null;
        NewStyleSelection = null;
        NewStyleCustom = string.Empty;
        IsAddStylePopupOpen = true;
    }

    /// <summary>Closes the popup without applying changes.</summary>
    [RelayCommand]
    private void CloseAddStylePopup()
    {
        IsAddStylePopupOpen = false;
        NewStyleValidationError = null;
    }

    /// <summary>
    /// Adds the chosen (or custom) style to the user's <c>StylePreferences</c>,
    /// persists the merged set back to <c>User.StylePreference</c>, and closes
    /// the popup. Duplicates are silently ignored.
    /// </summary>
    [RelayCommand]
    private async Task SaveNewStyleAsync()
    {
        var newStyle = ResolveNewStyle();
        if (string.IsNullOrWhiteSpace(newStyle))
        {
            NewStyleValidationError = "Pick a style or type a custom one (max 30 chars).";
            return;
        }

        if (StylePreferences.Contains(newStyle!, StringComparer.OrdinalIgnoreCase))
        {
            NewStyleValidationError = $"\"{newStyle}\" is already on your profile.";
            return;
        }

        try
        {
            StylePreferences.Add(newStyle!);
            await PersistStylePreferencesAsync();
            IsAddStylePopupOpen = false;
            NewStyleValidationError = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save new style preference.");
            NewStyleValidationError = "Couldn't save that style — please try again.";
        }
    }

    /// <summary>
    /// Persists the user's style preferences after a chip has been removed
    /// via the <c>SfChipGroup</c>'s built-in close button. By the time this
    /// is called, the <c>SfChipGroup</c> has already removed the item from
    /// the bound <c>StylePreferences</c> collection and re-rendered the chip
    /// list, so this method only writes the trimmed value back to the DB.
    /// On a persist failure the chip is re-added so the UI matches the DB
    /// rather than the user thinking the save succeeded.
    /// </summary>
    public async Task OnStyleRemovedAsync(string? style)
    {
        if (string.IsNullOrWhiteSpace(style))
        {
            return;
        }

        // Defensive: the SfChipGroup's ItemsSource is StylePreferences, so it
        // has already removed the item. If for any reason the item is still
        // present (case/whitespace mismatch, etc.), remove it now.
        var match = StylePreferences.FirstOrDefault(
            s => string.Equals(s, style, StringComparison.OrdinalIgnoreCase));
        if (match is not null)
        {
            StylePreferences.Remove(match);
        }

        try
        {
            await PersistStylePreferencesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist style removal of '{Style}'.", style);
            // Roll the chip back so the UI matches the DB.
            if (match is not null && !StylePreferences.Contains(match, StringComparer.OrdinalIgnoreCase))
            {
                StylePreferences.Add(match);
            }
        }
    }

    /// <summary>
    /// Resolves which style to add from the popup: prefers the chip selection,
    /// falls back to a trimmed custom entry. Returns null/empty when nothing
    /// is filled in.
    /// </summary>
    private string? ResolveNewStyle()
    {
        if (!string.IsNullOrWhiteSpace(NewStyleSelection))
        {
            return NewStyleSelection.Trim();
        }

        if (!string.IsNullOrWhiteSpace(NewStyleCustom))
        {
            return NewStyleCustom.Trim();
        }

        return null;
    }

    /// <summary>
    /// Persists the current <c>StylePreferences</c> observable collection back
    /// into <c>User.StylePreference</c> (comma-separated) and saves the row.
    /// The user is loaded through the repository (so it is tracked by the
    /// shared <see cref="WardrobeDbContext"/>) and we just call
    /// <see cref="IUserRepository.SaveChangesAsync"/> rather than
    /// <c>UpdateAsync</c> — the entity is already tracked, so a redundant
    /// <c>Update(user)</c> would mark every column Modified (including
    /// the unchanged <c>ProfileImage</c> base64 blob) and balloon the
    /// generated UPDATE statement to a few hundred KB on every chip tap.
    /// </summary>
    private async Task PersistStylePreferencesAsync()
    {
        var userId = _userContext.GetCurrentUserId();
        if (userId <= 0)
        {
            return;
        }

        var user = await _userRepository.GetAsync(userId);
        if (user is null)
        {
            _logger.LogWarning("Cannot persist style preferences — user {UserId} not found.", userId);
            return;
        }

        // Column cap is 64 chars (WardrobeDbContext.OnModelCreating). If the
        // user has packed the field with many long styles, the joined string
        // could overflow — guard so we never throw a
        // SQLiteException: String or BLOB exceeds size limit on a chip tap.
        var joined = string.Join(", ", StylePreferences);
        if (joined.Length > 64)
        {
            _logger.LogWarning(
                "Style preferences string is {Length} chars; truncating to fit the 64-char column. " +
                "User should prune custom styles to stay within budget.",
                joined.Length);
            joined = joined[..64];
        }

        user.StylePreference = joined;
        await _userRepository.SaveChangesAsync();
    }


    /// <summary>
    /// Toggle the budget slider's edit mode. The pill flips to a slider so
    /// the user can drag; the edit icon flips back to a check / save icon.
    /// </summary>
    [RelayCommand]
    private void ToggleBudgetEdit()
    {
        IsEditingBudget = !IsEditingBudget;
        if (!IsEditingBudget)
        {
            // Save on collapse so the value is always persisted when the user
            // dismisses the slider.
            var userId = _userContext.GetCurrentUserId();
            if (userId > 0)
            {
                _budgetService.SetMonthlyBudget(userId, MonthlyBudgetUsd);
            }
        }
    }

    /// <summary>
    /// Saves the budget value (called from the "Save" button next to the
    /// slider). Also closes the edit affordance so the pill returns.
    /// </summary>
    [RelayCommand]
    private void SaveBudget()
    {
        var userId = _userContext.GetCurrentUserId();
        if (userId <= 0)
        {
            return;
        }

        _budgetService.SetMonthlyBudget(userId, MonthlyBudgetUsd);
        BudgetHelperText = ComposeBudgetHelper(MonthlyBudgetUsd);
        IsEditingBudget = false;
        OnPropertyChanged(nameof(MonthlyBudgetDisplay));
    }

    /// <summary>
    /// Called from the slider's <c>ValueChanged</c> event so the helper text
    /// stays in sync as the user drags. Cheap string composition — no DB hit.
    /// </summary>
    [RelayCommand]
    private void OnBudgetSliderChanged(double newValue)
    {
        MonthlyBudgetUsd = (decimal)newValue;
        BudgetHelperText = ComposeBudgetHelper(MonthlyBudgetUsd);
        OnPropertyChanged(nameof(MonthlyBudgetDisplay));
    }

    /// <summary>
    /// Composes the helper text under the slider based on the chosen monthly
    /// budget. Anchors on a "5 staples" rule of thumb so the user can reason
    /// about what their budget actually buys.
    /// </summary>
    private static string ComposeBudgetHelper(decimal budget)
    {
        if (budget <= 0m)
        {
            return "Set to $0 to opt out of value-for-money recommendations.";
        }

        // At a $40 average staple cost, a $200 budget buys ~5 staples/month.
        const decimal stapleCost = 40m;
        var staples = (int)Math.Round(budget / stapleCost);
        if (staples < 1)
        {
            return $"Save up — that's under one ${(int)stapleCost} staple this month.";
        }

        return staples == 1
            ? $"That buys about 1 quality staple around ${(int)stapleCost} this month."
            : $"That buys about {staples} quality staples around ${(int)stapleCost} each this month.";
    }

    public sealed record ColorSwatch(string Hex);
    public sealed record Achievement(string Name, bool Unlocked);
    public sealed record SettingItem(string Name, string Icon);

    /// <summary>
    /// <see cref="ITabRefreshable"/> entry point used by the
    /// <see cref="MainTabHostViewModel"/> when the <c>SfTabView</c> selection
    /// lands on (or re-enters) the Profile tab. Re-runs the same profile +
    /// analytics load the old Shell <c>TabBar</c> triggered via
    /// <c>OnAppearing</c>.
    /// </summary>
    public Task RefreshAsync() => LoadAsync();
}
