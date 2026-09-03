using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriLens.Helpers;
using NutriLens.Models;
using NutriLens.Services;

namespace NutriLens.ViewModels;

public partial class NutriLensDashboardViewModel : ObservableObject
{
    private readonly IDailyInsightGenerator dailyInsightGenerator;
    private readonly IScanHistoryStore scanStore;

    [ObservableProperty]
    private string userName = "Alex";

    [ObservableProperty]
    private double sugarReductionPercentage = 0.70;

    // Keep this exact property name so the existing XAML binding works unchanged.
    [ObservableProperty]
    private string dailyInsight =
        "Loading today’s nutrition insight...";

    public ObservableCollection<RecentScanItem> RecentScans { get; } = new();

    public string BiotechIcon => MaterialIcons.Biotech;
    public string ScannerIcon => MaterialIcons.DocumentScanner;
    public string HomeIcon => MaterialIcons.Home;
    public string HistoryIcon => MaterialIcons.History;
    public string TrendsIcon => MaterialIcons.ShowChart;
    public string PersonIcon => MaterialIcons.Person;
    public string TipsIcon => MaterialIcons.TipsAndUpdates;
    public string WaterDropIcon => MaterialIcons.WaterDrop;
    public string ChevronIcon => MaterialIcons.ChevronRight;

    public NutriLensDashboardViewModel()
        : this(
            Resolve<IDailyInsightGenerator>()
                ?? new DailyInsightGenerator(
                    new AzureOpenAIChatService(),
                    new PreferencesDailyInsightCacheStore(),
                    Resolve<IScanHistoryStore>() ?? new JsonScanHistoryStore()),
            Resolve<IScanHistoryStore>() ?? new JsonScanHistoryStore())
    {
    }

    public NutriLensDashboardViewModel(
        IDailyInsightGenerator dailyInsightGenerator,
        IScanHistoryStore scanStore)
    {
        this.dailyInsightGenerator = dailyInsightGenerator
            ?? throw new ArgumentNullException(nameof(dailyInsightGenerator));

        this.scanStore = scanStore
            ?? throw new ArgumentNullException(nameof(scanStore));
        UpdateGreeting();
        // Generate once per day using an AI service + local cache.
        LoadDailyInsightAsync();

        // Load saved scans from the store. Never fire-and-forget a naked
        // task from the constructor — exceptions get lost. LoadRecentScansAsync
        // handles its own errors internally.
        _ = LoadRecentScansAsync();
    }

    private async Task LoadRecentScansAsync()
    {
        try
        {
            // FIX: null-guard the store and null-guard deserialized rows —
            // a corrupt/partial scan_history.json could produce entries with
            // a null Result, which previously threw NullReferenceException.
            if (scanStore is not null)
            {
                var scans = (await scanStore.GetAllAsync())
                    .Where(s => s is not null && s.Result is not null)
                    .OrderByDescending(s => s.SavedAtUtc)
                    .Take(3);

                foreach (var scan in scans)
                {
                    RecentScans.Add(ToRecentScanItem(scan));
                }
            }
        }
        catch (Exception ex)
        {
            // Fire-and-forget from the ctor — must never surface as an
            // unobserved task exception.
            Debug.WriteLine($"[Dashboard] Failed to load recent scans: {ex}");
        }

        // The three showcase items must always be present.
        // Newly saved scans were inserted above, so they appear on top.
        AddShowcaseScans();
    }
    [ObservableProperty]
    private string greeting = string.Empty;

    private void UpdateGreeting()
    {
        var hour = DateTime.Now.Hour;
        var greetingText = hour switch
        {
            >= 5 and < 12 => "Good Morning, Alex",
            >= 12 and < 17 => "Good Afternoon, Alex",
            >= 17 and < 21 => "Good Evening, Alex",
            _ => "Good Night, Alex"
        };

        Greeting = $"{greetingText}";
    }
    private static RecentScanItem ToRecentScanItem(SavedScan scan)
    {
        var score = scan.Result.Score;

        return new RecentScanItem
        {
            ProductName = string.IsNullOrWhiteSpace(scan.Result.ProductName)
                ? "Unknown product"
                : scan.Result.ProductName,
            ScanTime = scan.SavedAtUtc.ToLocalTime().ToString("MMM d, h:mm tt"),
            Score = score,
            Rating = string.IsNullOrWhiteSpace(scan.Result.Category)
                ? "Unknown"
                : scan.Result.Category,
            // Fall back to a bundled image when the captured file is missing.
            ImagePath = !string.IsNullOrWhiteSpace(scan.ImagePath) &&
                        File.Exists(scan.ImagePath)
                ? scan.ImagePath
                : "oats_label.webp",
            RatingColor = score >= 70 ? "#047857"
                        : score >= 40 ? "#D99024"
                        : "#D64545"
        };
    }

    private void AddShowcaseScans()
    {
        RecentScans.Add(new RecentScanItem
        {
            ProductName = "Greek Yogurt",
            ScanTime = "Today, 8:30 AM",
            Score = 88,
            Rating = "Excellent",
            ImagePath = "yogurt.webp",
            RatingColor = "#047857"
        });

        RecentScans.Add(new RecentScanItem
        {
            ProductName = "Oat & Honey Granola Bar",
            ScanTime = "Yesterday, 3:15 PM",
            Score = 72,
            Rating = "Good",
            ImagePath = "oats_label.webp",
            RatingColor = "#D99024"
        });

        RecentScans.Add(new RecentScanItem
        {
            ProductName = "Classic Potato Chips",
            ScanTime = "Mon, 12:45 PM",
            Score = 35,
            Rating = "Poor",
            ImagePath = "potato.webp",
            RatingColor = "#D64545"
        });
    }

    private async void LoadDailyInsightAsync()
    {
        try
        {
            var generatedInsight =
                await dailyInsightGenerator.GetTodayInsightAsync();

            if (!string.IsNullOrWhiteSpace(generatedInsight))
            {
                DailyInsight = generatedInsight;
                return;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DailyInsight] Error: {ex}");
        }

        // Fallback keeps the UI useful even if AI is unavailable.
        DailyInsight =
            "A balanced plate with more fiber and lean protein can help support steady energy and better blood sugar control today.";
    }

    [RelayCommand]
    private Task ScanAsync()
    {
        return AppNavigator.GoScanAsync();
    }

    [RelayCommand]
    private Task OpenHistoryAsync()
    {
        return AppNavigator.GoHistoryAsync();
    }

    [RelayCommand]
    private Task ViewAllAsync()
    {
        return AppNavigator.GoHistoryAsync();
    }

    [RelayCommand]
    private Task OpenProfileAsync()
    {
        return AppNavigator.GoProfileAsync();
    }

    [RelayCommand]
    private Task OpenTrendsAsync()
    {
        return AppNavigator.GoTrendAsync();
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