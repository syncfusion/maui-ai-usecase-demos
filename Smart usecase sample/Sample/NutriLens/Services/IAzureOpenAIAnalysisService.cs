using NutriLens.Models;

namespace NutriLens.Services;

public interface IIngredientImageExtractionService
{
    Task<string> ExtractContentAsync(
        FileResult image,
        CancellationToken cancellationToken = default);
}
public interface IAzureOpenAIAnalysisService
{
    Task<ProductAnalysis> AnalyzeAsync(
        FileResult image,
        CancellationToken cancellationToken = default);
}

public interface IAzureOpenAIIngredientService
{
    /// <summary>Existing default (no personalization) — unchanged behavior.</summary>
    Task<IngredientAnalysisResult> AnalyzeAsync(
        string extractedText,
        CancellationToken cancellationToken = default);

    /// <summary>Personalized analysis path used by Review page.</summary>
    Task<IngredientAnalysisResult> AnalyzeAsync(
        string extractedText,
        UserDietaryPreference? preferences,
        CancellationToken cancellationToken = default);
}
public interface IIngredientOcrService
{
    Task<string> ExtractTextAsync(
        FileResult image,
        CancellationToken cancellationToken = default);
}