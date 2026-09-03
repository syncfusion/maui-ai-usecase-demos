using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using NutriLens.Helpers;
using NutriLens.Models;
using NutriLens.Services;

namespace NutriLens.ViewModels;

public partial class HistoryViewModel : ObservableObject
{
    // Unified source: sample seed records + saved AI-generated records.
    private readonly ICombinedScanHistory history;
    private IReadOnlyList<SavedScan> allScans = [];

    public ObservableCollection<HistoryItem> HistoryItems { get; } = [];

    public ObservableCollection<string> UnselectedFilters { get; } =
    [
        "Excellent",
        "Good",
        "Moderate",
        "Poor"
    ];

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string summaryText = "Showing 0 scans";

    public string BiotechIcon => MaterialIcons.Biotech;
    public string PersonIcon => MaterialIcons.Person;
    public string SearchIcon => MaterialIcons.Search;
    public string DoneAllIcon => MaterialIcons.DoneAll;
    public string HomeIcon => MaterialIcons.Home;
    public string HistoryIcon => MaterialIcons.History;
    public string ScannerIcon => MaterialIcons.DocumentScanner;
    public string TrendsIcon => MaterialIcons.ShowChart;
    public string ProfileIcon => MaterialIcons.PersonOutline;
    public string ChevronIcon => MaterialIcons.ChevronRight;

    public HistoryViewModel(ICombinedScanHistory history)
    {
        this.history = history;
    }

    public HistoryViewModel()
        : this(new CombinedScanHistory(
            Resolve<IScanHistoryStore>() ?? new JsonScanHistoryStore()))
    {
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }

    /// <summary>Reloads combined history (seeds + saved). Call on every OnAppearing.</summary>
    public async Task LoadAsync()
    {
        allScans = await history.GetCombinedAsync();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var query = SearchText?.Trim() ?? string.Empty;

        IEnumerable<SavedScan> filtered = allScans;

        if (query.Length > 0)
        {
            filtered = filtered.Where(s =>
                s.Result.ProductName.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        var ordered = filtered
            .OrderByDescending(s => s.SavedAtUtc)
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

    private static HistoryItem ToHistoryItem(SavedScan scan)
    {
        var score = Math.Clamp(scan.Result.Score, 0, 100);

        var (scoreColor, statusIcon) = score switch
        {
            >= 70 => ("#047857", MaterialIcons.Verified),
            >= 40 => ("#D99024", MaterialIcons.Warning),
            _ => ("DC2626", MaterialIcons.Warning)
        };

        //// AI-saved scans reference a captured file that may have been
        //// cleaned up by the OS — fall back to a bundled image so every
        //// card keeps the exact same thumbnail UI.
        //var image = !string.IsNullOrWhiteSpace(scan.ImagePath) &&
        //            File.Exists(scan.ImagePath)
        //    ? scan.ImagePath
        //    : "oats_label.webp";

        return new HistoryItem
        {
            Image = scan.ImagePath,
            ProductName = scan.Result.ProductName,
            Score = $"{score}/100",
            ScanDate = FormatScanDate(scan.SavedAtUtc),
            ScoreColor = scoreColor,
            StatusIcon = statusIcon
        };
    }

    private static string FormatScanDate(DateTime savedAtUtc)
    {
        var local = savedAtUtc.ToLocalTime();
        var age = DateTime.Now - local;

        if (age.TotalMinutes < 60)
            return $"{Math.Max(1, (int)age.TotalMinutes)} min ago";

        if (age.TotalHours < 24)
            return $"{(int)age.TotalHours} hrs ago";

        if (age.TotalDays < 2)
            return "Yesterday";

        if (age.TotalDays < 7)
            return local.ToString("ddd");

        return local.ToString("MMM d");
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