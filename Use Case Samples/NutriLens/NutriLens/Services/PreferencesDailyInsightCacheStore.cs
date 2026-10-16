using System.Text.Json;
using NutriLens.Models;

namespace NutriLens.Services;

/// <summary>
/// <see cref="IDailyInsightCacheStore"/> backed by <see cref="Preferences"/>.
/// </summary>
public sealed class PreferencesDailyInsightCacheStore : IDailyInsightCacheStore
{
    private const string Prefix = "nutrilens.daily_insight.";
    private const string KeysPrefix = "nutrilens.daily_insight.keys.";

    public Task<DailyInsightCacheEntry?> GetAsync(
        string profileKey,
        DateOnly date)
    {
        var key = BuildKey(profileKey, date);

        var raw = Preferences.Default.Get(key, string.Empty);

        if (string.IsNullOrWhiteSpace(raw))
            return Task.FromResult<DailyInsightCacheEntry?>(null);

        try
        {
            var entry = JsonSerializer.Deserialize<DailyInsightCacheEntry>(raw);

            if (entry is null)
                return Task.FromResult<DailyInsightCacheEntry?>(null);

            return Task.FromResult<DailyInsightCacheEntry?>(entry);
        }
        catch
        {
            return Task.FromResult<DailyInsightCacheEntry?>(null);
        }
    }

    public Task SaveAsync(DailyInsightCacheEntry entry)
    {
        var key = BuildKey(entry.ProfileKey, DateOnly.FromDateTime(entry.GeneratedOnUtc));
        var raw = JsonSerializer.Serialize(entry);

        Preferences.Default.Set(key, raw);

        var storedKeys = Preferences.Default.Get(
            $"{KeysPrefix}{entry.ProfileKey}",
            Array.Empty<string>());

        if (!storedKeys.Contains(key, StringComparer.Ordinal))
        {
            var updatedKeys = storedKeys
                .Append(key)
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            Preferences.Default.Set($"{KeysPrefix}{entry.ProfileKey}", updatedKeys);
        }

        return Task.CompletedTask;
    }

    public Task ClearAsync(string profileKey)
    {
        var keys = Preferences.Default.Get(
            $"{KeysPrefix}{profileKey}",
            Array.Empty<string>());

        foreach (var key in keys)
        {
            Preferences.Default.Remove(key);
        }

        Preferences.Default.Remove($"{KeysPrefix}{profileKey}");

        return Task.CompletedTask;
    }

    private static string BuildKey(string profileKey, DateOnly date)
    {
        return $"{Prefix}{profileKey}.{date:yyyy-MM-dd}";
    }
}