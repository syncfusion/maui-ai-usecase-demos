using System.Diagnostics;
using System.Text;
using NutriLens.Models;

namespace NutriLens.Services;

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

        // Only genuine AI results are valid cache hits — fallback text is
        // never cached, so a cached fallback would silently bypass retries.
        if (cached is not null &&
            string.Equals(cached.Source, "ai", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(cached.InsightText))
        {
            Debug.WriteLine(
                $"[DailyInsight] Reusing AI-generated insight for {today:yyyy-MM-dd}");

            return cached.InsightText;
        }

        // Build the prompt from the user's saved scan history so the insight
        // reflects products they actually scanned, not invented data.
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

        // A transient AI failure must not be cached. Return the fallback
        // for this appearance only; the next dashboard visit retries the
        // AI call instead of showing canned text all day.
        Debug.WriteLine(
            "[DailyInsight] AI call returned no content; using uncached fallback (will retry).");

        return BuildFallbackInsight(context);
    }

    private async Task<NutritionInsightContext> BuildNutritionContextAsync(
        CancellationToken cancellationToken)
    {
        // Default placeholder identity — a real app would read this from
        // the user's saved profile preferences.
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
            Healthiest scan: {context.BestProduct?.Result?.ProductName ?? "Unknown product"} (score {context.BestProduct?.Result?.Score ?? 0})
            Least healthy scan: {context.WorstProduct?.Result?.ProductName ?? "Unknown product"} (score {context.WorstProduct?.Result?.Score ?? 0})
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
        // Uncached fallback — only shown once per failed AI call, until the
        // next dashboard appearance retries the AI.
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