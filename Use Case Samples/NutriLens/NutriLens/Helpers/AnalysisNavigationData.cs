using NutriLens.Models;

namespace NutriLens.Helpers;

public static class AnalysisNavigationData
{
    public static IngredientAnalysisResult? CurrentResult { get; set; }
    public static string? CurrentImagePath { get; set; }
    public static PendingReviewData? PendingReview { get; set; }
}

public sealed class PendingReviewData
{
    public required string ExtractedText { get; init; }
    public required FileResult Image { get; init; }
}