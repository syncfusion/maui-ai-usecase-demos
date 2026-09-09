using NutriLens.Services;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NutriLens.Models;

public sealed class DailyInsightGenerator : IDailyInsightGenerator
{
    private readonly IAzureOpenAIChatService aiService;
    private readonly IDailyInsightCacheStore cacheStore;
    private readonly IScanHistoryStore scanStore;

    public DailyInsightGenerator(IAzureOpenAIChatService aiService,
        IDailyInsightCacheStore cacheStore,
        IScanHistoryStore scanStore)
    {
        this.aiService = aiService;
        this.cacheStore = cacheStore;
        this.scanStore = scanStore;
    }

    public async Task<string> GetTodayInsightAsync(
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var profileKey = "default";

        var cached = await cacheStore.GetAsync(profileKey, today);

        // FIX (Bug 2a): a cached FALLBACK must never be served as final data.
        // Only genuine AI results count as a valid cache hit — anything else
        // falls through so the AI call is retried automatically.
        if (cached is not null &&
            string.Equals(cached.Source, "ai", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(cached.InsightText))
        {
            Debug.WriteLine(
                $"[DailyInsight] Reusing AI-generated insight for {today:yyyy-MM-dd}");

            return cached.InsightText;
        }

        // FIX (Bug 3): build the prompt from the user's REAL saved scan
        // history — the same data source every other page now uses —
        // instead of invented meals and numbers.
        var context = await BuildNutritionContextAsync(cancellationToken);
        var prompt = BuildPrompt(context);

        var generated = await aiService.GetCompletionAsync(prompt, cancellationToken);

        if (!string.IsNullOrWhiteSpace(generated))
        {
            await cacheStore.SaveAsync(new DailyInsightCacheEntry
            {
                InsightText = generated,
                GeneratedOnUtc = DateTime.UtcNow,
                ProfileKey = profileKey,
                Source = "ai"
            });

            Debug.WriteLine($"[DailyInsight] AI insight generated for {today:yyyy-MM-dd}");

            return generated;
        }

        // FIX (Bug 2b): a transient AI failure must NOT be cached. Return the
        // fallback for this appearance only — the next dashboard visit retries
        // the AI call instead of showing canned text all day.
        Debug.WriteLine(
            "[DailyInsight] AI call returned no content; using uncached fallback (will retry).");

        return BuildFallbackInsight(context);
    }

    private async Task<NutritionInsightContext> BuildNutritionContextAsync(
        CancellationToken cancellationToken)
    {
        var context = new NutritionInsightContext
        {
            UserName = "Alex",
            Goal = "Reduce sugar intake and improve daily energy stability",
            HealthFocus = "Diabetes-friendly eating and better blood sugar control"
        };

        try
        {
            var scans = (await scanStore.GetAllAsync())
                .Where(s => s is not null && s.Result is not null)
                .OrderBy(s => s.SavedAtUtc)
                .ToList();

            if (scans.Count > 0)
            {
                context.TotalScans = scans.Count;
                context.AverageScore = (int)Math.Round(
                    scans.Average(s => s.Result.Score));
                context.BestProduct = scans
                    .OrderByDescending(s => s.Result.Score)
                    .First();
                context.WorstProduct = scans
                    .OrderBy(s => s.Result.Score)
                    .First();

                context.RecentProducts = scans
                    .TakeLast(5)
                    .Select(s => new ScannedProduct
                    {
                        Name = s.Result.ProductName,
                        Score = s.Result.Score,
                        Category = s.Result.Category
                    })
                    .ToArray();

                // Real top concerns: High/Moderate-risk ingredients flagged
                // by the AI analyses across all saved scans.
                context.TopConcerns = scans
                    .SelectMany(s => s.Result.IngredientBreakdown)
                    .Where(i =>
                        !string.IsNullOrWhiteSpace(i.Name) &&
                        (string.Equals(i.RiskLevel, "High", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(i.RiskLevel, "Moderate", StringComparison.OrdinalIgnoreCase)))
                    .GroupBy(i => i.Name.Trim(), StringComparer.OrdinalIgnoreCase)
                    .OrderByDescending(g => g.Count())
                    .Take(3)
                    .Select(g => g.Key)
                    .ToArray();

                // Real nutrition averages from labels the user actually scanned.
                var proteins = scans
                    .Select(s => s.Result.Nutrition.ProteinGrams)
                    .Where(p => p is > 0)
                    .Select(p => p!.Value)
                    .ToList();

                var sugars = scans
                    .Select(s => s.Result.Nutrition.SugarsGrams ?? s.Result.Nutrition.AddedSugarsGrams)
                    .Where(g => g is > 0)
                    .Select(g => g!.Value)
                    .ToList();

                if (proteins.Count > 0)
                    context.AverageProteinGrams = Math.Round(proteins.Average(), 1);

                if (sugars.Count > 0)
                    context.AverageSugarGrams = Math.Round(sugars.Average(), 1);
            }
        }
        catch (Exception ex)
        {
            // History is a nice-to-have for the prompt — never let store
            // issues break the insight flow.
            Debug.WriteLine($"[DailyInsight] History load failed: {ex}");
        }

        return context;
    }

    private static string BuildPrompt(NutritionInsightContext context)
    {
        var builder = new StringBuilder();

        builder.AppendLine(
            $"""
            User name: {context.UserName}
            Goal: {context.Goal}
            Health focus: {context.HealthFocus}
            """);

        if (context.TotalScans > 0)
        {
            builder.AppendLine($"""
            The user has scanned {context.TotalScans} food products with this app.
            Average health score of scanned products: {context.AverageScore}/100
            Healthiest scan: {context.BestProduct.Result.ProductName} (score {context.BestProduct.Result.Score})
            Least healthy scan: {context.WorstProduct.Result.ProductName} (score {context.WorstProduct.Result.Score})
            """);

            if (context.RecentProducts.Length > 0)
            {
                var products = string.Join(", ",
                    context.RecentProducts.Select(p => $"{p.Name} ({p.Score}/100)"));

                builder.AppendLine($"Recently scanned products: {products}");
            }

            if (context.TopConcerns.Length > 0)
            {
                builder.AppendLine(
                    $"Ingredients most frequently flagged in their scans: {string.Join(", ", context.TopConcerns)}");
            }

            if (context.AverageProteinGrams is > 0)
                builder.AppendLine($"Average protein per scanned serving: {context.AverageProteinGrams}g");

            if (context.AverageSugarGrams is > 0)
                builder.AppendLine($"Average sugars per scanned serving: {context.AverageSugarGrams}g");

            builder.AppendLine();
            builder.AppendLine(
                """
                Write one practical daily nutrition insight for this user based on their
                actual scan history. Reference their real products, scores or flagged
                ingredients where relevant. Be positive, specific, and easy to understand.
                Suggest one healthy choice they can make today.
                Keep it to two sentences max.
                Return only the final insight text.
                """);
        }
        else
        {
            builder.AppendLine(
                """
                The user has not scanned any products yet.
                Write one welcoming daily nutrition insight that encourages them to scan
                their first food label. Mention steady energy and blood sugar control.
                Keep it to two sentences max.
                Return only the final insight text.
                """);
        }

        return builder.ToString();
    }

    private static string BuildFallbackInsight(NutritionInsightContext context)
    {
        // Uncached fallback — only shown once per failed AI call, and only
        // until the next dashboard appearance retries the AI.
        return
            "A balanced plate with more fiber and lean protein can help support steady " +
            "energy and better blood sugar control today.";
    }

    private sealed class ScannedProduct
    {
        public string Name { get; set; } = string.Empty;
        public int Score { get; set; }
        public string Category { get; set; } = string.Empty;
    }

    private sealed class NutritionInsightContext
    {
        public string UserName { get; set; } = string.Empty;
        public string Goal { get; set; } = string.Empty;
        public string HealthFocus { get; set; } = string.Empty;
        public int TotalScans { get; set; }
        public int AverageScore { get; set; }
        public double AverageProteinGrams { get; set; }
        public double AverageSugarGrams { get; set; }
        public SavedScan? BestProduct { get; set; }
        public SavedScan? WorstProduct { get; set; }
        public ScannedProduct[] RecentProducts { get; set; } = [];
        public string[] TopConcerns { get; set; } = [];
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