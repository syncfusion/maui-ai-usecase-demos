namespace NutriLens.Models;

public sealed class StaticIngredientAnalysis
{
    public string Profile { get; init; } =
        "PERSONALIZED FOR: DIABETES CONSIDERATIONS";

    public int Score { get; init; } = 68;

    public string Category { get; init; } = "MODERATE";

    public string Recommendation { get; init; } =
        "Proceed with Caution";

    public string RecommendationDescription { get; init; } =
        "High in processed fats, moderate sugar. " +
        "May cause blood sugar spikes based on your profile.";

    public IReadOnlyList<string> PositiveAttributes { get; init; } =
    [
        "Contains natural dietary fiber",
        "No artificial colors or dyes"
    ];

    public string MetabolicConflict { get; init; } =
        "May not align with your blood sugar goals due to refined carbohydrates " +
        "compounding with saturated fats.";

    public IReadOnlyList<StaticIngredientItem> Ingredients { get; init; } =
    [
        new StaticIngredientItem
        {
            Name = "Palm Oil",
            Badge = "High concern",
            BadgeBackground = "#FFE4E6",
            BadgeTextColor = "#DC2626",
            Description =
                "High in saturated fats which can negatively impact lipid " +
                "profiles when combined with insulin resistance."
        },
        new StaticIngredientItem
        {
            Name = "Cane Sugar",
            Badge = "Consider limiting",
            BadgeBackground = "#FDE7C7",
            BadgeTextColor = "#B45309",
            Description =
                "Added sugars contribute directly to glycemic load and rapid " +
                "insulin response."
        },
        new StaticIngredientItem
        {
            Name = "Soy Lecithin",
            Badge = "No concern",
            BadgeBackground = "#E7F7F1",
            BadgeTextColor = "#0F766E",
            Description =
                "Common emulsifier. Generally recognized as safe and " +
                "metabolically neutral in small quantities."
        },
        new StaticIngredientItem
        {
            Name = "Sea Salt",
            Badge = "No concern",
            BadgeBackground = "#E7F7F1",
            BadgeTextColor = "#0F766E",
            Description =
                "Within normal sodium limits for standard daily intake."
        }
    ];

    public string AlternativeRecommendation { get; init; } =
        "Products utilizing avocado or coconut oils provide healthier fat " +
        "profiles that are less inflammatory for metabolic syndrome. Look for " +
        "items sweetened with stevia, monk fruit, or containing no added sugars.";
}

public sealed class StaticIngredientItem
{
    public string Name { get; init; } = string.Empty;
    public string Badge { get; init; } = string.Empty;
    public string BadgeBackground { get; init; } = string.Empty;
    public string BadgeTextColor { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}