using System.Diagnostics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Syncfusion.Maui.ImageEditor;
using SmartCropping.ViewModel;

namespace SmartCropping;

/// <summary>
/// Hosts the Smart Cropping UI. Implements <see cref="IImageEditorController"/>
/// so the view model can drive the underlying <see cref="SfImageEditor"/>
/// without knowing about the Syncfusion control surface.
/// </summary>
/// <remarks>
/// The view model never touches the editor directly; instead, it calls the
/// methods on this page, which forwards them to <see cref="SfImageEditor"/>.
/// Editor events (<see cref="SfImageEditor.ImageLoaded"/>,
/// <see cref="SfImageEditor.ImageSaving"/>) are observed here and translated
/// into view-model notifications.
/// </remarks>
public partial class SmartCropping : ContentPage, IImageEditorController
{
    private readonly SmartCroppingViewModel viewModel;
    private TaskCompletionSource<bool>? imageLoadedTaskSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartCropping"/> class.
    /// </summary>
    /// <param name="viewModel">The view model bound to this page.</param>
    public SmartCropping(SmartCroppingViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;

        ImageEditor.ImageLoaded += OnImageLoaded;
        ImageEditor.ImageSaving += OnImageSaving;
        ImageEditor.ImageSaved += OnImageSaved;
        ImageEditor.SizeChanged += OnEditorSizeChanged;

        viewModel.SetEditorController(this);
    }

    #region IImageEditorController

    /// <inheritdoc />
    public Size OriginalImageSize => ImageEditor.OriginalImageSize;

    /// <inheritdoc />
    public Size ImageRenderedSize => ImageEditor.ImageRenderedSize;

    /// <inheritdoc />
    public async Task LoadImageAsync(ImageSource source)
    {
        imageLoadedTaskSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        ImageEditor.Source = source;
        await imageLoadedTaskSource.Task;
    }

    /// <inheritdoc />
    public void ApplyCrop(Rect cropRectangle)
    {
        ImageEditor.Crop(cropRectangle, false);
    }

    /// <inheritdoc />
    public void StartManualCrop(ImageCropType cropType)
    {
        ImageEditor.Crop(cropType);
    }

    /// <inheritdoc />
    public async Task ResetEditorAsync()
    {
        if (ImageEditor.Source is null)
        {
            return;
        }

        imageLoadedTaskSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        try
        {
            ImageEditor.Reset();
        }
        catch
        {
            // Reset can throw when no image is loaded.
        }

        // Wait a short period for the ImageLoaded event to fire after the reset.
        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));
        var completedTask = await Task.WhenAny(imageLoadedTaskSource.Task, timeoutTask);

        if (completedTask == timeoutTask)
        {
            imageLoadedTaskSource.TrySetResult(false);
        }
    }

    /// <inheritdoc />
    public void SaveEdits()
    {
        ImageEditor.SaveEdits();
    }

    /// <inheritdoc />
    public void CancelEdits()
    {
        ImageEditor.CancelEdits();
    }

    /// <inheritdoc />
    public async Task<byte[]> ExportImageBytesAsync()
    {
        using Stream stream = await ImageEditor.GetImageStream();
        if (stream is null)
        {
            return Array.Empty<byte>();
        }

        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    /// <inheritdoc />
    public Task SaveImageAsync()
    {
        ImageEditor.Save(ImageFileType.Png);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public ImageDisplayMetrics? GetDisplayMetrics()
    {
        if (ImageEditor.Width <= 0 || ImageEditor.Height <= 0)
        {
            return null;
        }

        Size original = ImageEditor.OriginalImageSize;
        Size rendered = ImageEditor.ImageRenderedSize;

        if (original.Width <= 0 || original.Height <= 0 ||
            rendered.Width <= 0 || rendered.Height <= 0)
        {
            return null;
        }

        return new ImageDisplayMetrics
        {
            OriginalSize = original,
            RenderedSize = rendered,
            ControlSize = new Size(ImageEditor.Width, ImageEditor.Height)
        };
    }

    #endregion

    #region Event Handlers

    private void OnImageLoaded(object? sender, EventArgs e)
    {
        try
        {
            // When a manual crop is committed by the built-in UI, the editor
            // commits the change and re-renders the cropped image, which
            // re-fires ImageLoaded. Surface that completion to the VM.
            if (viewModel.IsManualCropping)
            {
                viewModel.NotifyManualCropCompleted(saved: true);
            }

            viewModel.NotifyCommandsChanged();

            _ = RefreshOverlayAfterLayoutAsync();
        }
        finally
        {
            imageLoadedTaskSource?.TrySetResult(true);
        }
    }

    private void OnEditorSizeChanged(object? sender, EventArgs e)
    {
        // The editor reports its rendered image size only after layout has
        // settled, so defer the overlay recomputation by one layout pass.
        _ = RefreshOverlayAfterLayoutAsync();
    }

    private async Task RefreshOverlayAfterLayoutAsync()
    {
        try
        {
            await Task.Yield();
            viewModel.RefreshOverlayGeometry();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Overlay refresh failed: {ex.Message}");
        }
    }

    private void OnImageSaving(object? sender, ImageSavingEventArgs e)
    {
        // Allow Syncfusion's default save flow to proceed.
        e.Cancel = false;
    }

    private void OnImageSaved(object? sender, ImageSavedEventArgs e)
    {
        Debug.WriteLine($"Image saved to: {e.Location}");
    }

    #endregion

    #region Lifecycle

    /// <inheritdoc />
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (ImageEditor.Source is null)
        {
            try
            {
                await viewModel.LoadSampleImageCommand.ExecuteAsync(null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load sample image on appearing: {ex.Message}");
            }
        }
    }

    #endregion
}
