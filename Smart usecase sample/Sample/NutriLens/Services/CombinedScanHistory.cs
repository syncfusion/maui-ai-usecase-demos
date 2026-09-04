using NutriLens.Models;

namespace NutriLens.Services;

/// <summary>
/// The three sample historical records that ship with the app.
/// Treated as baseline history — never removed, never persisted.
/// The Result of each seed is the exact static analysis instance from
/// <see cref="SampleIngredientAnalysisService"/>, so tapping a seed card
/// always opens the fully-populated mock analysis page.
/// </summary>
public static class SeedHistory
{
    // Timestamps are computed once, relative to first access, so the seeds
    // always render with natural-looking relative dates.
    private static readonly IReadOnlyList<SavedScan> Items = Build();

    public static IReadOnlyList<SavedScan> GetItems() => Items;

    private static IReadOnlyList<SavedScan> Build()
    {
        var service = new SampleIngredientAnalysisService();
        var now = DateTime.UtcNow;

        return
        [
            // Oldest first — chronological baseline order.
            Scan(service, "Yogurt", "yogurt.webp", now.AddDays(-6)),
            Scan(service, "Chips", "potato.webp", now.AddDays(-1)),
            Scan(service, "Oats", "oats_label.webp", now.AddHours(-2))
        ];
    }

    private static SavedScan Scan(
        SampleIngredientAnalysisService service,
        string productName,
        string imagePath,
        DateTime savedAtUtc) => new()
        {
            Id = Guid.NewGuid(),
            SavedAtUtc = savedAtUtc,
            ImagePath = imagePath,
            Result = service.TryGet(productName)
            ?? new IngredientAnalysisResult { ProductName = productName }
        };
}

/// <summary>
/// Unified history source: seed (sample) records + saved AI-generated records.
/// CombinedHistory = SeedHistoryItems + SavedAnalysisItems, newest first.
/// This is the single data source for HistoryPage, TrendsPage and statistics.
/// </summary>
public interface ICombinedScanHistory
{
    Task<IReadOnlyList<SavedScan>> GetCombinedAsync();
}

public sealed class CombinedScanHistory : ICombinedScanHistory
{
    private readonly IScanHistoryStore store;

    public CombinedScanHistory(IScanHistoryStore store)
    {
        this.store = store;
    }

    public async Task<IReadOnlyList<SavedScan>> GetCombinedAsync()
    {
        var saved = await store.GetAllAsync();

        return SeedHistory.GetItems()
            .Concat(saved)
            .Where(s => s is not null && s.Result is not null)
            .OrderByDescending(s => s.SavedAtUtc)
            .ToList();
    }
}