using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NutriLens.Helpers;
using NutriLens.Models;
using NutriLens.Services;

namespace NutriLens.ViewModels;

public partial class ReviewIngredientsViewModel : ObservableObject
{
    private readonly IAzureOpenAIIngredientService analysisService;
    private readonly IUserPreferenceStore preferenceStore;

    private FileResult? sourceImage;
    private string extractedText = string.Empty;

    public ObservableCollection<IngredientReviewItem> Ingredients { get; } = [];

    [ObservableProperty]
    private bool isAnalyzing;

    [ObservableProperty]
    private string analyzingStatus = string.Empty;

    [ObservableProperty]
    private string newIngredientName = string.Empty;

    [ObservableProperty]
    private string newIngredientDescription = string.Empty;

    [ObservableProperty]
    private bool isAddIngredientPopupVisible;

    public string IngredientSummary => $"{Ingredients.Count} Ingredients Found";

    // Icons
    public string BackIcon => MaterialIcons.ArrowBack;
    public string PersonIcon => MaterialIcons.Person;
    public string EditIcon => MaterialIcons.EditNote;
    public string AddIcon => MaterialIcons.AddCircleOutline;
    public string CloseIcon => MaterialIcons.Close;
    public string CameraIcon => MaterialIcons.PhotoCamera;
    public string ForwardIcon => MaterialIcons.ArrowForward;

    public ReviewIngredientsViewModel()
        : this(
            Resolve<IAzureOpenAIIngredientService>() ?? new AzureOpenAIIngredientService(),
            Resolve<IUserPreferenceStore>() ?? new UserPreferenceStore())
    {
    }

    public ReviewIngredientsViewModel(
        IAzureOpenAIIngredientService analysisService,
        IUserPreferenceStore preferenceStore)
    {
        this.analysisService = analysisService
            ?? throw new ArgumentNullException(nameof(analysisService));
        this.preferenceStore = preferenceStore
            ?? throw new ArgumentNullException(nameof(preferenceStore));

        Ingredients.CollectionChanged += (_, _)
            => OnPropertyChanged(nameof(IngredientSummary));
    }

    /// <summary>
    /// Called by the page's OnAppearing. Reads the one-shot handoff
    /// populated by AppNavigator.GoReviewIngredientsAsync and parses
    /// it into ingredient rows. No-op if the slot was already consumed.
    /// </summary>
    public void HydrateFromPendingReview()
    {
        var pending = AnalysisNavigationData.PendingReview;
        if (pending is null)
            return; // Returning to the page after navigation — keep state.

        extractedText = pending.ExtractedText;
        sourceImage = pending.Image;
        AnalysisNavigationData.PendingReview = null; // consume

        Ingredients.Clear();
        foreach (var item in ParseIngredients(extractedText))
            Ingredients.Add(item);
    }

    // ----------------------------------------------------------------
    // Ingredient parsing — splits OCR text into name + optional desc.
    // Sample line shapes handled:
    //   "Sugar"                       → name only
    //   "INS 322 (Soy Lecithin)"      → name "INS 322", desc "Soy Lecithin"
    //   "Palm Oil — frying medium"    → name "Palm Oil", desc "frying medium"
    // ----------------------------------------------------------------
    private static IEnumerable<IngredientReviewItem> ParseIngredients(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            yield break;

        // Split on newlines, semicolons, or numbered prefixes like "2." / "2)"
        var lines = text
            .Split(['\r', '\n', ';'], StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)
            .ToList();

        var index = 1;

        foreach (var raw in lines)
        {
            // Strip a leading number/bullet, e.g. "1. Sugar" / "2) Sugar" / "- Sugar"
            var cleaned = Regex.Replace(raw, @"^\s*(?:\d+[.\)]|-|\*)\s*", "");

            var (name, desc) = SplitNameAndDescription(cleaned);

            if (string.IsNullOrWhiteSpace(name))
                continue;

            yield return new IngredientReviewItem
            {
                IndexNumber = index.ToString("00"),
                Name = name.Trim(),
                Description = desc?.Trim() ?? string.Empty
            };
            index++;
        }
    }

    private static (string Name, string? Description) SplitNameAndDescription(
        string line)
    {
        // "INS 322 (Soy Lecithin)"
        var parenMatch = Regex.Match(line, @"^(.*?)\s*\((.+)\)\s*$");
        if (parenMatch.Success)
            return (parenMatch.Groups[1].Value, parenMatch.Groups[2].Value);

        // "Palm Oil — frying medium" / "Palm Oil: frying medium"
        var sepMatch = Regex.Match(line, @"^(.*?)\s*(?:—|–|-|:)\s*(.+)$");
        if (sepMatch.Success && sepMatch.Groups[1].Value.Trim().Length > 0)
            return (sepMatch.Groups[1].Value, sepMatch.Groups[2].Value);

        return (line, null);
    }

    // ----------------------------------------------------------------
    // Commands
    // ----------------------------------------------------------------
    [RelayCommand]
    private async Task BackAsync() => await AppNavigator.PopAsync();

    [RelayCommand]
    private Task EditAllAsync()
    {
        // Editing is done inline via two-way Entry bindings —
        // no extra action required, but we keep the button for affordance.
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task ShowAddIngredientPopupAsync()
    {
        NewIngredientName = string.Empty;
        NewIngredientDescription = string.Empty;
        IsAddIngredientPopupVisible = true;
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task CancelAddIngredientAsync()
    {
        IsAddIngredientPopupVisible = false;
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task ConfirmAddIngredientAsync()
    {
        var name = (NewIngredientName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
            return Task.CompletedTask;

        Ingredients.Add(new IngredientReviewItem
        {
            IndexNumber = (Ingredients.Count + 1).ToString("00"),
            Name = name,
            Description = (NewIngredientDescription ?? string.Empty).Trim()
        });

        IsAddIngredientPopupVisible = false;
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task RemoveIngredientAsync(IngredientReviewItem? item)
    {
        if (item is not null)
        {
            Ingredients.Remove(item);
            RenumberIngredients();
        }
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task RetakeAsync()
    {
        // Reset state so a fresh capture doesn't see stale rows.
        Ingredients.Clear();
        extractedText = string.Empty;
        sourceImage = null;
        await AppNavigator.GoScanAsync();
    }

    [RelayCommand]
    private async Task AnalyzeNowAsync()
    {
        if (Ingredients.Count == 0)
        {
            await AppNavigator.ShowAlertAsync(
                "No Ingredients",
                "Please add at least one ingredient before analyzing.",
                "OK");
            return;
        }

        if (IsAnalyzing)
            return;

        var combined = string.Join('\n', Ingredients
            .Select(i => string.IsNullOrWhiteSpace(i.Description)
                ? i.Name
                : $"{i.Name} ({i.Description})"));

        try
        {
            IsAnalyzing = true;
            AnalyzingStatus = "Reading your profile…";

            UserDietaryPreference? prefs = preferenceStore.Load();
            prefs = prefs.IsEmpty ? null : prefs;

            AnalyzingStatus = "Analyzing ingredients with AI…";
            var result = await analysisService.AnalyzeAsync(combined, prefs);

            if (result is null)
                throw new InvalidOperationException(
                    "The AI analysis returned no result.");

            await AppNavigator.GoAnalyzeIngredientsAsync(result);
        }
        catch (InvalidOperationException ex)
        {
            AnalyzingStatus = string.Empty;
            System.Diagnostics.Debug.WriteLine(
                $"[Review] InvalidOperationException: {ex}");
            await AppNavigator.ShowAlertAsync(
                "Analysis Error", ex.Message, "OK");
        }
        catch (HttpRequestException ex)
        {
            AnalyzingStatus = string.Empty;
            System.Diagnostics.Debug.WriteLine(
                $"[Review] HttpRequestException: {ex}");
            await AppNavigator.ShowAlertAsync(
                "Network Issue",
                "Could not reach the AI service. Check your connection and try again.",
                "OK");
        }
        catch (Exception ex)
        {
            AnalyzingStatus = string.Empty;
            System.Diagnostics.Debug.WriteLine($"[Review] Exception: {ex}");
            await AppNavigator.ShowAlertAsync(
                "Analysis Unavailable",
                "Something went wrong while analyzing the product. Please try again.",
                "OK");
        }
        finally
        {
            IsAnalyzing = false;
            AnalyzingStatus = string.Empty;
        }
    }

    private void RenumberIngredients()
    {
        for (var i = 0; i < Ingredients.Count; i++)
            Ingredients[i].IndexNumber = (i + 1).ToString("00");
    }

    private static T? Resolve<T>() where T : class =>
        Application.Current?.Handler?.MauiContext?.Services.GetService<T>();
}