using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NutriLens.Helpers;
using NutriLens.Models;
using NutriLens.Services;

namespace NutriLens.ViewModels;

public partial class NutriLensDashboardViewModel : ObservableObject
{
    private readonly IDailyInsightGenerator dailyInsightGenerator;
    private readonly ICombinedScanHistory combinedHistory;
    private readonly ISampleAnalysisDataService sampleAnalysisService;
    private IReadOnlyList<SavedScan> allScans = []; 
     
    public ObservableCollection<RecentScanItem> RecentScans { get; } = [];

    [ObservableProperty]
    private string greeting = string.Empty;

    [ObservableProperty]
    private string dailyInsight =
        "Analyzing your recent scans…";

    public NutriLensDashboardViewModel()
        : this(
            Resolve<IDailyInsightGenerator>()
                ?? new DailyInsightGenerator(
                    new AzureOpenAIChatService(),
                    new PreferencesDailyInsightCacheStore(),
                    Resolve<IScanHistoryStore>() ?? new JsonScanHistoryStore()),
            Resolve<ICombinedScanHistory>()
                ?? new CombinedScanHistory(
                    Resolve<IScanHistoryStore>() ?? new JsonScanHistoryStore()),
            Resolve<ISampleAnalysisDataService>()
                ?? new SampleIngredientAnalysisService())
    {
    }

    public NutriLensDashboardViewModel(
        IDailyInsightGenerator dailyInsightGenerator,
        ICombinedScanHistory combinedHistory,
        ISampleAnalysisDataService sampleAnalysisService)
    {
        this.dailyInsightGenerator = dailyInsightGenerator
            ?? throw new ArgumentNullException(nameof(dailyInsightGenerator));
        this.combinedHistory = combinedHistory
            ?? throw new ArgumentNullException(nameof(combinedHistory));
        this.sampleAnalysisService = sampleAnalysisService
            ?? throw new ArgumentNullException(nameof(sampleAnalysisService));

        UpdateGreeting();
        _ = LoadDailyInsightAsync();
        _ = LoadRecentScansAsync();
    }

    // ----- Icons -----
    public string BiotechIcon => MaterialIcons.Biotech;
    public string PersonIcon => MaterialIcons.Person;
    public string ScannerIcon => MaterialIcons.DocumentScanner;
    public string TipsIcon => MaterialIcons.TipsAndUpdates;
    public string HomeIcon => MaterialIcons.Home;
    public string HistoryIcon => MaterialIcons.History;
    public string TrendsIcon => MaterialIcons.ShowChart;

    // ----- Navigation commands -----
    [RelayCommand]
    private Task Scan() => AppNavigator.GoScanAsync();

    [RelayCommand]
    private Task OpenHistory() => AppNavigator.GoHistoryAsync();

    [RelayCommand]
    private Task OpenProfile() => AppNavigator.GoProfileAsync();

    [RelayCommand]
    private Task OpenTrends() => AppNavigator.GoTrendAsync();

    [RelayCommand]
    private Task ViewAll() => AppNavigator.GoHistoryAsync();

    /// <summary>
    /// Unified tap handler for any RecentScanItem on the Dashboard.
    /// Sample products (Oats/Yogurt/Chips) resolve through
    /// <see cref="ISampleAnalysisDataService"/> — the AI service is NEVER
    /// invoked for them. Non-sample items fall back to the stored
    /// IngredientAnalysisResult on the matching saved scan.
    /// </summary>
    [RelayCommand]
    private async Task RecentScanTapped(RecentScanItem? item)
    {
        if (item is null || string.IsNullOrWhiteSpace(item.ProductName))
            return; // Prevents null-reference on malformed taps.

        // Step 1: predefined sample product? → static data path (no API).
        if (sampleAnalysisService.IsSampleProduct(item.ProductName))
        {
            var sampleResult = sampleAnalysisService.TryGet(item.ProductName);
            if (sampleResult is not null)
            {
                await AppNavigator.GoAnalyzeIngredientsAsync(sampleResult);
                return;
            }
        }

        // Step 2: non-sample product — reuse the saved analysis result.
        try
        {
            allScans = [.. (await combinedHistory.GetCombinedAsync())];

            var match = allScans.FirstOrDefault(s =>
                string.Equals(s.Result?.ProductName, item.ProductName,
                    StringComparison.OrdinalIgnoreCase));

            if (match?.Result is not null)
            {
                await AppNavigator.GoAnalyzeIngredientsAsync(match.Result);
                return;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Dashboard] RecentScanTapped lookup failed: {ex}");
        }

        await AppNavigator.ShowAlertAsync(
            "Analysis Unavailable",
            $"No analysis data is available for '{item.ProductName}'.",
            "OK");
    }

    // ----- Data loading -----
    private async Task LoadRecentScansAsync()
    {
        try
        {
            allScans = [.. (await combinedHistory.GetCombinedAsync())];

            var items = allScans
                .Take(3)
                .Select(ToRecentScanItem)
                .ToList();

            RecentScans.Clear();
            foreach (var scanItem in items)
            {
                RecentScans.Add(scanItem);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Dashboard] LoadRecentScansAsync failed: {ex}");
        }
    }

    private async Task LoadDailyInsightAsync()
    {
        try
        {
            DailyInsight = await dailyInsightGenerator.GetTodayInsightAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Dashboard] Daily insight failed: {ex}");
            DailyInsight =
                "Focus on high-fiber whole foods today and pair carbohydrates " +
                "with protein for steadier blood sugar.";
        }
    }

    private void UpdateGreeting()
    {
        var hour = DateTime.Now.Hour;

        Greeting = hour switch
        {
            < 12 => "Good Morning, Alex",
            < 17 => "Good Afternoon, Alex",
            _ => "Good Evening, Alex"
        };
    }

    private static RecentScanItem ToRecentScanItem(SavedScan scan)
    {
        var score = Math.Clamp(scan.Result.Score, 0, 100);

        var (rating, ratingColor) = score switch
        {
            >= 70 => ("Excellent", "#047857"),
            >= 40 => ("Moderate", "#D99024"),
            _ => ("Poor", "#DC2626")
        };

        return new RecentScanItem
        {
            ProductName = scan.Result.ProductName,
            ImagePath = string.IsNullOrWhiteSpace(scan.ImagePath)
                ? "yogurt.webp"
                : scan.ImagePath,
            Score = score,
            Rating = rating,
            RatingColor = ratingColor,
            ScanTime = FormatScanDate(scan.SavedAtUtc)
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