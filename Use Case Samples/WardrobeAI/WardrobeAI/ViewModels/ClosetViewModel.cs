using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WardrobeAI.Constants;
using WardrobeAI.Models;
using WardrobeAI.Repositories;
using WardrobeAI.Services;

namespace WardrobeAI.ViewModels;

/// <summary>
/// Closet management view model (SDS Module 3). Mirrors /designs/closet.html:
/// search bar, filter chips, clothing grid, and an AI analysis detail card.
///
/// Loads the signed-in user's <see cref="ClothingItem"/> rows from the database
/// (no hard-coded seed data) and exposes an Add-Clothing popup that captures
/// an image (gallery or camera) and the seven attribute chips (Category,
/// Occasion, Style, Color, Material, Season, Pattern) per
/// <see cref="ClothingOptions"/>.
/// </summary>
public partial class ClosetViewModel : ObservableObject, ITabRefreshable
{
    private readonly IClothingItemRepository _repository;
    private readonly IUserContext _userContext;
    private readonly ILogger<ClosetViewModel> _logger;

    public ClosetViewModel(
        IClothingItemRepository repository,
        IUserContext userContext,
        ILogger<ClosetViewModel> logger)
    {
        _repository = repository;
        _userContext = userContext;
        _logger = logger;

        // Real, DB-bound clothing items. Populated by LoadClothingAsync().
        // MUST be initialised BEFORE the call to RebuildFiltersFromCloset() below
        // — that helper enumerates `Clothing` to derive the category chip list,
        // and at this point in the constructor the field is otherwise `null`,
        // which makes the LINQ call throw
        //   ArgumentNullException: Value cannot be null. (Parameter 'source')
        // at app startup the moment the VM is constructed by the DI container.
        Clothing = new ObservableCollection<ClothingItem>();
        Clothing.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasClothing));

        // Filter chips are derived from the categories actually present in the
        // user's closet (plus a synthetic "All"). The collection is rebuilt by
        // RebuildFiltersFromCloset() after every load/save so the chip row
        // reflects what the closet contains.
        Filters = new ObservableCollection<FilterChip>();
        RebuildFiltersFromCloset(preserveSelectionLabel: null);

        // Mirror the default selection onto the SfChipGroup binding so the
        // first chip renders as selected on initial load.
        SelectedFilter = Filters.FirstOrDefault(f => f.IsSelected) ?? Filters[0];

        IsAnalyzing = false;
        IsLoading = false;

        // Add-Clothing popup default state.
        IsAddClothingPopupOpen = false;
        NewItemName = string.Empty;
        NewItemImageBase64 = null;
        NewItemValidationError = null;
        IsSavingNewItem = false;
        NewItemCategory = null;
        NewItemOccasion = null;
        NewItemStyle = null;
        NewItemColor = null;
        NewItemMaterial = null;
        NewItemSeason = null;
        NewItemPattern = null;
    }

    // ---- Bound option lists for the Add-Clothing popup chip groups ----
    public IReadOnlyList<string> CategoryOptions => ClothingOptions.Categories;
    public IReadOnlyList<string> OccasionOptions => ClothingOptions.Occasions;
    public IReadOnlyList<string> StyleOptions => ClothingOptions.Styles;
    public IReadOnlyList<string> ColorOptions => ClothingOptions.Colors;
    public IReadOnlyList<string> MaterialOptions => ClothingOptions.Materials;
    public IReadOnlyList<string> SeasonOptions => ClothingOptions.Seasons;
    public IReadOnlyList<string> PatternOptions => ClothingOptions.Patterns;

    // ---- Top-level filter row ----
    public ObservableCollection<FilterChip> Filters { get; }
    public ObservableCollection<ClothingItem> Clothing { get; }

    /// <summary>
    /// Rebuilds the closet-category <see cref="FilterChip"/> list from the
    /// distinct <see cref="ClothingItem.Category"/> values currently in the
    /// closet. A synthetic "All" chip is always inserted first so the user can
    /// clear the category filter. Empty/whitespace categories are ignored.
    /// When <paramref name="preserveSelectionLabel"/> is non-null and still
    /// exists in the new chip list, that chip is re-selected; otherwise "All"
    /// is selected.
    /// </summary>
    private void RebuildFiltersFromCloset(string? preserveSelectionLabel)
    {
        FilterChip? previousSelected = null;
        foreach (var f in Filters)
        {
            if (f.IsSelected)
            {
                previousSelected = f;
                break;
            }
        }

        Filters.Clear();

        // Always present "All" as the default view.
        var allChip = new FilterChip("All", isSelected: false);
        Filters.Add(allChip);

        // Distinct non-empty categories drawn from the actual closet contents,
        // ordered alphabetically for a stable, predictable chip layout.
        var distinctCategories = Clothing
            .Where(c => !string.IsNullOrWhiteSpace(c.Category))
            .Select(c => c.Category.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c, StringComparer.OrdinalIgnoreCase);

        foreach (var category in distinctCategories)
        {
            Filters.Add(new FilterChip(category, isSelected: false));
        }

        // Restore / default selection.
        string? targetLabel = preserveSelectionLabel
            ?? (previousSelected is null ? "All" : previousSelected.Label);

        FilterChip? target = null;
        if (!string.IsNullOrEmpty(targetLabel))
        {
            target = Filters.FirstOrDefault(f =>
                string.Equals(f.Label, targetLabel, StringComparison.OrdinalIgnoreCase));
        }

        if (target is null)
        {
            target = allChip;
        }

        foreach (var f in Filters)
        {
            f.IsSelected = ReferenceEquals(f, target);
        }

        SelectedFilter = target;
        OnPropertyChanged(nameof(FilteredClothing));
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilteredClothing))]
    private string _searchQuery = string.Empty;

    /// <summary>
    /// Applies the search filter and the active top-level filter chip to the
    /// backing <see cref="Clothing"/> collection. The "All" filter chip disables
    /// category-based filtering; any other chip restricts the grid to that
    /// category label.
    /// </summary>
    public IEnumerable<ClothingItem> FilteredClothing
    {
        get
        {
            var activeFilter = Filters.FirstOrDefault(f => f.IsSelected);
            var categoryFilter = activeFilter is null || string.Equals(activeFilter.Label, "All", StringComparison.Ordinal)
                ? null
                : activeFilter.Label;

            var query = Clothing.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(categoryFilter))
            {
                query = query.Where(c => string.Equals(c.Category, categoryFilter, StringComparison.Ordinal));
            }

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                query = query.Where(c =>
                    c.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)
                    || c.Category.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)
                    || c.Color.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
            }

            return query.ToList();
        }
    }

    public bool HasClothing => Clothing.Count > 0;
    public string EmptyStateMessage => "Your closet is empty. Tap + to add your first item.";

    // ---- Responsive card sizing (closet grid) ----
    // Updated by the page on size changes so each card grows/shrinks with
    // the screen while keeping a pleasant number of columns. The card height
    // is derived from the width so every card stays the same size, regardless
    // of whether an item has both the category and color chips visible.
    [ObservableProperty] private double _closetCardWidth = 170;
    [ObservableProperty] private double _closetCardHeight = 230;
    [ObservableProperty] private double _closetImageHeight = 180;

    // Keep ClosetCardHeight and ClosetImageHeight in lock-step with the width
    // so every card in the grid is the same size.
    partial void OnClosetCardWidthChanged(double value)
    {
        // Fixed footer (name label + category/color chip row ≈ 50 px) plus an
        // image area that's ~1.05x the card width for a balanced portrait look.
        var imageHeight = Math.Round(value * 1.05, 0);
        var totalHeight = Math.Round(imageHeight + 50, 0);
        if (Math.Abs(ClosetImageHeight - imageHeight) > 0.5)
        {
            ClosetImageHeight = imageHeight;
        }
        if (Math.Abs(ClosetCardHeight - totalHeight) > 0.5)
        {
            ClosetCardHeight = totalHeight;
        }
    }

    public void UpdateClosetCardSizes(double containerWidth)
    {
        if (DeviceInfo.Idiom == DeviceIdiom.Desktop)
        {
            if (containerWidth <= 0)
            {
                return;
            }

            // Each card uses Margin="4" (left + right => 8 px of horizontal slack).
            const double marginPerCardSides = 8;

            // Cap cards so they never grow comically wide on huge windows.
            // Floor at minCardWidth so very narrow windows still render cards.
            const double minCardWidth = 160;
            const double maxCardWidth = 220;

            // Aim for the standard 4-up grid like the design when there is room.
            int bestCardsPerRow = 1;
            double bestWidth = 0;

            // Pick the highest column count whose resulting card width lands in
            // the comfort range — this fills the row edge-to-edge while keeping
            // each card readable.
            for (int cardsPerRow = 3; cardsPerRow >= 1; cardsPerRow--)
            {
                var availableForCards = containerWidth - (cardsPerRow * marginPerCardSides);
                if (availableForCards <= 0)
                {
                    continue;
                }

                var width = availableForCards / cardsPerRow;
                if (width < minCardWidth)
                {
                    continue;
                }

                if (width > maxCardWidth)
                {
                    continue;
                }

                bestCardsPerRow = cardsPerRow;
                bestWidth = width;
                break; // highest valid column count wins (more cards per row)
            }

            // Fallback when no column count yields an in-range width.
            if (bestWidth <= 0)
            {
                var rawWidth = containerWidth - marginPerCardSides; // single-column
                bestWidth = Math.Clamp(rawWidth, minCardWidth, maxCardWidth);
            }

            ClosetCardWidth = Math.Round(bestWidth, 0);
            // ClosetCardHeight / ClosetImageHeight follow automatically via the
            // OnClosetCardWidthChanged partial method so every card stays the
            // same size, regardless of which optional chips are present.
        }
    }

    // ---- AI Analysis card (existing behavior, retained) ----
    [ObservableProperty] private bool _isAnalyzing;
    [ObservableProperty] private string _analysisTitle = "Navy Oxford";
    [ObservableProperty] private string _analysisStyle = "Business Casual";
    [ObservableProperty] private string _analysisSeason = "All Seasons";
    [ObservableProperty] private int _analysisMatchPotential = 92;

    // ---- Add-Clothing popup state ----
    [ObservableProperty] private bool _isAddClothingPopupOpen;
    [ObservableProperty] private string _newItemName = string.Empty;
    [ObservableProperty] private string? _newItemImageBase64;
    [ObservableProperty] private string? _newItemValidationError;
    [ObservableProperty] private bool _isSavingNewItem;
    [ObservableProperty] private bool _isLoading;

    // ---- Add-Clothing popup chip selections ----
    [ObservableProperty] private string? _newItemCategory;
    [ObservableProperty] private string? _newItemOccasion;
    [ObservableProperty] private string? _newItemStyle;
    [ObservableProperty] private string? _newItemColor;
    [ObservableProperty] private string? _newItemMaterial;
    [ObservableProperty] private string? _newItemSeason;
    [ObservableProperty] private string? _newItemPattern;

    // ---- Top-of-page filter chip selection (SfChipGroup, Choice type) ----
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilteredClothing))]
    private FilterChip? _selectedFilter;

    /// <summary>
    /// Mirrors <see cref="SelectedFilter"/> onto every <see cref="FilterChip"/>'s
    /// <c>IsSelected</c> flag and refreshes the filtered grid. Driven by the
    /// SfChipGroup two-way <c>SelectedItem</c> binding.
    /// </summary>
    partial void OnSelectedFilterChanged(FilterChip? value)
    {
        foreach (var f in Filters)
        {
            f.IsSelected = ReferenceEquals(f, value);
        }
        OnPropertyChanged(nameof(FilteredClothing));
    }

    // ---- Commands ----
    [RelayCommand]
    private void Search() => OnPropertyChanged(nameof(FilteredClothing));

    /// <summary>
    /// Re-derives <see cref="SelectedFilter"/> from the current
    /// <c>IsSelected</c> state of the <see cref="Filters"/> collection. Kept
    /// as a command so it can be invoked from the SfChipGroup's
    /// <c>SelectionChanged</c> event in XAML as a safety net alongside the
    /// two-way <c>SelectedItem</c> binding.
    /// </summary>
    [RelayCommand]
    private void SelectFilter()
    {
        // The SfChipGroup's two-way SelectedItem binding keeps SelectedFilter
        // (and therefore every FilterChip.IsSelected flag) in sync via the
        // OnSelectedFilterChanged partial above. This command is wired to the
        // SelectionChanged event as a safety net so the grid is always
        // refreshed after a user picks a chip.
        OnPropertyChanged(nameof(FilteredClothing));
    }

    [RelayCommand]
    private async Task AnalyzeAsync(ClothingItem card)
    {
        IsAnalyzing = true;
        AnalysisTitle = card.Name;
        AnalysisStyle = string.IsNullOrWhiteSpace(card.Style) ? "Casual" : card.Style;
        AnalysisSeason = string.IsNullOrWhiteSpace(card.Season) ? "All Seasons" : card.Season;
        // Lightweight heuristic until the real AI service (P3) lands: a mix of
        // the color/pattern richness drives the match potential score.
        AnalysisMatchPotential = ComputePlaceholderMatchScore(card);
        await Task.Delay(300);
        IsAnalyzing = false;
    }

    private static int ComputePlaceholderMatchScore(ClothingItem item)
    {
        // Bias toward neutral classics (Black/White/Grey/Beige/Blue) + Solid
        // and bump staples. Range 70..96.
        var score = 82;
        var neutralColors = new[] { "Black", "White", "Grey", "Beige", "Blue" };
        if (neutralColors.Contains(item.Color, StringComparer.OrdinalIgnoreCase))
        {
            score += 6;
        }
        if (string.Equals(item.Pattern, "Solid", StringComparison.OrdinalIgnoreCase))
        {
            score += 4;
        }
        if (item.Category is "T-Shirts" or "Shirts" or "Jeans" or "Trousers" or "Blazers")
        {
            score += 4;
        }
        return Math.Clamp(score, 70, 96);
    }

    [RelayCommand]
    private void CloseAnalysis() => IsAnalyzing = false;

    /// <summary>Loads the signed-in user's clothing items from the database.</summary>
    [RelayCommand]
    public async Task LoadClothingAsync()
    {
        if (IsLoading)
        {
            return;
        }

        try
        {
            IsLoading = true;
            var userId = _userContext.GetCurrentUserId();
            var items = await _repository.GetAllForUserAsync(userId);

            Clothing.Clear();
            foreach (var item in items)
            {
                Clothing.Add(item);
            }
            // Re-derive filter chips from the actual closet contents so the
            // SfChipGroup only shows categories the user owns.
            RebuildFiltersFromCloset(preserveSelectionLabel: SelectedFilter?.Label);
            OnPropertyChanged(nameof(HasClothing));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load clothing items for the closet.");
            Clothing.Clear();
            RebuildFiltersFromCloset(preserveSelectionLabel: null);
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ---- Add-Clothing popup flow ----
    [RelayCommand]
    private void OpenAddClothingPopup()
    {
        ResetNewItemForm();
        IsAddClothingPopupOpen = true;
    }

    [RelayCommand]
    private void CloseAddClothingPopup()
    {
        IsAddClothingPopupOpen = false;
    }

    [RelayCommand]
    private async Task PickFromGalleryAsync()
    {
        try
        {
#pragma warning disable CS0618 // MediaPicker.PickPhotoAsync is the single-photo API; PickPhotosAsync allows multiple. We deliberately pick one item.
            var photo = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Select a clothing photo"
            });
#pragma warning restore CS0618
            await ApplyPickedPhotoAsync(photo);
        }
        catch (Exception ex)
        {
            // PickPhotoAsync throws OperationCanceledException when the user
            // dismisses the picker — that's not an error worth surfacing.
            if (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Failed to pick photo from gallery.");
                NewItemValidationError = "Couldn't open the photo library. Please try again.";
            }
        }
    }

    [RelayCommand]
    private async Task CaptureFromCameraAsync()
    {
        try
        {
            if (!MediaPicker.IsCaptureSupported)
            {
                NewItemValidationError = "Camera capture is not supported on this device.";
                return;
            }

            var photo = await MediaPicker.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = "Capture a clothing photo"
            });
            await ApplyPickedPhotoAsync(photo);
        }
        catch (Exception ex)
        {
            if (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Failed to capture photo from camera.");
                NewItemValidationError = "Couldn't capture a photo. Please try again.";
            }
        }
    }

    private async Task ApplyPickedPhotoAsync(FileResult? photo)
    {
        if (photo is null)
        {
            return;
        }

        // Cap the captured image at ~3 MB to keep the base64 column reasonable.
        // 3 MB ≈ 4 MB after base64, still small enough for SQLite TEXT columns.
        const long maxBytes = 3L * 1024 * 1024;
        await using var source = await photo.OpenReadAsync();
        using var ms = new MemoryStream();
        await source.CopyToAsync(ms);
        var bytes = ms.ToArray();
        if (bytes.Length > maxBytes)
        {
            NewItemValidationError = "That photo is too large (max 3 MB). Please pick a smaller image.";
            return;
        }

        NewItemImageBase64 = Convert.ToBase64String(bytes);
        // Auto-fill the name placeholder if the user hasn't typed anything yet.
        if (string.IsNullOrWhiteSpace(NewItemName))
        {
            NewItemName = BuildSuggestedName();
        }
        NewItemValidationError = null;
    }

    private string BuildSuggestedName()
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(NewItemColor))
        {
            parts.Add(NewItemColor);
        }
        if (!string.IsNullOrWhiteSpace(NewItemCategory))
        {
            parts.Add(NewItemCategory);
        }
        return parts.Count > 0 ? string.Join(' ', parts) : "New Item";
    }

    [RelayCommand]
    private async Task SaveNewClothingAsync()
    {
        if (IsSavingNewItem)
        {
            return;
        }
        var name = (NewItemName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            NewItemValidationError = "Please give this item a name.";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewItemCategory))
        {
            NewItemValidationError = "Please pick a category.";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewItemColor))
        {
            NewItemValidationError = "Please pick a color.";
            return;
        }

        try
        {
            IsSavingNewItem = true;
            NewItemValidationError = null;

            await _userContext.EnsureUserAsync();
            var userId = _userContext.GetCurrentUserId();
            if (userId <= 0)
            {
                throw new InvalidOperationException("The current user could not be initialized.");
            }

            var item = new ClothingItem
            {
                UserId = userId,
                Name = name,
                Category = NewItemCategory!,
                Color = NewItemColor!,
                Pattern = string.IsNullOrWhiteSpace(NewItemPattern) ? "Solid" : NewItemPattern!,
                Fabric = string.IsNullOrWhiteSpace(NewItemMaterial) ? "Cotton" : NewItemMaterial!,
                Season = string.IsNullOrWhiteSpace(NewItemSeason) ? "All Season" : NewItemSeason!,
                Style = string.IsNullOrWhiteSpace(NewItemStyle) ? "Casual" : NewItemStyle!,
                Occasion = string.IsNullOrWhiteSpace(NewItemOccasion) ? "Casual" : NewItemOccasion!,
                ImageUrl = NewItemImageBase64,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
            };

            var saved = await _repository.AddAsync(item);
            ClothingItemEvents.RaiseItemAdded(userId);
            // Insert at the top so the user sees it immediately (the underlying
            // query is ordered by CreatedDate desc).
            Clothing.Insert(0, saved);
            // Refresh chip group to include the newly-added category if it's
            // the first of its kind in the closet.
            RebuildFiltersFromCloset(preserveSelectionLabel: SelectedFilter?.Label);
            // Belt-and-braces: RebuildFiltersFromCloset already raises
            // PropertyChanged for FilteredClothing, but mobile idiom
            // animation/close work can race the CollectionChanged event and
            // leave the grid showing the stale filtered list. Re-raise
            // explicitly and route through MainThread so the notification
            // lands after SfPopup has begun its close animation.
            OnPropertyChanged(nameof(HasClothing));
            OnPropertyChanged(nameof(FilteredClothing));

            IsAddClothingPopupOpen = false;
            ResetNewItemForm();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save new clothing item.");
            NewItemValidationError = "Couldn't save the item. Please try again.";
        }
        finally
        {
            IsSavingNewItem = false;
        }
    }

    /// <summary>
    /// Clears the Add-Clothing popup back to its pristine state. Called when
    /// the popup opens and after a successful save.
    /// </summary>
    private void ResetNewItemForm()
    {
        NewItemName = string.Empty;
        NewItemImageBase64 = null;
        NewItemValidationError = null;
        IsSavingNewItem = false;
        NewItemCategory = null;
        NewItemOccasion = null;
        NewItemStyle = null;
        NewItemColor = null;
        NewItemMaterial = null;
        NewItemSeason = null;
        NewItemPattern = null;
    }

    // ---- View-model DTOs ----
    public partial class FilterChip : ObservableObject
    {
        public FilterChip(string label, bool isSelected)
        {
            _label = label;
            _isSelected = isSelected;
        }

        [ObservableProperty] private string _label = string.Empty;

        [ObservableProperty] private bool _isSelected;
    }

    /// <summary>
    /// <see cref="ITabRefreshable"/> entry point used by the
    /// <see cref="MainTabHostViewModel"/> when the <c>SfTabView</c> selection
    /// lands on (or re-enters) the Closet tab. Re-runs the same DB query the
    /// old Shell <c>TabBar</c> triggered via <c>OnAppearing</c>.
    /// </summary>
    public Task RefreshAsync() => LoadClothingAsync();
}
