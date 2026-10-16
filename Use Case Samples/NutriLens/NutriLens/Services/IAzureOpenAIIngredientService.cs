using NutriLens.Models;

namespace NutriLens.Services;

/// <summary>
/// Sends extracted OCR text from a food label to Azure OpenAI and
/// returns the structured ingredient analysis for the product.
/// </summary>
public interface IAzureOpenAIIngredientService
{
    Task<IngredientAnalysisResult> AnalyzeAsync(
        string extractedText,
        CancellationToken cancellationToken = default);

    Task<IngredientAnalysisResult> AnalyzeAsync(
        string extractedText,
        UserDietaryPreference? preferences,
        CancellationToken cancellationToken = default);
}