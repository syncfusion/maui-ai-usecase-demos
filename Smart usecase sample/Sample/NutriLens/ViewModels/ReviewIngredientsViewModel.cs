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

    public ObservableCollection<IngredientItem> Ingredients { get; } = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isAddIngredientPopupVisible;

    [ObservableProperty]
    private string newIngredientName = string.Empty;

    [ObservableProperty]
    private string newIngredientDescription = string.Empty;

    [ObservableProperty]
    private string analyzingStatus = string.Empty;

    public int IngredientCount => Ingredients.Count;

    public string IngredientSummary => $"{IngredientCount} Ingredients Found";
    public ImageSource? ScannedImage { get; private set; }
    public string BackIcon => MaterialIcons.ArrowBack;
    public string PersonIcon => MaterialIcons.Person;
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
        this.analysisService = analysisService ?? throw new ArgumentNullException(nameof(analysisService));
        this.preferenceStore = preferenceStore ?? throw new ArgumentNullException(nameof(preferenceStore));

        Ingredients.CollectionChanged += (_, _) => RefreshNumbers();
    }

    public void HydrateFromPendingReview()
    {
        var pending = AnalysisNavigationData.PendingReview;
        if (pending is null)
            return;
        ScannedImage = CreateImageSource(pending.Image);
        OnPropertyChanged(nameof(ScannedImage));
        AnalysisNavigationData.PendingReview = null;

        Ingredients.Clear();

        foreach (var item in ParseIngredients(pending.ExtractedText))
            Ingredients.Add(item);

        if (Ingredients.Count == 0 && !string.IsNullOrWhiteSpace(pending.ExtractedText))
        {
            Ingredients.Add(new IngredientItem
            {
                IngredientName = pending.ExtractedText.Trim(),
                IngredientDescription = string.Empty
            });
        }

        RefreshNumbers();
    }
    private static ImageSource? CreateImageSource(FileResult image)
    {
        if (image is null)
            return null;

        if (!string.IsNullOrWhiteSpace(image.FullPath) &&
            File.Exists(image.FullPath))
        {
            return ImageSource.FromFile(image.FullPath);
        }

        return ImageSource.FromStream(async cancellationToken =>
        {
            var stream = await image.OpenReadAsync();
            return stream;
        });
    }
    private static IEnumerable<IngredientItem> ParseIngredients(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            yield break;

        var cleanedText = NormalizeExtractedText(text);

        var lines = cleanedText
            .Split(['\r', '\n', ';'], StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)
            .ToList();

        var index = 1;

        foreach (var raw in lines)
        {
            var cleaned = Regex.Replace(raw, @"^\s*(?:\d+[.\)]|-|\*|•)\s*", "").Trim();

            if (string.IsNullOrWhiteSpace(cleaned))
                continue;

            var (name, description) = SplitIngredient(cleaned);

            if (string.IsNullOrWhiteSpace(name))
                continue;

            yield return new IngredientItem
            {
                DisplayNumber = index.ToString("00"),
                IngredientName = name.Trim(),
                IngredientDescription = description?.Trim() ?? string.Empty
            };

            index++;
        }
    }

    private static string NormalizeExtractedText(string text)
    {
        var lines = text
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)
            .ToArray();

        var ingredientSection = new List<string>();
        var capture = false;

        foreach (var line in lines)
        {
            var lower = line.ToLowerInvariant();

            if (lower.Contains("ingredient"))
                capture = true;

            if (capture)
            {
                var stopWords = new[]
                {
                    "nutrition facts",
                    "serving size",
                    "calories",
                    "protein",
                    "fat",
                    "carbohydrate",
                    "carbohydrates",
                    "sugar",
                    "sodium",
                    "allergen",
                    "warning",
                    "health claim",
                    "directions",
                    "storage"
                };

                if (stopWords.Any(lower.Contains) && !lower.Contains("ingredient"))
                    break;

                ingredientSection.Add(line);
            }
        }

        if (ingredientSection.Count == 0)
            return text;

        if (ingredientSection.Count == 1 && ingredientSection[0].ToLowerInvariant().Contains("ingredients"))
        {
            var first = ingredientSection[0];
            var colonIndex = first.IndexOf(':');
            if (colonIndex >= 0 && colonIndex < first.Length - 1)
                return first[(colonIndex + 1)..].Trim();

            return string.Empty;
        }

        return string.Join(Environment.NewLine, ingredientSection);
    }

    private static (string Name, string? Description) SplitIngredient(string line)
    {
        var parenMatch = Regex.Match(line, @"^(.*?)\s*\((.+)\)\s*$");
        if (parenMatch.Success)
            return (parenMatch.Groups[1].Value, parenMatch.Groups[2].Value);

        var sepMatch = Regex.Match(line, @"^(.*?)\s*(?:—|–|-|:)\s*(.+)$");
        if (sepMatch.Success && sepMatch.Groups[1].Value.Trim().Length > 0)
            return (sepMatch.Groups[1].Value, sepMatch.Groups[2].Value);

        return (line, null);
    }

    private void RefreshNumbers()
    {
        for (var i = 0; i < Ingredients.Count; i++)
            Ingredients[i].DisplayNumber = (i + 1).ToString("00");

        OnPropertyChanged(nameof(IngredientCount));
        OnPropertyChanged(nameof(IngredientSummary));
    }

    [RelayCommand]
    private async Task BackAsync()
    {
        Ingredients.Clear();
        await AppNavigator.PopAsync();
    }

    [RelayCommand]
    private async Task DeleteIngredientAsync(IngredientItem? item)
    {
        if (item is null)
            return;

        Ingredients.Remove(item);
        RefreshNumbers();
         
    }

    [RelayCommand]
    private async Task AddIngredientAsync()
    {
        var name = (NewIngredientName ?? string.Empty).Trim();
        var desc = (NewIngredientDescription ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            await AppNavigator.ShowAlertAsync("Validation", "Ingredient name cannot be empty.", "OK");
            return;
        }

        if (Ingredients.Any(x =>
                string.Equals(x.IngredientName?.Trim(), name, StringComparison.OrdinalIgnoreCase)))
        {
            await AppNavigator.ShowAlertAsync("Duplicate", "This ingredient already exists.", "OK");
            return;
        }

        Ingredients.Add(new IngredientItem
        {
            IngredientName = name,
            IngredientDescription = desc
        });

        RefreshNumbers();

        NewIngredientName = string.Empty;
        NewIngredientDescription = string.Empty;
        IsAddIngredientPopupVisible = false;
    }

    [RelayCommand]
    private void ShowAddIngredientPopup()
    {
        NewIngredientName = string.Empty;
        NewIngredientDescription = string.Empty;
        IsAddIngredientPopupVisible = true;
    }

    [RelayCommand]
    private void CancelAddIngredient()
    {
        IsAddIngredientPopupVisible = false;
    }

    [RelayCommand]
    private async Task RetakeAsync()
    {  
        Ingredients.Clear();
        await AppNavigator.GoScanAsync();
    }

    [RelayCommand]
    private async Task OpenProfileAsync() => await AppNavigator.GoProfileAsync();

    [RelayCommand]
    private async Task AnalyzeNowAsync()
    {
        if (IsLoading)
            return;

        if (Ingredients.Count == 0)
        {
            await AppNavigator.ShowAlertAsync("Validation", "At least one ingredient is required.", "OK");
            return;
        }

        try
        {
            IsLoading = true;
            AnalyzingStatus = "Analyzing ingredients...";

            var prefs = preferenceStore.Load();
            if (prefs.IsEmpty)
                prefs = null;

            var combined = string.Join('\n', Ingredients.Select(i =>
                string.IsNullOrWhiteSpace(i.IngredientDescription)
                    ? i.IngredientName
                    : $"{i.IngredientName} ({i.IngredientDescription})"));

            var result = await analysisService.AnalyzeAsync(combined, prefs);

            if (result is null)
                throw new InvalidOperationException("The AI analysis returned no result.");

            await AppNavigator.GoAnalyzeIngredientsAsync(result);
        }
        catch (Exception ex)
        {
            await AppNavigator.ShowAlertAsync("Analysis Error", ex.Message, "OK");
        }
        finally
        {
            IsLoading = false;
            AnalyzingStatus = string.Empty;
        }
    }

    private static T? Resolve<T>() where T : class =>
        Application.Current?.Handler?.MauiContext?.Services.GetService<T>();
}