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

        var safeResult = result ?? new IngredientAnalysisResult
        {
            Profile = "Diabetes",
            ProductName = "Unknown product",
            Score = 0,
            Category = "Unknown",
            Recommendation = "Unable to analyze this product.",
            Summary = "No completed analysis was found for this item.",
            MetabolicConflict = "No detailed metabolic conflict data is available.",
            AlternativeRecommendations =
            [
                "Choose products with fewer additives and more whole-food ingredients."
            ]
        };

        BindingContext = safeResult;
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnDetailedBreakdownClicked(object? sender, EventArgs e)
    {
        if (BindingContext is IngredientAnalysisResult currentResult)
        {
            // Pass the SAME AI result object already displayed on this page.
            // No regeneration, no second AI call, identical data on both pages.
            await Navigation.PushAsync(new DetailBreakdownPage(currentResult));
        }
        else
        {
            await Navigation.PushAsync(new DetailBreakdownPage());
        }
    }

    private async void OnSaveResultClicked(object? sender, EventArgs e)
    {
        if (BindingContext is not IngredientAnalysisResult currentResult)
        {
            await DisplayAlert("Save Result", "No analysis is available to save.", "OK");
            return;
        }

        try
        {
            var store = Resolve<IScanHistoryStore>() ?? new JsonScanHistoryStore();
            await store.AddAsync(currentResult, SelectedImageHolder.Current?.FullPath);
            await DisplayAlert("Save Result", $"Saved analysis for {currentResult.ProductName}.", "OK");
            await AppNavigator.GoHistoryAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AnalyzeResult] Save failed: {ex}");
            await DisplayAlert("Save Result", "The scan could not be saved. Please try again.", "OK");
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
    private async void OnCompareClicked(object? sender, EventArgs e)
    {
        if (BindingContext is not IngredientAnalysisResult currentResult)
        {
            await DisplayAlert("Compare", "No analysis is available to compare.", "OK");
            return;
        }

        await DisplayAlert("Compare", $"Compare is available for {currentResult.ProductName}.", "OK");
    }
}