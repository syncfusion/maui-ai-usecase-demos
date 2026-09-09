using System.Text.Json.Serialization;

namespace NutriLens.Models;

public sealed class IngredientAnalysisResult
{
    [JsonPropertyName("profile")]
    public string Profile { get; set; } = "Standard";

    [JsonPropertyName("productName")]
    public string ProductName { get; set; } = "Unknown product";

    [JsonPropertyName("extractedText")]
    public string ExtractedText { get; set; } = string.Empty;

    [JsonPropertyName("score")]
    public int Score { get; set; }

    [JsonPropertyName("scoreExplanation")]
    public string ScoreExplanation { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = "Unknown";

    [JsonPropertyName("confidence")]
    public string Confidence { get; set; } = "Unknown";

    [JsonPropertyName("recommendation")]
    public string Recommendation { get; set; } = string.Empty;

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("positiveAttributes")]
    public List<string> PositiveAttributes { get; set; } = [];

    [JsonPropertyName("negativeAttributes")]
    public List<string> NegativeAttributes { get; set; } = [];

    [JsonPropertyName("metabolicConflict")]
    public string MetabolicConflict { get; set; } = string.Empty;

    [JsonPropertyName("alternativeRecommendations")]
    public List<string> AlternativeRecommendations { get; set; } = [];

    [JsonPropertyName("fullIngredients")]
    public string FullIngredients { get; set; } = string.Empty;

    [JsonPropertyName("allergens")]
    public List<string> Allergens { get; set; } = [];

    [JsonPropertyName("artificialColors")]
    public List<string> ArtificialColors { get; set; } = [];

    [JsonPropertyName("additives")]
    public List<AdditiveInfo> Additives { get; set; } = [];

    [JsonPropertyName("preservatives")]
    public List<AdditiveInfo> Preservatives { get; set; } = [];

    [JsonPropertyName("nutrition")]
    public NutritionInfo Nutrition { get; set; } = new();

    [JsonPropertyName("ingredientBreakdown")]
    public List<IngredientBreakdown> IngredientBreakdown { get; set; } = [];

    [JsonIgnore]
    public string ScoreLabelText => Score.ToString();

    [JsonIgnore]
    public string CategoryDisplay =>
        string.IsNullOrWhiteSpace(Category)
            ? "UNKNOWN"
            : Category.Trim().ToUpperInvariant();

    [JsonIgnore]
    public string PositiveSummary =>
        PositiveAttributes.Count > 0
            ? string.Join(Environment.NewLine, PositiveAttributes)
            : "No major positive markers were identified from the available label information.";

    [JsonIgnore]
    public string AlternativeSummary =>
        AlternativeRecommendations.Count > 0
            ? string.Join(Environment.NewLine, AlternativeRecommendations)
            : "Choose products with fewer additives and more whole-food ingredients.";

    [JsonIgnore]
    public string ConcernSummary =>
        NegativeAttributes.Count > 0
            ? string.Join(Environment.NewLine, NegativeAttributes)
            : "No material concerns were identified from the available information.";

    [JsonIgnore]
    public string ConfidenceDisplay =>
        string.IsNullOrWhiteSpace(Confidence)
            ? "Unknown Confidence"
            : $"{CategoryCase(Confidence)} Confidence";

    [JsonIgnore]
    public string FullIngredientsDisplay =>
        string.IsNullOrWhiteSpace(FullIngredients)
            ? (string.IsNullOrWhiteSpace(ExtractedText)
                ? "Ingredient list was not available on this label."
                : ExtractedText.Trim())
            : FullIngredients.Trim();

    [JsonIgnore]
    public List<AdditiveInfo> BreakdownAdditives
    {
        get
        {
            var combined = Additives.Concat(Preservatives).ToList();

            if (combined.Count > 0)
                return combined;

            return IngredientBreakdown
                .Where(i => !string.IsNullOrWhiteSpace(i.Name))
                .Select(i => new AdditiveInfo
                {
                    Name = i.Name,
                    Purpose = i.Purpose,
                    SafetyNote = i.Explanation
                })
                .ToList();
        }
    }

    [JsonIgnore]
    public string BenefitTitle =>
        TitleFrom(PositiveAttributes.FirstOrDefault(), "Benefits Identified");

    [JsonIgnore]
    public string BenefitDescription =>
        PositiveAttributes.FirstOrDefault()
            ?? "The analysis did not identify specific positive attributes for this product.";

    [JsonIgnore]
    public string ConcernTitle =>
        TitleFrom(NegativeAttributes.FirstOrDefault(), "Concerns Identified");

    [JsonIgnore]
    public string ConcernDescription
    {
        get
        {
            var concern = NegativeAttributes.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(concern))
                concern = !string.IsNullOrWhiteSpace(MetabolicConflict)
                    ? MetabolicConflict
                    : "The analysis did not identify specific concerns for this product.";

            if (Allergens.Count > 0)
                concern += $"{Environment.NewLine}Allergens: {string.Join(", ", Allergens)}.";

            return concern;
        }
    }

    [JsonIgnore]
    public string InfoTitle =>
        TitleFrom(ScoreExplanation, $"Why Score {Score}");

    [JsonIgnore]
    public string InfoDescription =>
        string.IsNullOrWhiteSpace(ScoreExplanation)
            ? (!string.IsNullOrWhiteSpace(Summary)
                ? Summary
                : "A detailed score explanation was not provided by the analysis.")
            : ScoreExplanation;

    private static string TitleFrom(string? text, string fallback)
    {
        if (string.IsNullOrWhiteSpace(text))
            return fallback;

        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (words.Length <= 4)
            return text.TrimEnd('.');

        return string.Join(' ', words.Take(4)) + "...";
    }

    private static string CategoryCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Unknown";

        return char.ToUpperInvariant(value[0]) + value[1..].ToLowerInvariant();
    }
}

public sealed class AdditiveInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("purpose")]
    public string Purpose { get; set; } = string.Empty;

    [JsonPropertyName("safetyNote")]
    public string SafetyNote { get; set; } = string.Empty;

    [JsonIgnore]
    public string PurposeDisplay =>
        string.IsNullOrWhiteSpace(Purpose)
            ? "Additive present in this product."
            : Purpose;

    [JsonIgnore]
    public string SafetyDisplay =>
        string.IsNullOrWhiteSpace(SafetyNote)
            ? "Safety Status: Not specified by the analysis."
            : $"Safety Status: {SafetyNote}";
}

public sealed class IngredientBreakdown
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("purpose")]
    public string Purpose { get; set; } = string.Empty;

    [JsonPropertyName("riskLevel")]
    public string RiskLevel { get; set; } = "Unknown";

    [JsonPropertyName("explanation")]
    public string Explanation { get; set; } = string.Empty;

    [JsonIgnore]
    public string BadgeText =>
        string.IsNullOrWhiteSpace(RiskLevel)
            ? "Unknown"
            : RiskLevel.Trim();

    [JsonIgnore]
    public string BadgeBackground =>
        RiskLevel.ToLowerInvariant() switch
        {
            "high" => "#FFE4E6",
            "moderate" => "#FDE7C7",
            "low" => "#E7F7F1",
            _ => "#E5E7EB"
        };
}