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
    private bool isBusy;

    [ObservableProperty]
    private bool isExtracting;

    [ObservableProperty]
    private double extractionProgress;

    [ObservableProperty]
    private string statusMessage =
        "Position the ingredients label inside the frame";

    [ObservableProperty]
    private ImageSource? capturedImage;

    [ObservableProperty]
    private bool isFlashEnabled;

    public bool HasCapturedImage => CapturedImage is not null;

    public string BackIcon => MaterialIcons.ArrowBack;
    public string PersonIcon => MaterialIcons.Person;
    public string GalleryIcon => MaterialIcons.PhotoLibrary;
    public string ScanIcon => MaterialIcons.CenterFocusStrong;
    public string FlashIcon =>
        IsFlashEnabled ? MaterialIcons.FlashOn : MaterialIcons.FlashOff;
    public string AnalysisIcon => MaterialIcons.AutoAwesome;

    public ICommand BackCommand { get; }
    public ICommand ScanCommand { get; }
    public ICommand PickImageCommand { get; }
    public ICommand ToggleFlashCommand { get; }

    public string ScannerMessage { get; set; } =
        "Align the ingredient list within the frame to begin extraction.";

    public ScanIngredientsViewModel()
        : this(
            Resolve<IImagePickerService>() ?? new ImagePickerService(),
            Resolve<IIngredientImageExtractionService>()
                ?? new IngredientImageExtractionService())
    {
    }

    public ScanIngredientsViewModel(
        IImagePickerService imagePickerService,
        IIngredientImageExtractionService imageExtractionService)
    {
        this.imagePickerService = imagePickerService
            ?? throw new ArgumentNullException(nameof(imagePickerService));
        this.imageExtractionService = imageExtractionService
            ?? throw new ArgumentNullException(nameof(imageExtractionService));

        BackCommand = new AsyncRelayCommand(BackAsync);
        ScanCommand = new AsyncRelayCommand(ScanAsync);
        PickImageCommand = new AsyncRelayCommand(PickImageAsync);
        ToggleFlashCommand = new AsyncRelayCommand(ToggleFlashAsync);
    }

    partial void OnCapturedImageChanged(ImageSource? value)
    {
        OnPropertyChanged(nameof(HasCapturedImage));
        if (value is not null && !IsExtracting)
            StatusMessage = "Image ready — tap Scan to extract ingredients";
    }

    partial void OnIsFlashEnabledChanged(bool value)
        => OnPropertyChanged(nameof(FlashIcon));

    /// <summary>
    /// STEP 2 of the new flow: OCR / content extraction ONLY.
    /// No AI analysis, no score, no recommendation here.
    /// On success → navigate to ReviewIngredientsPage.
    /// </summary>
    private async Task ScanAsync()
    {
        if (isBusy)
            return;

        FileResult? image = SelectedImageHolder.Current;

        try
        {
            isBusy = true;
            IsExtracting = true;

            if (image is null)
            {
                image = await imagePickerService.CapturePhotoAsync();
                if (image is null)
                {
                    StatusMessage = "No image was selected. Please choose a product image.";
                    return;
                }

                SelectedImageHolder.Current = image;
                CapturedImage = ImageSource.FromFile(image.FullPath);
            }

            ExtractionProgress = 0.15;
            StatusMessage = "Preparing image…";
            await Task.Delay(200);

            ExtractionProgress = 0.50;
            StatusMessage = "Extracting ingredients from image (OCR)…";
            var extractedText =
                await imageExtractionService.ExtractContentAsync(image);

            if (string.IsNullOrWhiteSpace(extractedText))
            {
                throw new InvalidOperationException(
                    "No readable ingredient content was detected on the label. " +
                    "Retake with better lighting and focus.");
            }

            ExtractionProgress = 1.0;
            StatusMessage = "Extraction complete";

            await Task.Delay(250);

            // Hand off to Review page and clear the scan-side image slot.
            SelectedImageHolder.Current = null;
            await AppNavigator.GoReviewIngredientsAsync(extractedText, image);
        }
        catch (InvalidOperationException ex)
        {
            StatusMessage = ex.Message;
            System.Diagnostics.Debug.WriteLine(
                $"[ScanIngredients] InvalidOperationException: {ex}");
        }
        catch (HttpRequestException ex)
        {
            StatusMessage = "Network issue during OCR. Please try again.";
            System.Diagnostics.Debug.WriteLine(
                $"[ScanIngredients] HttpRequestException: {ex}");
        }
        catch (Exception ex)
        {
            StatusMessage = "Extraction failed. Please try again.";
            System.Diagnostics.Debug.WriteLine($"[ScanIngredients] Exception: {ex}");
        }
        finally
        {
            isBusy = false;
            IsExtracting = false;
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
                StatusMessage = "No valid image was selected.";
                return;
            }

            SelectedImageHolder.Current = image;
            CapturedImage = ImageSource.FromFile(image.FullPath);
            StatusMessage = "Image ready — tap Scan to extract ingredients";
        }
        catch
        {
            StatusMessage = "Unable to load the selected image.";
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

    private async Task BackAsync() => await AppNavigator.PopAsync();

    private static T? Resolve<T>() where T : class =>
        Application.Current?.Handler?.MauiContext?.Services.GetService<T>();
}