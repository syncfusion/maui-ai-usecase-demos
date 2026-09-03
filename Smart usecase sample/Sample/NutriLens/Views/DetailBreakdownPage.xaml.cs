using Microsoft.Extensions.DependencyInjection;
using NutriLens.Helpers;
using NutriLens.Models;
using NutriLens.Services;

namespace NutriLens.Views;

public partial class DetailBreakdownPage : ContentPage
{
    private IngredientAnalysisResult? currentResult;

    public DetailBreakdownPage()
        : this(AnalysisNavigationData.CurrentResult)
    {
    }

    public DetailBreakdownPage(IngredientAnalysisResult? result)
    {
        InitializeComponent();

        currentResult = result ?? new IngredientAnalysisResult
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

        BindingContext = currentResult;
    }

    private async void OnBackTapped(object? sender, TappedEventArgs e)
    {
        if (Navigation.NavigationStack.Count > 1)
        {
            await Navigation.PopAsync();
        }
    }

    private async void SfButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnSaveResultTapped(object sender, TappedEventArgs e)
    {
        if (BindingContext is not IngredientAnalysisResult result)
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

            // Persist the SAME AI object already displayed (no new AI call).
            // Keep the captured label image path so history can show a thumbnail.
            await store.AddAsync(
                result,
                SelectedImageHolder.Current?.FullPath);

            await DisplayAlertAsync(
                "Save Result",
                $"Saved analysis for {result.ProductName}.",
                "OK");

            // HistoryPage.OnAppearing refreshes from the store,
            // so the new scan appears immediately — no restart needed.
            await AppNavigator.GoHistoryAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DetailBreakdown] Save failed: {ex}");
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