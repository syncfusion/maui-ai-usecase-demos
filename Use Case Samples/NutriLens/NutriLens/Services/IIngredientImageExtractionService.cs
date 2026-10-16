namespace NutriLens.Services;

/// <summary>
/// Reads a captured label image and returns plain text suitable for
/// downstream ingredient parsing and AI analysis.
/// </summary>
public interface IIngredientImageExtractionService
{
    Task<string> ExtractContentAsync(
        FileResult image,
        CancellationToken cancellationToken = default);
}