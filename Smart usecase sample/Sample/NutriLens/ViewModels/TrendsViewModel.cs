using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriLens.Helpers;
using NutriLens.Models;
using NutriLens.Services;

namespace NutriLens.ViewModels;

public partial class TrendsViewModel : ObservableObject
{
    // Unified source: sample seed records + saved AI-generated records.
    private readonly ICombinedScanHistory history;
    private IReadOnlyList<SavedScan> combined = [];

    [ObservableProperty]
    private string selectedPeriod = "7D";

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

    public TrendsViewModel(ICombinedScanHistory history)
    {
        this.history = history;
    }

    public TrendsViewModel()
        : this(new CombinedScanHistory(
            Resolve<IScanHistoryStore>() ?? new JsonScanHistoryStore()))
    {
    }

    /// <summary>Reloads combined history (seeds + saved) and recomputes all metrics.</summary>
    public async Task LoadAsync()
    {
        combined = await history.GetCombinedAsync();
        Recompute();
    }

    [RelayCommand]
    private void SelectPeriod(string period)
    {
        SelectedPeriod = period;
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
        ComputeScoreTrend();
        ComputeSummaryMetrics();
        ComputeFrequentAdditives();
        ComputeNutritionCards();
        ComputeInsight();
    }

    // Chart: every record (seed + saved) becomes its own point,
    // chronological order → 92 → 74 → 38 → 68 → 85 …
    private void ComputeScoreTrend()
    {
        var chronological = combined
            .OrderBy(s => s.SavedAtUtc)
            .ToList();

        // Period filter; fall back to full history when the window holds
        // fewer than 2 points so the chart always shows the baseline trend.
        var days = SelectedPeriod switch
        {
            "30D" => 30,
            "90D" => 90,
            _ => 7
        };

        var cutoff = DateTime.UtcNow.AddDays(-days);

        var window = chronological
            .Where(s => s.SavedAtUtc >= cutoff)
            .ToList();

        if (window.Count < 2)
            window = chronological;

        // Keep the chart readable — show the most recent 12 points.
        var plotted = window.Count > 12 ? window.TakeLast(12).ToList() : window;

        HealthScoreTrend.Clear();

        var dateUseCount = new Dictionary<DateTime, int>();
        var seenOrder = 0;

        foreach (var scan in plotted)
        {
            var localDate = scan.SavedAtUtc.ToLocalTime().Date;

            dateUseCount.TryGetValue(localDate, out var uses);
            dateUseCount[localDate] = uses + 1;

            // Multiple scans on the same day keep distinct labels
            // so no point is visually collapsed.
            var label = uses == 0
                ? localDate.ToString("M/d")
                : $"{localDate:M/d}·{uses + 1}";

            HealthScoreTrend.Add(new HealthTrendPoint
            {
                Day = label,
                Score = scan.Result.Score
            });

            seenOrder++;
        }

        // Adaptive Y axis — never clips real scores (was Min=50/Max=90).
        if (plotted.Count == 0)
        {
            YMinimum = 0;
            YMaximum = 100;
        }
        else
        {
            var min = plotted.Min(s => s.Result.Score);
            var max = plotted.Max(s => s.Result.Score);

            YMinimum = Math.Max(0, Math.Floor((min - 10) / 10.0) * 10);
            YMaximum = Math.Min(100, Math.Ceiling((max + 10) / 10.0) * 10);

            if (YMaximum - YMinimum < 20)
                YMaximum = Math.Min(100, YMinimum + 20);
        }
    }

    private void ComputeSummaryMetrics()
    {
        if (combined.Count == 0)
        {
            AverageScoreText = "0";
            AverageScoreDelta = "—";
            TotalScansText = "0";
            BestScoreText = "0";
            LowestScoreText = "0";
            MostRecentScoreText = "0";
            return;
        }

        var scores = combined.Select(s => (double)s.Result.Score).ToList();

        AverageScoreText = ((int)Math.Round(scores.Average())).ToString();
        TotalScansText = combined.Count.ToString();
        BestScoreText = ((int)scores.Max()).ToString();
        LowestScoreText = ((int)scores.Min()).ToString();
        MostRecentScoreText = combined
            .OrderByDescending(s => s.SavedAtUtc)
            .First()
            .Result
            .Score
            .ToString();

        // Improvement trend: newer half vs older half of the combined history.
        var ordered = combined.OrderBy(s => s.SavedAtUtc).ToList();
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

    private void ComputeFrequentAdditives()
    {
        FrequentAdditives.Clear();

        if (combined.Count == 0)
            return;

        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var scan in combined)
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

        // Fallback: top High-risk ingredients when labels declare no additives.
        if (counts.Count == 0)
        {
            foreach (var scan in combined)
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

        var total = Math.Max(1, combined.Count);

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

    private void ComputeNutritionCards()
    {
        var withProtein = combined
            .Where(s => s.Result.Nutrition.ProteinGrams is > 0)
            .ToList();

        if (withProtein.Count > 0)
        {
            var avg = withProtein.Average(s => s.Result.Nutrition.ProteinGrams!.Value);

            ProteinAverageText = $"Avg. {avg:0.#}g/serving";
            ProteinProgress = Math.Clamp(avg / 15.0, 0, 1);
            ProteinNote = withProtein.Count == combined.Count
                ? "Based on protein values across your scan history."
                : $"Based on {withProtein.Count} of {combined.Count} scans that listed protein.";
        }
        else
        {
            ProteinAverageText = "Avg. —";
            ProteinProgress = 0;
            ProteinNote = "Scan products with nutrition labels to track protein intake.";
        }

        var sugarScans = combined
            .Where(s => (s.Result.Nutrition.SugarsGrams ?? s.Result.Nutrition.AddedSugarsGrams) is > 0)
            .OrderBy(s => s.SavedAtUtc)
            .ToList();

        var sugarValues = sugarScans
            .Select(s => (s.Result.Nutrition.SugarsGrams ?? s.Result.Nutrition.AddedSugarsGrams)!.Value)
            .ToList();

        if (sugarValues.Count > 0)
        {
            var avg = sugarValues.Average();

            SugarProgress = Math.Clamp(avg / 25.0, 0, 1);

            // Improvement measured across time — a sugar DECREASE renders
            // as the green "−x%" badge, matching the original card.
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

            SugarNote = $"Averaging {avg:0.#}g of sugars per serving across your scan history.";
        }
        else
        {
            SugarDeltaText = "—";
            SugarProgress = 0;
            SugarNote = "Scan products with nutrition labels to track sugar intake.";
        }
    }

    private void ComputeInsight()
    {
        if (combined.Count == 0)
        {
            InsightTitle = "Need more data";
            InsightText =
                "Scan a few products to unlock detailed monthly forecasting and deeper ingredient insights.";
            return;
        }

        var ordered = combined.OrderBy(s => s.SavedAtUtc).ToList();

        if (ordered.Count < 4)
        {
            InsightTitle = "Need more data";
            var remaining = 4 - ordered.Count;
            InsightText =
                $"Scan {remaining} more product{(remaining == 1 ? "" : "s")} this week to unlock detailed monthly forecasting and deeper ingredient insights.";
            return;
        }

        var half = ordered.Count / 2;
        var older = ordered.Take(half).Average(s => s.Result.Score);
        var newer = ordered.TakeLast(half).Average(s => s.Result.Score);
        var delta = (int)Math.Round(newer - older);

        if (delta > 0)
        {
            InsightTitle = "Improving trend";
            InsightText =
                $"Across your last {ordered.Count} scans, your food choices improved by {delta} points on average. Keep it up!";
        }
        else if (delta < 0)
        {
            InsightTitle = "Watch your choices";
            InsightText =
                $"Your recent scans average {Math.Abs(delta)} points lower than earlier ones. Consider the alternatives suggested in each analysis.";
        }
        else
        {
            InsightTitle = "Steady pattern";
            InsightText =
                $"Your average health score has held steady at {(int)Math.Round(newer)}/100 across your {ordered.Count} scans.";
        }
    }

    private static string FormatDelta(double percent)
    {
        var rounded = (int)Math.Round(Math.Abs(percent));
        return percent >= 0 ? $"+{rounded}%" : $"-{rounded}%";
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