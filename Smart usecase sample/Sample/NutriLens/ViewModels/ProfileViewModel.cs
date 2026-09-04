using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NutriLens.Helpers;
using NutriLens.Models;
using NutriLens.Services;
using System.Collections.ObjectModel;

namespace NutriLens.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly IScanHistoryStore scanStore;
    private readonly IUserPreferenceStore preferenceStore;

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

    // Icons
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
    private bool hypertensionEnabled;

    [ObservableProperty]
    private bool cholesterolEnabled;

    [ObservableProperty]
    private bool kidneyHealthEnabled;

    public ObservableCollection<ProfileGoal> DietaryGoals { get; } =
    [
        new() { Title = "Reduce Sugar", IsSelected = true },
        new() { Title = "High Protein",  IsSelected = false },
        new() { Title = "Low Carb",      IsSelected = false },
        new() { Title = "Heart Health",  IsSelected = true }
    ];

    public ObservableCollection<ProfileGoal> Preferences { get; } =
    [
        new() { Title = "Nut-Free",    IsSelected = true },
        new() { Title = "Gluten-Free", IsSelected = false },
        new() { Title = "Vegan",       IsSelected = false },
        new() { Title = "Dairy-Free",  IsSelected = false }
    ];

    public ObservableCollection<ProfileSetting> Settings { get; } =
    [
        new() { Icon = MaterialIcons.PersonOutline,     Title = "Account Settings" },
        new() { Icon = MaterialIcons.NotificationsNone, Title = "Notifications" },
        new() { Icon = MaterialIcons.HelpOutline,       Title = "Help & Support" }
    ];

    public ProfileViewModel()
        : this(
            Resolve<IScanHistoryStore>() ?? new JsonScanHistoryStore(),
            Resolve<IUserPreferenceStore>() ?? new UserPreferenceStore())
    {
    }

    public ProfileViewModel(
        IScanHistoryStore scanStore,
        IUserPreferenceStore preferenceStore)
    {
        this.scanStore = scanStore
            ?? throw new ArgumentNullException(nameof(scanStore));
        this.preferenceStore = preferenceStore
            ?? throw new ArgumentNullException(nameof(preferenceStore));

        // Hydrate toggles from store (goals/preferences hydrate below).
        var saved = preferenceStore.Load();
        DiabetesEnabled = saved.HealthConsiderations
            .Contains("Diabetes", StringComparer.OrdinalIgnoreCase);
        HypertensionEnabled = saved.HealthConsiderations
            .Contains("Hypertension", StringComparer.OrdinalIgnoreCase);
        CholesterolEnabled = saved.HealthConsiderations
            .Contains("Cholesterol", StringComparer.OrdinalIgnoreCase);
        KidneyHealthEnabled = saved.HealthConsiderations
            .Contains("Kidney Health", StringComparer.OrdinalIgnoreCase);

        ApplyStoredSelections(DietaryGoals, saved.DietaryGoals);
        ApplyStoredSelections(Preferences, saved.AllergiesAndPreferences);

        // Track collection mutations → persist.
        DietaryGoals.CollectionChanged += (_, _) => PersistPreferences();
        Preferences.CollectionChanged += (_, _) => PersistPreferences();
        foreach (var g in DietaryGoals)
            g.PropertyChanged += (_, _) => PersistPreferences();
        foreach (var p in Preferences)
            p.PropertyChanged += (_, _) => PersistPreferences();

        PersistPreferences(); // ensure store matches in-memory initial state
    }

    private static void ApplyStoredSelections(
        ObservableCollection<ProfileGoal> targets,
        IReadOnlyList<string> selected)
    {
        if (selected.Count == 0)
            return;

        foreach (var item in targets)
            item.IsSelected = selected.Any(s =>
                string.Equals(s, item.Title, StringComparison.OrdinalIgnoreCase));
    }

    // Persist whenever any toggle flips.
    partial void OnDiabetesEnabledChanged(bool value) => PersistPreferences();
    partial void OnHypertensionEnabledChanged(bool value) => PersistPreferences();
    partial void OnCholesterolEnabledChanged(bool value) => PersistPreferences();
    partial void OnKidneyHealthEnabledChanged(bool value) => PersistPreferences();

    /// <summary>Builds the snapshot and writes it to the store.</summary>
    private void PersistPreferences()
    {
        var snapshot = new UserDietaryPreference
        {
            DietaryGoals = DietaryGoals
                .Where(g => g.IsSelected)
                .Select(g => g.Title)
                .ToList(),
            AllergiesAndPreferences = Preferences
                .Where(p => p.IsSelected)
                .Select(p => p.Title)
                .ToList(),
            HealthConsiderations = BuildHealthConsiderations()
                .ToList()
        };
        preferenceStore.Save(snapshot);
    }

    private IEnumerable<string> BuildHealthConsiderations()
    {
        if (DiabetesEnabled) yield return "Diabetes";
        if (HypertensionEnabled) yield return "Hypertension";
        if (CholesterolEnabled) yield return "Cholesterol";
        if (KidneyHealthEnabled) yield return "Kidney Health";
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
            ScanInsightText =
                "Scan your first product to unlock personalized insights.";
            return;
        }

        var scores = scans.Select(s => s.Result.Score).ToList();
        var avg = scores.Average();
        var healthy = scores.Count(s => s >= 70);

        TotalScansText = scans.Count.ToString();
        AverageScoreText = $"{(int)Math.Round(avg)}/100";
        BestScoreText = scores.Max().ToString();
        HealthyChoicesText = $"{(int)Math.Round(healthy * 100.0 / scores.Count)}%";

        var mostCommonRisk = scans
            .SelectMany(s => s.Result.IngredientBreakdown)
            .Where(i => !string.IsNullOrWhiteSpace(i.Name)
                && string.Equals(i.RiskLevel, "High",
                    StringComparison.OrdinalIgnoreCase))
            .GroupBy(i => i.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        MostCommonRiskText = mostCommonRisk?.Key ?? "None identified";

        var ordered = scans.OrderBy(s => s.SavedAtUtc).ToList();
        if (ordered.Count >= 4)
        {
            var half = ordered.Count / 2;
            var older = ordered.Take(half).Average(s => s.Result.Score);
            var newer = ordered.TakeLast(half).Average(s => s.Result.Score);

            if (newer > older)
                ScanInsightText =
                    $"Over the last {ordered.Count} scans, your food choices improved by " +
                    $"{(int)Math.Round(newer - older)} points. Keep favoring higher-scoring products.";
            else if (newer < older)
                ScanInsightText =
                    $"Your recent scans average {(int)Math.Round(older - newer)} points lower than earlier ones. " +
                    $"Watch out for {MostCommonRiskText.ToLowerInvariant()} in upcoming labels.";
            else
                ScanInsightText =
                    $"Your average health score has held steady at {(int)Math.Round(newer)}/100 across {ordered.Count} scans.";
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
    private Task ToggleGoalAsync(ProfileGoal goal)
    {
        if (goal is null) return Task.CompletedTask;
        goal.IsSelected = !goal.IsSelected; // raises PropertyChanged → PersistPreferences
        return Task.CompletedTask;
    }
    [RelayCommand]
    private async Task OpenHomeAsync() => await AppNavigator.GoDashboardAsync();
    [RelayCommand]
    private async Task OpenHistoryAsync() => await AppNavigator.GoHistoryAsync();
    [RelayCommand]
    private async Task OpenScanAsync() => await AppNavigator.GoScanAsync();
    [RelayCommand]
    private async Task OpenTrendsAsync() => await AppNavigator.GoTrendAsync();
    [RelayCommand]
    private Task EditProfileAsync() => Task.CompletedTask;
    [RelayCommand]
    private Task SignOutAsync() => Task.CompletedTask;

    private static T? Resolve<T>() where T : class =>
        Application.Current?.Handler?.MauiContext?.Services.GetService<T>();
}