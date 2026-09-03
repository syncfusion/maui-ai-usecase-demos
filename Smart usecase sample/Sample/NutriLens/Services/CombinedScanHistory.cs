using NutriLens.Models;

namespace NutriLens.Services;

/// <summary>
/// The three sample historical records that ship with the app.
/// Treated as baseline history — never removed, never persisted.
/// </summary>
public static class SeedHistory
{
    // Timestamps are computed once, relative to first access, so the seeds
    // always render with natural-looking relative dates ("2 hrs ago",
    // "Yesterday", "Oct 12") exactly like the original sample app.
    private static readonly IReadOnlyList<SavedScan> Items = Build();

    public static IReadOnlyList<SavedScan> GetItems() => Items;

    private static IReadOnlyList<SavedScan> Build()
    {
        var now = DateTime.UtcNow;

        return
        [
            // Oldest first — chronological baseline order.
            new SavedScan
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                SavedAtUtc = now.AddDays(-6),
                ImagePath = "yogurt.webp",
                Result = new IngredientAnalysisResult
                {
                    ProductName = "Greek Yogurt",
                    Score = 92,
                    Category = "Excellent",
                    Summary = "High protein, low added sugar. An excellent choice for blood sugar stability.",
                    Recommendation = "Enjoy regularly",
                    Nutrition = new NutritionInfo
                    {
                        ServingSize = "1 cup (170g)",
                        Calories = 100,
                        ProteinGrams = 17,
                        SugarsGrams = 6,
                        CarbohydratesGrams = 6
                    }
                }
            },
            new SavedScan
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                SavedAtUtc = now.AddDays(-1),
                ImagePath = "oats_label.webp",
                Result = new IngredientAnalysisResult
                {
                    ProductName = "Oat & Honey Granola Bar",
                    Score = 74,
                    Category = "Good",
                    Summary = "Whole grain base with fiber, but contains added sugars from honey.",
                    Recommendation = "Good with moderation",
                    Nutrition = new NutritionInfo
                    {
                        ServingSize = "1 bar (40g)",
                        Calories = 160,
                        ProteinGrams = 5,
                        SugarsGrams = 11,
                        AddedSugarsGrams = 8,
                        CarbohydratesGrams = 24
                    }
                }
            },
            new SavedScan
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                SavedAtUtc = now.AddHours(-2),
                ImagePath = "potato.webp",
                Result = new IngredientAnalysisResult
                {
                    ProductName = "Classic Potato Chips",
                    Score = 38,
                    Category = "Poor",
                    Summary = "High in refined carbohydrates, saturated fat and sodium. Spikes blood sugar quickly.",
                    Recommendation = "Limit or seek alternatives",
                    Nutrition = new NutritionInfo
                    {
                        ServingSize = "1 oz (28g)",
                        Calories = 152,
                        ProteinGrams = 2,
                        SugarsGrams = 0,
                        CarbohydratesGrams = 15
                    }
                }
            }
        ];
    }
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