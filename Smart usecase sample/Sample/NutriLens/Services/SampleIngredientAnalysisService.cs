using NutriLens.Models;
using System.Collections.ObjectModel;

namespace NutriLens.Services;

public interface ISampleAnalysisDataService
{
    /// <summary>
    /// Returns true when <paramref name="productName"/> matches one of the
    /// predefined sample products (ordinal, case-insensitive, trimmed).
    /// </summary>
    bool IsSampleProduct(string productName);

    /// <summary>
    /// Returns the predefined static analysis for a sample product, or
    /// <c>null</c> when the name is not a sample product.
    /// </summary>
    IngredientAnalysisResult? TryGet(string productName);

    /// <summary>Read-only list of the sample product names.</summary>
    IReadOnlyCollection<string> SampleProductNames { get; }
}

/// <summary>
/// Mock analysis repository. Holds three fully-populated
/// <see cref="IngredientAnalysisResult"/> objects (Oats, Yogurt, Chips)
/// in the exact format <c>AnalyzeIngredientsResultPage</c> binds against.
/// 100% predefined static data — no API, no dynamic generation.
/// </summary>
public sealed class SampleIngredientAnalysisService
    : ISampleAnalysisDataService
{
    private readonly IReadOnlyDictionary<string, IngredientAnalysisResult> byKey;

    public SampleIngredientAnalysisService()
    {
        var samples = new[]
        {
            BuildOats(),
            BuildYogurt(),
            BuildChips()
        };

        var dict = new Dictionary<string, IngredientAnalysisResult>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var sample in samples)
        {
            if (!dict.ContainsKey(Normalize(sample.ProductName)))
                dict[Normalize(sample.ProductName)] = sample;
        }

        byKey = dict;
        SampleProductNames = new ReadOnlyCollection<string>(
            samples.Select(s => s.ProductName).ToList());
    }

    public IReadOnlyCollection<string> SampleProductNames { get; }

    public bool IsSampleProduct(string productName)
        => byKey.ContainsKey(Normalize(productName));

    public IngredientAnalysisResult? TryGet(string productName)
    {
        if (string.IsNullOrWhiteSpace(productName))
            return null;

        return byKey.TryGetValue(Normalize(productName), out var result)
            ? result
            : null;
    }

    private static string Normalize(string value)
        => (value ?? string.Empty).Trim();

    // -----------------------------------------------------------------
    // Sample 1 — OATS (Score 92)
    // -----------------------------------------------------------------
    private static IngredientAnalysisResult BuildOats() => new()
    {
        Profile = "Diabetes",
        ProductName = "Oats",
        ExtractedText = "Whole grain oats.",
        Score = 92,
        ScoreExplanation =
            "High soluble-fiber (beta-glucan) content with zero added sugar " +
            "keeps the glycemic load low and supports steady blood sugar.",
        Category = "Whole Grain",
        Confidence = "High",
        Recommendation = "Excellent healthy breakfast option.",
        Summary =
            "Oats are a nutrient-dense whole grain rich in soluble fiber and " +
            "beneficial for heart health and digestion.",
        PositiveAttributes =
        [
            "High Fiber",
            "Heart Healthy",
            "Supports Digestion"
        ],
        NegativeAttributes =
        [
            "May contain gluten traces"
        ],
        MetabolicConflict =
            "Minimal. Oats have a low glycemic load; products processed in " +
            "shared facilities may carry gluten traces — relevant only for " +
            "gluten-sensitive users.",
        AlternativeRecommendations =
        [
            "Choose certified gluten-free oats if you are gluten sensitive.",
            "Add nuts or seeds for extra protein and healthy fats."
        ],
        FullIngredients = "Whole grain oats.",
        Allergens = ["Gluten (traces)"],
        ArtificialColors = [],
        Additives = [],
        Preservatives = [],
        Nutrition = new NutritionInfo
        {
            ServingSize = "1/2 cup dry (40g)",
            ServingsPerContainer = "10",
            Calories = 150,
            TotalFatGrams = 3,
            SaturatedFatGrams = 0.5,
            SodiumMg = 2,
            CarbohydratesGrams = 27,
            SugarsGrams = 1,
            AddedSugarsGrams = 0,
            ProteinGrams = 5,
            FiberGrams = 4
        },
        IngredientBreakdown =
        [
            new IngredientBreakdown
            {
                Name = "Whole Grain Oats",
                Purpose = "Primary ingredient — fiber and sustained energy.",
                RiskLevel = "Low",
                Explanation =
                    "Beta-glucan soluble fiber supports healthy cholesterol " +
                    "and slow, steady glucose release."
            }
        ]
    };

    // -----------------------------------------------------------------
    // Sample 2 — YOGURT (Score 88)
    // -----------------------------------------------------------------
    private static IngredientAnalysisResult BuildYogurt() => new()
    {
        Profile = "Diabetes",
        ProductName = "Yogurt",
        ExtractedText = "Milk, live cultures.",
        Score = 88,
        ScoreExplanation =
            "High protein with probiotics and no artificial additives; " +
            "moderate naturally occurring sugars keep blood sugar stable.",
        Category = "Dairy Product",
        Confidence = "High",
        Recommendation = "Good daily snack or breakfast addition.",
        Summary =
            "Yogurt provides protein and probiotics that help maintain " +
            "digestive health and support a balanced diet.",
        PositiveAttributes =
        [
            "Rich in Protein",
            "Contains Probiotics",
            "Supports Gut Health"
        ],
        NegativeAttributes =
        [
            "May contain added sugar"
        ],
        MetabolicConflict =
            "Naturally occurring lactose and possible added sugars can raise " +
            "glucose — prefer unsweetened varieties when monitoring blood sugar.",
        AlternativeRecommendations =
        [
            "Choose plain unsweetened yogurt and add fresh fruit for sweetness.",
            "Coconut-milk yogurt (unsweetened) for dairy-sensitive users."
        ],
        FullIngredients = "Milk, live cultures.",
        Allergens = ["Milk"],
        ArtificialColors = [],
        Additives = [],
        Preservatives = [],
        Nutrition = new NutritionInfo
        {
            ServingSize = "1 cup (170g)",
            ServingsPerContainer = "1",
            Calories = 100,
            TotalFatGrams = 0.5,
            SaturatedFatGrams = 0,
            SodiumMg = 60,
            CarbohydratesGrams = 8,
            SugarsGrams = 8,
            AddedSugarsGrams = 2,
            ProteinGrams = 17,
            FiberGrams = 0
        },
        IngredientBreakdown =
        [
            new IngredientBreakdown
            {
                Name = "Milk",
                Purpose = "Primary base providing protein and calcium.",
                RiskLevel = "Low",
                Explanation =
                    "Milk protein supports satiety; lactose content is " +
                    "manageable within a balanced diet."
            },
            new IngredientBreakdown
            {
                Name = "Live Cultures",
                Purpose = "Probiotic strains for fermentation and gut health.",
                RiskLevel = "Low",
                Explanation =
                    "Live active cultures support gut microbiome diversity " +
                    "and digestive health."
            }
        ]
    };

    // -----------------------------------------------------------------
    // Sample 3 — CHIPS (Score 40)
    // -----------------------------------------------------------------
    private static IngredientAnalysisResult BuildChips() => new()
    {
        Profile = "Diabetes",
        ProductName = "Chips",
        ExtractedText = "Potatoes, vegetable oil, salt.",
        Score = 40,
        ScoreExplanation =
            "Refined starch fried in oil with elevated sodium — high glycemic, " +
            "calorie dense, and minimal fiber or protein.",
        Category = "Processed Snack",
        Confidence = "High",
        Recommendation = "Consume occasionally and in moderation.",
        Summary =
            "Chips are a processed snack typically high in sodium and fat, " +
            "making them suitable only for occasional consumption.",
        PositiveAttributes =
        [
            "Convenient snack"
        ],
        NegativeAttributes =
        [
            "High Sodium",
            "High Fat",
            "Highly Processed"
        ],
        MetabolicConflict =
            "High-glycemic potato starch combined with frying fat can cause " +
            "sharp post-meal glucose spikes — keep portions small.",
        AlternativeRecommendations =
        [
            "Baked vegetable chips or air-popped popcorn.",
            "Roasted chickpeas for a protein-and-fiber-rich alternative."
        ],
        FullIngredients = "Potatoes, vegetable oil, salt.",
        Allergens = [],
        ArtificialColors = [],
        Additives = [],
        Preservatives = [],
        Nutrition = new NutritionInfo
        {
            ServingSize = "1 oz (28g)",
            ServingsPerContainer = "10",
            Calories = 152,
            TotalFatGrams = 10,
            SaturatedFatGrams = 1.5,
            SodiumMg = 170,
            CarbohydratesGrams = 15,
            SugarsGrams = 0,
            AddedSugarsGrams = 0,
            ProteinGrams = 2,
            FiberGrams = 1
        },
        IngredientBreakdown =
        [
            new IngredientBreakdown
            {
                Name = "Potatoes",
                Purpose = "Primary ingredient.",
                RiskLevel = "Moderate",
                Explanation =
                    "Frying concentrates starch into a high-glycemic snack " +
                    "with little remaining fiber."
            },
            new IngredientBreakdown
            {
                Name = "Vegetable Oil",
                Purpose = "Frying medium.",
                RiskLevel = "Moderate",
                Explanation =
                    "Adds significant fat calories per serving; repeatedly " +
                    "heated oil can degrade quality."
            },
            new IngredientBreakdown
            {
                Name = "Salt",
                Purpose = "Flavor.",
                RiskLevel = "High",
                Explanation =
                    "170mg sodium per serving contributes to the daily " +
                    "sodium load and blood-pressure concerns."
            }
        ]
    };
}