using System.Diagnostics;
using Microsoft.Maui.Graphics;
using Syncfusion.Maui.ImageEditor;
using SmartCropping.ViewModel;

namespace SmartCropping;

/// <summary>
/// Bridges a <see cref="SfImageEditor"/> instance to
/// <see cref="SmartCroppingViewModel"/>. Both the desktop and mobile views
/// own one of these, so the view model never touches the Syncfusion
/// control directly and the two layouts share identical event wiring.
/// </summary>
public sealed class SmartCroppingViewBase
{
    private readonly SfImageEditor editor;
    private readonly SmartCroppingViewModel viewModel;
    private TaskCompletionSource<bool>? imageLoadedTaskSource;

    public SmartCroppingViewBase(SfImageEditor editor, SmartCroppingViewModel viewModel)
    {
        this.editor = editor;
        this.viewModel = viewModel;

        editor.ImageLoaded += OnImageLoaded;
        editor.ImageSaving += OnImageSaving;
        editor.ImageSaved += OnImageSaved;
        editor.SizeChanged += OnEditorSizeChanged;

        viewModel.SetEditorController(new EditorController(editor, this));
    }

    /// <summary>
    /// Force a re-paint of the overlay against the editor's current metrics.
    /// Called from the page after layout has settled.
    /// </summary>
    public void RefreshOverlay() => viewModel.RefreshOverlayGeometry();

    private void OnImageLoaded(object? sender, EventArgs e)
    {
        try
        {
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

    private void OnImageSaving(object? sender, ImageSavingEventArgs e) => e.Cancel = false;

    private void OnImageSaved(object? sender, ImageSavedEventArgs e) =>
        Debug.WriteLine($"Image saved to: {e.Location}");

    /// <summary>Completes the LoadImageAsync awaiter.</summary>
    internal void CompleteImageLoad() => imageLoadedTaskSource?.TrySetResult(true);

    /// <summary>Starts a fresh load and returns a task that completes on ImageLoaded.</summary>
    internal Task BeginImageLoadAsync()
    {
        imageLoadedTaskSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        return imageLoadedTaskSource.Task;
    }

    /// <summary>
    /// Adapter that exposes <see cref="SfImageEditor"/> through
    /// <see cref="IImageEditorController"/>. Implemented in the bridge so
    /// both views can share the same controller surface.
    /// </summary>
    private sealed class EditorController : IImageEditorController
    {
        private readonly SfImageEditor editor;
        private readonly SmartCroppingViewBase owner;

        public EditorController(SfImageEditor editor, SmartCroppingViewBase owner)
        {
            this.editor = editor;
            this.owner = owner;
        }

        public Size OriginalImageSize => editor.OriginalImageSize;
        public Size ImageRenderedSize => editor.ImageRenderedSize;

        public async Task LoadImageAsync(ImageSource source)
        {
            Task loaded = owner.BeginImageLoadAsync();
            editor.Source = source;
            await loaded;
        }

        public void ApplyCrop(Rect cropRectangle)
        {
            // Syncfusion's SfImageEditor.Crop(Rect, bool) interprets the rect
            // in RENDERED-image coordinate space (the same space as
            // ImageRenderedSize / imageLayout.Frame), NOT in original-pixel
            // space. The view model gives us original-pixel coordinates, so
            // convert them to rendered-image space before the call. The
            // conversion must use the per-axis scale: the editor's crop
            // pipeline uses renderedSize/originalSize on each axis
            // independently when it maps the rect back to the source bitmap.
            Size original = editor.OriginalImageSize;
            Size rendered = editor.ImageRenderedSize;

            if (original.Width <= 0 || original.Height <= 0 ||
                rendered.Width <= 0 || rendered.Height <= 0)
            {
                return;
            }

            double scaleX = rendered.Width / original.Width;
            double scaleY = rendered.Height / original.Height;
            if (scaleX <= 0 || scaleY <= 0)
            {
                return;
            }

            Rect renderedRect = new Rect(
                cropRectangle.X * scaleX,
                cropRectangle.Y * scaleY,
                cropRectangle.Width * scaleX,
                cropRectangle.Height * scaleY);

            editor.Crop(renderedRect, false);
        }

        public Task ResetEditorAsync()
        {
            if (editor.Source is null)
            {
                return Task.CompletedTask;
            }

            try
            {
                editor.Reset();
            }
            catch
            {
                // Reset can throw when no image is loaded.
            }

            return Task.CompletedTask;
        }

        public void SaveEdits() => editor.SaveEdits();
        public void CancelEdits() => editor.CancelEdits();

        public async Task<byte[]> ExportImageBytesAsync()
        {
            using Stream stream = await editor.GetImageStream();
            if (stream is null)
            {
                return Array.Empty<byte>();
            }

            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        public Task SaveImageAsync()
        {
            editor.Save(ImageFileType.Png);
            return Task.CompletedTask;
        }

        public ImageDisplayMetrics? GetDisplayMetrics()
        {
            if (editor.Width <= 0 || editor.Height <= 0)
            {
                return null;
            }

            Size original = editor.OriginalImageSize;
            Size rendered = editor.ImageRenderedSize;

            if (original.Width <= 0 || original.Height <= 0 ||
                rendered.Width <= 0 || rendered.Height <= 0)
            {
                return null;
            }

            return new ImageDisplayMetrics
            {
                OriginalSize = original,
                RenderedSize = rendered,
                ControlSize = new Size(editor.Width, editor.Height)
            };
        }
    }
}