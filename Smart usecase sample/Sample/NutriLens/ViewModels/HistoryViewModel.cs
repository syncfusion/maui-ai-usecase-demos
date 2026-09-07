using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NutriLens.Helpers;
using NutriLens.Models;
using NutriLens.Services;

namespace NutriLens.ViewModels;

public partial class HistoryViewModel : ObservableObject
{
    // Unified source: sample seed records + saved AI-generated records.
    private readonly ICombinedScanHistory history;
    private readonly ISampleAnalysisDataService sampleAnalysisService;
    private IReadOnlyList<SavedScan> allScans = [];

    public ObservableCollection<HistoryItem> HistoryItems { get; } = [];

    public ObservableCollection<string> UnselectedFilters { get; } =
    [
        "All Scans",
        "Today",
        "Yesterday",
        "Last Week",
        "Last Month"
    ];

    [ObservableProperty]
    private string selectedFilter = "All Scans";

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string summaryText = "Showing 0 scans";

    // ----- Icons -----
    public string BiotechIcon => MaterialIcons.Biotech;
    public string PersonIcon => MaterialIcons.Person;
    public string SearchIcon => MaterialIcons.Search;
    public string HomeIcon => MaterialIcons.Home;
    public string HistoryIcon => MaterialIcons.History;
    public string ScannerIcon => MaterialIcons.DocumentScanner;
    public string TrendsIcon => MaterialIcons.ShowChart;
    public string ProfileIcon => MaterialIcons.PersonOutline;
    public string ChevronIcon => MaterialIcons.ChevronRight;
    public string DoneAllIcon => MaterialIcons.DoneAll;

    public HistoryViewModel()
        : this(
            Resolve<ICombinedScanHistory>()
                ?? new CombinedScanHistory(
                    Resolve<IScanHistoryStore>() ?? new JsonScanHistoryStore()),
            Resolve<ISampleAnalysisDataService>()
                ?? new SampleIngredientAnalysisService())
    {
    }

    public HistoryViewModel(
        ICombinedScanHistory history,
        ISampleAnalysisDataService sampleAnalysisService)
    {
        this.history = history
            ?? throw new ArgumentNullException(nameof(history));
        this.sampleAnalysisService = sampleAnalysisService
            ?? throw new ArgumentNullException(nameof(sampleAnalysisService));
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnSelectedFilterChanged(string value) => ApplyFilter();

    /// <summary>Reloads combined history (seeds + saved). Call on every OnAppearing.</summary>
    public async Task LoadAsync()
    {
        allScans = [.. (await history.GetCombinedAsync())];
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var query = SearchText?.Trim() ?? string.Empty;
        var now = DateTime.Now;

        IEnumerable<SavedScan> filtered = allScans;

        filtered = SelectedFilter switch
        {
            "Today" => filtered.Where(scan =>
                scan.SavedAtUtc.ToLocalTime().Date == now.Date),

            "Yesterday" => filtered.Where(scan =>
                scan.SavedAtUtc.ToLocalTime().Date == now.Date.AddDays(-1)),

            "Last Week" => filtered.Where(scan =>
                scan.SavedAtUtc.ToLocalTime() >= now.AddDays(-7)),

            "Last Month" => filtered.Where(scan =>
                scan.SavedAtUtc.ToLocalTime() >= now.AddMonths(-1)),

            _ => filtered
        };

        if (query.Length > 0)
        {
            filtered = filtered.Where(scan =>
                scan.Result.ProductName.Contains(
                    query,
                    StringComparison.OrdinalIgnoreCase));
        }

        var ordered = filtered
            .OrderByDescending(scan => scan.SavedAtUtc)
            .ToList();

        HistoryItems.Clear();

        foreach (var scan in ordered)
        {
            HistoryItems.Add(ToHistoryItem(scan));
        }

        SummaryText = HistoryItems.Count == 1
            ? "Showing 1 scan"
            : $"Showing {HistoryItems.Count} scans";
    }

    /// <summary>
    /// Unified tap handler for HistoryPage cards. Same routing logic as the
    /// Dashboard: sample products use the static analysis service; other items
    /// resolve to their saved IngredientAnalysisResult.
    /// </summary>
    [RelayCommand]
    private async Task RecentScanTapped(HistoryItem? item)
    {
        if (item is null || string.IsNullOrWhiteSpace(item.ProductName))
            return; // Prevents null-reference on malformed taps.

        // Sample product (Oats/Yogurt/Chips)? → static data path (no API).
        if (sampleAnalysisService.IsSampleProduct(item.ProductName))
        {
            var sampleResult = sampleAnalysisService.TryGet(item.ProductName);
            if (sampleResult is not null)
            {
                await AppNavigator.GoAnalyzeIngredientsAsync(sampleResult);
                return;
            }
        }

        // Non-sample product — reuse the saved analysis result.
        var match = allScans.FirstOrDefault(s =>
            string.Equals(s.Result?.ProductName, item.ProductName,
                StringComparison.OrdinalIgnoreCase));

        if (match?.Result is not null)
        {
            await AppNavigator.GoAnalyzeIngredientsAsync(match.Result);
            return;
        }

        await AppNavigator.ShowAlertAsync(
            "Analysis Unavailable",
            $"No analysis data is available for '{item.ProductName}'.",
            "OK");
    }

    private static HistoryItem ToHistoryItem(SavedScan scan)
    {
        var score = Math.Clamp(scan.Result.Score, 0, 100);

        var (scoreColor, statusIcon) = score switch
        {
            >= 80 => ("#10b981", MaterialIcons.Verified),  // Excellent
            >= 50 => ("#fea619", MaterialIcons.Verified),  // Good
            _ => ("#ff7a73", MaterialIcons.Warning),   // Moderate 
        };

        return new HistoryItem
        {
            Image = string.IsNullOrWhiteSpace(scan.ImagePath)
                ? "yogurt.webp"
                : scan.ImagePath,

            ProductName = scan.Result.ProductName,
             
            Score = $"{score}",

            ScanDate = FormatScanDate(scan.SavedAtUtc),
            ScoreColor = scoreColor,
            StatusIcon = statusIcon
        };
    }

    private static string FormatScanDate(DateTime savedAtUtc)
    {
        var local = savedAtUtc.ToLocalTime();
        if (local.Date == DateTime.Now.Date)
            return $"Today, {local:hh:mm tt}";

        if (local.Date == DateTime.Now.Date.AddDays(-1))
            return $"Yesterday, {local:hh:mm tt}";

        var age = DateTime.Now.Date - local.Date;

        if (age.TotalDays < 7)
            return $"{local:ddd}, {local:hh:mm tt}";

        return local.ToString("MMM d, hh:mm tt");
    }

    private static T? Resolve<T>()
        where T : class
    {
        return Application.Current?
            .Handler?
            .MauiContext?
            .Services
            .GetService<T>();
    }
}