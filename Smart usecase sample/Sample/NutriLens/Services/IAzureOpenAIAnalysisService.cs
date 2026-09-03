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
    Task<IngredientAnalysisResult> AnalyzeAsync(
        string extractedText,
        CancellationToken cancellationToken = default);
}
public interface IIngredientOcrService
{
    Task<string> ExtractTextAsync(
        FileResult image,
        CancellationToken cancellationToken = default);
}