using NutriLens.Helpers;
using NutriLens.Models;
using NutriLens.Services;

namespace NutriLens.Views;

public partial class AnalyzeIngredientsResultPage : ContentPage
{
    public AnalyzeIngredientsResultPage()
        : this(AnalysisNavigationData.CurrentResult)
    {
    }

    public AnalyzeIngredientsResultPage(IngredientAnalysisResult? result)
    {
        InitializeComponent();

        BindingContext = result ?? BuildFallbackResult();
    }

    private static IngredientAnalysisResult BuildFallbackResult() => new()
    {
        Profile = "Diabetes",
        ProductName = "Unknown product",
        Score = 0,
        Category = "Unknown",
        Confidence = "Unknown",
        Recommendation = "Unable to analyze this product.",
        Summary = "No completed analysis was found for this item.",
        ScoreExplanation =
            "No completed analysis was found for this item.",
        MetabolicConflict =
            "No detailed metabolic conflict data is available.",
        AlternativeRecommendations =
        [
            "Choose products with fewer additives and more whole-food ingredients."
        ]
    };

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnSaveResultClicked(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not IngredientAnalysisResult currentResult)
        {
            await DisplayAlertAsync(
                "Save Result",
                "No analysis is available to save.",
                "OK");
            return;
        }

        try
        {
            var store = Resolve<IScanHistoryStore>() ?? new JsonScanHistoryStore();
            await store.AddAsync(currentResult, SelectedImageHolder.Current?.FullPath);

            await DisplayAlertAsync(
                "Save Result",
                $"Saved analysis for {currentResult.ProductName}.",
                "OK");

            await AppNavigator.GoHistoryAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AnalyzeResult] Save failed: {ex}");
            await DisplayAlertAsync(
                "Save Result",
                "The scan could not be saved. Please try again.",
                "OK");
        }
    }

    private static T? Resolve<T>()
        where T : class
    {
        return Application.Current?
            .Handler?
            .MauiContext?
            .Services
            .GetService<T>();
    }
}