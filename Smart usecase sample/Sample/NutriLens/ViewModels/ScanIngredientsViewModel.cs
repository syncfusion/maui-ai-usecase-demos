using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriLens.Helpers;
using NutriLens.Models;
using NutriLens.Services;
using System.Windows.Input;

namespace NutriLens.ViewModels;

public partial class ScanIngredientsViewModel : ObservableObject
{
    private readonly IImagePickerService imagePickerService;
    private readonly IIngredientImageExtractionService imageExtractionService;
    private readonly IAzureOpenAIIngredientService aiService;
    private bool isBusy;

    [ObservableProperty]
    private bool isAnalyzing;

    [ObservableProperty]
    private double analysisProgress;

    [ObservableProperty]
    private string analysisStatusMessage = "Position ingredients label inside frame";

    [ObservableProperty]
    private ImageSource? capturedImage;

    [ObservableProperty]
    private string extractedText = string.Empty;

    [ObservableProperty]
    private IngredientAnalysisResult? aiAnalysisResult;

    [ObservableProperty]
    private bool isFlashEnabled;

    public bool HasCapturedImage => CapturedImage is not null;

    public string BackIcon => MaterialIcons.ArrowBack;
    public string PersonIcon => MaterialIcons.Person;
    public string GalleryIcon => MaterialIcons.PhotoLibrary;
    public string ScanIcon => MaterialIcons.CenterFocusStrong;
    public string FlashIcon => IsFlashEnabled ? MaterialIcons.FlashOn : MaterialIcons.FlashOff;
    public string AnalysisIcon => MaterialIcons.AutoAwesome;

    public ICommand BackCommand { get; }
    public ICommand AnalyzeIngredientsCommand { get; }
    public ICommand PickImageCommand { get; }
    public ICommand ToggleFlashCommand { get; }

    public ScanIngredientsViewModel(
        IImagePickerService imagePickerService,
        IIngredientImageExtractionService imageExtractionService,
        IAzureOpenAIIngredientService aiService)
    {
        this.imagePickerService = imagePickerService;
        this.imageExtractionService = imageExtractionService;
        this.aiService = aiService;

        BackCommand = new AsyncRelayCommand(BackAsync);
        AnalyzeIngredientsCommand = new AsyncRelayCommand(AnalyzeIngredientsAsync);
        PickImageCommand = new AsyncRelayCommand(PickImageAsync);
        ToggleFlashCommand = new AsyncRelayCommand(ToggleFlashAsync);
    }

    public ScanIngredientsViewModel()
        : this(
            Resolve<IImagePickerService>() ?? new ImagePickerService(),
            Resolve<IIngredientImageExtractionService>() ?? new IngredientImageExtractionService(),
            Resolve<IAzureOpenAIIngredientService>() ?? new AzureOpenAIIngredientService())
    {
    }

    partial void OnCapturedImageChanged(ImageSource? value)
    {
        OnPropertyChanged(nameof(HasCapturedImage));

        if (value is not null && !IsAnalyzing)
        {
            AnalysisStatusMessage = "Image ready for analysis";
        }
    }

    partial void OnIsFlashEnabledChanged(bool value)
    {
        OnPropertyChanged(nameof(FlashIcon));
    }

    private async Task AnalyzeIngredientsAsync()
    {
        if (isBusy)
            return;

        try
        {
            isBusy = true;
            IsAnalyzing = true;

            var image = SelectedImageHolder.Current; 
            if (image is null)
            {
                image = await imagePickerService.CapturePhotoAsync();

                if (image is null)
                {
                    AnalysisStatusMessage = "No image was selected. Please choose a product image.";
                    return;
                }

                SelectedImageHolder.Current = image;
                CapturedImage = ImageSource.FromFile(image.FullPath);
            }

            AnalysisProgress = 0.10;
            AnalysisStatusMessage = "Preparing image...";
            await Task.Delay(200);

            AnalysisProgress = 0.25;
            AnalysisStatusMessage = "Extracting product details from image...";
            ExtractedText = await imageExtractionService.ExtractContentAsync(image);

            if (string.IsNullOrWhiteSpace(ExtractedText))
            {
                throw new InvalidOperationException("No readable content was detected in the selected image.");
            }

            AnalysisProgress = 0.60;
            AnalysisStatusMessage = "Analyzing ingredient and nutrition quality...";
            AiAnalysisResult = await aiService.AnalyzeAsync(ExtractedText);

            if (AiAnalysisResult is null)
            {
                throw new InvalidOperationException("The AI analysis returned no result.");
            }

            AnalysisProgress = 1.0;
            AnalysisStatusMessage = "Analysis complete";

            AnalysisNavigationData.CurrentResult = AiAnalysisResult;

            await Task.Delay(300);
            await AppNavigator.GoAnalyzeIngredientsAsync();
        }
        catch (InvalidOperationException ex)
        {
            AnalysisStatusMessage = ex.Message;
            System.Diagnostics.Debug.WriteLine($"[ScanIngredients] InvalidOperationException: {ex}");
        }
        catch (HttpRequestException ex)
        {
            AnalysisStatusMessage = "Network issue while analyzing the product. Please try again.";
            System.Diagnostics.Debug.WriteLine($"[ScanIngredients] HttpRequestException: {ex}");
        }
        catch (Exception ex)
        {
            AnalysisStatusMessage = "Analysis service temporarily unavailable. Please try again.";
            System.Diagnostics.Debug.WriteLine($"[ScanIngredients] Exception: {ex}");
        }
        finally
        {
            isBusy = false;
            IsAnalyzing = false;
        }
    }

    private async Task PickImageAsync()
    {
        if (isBusy)
            return;

        try
        {
            isBusy = true;

            var image = await imagePickerService.PickPhotoAsync();

            if (image is null || !File.Exists(image.FullPath))
            {
                AnalysisStatusMessage = "No valid image was selected.";
                return;
            }

            SelectedImageHolder.Current = image;
            CapturedImage = ImageSource.FromFile(image.FullPath);
            AnalysisStatusMessage = "Image ready for analysis";
        }
        catch
        {
            AnalysisStatusMessage = "Unable to identify ingredient list.";
        }
        finally
        {
            isBusy = false;
        }
    }

    private Task ToggleFlashAsync()
    {
        IsFlashEnabled = !IsFlashEnabled;
        return Task.CompletedTask;
    }

    private async Task BackAsync()
    {
        await AppNavigator.PopAsync();
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