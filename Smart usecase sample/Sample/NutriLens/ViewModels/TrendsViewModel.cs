using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriLens.Helpers;
using NutriLens.Models;
using NutriLens.Services;

namespace NutriLens.ViewModels;

public partial class TrendsViewModel : ObservableObject
{
    private readonly ICombinedScanHistory history;
    private IReadOnlyList<SavedScan> combined = [];
     

    public ObservableCollection<string> Periods { get; } =
    [
        "7D",
        "30D",
        "90D"
    ];

    public ObservableCollection<HealthTrendPoint> HealthScoreTrend { get; } = [];
    public ObservableCollection<AdditiveTrendItem> FrequentAdditives { get; } = [];

    [ObservableProperty]
    private string averageScoreText = "0";

    [ObservableProperty]
    private string averageScoreDelta = "—";

    [ObservableProperty]
    private double yMinimum = 0;

    [ObservableProperty]
    private double yMaximum = 100;

    [ObservableProperty]
    private string proteinAverageText = "Avg. —";

    [ObservableProperty]
    private double proteinProgress;

    [ObservableProperty]
    private string proteinNote = "Protein intake from your scan history.";

    [ObservableProperty]
    private string sugarDeltaText = "—";

    [ObservableProperty]
    private double sugarProgress;

    [ObservableProperty]
    private string sugarNote = "Sugar intake from your scan history.";

    [ObservableProperty]
    private string insightTitle = "Need more data";

    [ObservableProperty]
    private string insightText =
        "Scan a few products to unlock detailed monthly forecasting and deeper ingredient insights.";

    [ObservableProperty]
    private string totalScansText = "0";

    [ObservableProperty]
    private string bestScoreText = "0";

    [ObservableProperty]
    private string lowestScoreText = "0";

    [ObservableProperty]
    private string mostRecentScoreText = "0";

    public string BiotechIcon => MaterialIcons.Biotech;
    public string PersonIcon => MaterialIcons.Person;
    public string HomeIcon => MaterialIcons.Home;
    public string HistoryIcon => MaterialIcons.History;
    public string ScannerIcon => MaterialIcons.DocumentScanner;
    public string TrendsIcon => MaterialIcons.ShowChart;
    public string ProfileIcon => MaterialIcons.PersonOutline;
    public string TrendingUpIcon => MaterialIcons.TrendingUp;
    public string ArrowDownIcon => MaterialIcons.ArrowDownward;
    public string AutoGraphIcon => MaterialIcons.AutoGraph;
    [ObservableProperty]
private string selectedPeriod = "7D";

private void UpdatePeriodSelection(string period)
{
    SelectedPeriod = period;
    OnPropertyChanged(nameof(Is7DSelected));
    OnPropertyChanged(nameof(Is30DSelected));
    OnPropertyChanged(nameof(Is90DSelected));
}

public bool Is7DSelected => SelectedPeriod == "7D";
public bool Is30DSelected => SelectedPeriod == "30D";
public bool Is90DSelected => SelectedPeriod == "90D";

[RelayCommand]
private void SelectPeriod(string period)
{
    if (string.IsNullOrWhiteSpace(period))
        return;

    if (SelectedPeriod == period)
        return;

    UpdatePeriodSelection(period);
    Recompute();
}

    public TrendsViewModel(ICombinedScanHistory history)
    {
        this.history = history;
    }

    public TrendsViewModel()
        : this(new CombinedScanHistory(
            Resolve<IScanHistoryStore>() ?? new JsonScanHistoryStore()))
    {
    }

    public async Task LoadAsync()
    {
        combined = await history.GetCombinedAsync();
        Recompute();
    } 

    [RelayCommand]
    private async Task OpenHomeAsync() => await AppNavigator.GoDashboardAsync();

    [RelayCommand]
    private async Task OpenHistoryAsync() => await AppNavigator.GoHistoryAsync();

    [RelayCommand]
    private async Task OpenScanAsync() => await AppNavigator.GoScanAsync();

    [RelayCommand]
    private Task OpenProfileAsync() => Task.CompletedTask;

    [RelayCommand]
    private Task OpenTrendsAsync() => Task.CompletedTask;

    private void Recompute()
    {
        var window = GetPeriodWindow();

        ComputeScoreTrend(window);
        ComputeSummaryMetrics(window);
        ComputeFrequentAdditives(window);
        ComputeNutritionCards(window);
        ComputeInsight(window);
    }

    private IReadOnlyList<SavedScan> GetPeriodWindow()
    {
        if (combined.Count == 0)
            return [];

        var days = SelectedPeriod switch
        {
            "30D" => 30,
            "90D" => 90,
            _ => 7
        };

        var cutoff = DateTime.UtcNow.AddDays(-days);

        var window = combined
            .Where(s => s.SavedAtUtc >= cutoff)
            .OrderBy(s => s.SavedAtUtc)
            .ToList();

        return window.Count > 0 ? window : combined.OrderBy(s => s.SavedAtUtc).ToList();
    }

    private void ComputeScoreTrend(IReadOnlyList<SavedScan> window)
    {
        HealthScoreTrend.Clear();

        if (window.Count == 0)
        {
            YMinimum = 0;
            YMaximum = 100;
            return;
        }

        var plotted = window.Count > 12 ? window.TakeLast(12).ToList() : window.ToList();

        var dateUseCount = new Dictionary<DateTime, int>();

        foreach (var scan in plotted)
        {
            var localDate = scan.SavedAtUtc.ToLocalTime().Date;

            dateUseCount.TryGetValue(localDate, out var uses);
            dateUseCount[localDate] = uses + 1;

            var label = uses == 0
                ? localDate.ToString("M/d")
                : $"{localDate:M/d}·{uses + 1}";

            HealthScoreTrend.Add(new HealthTrendPoint
            {
                Day = label,
                Score = scan.Result.Score
            });
        }

        var min = plotted.Min(s => s.Result.Score);
        var max = plotted.Max(s => s.Result.Score);

        YMinimum = Math.Max(0, Math.Floor((min - 10) / 10.0) * 10);
        YMaximum = Math.Min(100, Math.Ceiling((max + 10) / 10.0) * 10);

        if (YMaximum - YMinimum < 20)
            YMaximum = Math.Min(100, YMinimum + 20);
    }

    private void ComputeSummaryMetrics(IReadOnlyList<SavedScan> window)
    {
        if (window.Count == 0)
        {
            AverageScoreText = "0";
            AverageScoreDelta = "—";
            TotalScansText = "0";
            BestScoreText = "0";
            LowestScoreText = "0";
            MostRecentScoreText = "0";
            return;
        }

        var scores = window.Select(s => (double)s.Result.Score).ToList();

        AverageScoreText = ((int)Math.Round(scores.Average())).ToString();
        TotalScansText = window.Count.ToString();
        BestScoreText = ((int)scores.Max()).ToString();
        LowestScoreText = ((int)scores.Min()).ToString();
        MostRecentScoreText = window
            .OrderByDescending(s => s.SavedAtUtc)
            .First()
            .Result
            .Score
            .ToString();

        var ordered = window.OrderBy(s => s.SavedAtUtc).ToList();
        var half = ordered.Count / 2;

        if (ordered.Count >= 4 && half > 0)
        {
            var older = ordered.Take(half).Average(s => s.Result.Score);
            var newer = ordered.TakeLast(half).Average(s => s.Result.Score);

            AverageScoreDelta = older <= 0
                ? "—"
                : FormatDelta((newer - older) / older * 100);
        }
        else
        {
            AverageScoreDelta = ordered.Count >= 2
                ? FormatDelta(
                    (ordered.Last().Result.Score - ordered.First().Result.Score) /
                    Math.Max(1.0, ordered.First().Result.Score) * 100)
                : "—";
        }
    }

    private void ComputeFrequentAdditives(IReadOnlyList<SavedScan> window)
    {
        FrequentAdditives.Clear();

        if (window.Count == 0)
            return;

        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var scan in window)
        {
            var names = scan.Result.Additives
                .Concat(scan.Result.Preservatives)
                .Where(a => a is not null && !string.IsNullOrWhiteSpace(a.Name))
                .Select(a => a.Name.Trim());

            foreach (var name in names.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                counts.TryGetValue(name, out var current);
                counts[name] = current + 1;
            }
        }

        if (counts.Count == 0)
        {
            foreach (var scan in window)
            {
                var flagged = scan.Result.IngredientBreakdown
                    .Where(i =>
                        !string.IsNullOrWhiteSpace(i.Name) &&
                        string.Equals(i.RiskLevel, "High", StringComparison.OrdinalIgnoreCase))
                    .Select(i => i.Name.Trim());

                foreach (var name in flagged.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    counts.TryGetValue(name, out var current);
                    counts[name] = current + 1;
                }
            }
        }

        var total = Math.Max(1, window.Count);

        foreach (var entry in counts
            .OrderByDescending(k => k.Value)
            .ThenBy(k => k.Key)
            .Take(3))
        {
            var percent = (int)Math.Round(entry.Value * 100.0 / total);

            FrequentAdditives.Add(new AdditiveTrendItem
            {
                Name = entry.Key,
                Percentage = $"{percent}% of scans",
                Icon = MaterialIcons.WarningAmber,
                Color = percent >= 40 ? "#DC2626" : "#F59E0B"
            });
        }
    }

    private void ComputeNutritionCards(IReadOnlyList<SavedScan> window)
    {
        var withProtein = window
            .Where(s => s.Result.Nutrition.ProteinGrams is > 0)
            .ToList();

        if (withProtein.Count > 0)
        {
            var avg = withProtein.Average(s => s.Result.Nutrition.ProteinGrams!.Value);

            ProteinAverageText = $"Avg. {avg:0.#}g/serving";
            ProteinProgress = Math.Clamp(avg / 15.0, 0, 1);
            ProteinNote = withProtein.Count == window.Count
                ? "Based on protein values across the selected period."
                : $"Based on {withProtein.Count} of {window.Count} scans in the selected period.";
        }
        else
        {
            ProteinAverageText = "Avg. —";
            ProteinProgress = 0;
            ProteinNote = "Scan products with nutrition labels to track protein intake.";
        }

        var sugarValues = window
            .Select(s => s.Result.Nutrition.SugarsGrams ?? s.Result.Nutrition.AddedSugarsGrams)
            .Where(g => g is > 0)
            .Select(g => g!.Value)
            .ToList();

        if (sugarValues.Count > 0)
        {
            var avg = sugarValues.Average();

            SugarProgress = Math.Clamp(avg / 25.0, 0, 1);

            if (sugarValues.Count >= 2)
            {
                var half = sugarValues.Count / 2;

                var older = sugarValues.Take(half).Count() > 0
                    ? sugarValues.Take(half).Average()
                    : sugarValues.First();

                var newer = sugarValues.TakeLast(half).Count() > 0
                    ? sugarValues.TakeLast(half).Average()
                    : sugarValues.Last();

                if (older > 0)
                {
                    var change = (newer - older) / older * 100;
                    SugarDeltaText = change <= 0
                        ? $"-{(int)Math.Round(Math.Abs(change))}%"
                        : $"+{(int)Math.Round(change)}%";
                }
                else
                {
                    SugarDeltaText = "—";
                }
            }
            else
            {
                SugarDeltaText = "—";
            }

            SugarNote = $"Averaging {avg:0.#}g of sugars per serving across the selected period.";
        }
        else
        {
            SugarDeltaText = "—";
            SugarProgress = 0;
            SugarNote = "Scan products with nutrition labels to track sugar intake.";
        }
    }

    private void ComputeInsight(IReadOnlyList<SavedScan> window)
    {
        if (window.Count == 0)
        {
            InsightTitle = "Need more data";
            InsightText =
                "Scan a few products to unlock detailed monthly forecasting and deeper ingredient insights.";
            return;
        }

        var ordered = window.OrderBy(s => s.SavedAtUtc).ToList();

        if (ordered.Count < 4)
        {
            InsightTitle = "Need more data";
            var remaining = 4 - ordered.Count;
            InsightText =
                $"Scan {remaining} more product{(remaining == 1 ? "" : "s")} in this period to unlock detailed trend insights.";
            return;
        }

        var firstHalf = ordered.Take(ordered.Count / 2).Average(s => s.Result.Score);
        var secondHalf = ordered.TakeLast(ordered.Count / 2).Average(s => s.Result.Score);
        var delta = secondHalf - firstHalf;

        if (delta >= 5)
        {
            InsightTitle = "Trend improving";
            InsightText =
                $"Your health score improved by {delta:0.#} points across the selected period. Keep prioritizing lower-risk products.";
        }
        else if (delta <= -5)
        {
            InsightTitle = "Trend declining";
            InsightText =
                $"Your health score dropped by {Math.Abs(delta):0.#} points across the selected period. Check high-sugar and high-risk ingredients.";
        }
        else
        {
            InsightTitle = "Stable trend";
            InsightText =
                "Your health score is staying fairly steady across the selected period. Focus on consistent label choices.";
        }
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
    private static string FormatDelta(double value)
    {
        var rounded = (int)Math.Round(value);
        return rounded >= 0 ? $"+{rounded}%" : $"{rounded}%";
    }
}