using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Syncfusion.Maui.ImageEditor;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace SmartCropping.ViewModel;

/// <summary>
/// Drives the Smart Cropping sample: coordinates the AI service, the image
/// editor controller, and the overlay geometry. The view model owns all
/// transient state (busy, error, suggestion, crop applied) and exposes a
/// command set the XAML view binds against.
/// </summary>
/// <remarks>
/// This view model follows the same CommunityToolkit.Mvvm conventions used in
/// <c>DataFormAISolution</c>: <c>[ObservableProperty]</c> backed properties,
/// <c>[RelayCommand]</c> methods for actions, and an injected AI service for
/// the network call. All view-layer dependencies (the editor control itself)
/// flow through <see cref="IImageEditorController"/> so the view model stays
/// platform- and control-agnostic.
/// </remarks>
public partial class SmartCroppingViewModel : ObservableObject
{
    private readonly ISmartCropService aiService;
    private IImageEditorController? editorController;

    private ImageSource? originalImageSource;
    private byte[]? originalImageBytes;
    private CropRegion? suggestedRegion;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartCroppingViewModel"/> class.
    /// </summary>
    /// <param name="aiService">Service used to request crop suggestions.</param>
    public SmartCroppingViewModel(ISmartCropService aiService)
    {
        this.aiService = aiService;
    }

    /// <summary>
    /// Sets the controller that bridges this view model to the
    /// <c>SfImageEditor</c> instance in the view. The view assigns itself
    /// here during construction so the view model never takes a direct
    /// dependency on the Syncfusion control.
    /// </summary>
    /// <param name="controller">The bridge controller to use.</param>
    public void SetEditorController(IImageEditorController controller)
    {
        editorController = controller;
        NotifyCommandsChanged();
    }

    #region Observable State

    /// <summary>
    /// Gets or sets whether the AI request is in flight or the editor is
    /// performing another long-running operation.
    /// </summary>
    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    /// <summary>
    /// Gets or sets whether an AI crop suggestion is available and waiting
    /// for the user to apply, adjust, or reset.
    /// </summary>
    [ObservableProperty]
    public partial bool IsSuggestionAvailable { get; set; }

    /// <summary>
    /// Gets or sets whether the AI crop has been applied to the image and the
    /// result can be saved or reset.
    /// </summary>
    [ObservableProperty]
    public partial bool IsCropApplied { get; set; }

    /// <summary>Gets or sets whether an image is loaded in the editor.</summary>
    [ObservableProperty]
    public partial bool IsImageLoaded { get; set; }

    /// <summary>Gets or sets the crop intent chosen by the user.</summary>
    [ObservableProperty]
    public partial string? SelectedIntent { get; set; }

    /// <summary>Gets the crop intents offered to the user.</summary>
    public ObservableCollection<string> CropIntents { get; } = new()
    {
        "Profile Photo",
        "Product Image",
        "Pet Portrait",
        "Food Photography",
        "Hero Banner",
        "Object Focus"
    };

    /// <summary>Gets whether an intent has been selected.</summary>
    public bool IsIntentSelected => !string.IsNullOrEmpty(SelectedIntent);

    /// <summary>Gets whether a suggestion is waiting to be applied.</summary>
    public bool IsSuggestionReady => IsSuggestionAvailable;

    /// <summary>Gets whether the Smart Crop action is relevant (image + intent).</summary>
    public bool CanGenerateCrop => IsImageLoaded && IsIntentSelected;

    /// <summary>Gets whether the Apply Crop action is relevant.</summary>
    public bool CanApplyCrop => IsSuggestionReady || IsCropApplied;

    /// <summary>Gets whether the Reset action is relevant.</summary>
    public bool CanReset => IsCropApplied;

    /// <summary>Gets whether the Save action is relevant.</summary>
    public bool CanSave => IsCropApplied;

    /// <summary>Gets whether a status message should be displayed.</summary>
    public bool HasStatusMessage => !IsBusy && !IsError && !string.IsNullOrEmpty(StatusMessage);

    /// <summary>
    /// Gets or sets whether the most recent AI request failed and the retry
    /// banner should be shown.
    /// </summary>
    [ObservableProperty]
    public partial bool IsError { get; set; }

    /// <summary>
    /// Gets or sets the text shown in the status banner above the editor.
    /// </summary>
    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the AI crop preview overlay is visible.
    /// </summary>
    [ObservableProperty]
    public partial bool IsOverlayVisible { get; set; }

    /// <summary>
    /// Gets or sets the X position of the AI crop preview rectangle in
    /// display-space pixels relative to the editor view.
    /// </summary>
    [ObservableProperty]
    public partial double OverlayX { get; set; }

    /// <summary>
    /// Gets or sets the Y position of the AI crop preview rectangle in
    /// display-space pixels relative to the editor view.
    /// </summary>
    [ObservableProperty]
    public partial double OverlayY { get; set; }

    /// <summary>
    /// Gets or sets the width of the AI crop preview rectangle in
    /// display-space pixels.
    /// </summary>
    [ObservableProperty]
    public partial double OverlayWidth { get; set; }

    /// <summary>
    /// Gets or sets the height of the AI crop preview rectangle in
    /// display-space pixels.
    /// </summary>
    [ObservableProperty]
    public partial double OverlayHeight { get; set; }

    #endregion

    #region Commands

    /// <summary>
    /// Opens the file picker and loads the selected image as the new
    /// "original" image for the workflow.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanOpenImage))]
    public async Task OpenImageAsync()
    {
        if (IsBusy || editorController is null)
        {
            return;
        }

        try
        {
            FileResult? result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select an image",
                FileTypes = ImageFileTypes
            });

            if (result is null)
            {
                return;
            }

            await using Stream sourceStream = await result.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await sourceStream.CopyToAsync(memoryStream);
            byte[] bytes = memoryStream.ToArray();

            if (bytes.Length == 0)
            {
                ShowError("The selected image is empty.");
                return;
            }

            ImageSource source = CreateImageSourceFromBytes(bytes);

            await LoadImageInternalAsync(source, bytes);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to open image: {ex.Message}");
            ShowError("Unable to load the selected image.");
        }
    }

    private bool CanOpenImage() => !IsBusy && editorController is not null;

    /// <summary>
    /// Requests a crop suggestion from Azure OpenAI and shows it as a preview
    /// overlay on the editor.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSmartCrop))]
    public async Task SmartCropAsync()
    {
        if (IsBusy || !CanGenerateCrop || originalImageBytes is null || editorController is null || SelectedIntent is null)
        {
            return;
        }

        bool restoreOriginal = IsCropApplied;

        try
        {
            IsBusy = true;
            IsError = false;
            IsSuggestionAvailable = false;
            IsCropApplied = false;
            IsOverlayVisible = false;
            suggestedRegion = null;
            StatusMessage = "Finding the best crop for the selected intent...";

            // Smart Crop stays available after a crop is applied; restore the
            // original image first so coordinates match the bytes sent to the AI.
            if (restoreOriginal && originalImageSource is not null)
            {
                await editorController.LoadImageAsync(originalImageSource);
            }

            SmartCropResponse? response = await aiService.SuggestCropAsync(originalImageBytes, SelectedIntent);

            if (response?.CropRegion is null)
            {
                ShowError("Unable to suggest a crop. Try again.");
                return;
            }

            Size originalSize = editorController.OriginalImageSize;
            if (originalSize.Width <= 0 || originalSize.Height <= 0)
            {
                ShowError("Unable to suggest a crop. Try again.");
                return;
            }

            CropRegion region = response.CropRegion;
            if (!region.IsValid(originalSize.Width, originalSize.Height))
            {
                Debug.WriteLine($"Invalid crop region: ({region.X},{region.Y}) {region.Width}x{region.Height} for image {originalSize.Width}x{originalSize.Height}");
                ShowError("Unable to suggest a crop. Try again.");
                return;
            }

            // Apply a safety margin so a slightly tight AI bounding box
            // (e.g. one that clips an animal's tail or a person's hand) gets
            // a few percent of breathing room on every side. The expansion
            // is clamped to the image so we never escape it.
            suggestedRegion = region.ExpandedBy(0.08, originalSize.Width, originalSize.Height);

            RefreshOverlayGeometry();

            if (IsOverlayVisible)
            {
                IsSuggestionAvailable = true;
                StatusMessage = $"Suggested crop generated for: {SelectedIntent}";
            }
            else
            {
                ShowError("Unable to suggest a crop. Try again.");
            }
        }
        catch (OperationCanceledException)
        {
            ShowError("Unable to suggest a crop. Try again.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Smart crop failed: {ex.Message}");
            ShowError("Unable to suggest a crop. Try again.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSmartCrop() => !IsBusy && CanGenerateCrop && editorController is not null;

    /// <summary>
    /// Applies the AI-suggested crop to the image and commits the change.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanApplyCropNow))]
    public void ApplyCrop()
    {
        if (!IsSuggestionAvailable || suggestedRegion is null || editorController is null)
        {
            return;
        }

        try
        {
            CropRegion region = suggestedRegion;
            suggestedRegion = null;
            IsOverlayVisible = false;
            IsSuggestionAvailable = false;

            editorController.ApplyCrop(new Rect(region.X, region.Y, region.Width, region.Height));
            editorController.SaveEdits();

            IsCropApplied = true;
            StatusMessage = "Crop applied successfully.";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Apply crop failed: {ex.Message}");
            ShowError("Unable to apply the crop.");
        }
    }

    private bool CanApplyCropNow() => IsSuggestionAvailable && !IsBusy && editorController is not null;

    /// <summary>
    /// Fully restores the editor to the originally loaded image and clears
    /// any AI suggestion or applied crop.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanResetNow))]
    public async Task ResetAsync()
    {
        if (IsBusy || editorController is null)
        {
            return;
        }

        try
        {
            IsBusy = true;

            ClearSuggestion();

            try
            {
                editorController.CancelEdits();
            }
            catch
            {
                // Best-effort: CancelEdits can throw when no edit session is active.
            }

            if (originalImageSource is not null)
            {
                await editorController.ResetEditorAsync();
                RefreshOverlayGeometry();
            }

            IsCropApplied = false;
            IsError = false;
            SelectedIntent = null;
            StatusMessage = StageMessage();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Reset failed: {ex.Message}");
            ShowError("Unable to reset the image.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanResetNow() => !IsBusy && IsCropApplied && editorController is not null;

    /// <summary>
    /// Saves the cropped image to disk using the operating system save dialog.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSaveNow))]
    public async Task SaveAsync()
    {
        if (!IsCropApplied || editorController is null)
        {
            return;
        }

        try
        {
            await editorController.SaveImageAsync();
            StatusMessage = "Image saved.";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Save failed: {ex.Message}");
            ShowError("Unable to save the image.");
        }
    }

    private bool CanSaveNow() => IsCropApplied && !IsBusy && editorController is not null;

    /// <summary>
    /// Clears the error state and immediately re-runs Smart Crop for the
    /// currently selected intent.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRetry))]
    public async Task RetryAsync()
    {
        if (!IsError)
        {
            return;
        }

        IsError = false;
        await SmartCropAsync();
    }

    private bool CanRetry() => IsError && !IsBusy;

    /// <summary>
    /// Image MIME types accepted by the picker. These are declared explicitly
    /// rather than using a built-in image constant so the picker returns only
    /// formats the editor and Azure OpenAI can both decode.
    /// </summary>
    private static readonly FilePickerFileType ImageFileTypes = new(
        new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            [DevicePlatform.iOS] = new[] { "public.image" },
            [DevicePlatform.Android] = new[] { "image/png", "image/jpeg" },
            [DevicePlatform.WinUI] = new[] { ".png", ".jpg", ".jpeg" },
            [DevicePlatform.macOS] = new[] { "png", "jpg", "jpeg" }
        });

    /// <summary>
    /// Creates an <see cref="ImageSource"/> from raw image bytes.
    /// </summary>
    /// <remarks>
    /// <c>ImageSource.FromBytes</c> is an extension method on
    /// <c>IImageLoadingService</c> in .NET 9+, so it cannot be called
    /// statically from a view model. <c>FromStream</c> is a genuine static
    /// factory and needs no receiver, so it is used instead. The bytes are
    /// captured in a local so the array stays alive for the lifetime of the
    /// source, since the factory only takes a stream factory.
    /// </remarks>
    /// <param name="bytes">The encoded image bytes.</param>
    /// <returns>An <see cref="ImageSource"/> over the supplied bytes.</returns>
    private static ImageSource CreateImageSourceFromBytes(byte[] bytes)
    {
        var captured = bytes;
        return ImageSource.FromStream(() => new MemoryStream(captured));
    }

    #endregion

    #region Public Methods Used by the View

    /// <summary>
    /// Recomputes the overlay rectangle from the current editor geometry.
    /// Called by the view when the image finishes loading or the editor is
    /// resized.
    /// </summary>
    public void RefreshOverlayGeometry()
    {
        if (suggestedRegion is null || editorController is null)
        {
            IsOverlayVisible = false;
            return;
        }

        ImageDisplayMetrics? metrics = editorController.GetDisplayMetrics();
        if (metrics is null)
        {
            IsOverlayVisible = false;
            return;
        }

        if (!ComputeOverlayPosition(metrics, suggestedRegion,
                out double x, out double y, out double width, out double height))
        {
            IsOverlayVisible = false;
            return;
        }

        OverlayX = x;
        OverlayY = y;
        OverlayWidth = width;
        OverlayHeight = height;
        IsOverlayVisible = width > 0 && height > 0;
    }

    /// <summary>
    /// Forwards property-changed notifications to the command set so that
    /// <c>CanExecute</c> is re-evaluated when relevant state changes.
    /// </summary>
    public void NotifyCommandsChanged()
    {
        OpenImageCommand.NotifyCanExecuteChanged();
        SmartCropCommand.NotifyCanExecuteChanged();
        ApplyCropCommand.NotifyCanExecuteChanged();
        ResetCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
        RetryCommand.NotifyCanExecuteChanged();
    }

    #endregion

    #region Property Change Handlers

    /// <inheritdoc />
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        switch (e.PropertyName)
        {
            case nameof(IsBusy):
            case nameof(IsSuggestionAvailable):
            case nameof(IsCropApplied):
            case nameof(IsError):
            case nameof(IsOverlayVisible):
                NotifyCommandsChanged();
                NotifyStageChanged();
                break;
            case nameof(StatusMessage):
            case nameof(IsImageLoaded):
                NotifyStageChanged();
                break;
            case nameof(SelectedIntent):
                NotifyStageChanged();
                NotifyCommandsChanged();
                break;
        }
    }

    #endregion

    #region Helpers

    partial void OnSelectedIntentChanged(string? value)
    {
        if (!IsImageLoaded || IsBusy)
        {
            return;
        }

        // A new intent invalidates any pending suggestion for the old one.
        if (IsSuggestionAvailable)
        {
            ClearSuggestion();
        }

        if (!IsCropApplied && !IsError)
        {
            StatusMessage = StageMessage();
        }
    }

    private string StageMessage()
    {
        if (!IsImageLoaded)
        {
            return string.Empty;
        }

        return IsIntentSelected
            ? "Generate an AI-powered crop suggestion."
            : "Select how you want to use this image.";
    }

    private void NotifyStageChanged()
    {
        OnPropertyChanged(nameof(IsIntentSelected));
        OnPropertyChanged(nameof(IsSuggestionReady));
        OnPropertyChanged(nameof(CanGenerateCrop));
        OnPropertyChanged(nameof(CanApplyCrop));
        OnPropertyChanged(nameof(CanReset));
        OnPropertyChanged(nameof(CanSave));
        OnPropertyChanged(nameof(HasStatusMessage));
    }

    private async Task LoadImageInternalAsync(ImageSource source, byte[] bytes)
    {
        IsBusy = true;
        IsError = false;
        ClearSuggestion();
        IsCropApplied = false;
        SelectedIntent = null;

        try
        {
            originalImageSource = source;
            originalImageBytes = bytes;

            // hasImage is set to true AFTER LoadImageAsync completes so that
            // no other command can act on a half-loaded image.
            await editorController!.LoadImageAsync(source);

            IsImageLoaded = true;

            RefreshOverlayGeometry();
            StatusMessage = StageMessage();
        }
        finally
        {
            IsBusy = false;
            NotifyCommandsChanged();
        }
    }

    private void ClearSuggestion()
    {
        suggestedRegion = null;
        IsOverlayVisible = false;
        IsSuggestionAvailable = false;
    }

    private void ShowError(string message)
    {
        suggestedRegion = null;
        IsOverlayVisible = false;
        IsSuggestionAvailable = false;
        IsError = true;
        StatusMessage = message;
        NotifyCommandsChanged();
    }

    private static bool ComputeOverlayPosition(
        ImageDisplayMetrics metrics,
        CropRegion region,
        out double x,
        out double y,
        out double width,
        out double height)
    {
        x = 0;
        y = 0;
        width = 0;
        height = 0;

        double originalWidth = metrics.OriginalSize.Width;
        double originalHeight = metrics.OriginalSize.Height;
        double renderedWidth = metrics.RenderedSize.Width;
        double renderedHeight = metrics.RenderedSize.Height;
        double controlWidth = metrics.ControlSize.Width;
        double controlHeight = metrics.ControlSize.Height;

        if (originalWidth <= 0 || originalHeight <= 0 ||
            renderedWidth <= 0 || renderedHeight <= 0 ||
            controlWidth <= 0 || controlHeight <= 0)
        {
            return false;
        }

        double scale = Math.Min(renderedWidth / originalWidth, renderedHeight / originalHeight);
        if (scale <= 0)
        {
            return false;
        }

        double imageOffsetX = (controlWidth - renderedWidth) / 2d;
        double imageOffsetY = (controlHeight - renderedHeight) / 2d;

        x = imageOffsetX + region.X * scale;
        y = imageOffsetY + region.Y * scale;
        width = region.Width * scale;
        height = region.Height * scale;

        return true;
    }

    #endregion
}
