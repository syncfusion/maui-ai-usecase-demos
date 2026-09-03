using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NutriLens.Models;

public sealed class DailyInsightGenerator : IDailyInsightGenerator
{
    private readonly IAzureOpenAIChatService aiService;
    private readonly IDailyInsightCacheStore cacheStore;

    public DailyInsightGenerator(
        IAzureOpenAIChatService aiService,
        IDailyInsightCacheStore cacheStore)
    {
        this.aiService = aiService;
        this.cacheStore = cacheStore;
    }

    public async Task<string> GetTodayInsightAsync(
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var profileKey = "default";

        var cached = await cacheStore.GetAsync(profileKey, today);

        if (cached is not null &&
            !string.IsNullOrWhiteSpace(cached.InsightText))
        {
            Debug.WriteLine(
                $"[DailyInsight] Reusing cached insight for {today:yyyy-MM-dd}");

            return cached.InsightText;
        }

        var context = BuildNutritionContext();
        var prompt = BuildPrompt(context);

        var generated = await aiService.GetCompletionAsync(prompt, cancellationToken);

        if (!string.IsNullOrWhiteSpace(generated))
        {
            var entry = new DailyInsightCacheEntry
            {
                InsightText = generated,
                GeneratedOnUtc = DateTime.UtcNow,
                ProfileKey = profileKey,
                Source = "ai"
            };

            await cacheStore.SaveAsync(entry);

            return generated;
        }

        var fallback = BuildFallbackInsight(context);

        await cacheStore.SaveAsync(new DailyInsightCacheEntry
        {
            InsightText = fallback,
            GeneratedOnUtc = DateTime.UtcNow,
            ProfileKey = profileKey,
            Source = "fallback"
        });

        return fallback;
    }

    private static NutritionInsightContext BuildNutritionContext()
    {
        // The app currently does not have a full nutrition repository, so this uses
        // the latest user context available in memory/profile defaults and recent eating patterns.
        return new NutritionInsightContext
        {
            UserName = "Alex",
            Goal = "Reduce sugar intake and improve daily energy stability",
            HealthFocus = "Diabetes-friendly eating and better blood sugar control",
            Calories = 1880,
            ProteinGrams = 96,
            CarbohydrateGrams = 210,
            SugarGrams = 42,
            FiberGrams = 28,
            HydrationLiters = 2.0,
            RecentMeals = new[]
            {
                "oatmeal with berries",
                "grilled chicken salad",
                "Greek yogurt and fruit",
                "whole grain wrap",
                "vegetable stir-fry with tofu"
            }
        };
    }

    private static string BuildPrompt(NutritionInsightContext context)
    {
        var recentMeals = string.Join(", ", context.RecentMeals);

        return $"""
User name: {context.UserName}
Goal: {context.Goal}
Health focus: {context.HealthFocus}
Today's calories: {context.Calories}
Protein: {context.ProteinGrams}g
Carbohydrates: {context.CarbohydrateGrams}g
Sugar: {context.SugarGrams}g
Fiber: {context.FiberGrams}g
Water target: {context.HydrationLiters}L
Recent meals: {recentMeals}

Write one practical daily nutrition insight for this user.
It should be positive, specific, and easy to understand.
Mention fiber, protein, sugar control, and one healthy choice they can make today.
Keep it to two sentences max.
Return only the final insight text.
""";
    }

    private static string BuildFallbackInsight(NutritionInsightContext context)
    {
        return
            "Your recent meals show a strong pattern of balanced choices. " +
            "Keep building around lean protein, fiber-rich foods, and lower-sugar options to support steadier energy and better blood sugar control today.";
    }

    private sealed class NutritionInsightContext
    {
        public string UserName { get; set; } = string.Empty;
        public string Goal { get; set; } = string.Empty;
        public string HealthFocus { get; set; } = string.Empty;
        public int Calories { get; set; }
        public int ProteinGrams { get; set; }
        public int CarbohydrateGrams { get; set; }
        public int SugarGrams { get; set; }
        public int FiberGrams { get; set; }
        public double HydrationLiters { get; set; }
        public string[] RecentMeals { get; set; } = Array.Empty<string>();
    }
}
public interface IDailyInsightGenerator
{
    Task<string> GetTodayInsightAsync(
        CancellationToken cancellationToken = default);
}
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
public interface IDailyInsightCacheStore
{
    Task<DailyInsightCacheEntry?> GetAsync(string profileKey, DateOnly date);
    Task SaveAsync(DailyInsightCacheEntry entry);
    Task ClearAsync(string profileKey);
}
public interface IAzureOpenAIChatService
{
    Task<string> GetCompletionAsync(
        string prompt,
        CancellationToken cancellationToken = default);
}
public sealed class DailyInsightCacheEntry
{
    [JsonPropertyName("insightText")]
    public string InsightText { get; set; } = string.Empty;

    [JsonPropertyName("generatedOnUtc")]
    public DateTime GeneratedOnUtc { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("profileKey")]
    public string ProfileKey { get; set; } = "default";

    [JsonPropertyName("source")]
    public string Source { get; set; } = "ai";
}
public sealed class HealthTrendPoint
{
    public string Day { get; init; } = string.Empty;
    public double Score { get; init; }
}

public sealed class AdditiveTrendItem
{
    public string Name { get; init; } = string.Empty;
    public string Percentage { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public string Color { get; init; } = "#F59E0B";
}