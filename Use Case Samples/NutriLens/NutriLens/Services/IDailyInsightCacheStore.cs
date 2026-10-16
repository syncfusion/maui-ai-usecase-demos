using NutriLens.Models;

namespace NutriLens.Services;

/// <summary>
/// Per-day cache of generated daily insights. Stores AI results only —
/// fallback text must not be cached so a transient failure does not stick
/// for the rest of the day.
/// </summary>
public interface IDailyInsightCacheStore
{
    Task<DailyInsightCacheEntry?> GetAsync(string profileKey, DateOnly date);
    Task SaveAsync(DailyInsightCacheEntry entry);
    Task ClearAsync(string profileKey);
}