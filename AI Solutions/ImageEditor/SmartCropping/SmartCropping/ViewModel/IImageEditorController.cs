using Microsoft.Maui.Graphics;
using Syncfusion.Maui.ImageEditor;

namespace SmartCropping.ViewModel;

/// <summary>
/// Abstraction the view model uses to drive the Syncfusion <c>SfImageEditor</c>.
/// </summary>
/// <remarks>
/// The view model cannot call <c>SfImageEditor</c> directly because the
/// control exposes no data-bindable command surface for crop, save, or load.
/// The view implements this interface and forwards every call to the
/// underlying <c>SfImageEditor</c> instance, which keeps business logic out
/// of the view while still letting the editor remain a view-layer concern.
/// </remarks>
public interface IImageEditorController
{
    /// <summary>
    /// Gets the original (loaded) image size in pixels.
    /// </summary>
    Size OriginalImageSize { get; }

    /// <summary>
    /// Gets the size of the image as currently rendered in the editor.
    /// </summary>
    Size ImageRenderedSize { get; }

    /// <summary>
    /// Loads the supplied image source into the editor and waits for it to
    /// render before returning.
    /// </summary>
    /// <param name="source">The image source to load.</param>
    Task LoadImageAsync(ImageSource source);

    /// <summary>
    /// Applies the supplied crop rectangle to the editor using original-image
    /// coordinates. Does not commit the change; call <see cref="SaveEdits"/>
    /// afterwards.
    /// </summary>
    /// <param name="cropRectangle">The rectangle to crop to.</param>
    void ApplyCrop(Rect cropRectangle);

    /// <summary>
    /// Opens the built-in interactive crop mode with the specified crop type.
    /// </summary>
    /// <param name="cropType">The crop shape the user can choose.</param>
    void StartManualCrop(ImageCropType cropType);

    /// <summary>
    /// Cancels any active editing, restores the originally loaded image, and
    /// waits for it to render.
    /// </summary>
    Task ResetEditorAsync();

    /// <summary>
    /// Commits any pending edits to the editor.
    /// </summary>
    void SaveEdits();

    /// <summary>
    /// Cancels any pending edits without committing them.
    /// </summary>
    void CancelEdits();

    /// <summary>
    /// Exports the editor's current image as PNG bytes.
    /// </summary>
    /// <returns>The encoded PNG bytes for the current image.</returns>
    Task<byte[]> ExportImageBytesAsync();

    /// <summary>
    /// Exports the editor's current image to a file using the operating system
    /// save dialog.
    /// </summary>
    Task SaveImageAsync();

    /// <summary>
    /// Returns the displayed (control-level) image rectangle in pixel
    /// coordinates. Returns <see langword="null"/> when the editor has not
    /// loaded an image yet.
    /// </summary>
    ImageDisplayMetrics? GetDisplayMetrics();
}

/// <summary>
/// Display geometry of the image currently rendered by the editor.
/// </summary>
public sealed class ImageDisplayMetrics
{
    /// <summary>
    /// Gets the size of the image as rendered in the editor's viewport.
    /// </summary>
    public Size RenderedSize { get; init; }

    /// <summary>
    /// Gets the size of the original (untransformed) image.
    /// </summary>
    public Size OriginalSize { get; init; }

    /// <summary>
    /// Gets the size of the editor control itself.
    /// </summary>
    public Size ControlSize { get; init; }
}
