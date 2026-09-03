using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriLens.Helpers;
using NutriLens.Models;
using NutriLens.Services;
using NutriLens.Views;
using System.Collections.ObjectModel;

namespace NutriLens.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly IScanHistoryStore scanStore;

    [ObservableProperty]
    private string totalScansText = "0";

    [ObservableProperty]
    private string averageScoreText = "0/100";

    [ObservableProperty]
    private string bestScoreText = "0";

    [ObservableProperty]
    private string healthyChoicesText = "0%";

    [ObservableProperty]
    private string mostCommonRiskText = "None identified";

    [ObservableProperty]
    private string scanInsightText =
        "Scan your first product to unlock personalized insights.";
    public string BiotechIcon => MaterialIcons.Biotech;
    public string PersonIcon => MaterialIcons.Person;
    public string EditIcon => MaterialIcons.Edit;
    public string FlagIcon => MaterialIcons.Flag;
    public string RestaurantMenuIcon => MaterialIcons.RestaurantMenu;
    public string HealthIcon => MaterialIcons.HealthAndSafety;
    public string MedicalServicesIcon => MaterialIcons.MedicalServices;
    public string MedicationIcon => MaterialIcons.Medication;
    public string LockIcon => MaterialIcons.Lock;
    public string ChevronIcon => MaterialIcons.ChevronRight;
    public string HomeIcon => MaterialIcons.Home;
    public string HistoryIcon => MaterialIcons.History;
    public string ScannerIcon => MaterialIcons.DocumentScanner;
    public string TrendsIcon => MaterialIcons.ShowChart;

    [ObservableProperty]
    private bool diabetesEnabled;

    [ObservableProperty]
    private bool hypertensionEnabled = true;

    public ObservableCollection<ProfileGoal> DietaryGoals { get; } =
    [
        new ProfileGoal
        {
            Title = "Reduce Sugar",
            IsSelected = true
        },
        new ProfileGoal
        {
            Title = "High Protein",
            IsSelected = false
        },
        new ProfileGoal
        {
            Title = "Low Carb",
            IsSelected = false
        },
        new ProfileGoal
        {
            Title = "Heart Health",
            IsSelected = true
        }
    ];

    public ObservableCollection<ProfileGoal> Preferences { get; } =
    [
        new ProfileGoal
        {
            Title = "Nut-free",
            IsSelected = true
        },
        new ProfileGoal
        {
            Title = "Gluten-free",
            IsSelected = false
        },
        new ProfileGoal
        {
            Title = "Vegan",
            IsSelected = false
        },
        new ProfileGoal
        {
            Title = "Dairy-free",
            IsSelected = false
        }
    ];

    public ObservableCollection<ProfileSetting> Settings { get; } =
    [
        new ProfileSetting
        {
            Icon = MaterialIcons.PersonOutline,
            Title = "Account Settings"
        },
        new ProfileSetting
        {
            Icon = MaterialIcons.NotificationsNone,
            Title = "Notifications"
        },
        new ProfileSetting
        {
            Icon = MaterialIcons.HelpOutline,
            Title = "Help & Support"
        }
    ];

    public ProfileViewModel()
    : this(Resolve<IScanHistoryStore>() ?? new JsonScanHistoryStore())
    {
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
    public ProfileViewModel(IScanHistoryStore scanStore)
    {
        this.scanStore = scanStore;
    }

    /// <summary>Computes profile statistics from saved AI scan history.</summary>
    public async Task LoadScanInsightsAsync()
    {
        var scans = await scanStore.GetAllAsync();

        if (scans.Count == 0)
        {
            TotalScansText = "0";
            AverageScoreText = "0/100";
            BestScoreText = "0";
            HealthyChoicesText = "0%";
            MostCommonRiskText = "None identified";
            ScanInsightText = "Scan your first product to unlock personalized insights.";
            return;
        }

        var scores = scans.Select(s => s.Result.Score).ToList();
        var avg = scores.Average();
        var healthy = scores.Count(s => s >= 70);

        TotalScansText = scans.Count.ToString();
        AverageScoreText = $"{(int)Math.Round(avg)}/100";
        BestScoreText = scores.Max().ToString();
        HealthyChoicesText = $"{(int)Math.Round(healthy * 100.0 / scores.Count)}%";

        // Most common High-risk ingredient across all saved analyses.
        var mostCommonRisk = scans
            .SelectMany(s => s.Result.IngredientBreakdown)
            .Where(i =>
                !string.IsNullOrWhiteSpace(i.Name) &&
                string.Equals(i.RiskLevel, "High", StringComparison.OrdinalIgnoreCase))
            .GroupBy(i => i.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        MostCommonRiskText = mostCommonRisk?.Key ?? "None identified";

        // Insight sentence from real history.
        var ordered = scans.OrderBy(s => s.SavedAtUtc).ToList();

        if (ordered.Count >= 4)
        {
            var half = ordered.Count / 2;
            var older = ordered.Take(half).Average(s => s.Result.Score);
            var newer = ordered.TakeLast(half).Average(s => s.Result.Score);

            if (newer > older)
            {
                ScanInsightText =
                    $"Over the last {ordered.Count} scans, your food choices improved by " +
                    $"{(int)Math.Round(newer - older)} points. Keep favoring higher-scoring products.";
            }
            else if (newer < older)
            {
                ScanInsightText =
                    $"Your recent scans average {(int)Math.Round(older - newer)} points lower than earlier ones. " +
                    $"Watch out for {MostCommonRiskText.ToLowerInvariant()} in upcoming labels.";
            }
            else
            {
                ScanInsightText =
                    $"Your average health score has held steady at {(int)Math.Round(newer)}/100 across {ordered.Count} scans.";
            }
        }
        else
        {
            ScanInsightText =
                $"Based on {ordered.Count} saved scan{(ordered.Count == 1 ? "" : "s")}, " +
                $"your average product score is {(int)Math.Round(avg)}/100. " +
                "Scan more products for deeper insights.";
        }
    }

    [RelayCommand]
    private async Task OpenHomeAsync()
    {
        await AppNavigator.GoDashboardAsync();
    }

    [RelayCommand]
    private async Task OpenHistoryAsync()
    {
        await AppNavigator.GoHistoryAsync();
    }

    [RelayCommand]
    private async Task OpenScanAsync()
    {
        await AppNavigator.GoScanAsync();
    }

    [RelayCommand]
    private async Task OpenTrendsAsync()
    {
        await AppNavigator.GoTrendAsync();
    }

    [RelayCommand]
    private Task EditProfileAsync()
    {
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task SignOutAsync()
    {
        return Task.CompletedTask;
    }
}